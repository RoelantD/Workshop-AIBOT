using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.System.Display;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using RaspberryModules.App.Modules;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TheSeeingPI.App
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();

            _spiDisplay.InitAll();

            StartVideoPreviewAsync();

            LoadModelAsync();
        }

        private readonly SPIDisplay _spiDisplay = new SPIDisplay();

        private readonly DisplayRequest _displayRequest = new DisplayRequest();

        private readonly MediaCapture _mediaCapture = new MediaCapture();

        private readonly SemaphoreSlim _frameProcessingSemaphore = new SemaphoreSlim(1);

        private ThreadPoolTimer _frameProcessingTimer;

        public VideoEncodingProperties VideoProperties;

        private string _modelFileName = "mycustomvision.onnx";

        private MyCustomVisionModel _model = null;

        private async Task StartVideoPreviewAsync()
        {
            await _mediaCapture.InitializeAsync();
            _displayRequest.RequestActive();

            PreviewControl.Source = _mediaCapture;
            await _mediaCapture.StartPreviewAsync();

            TimeSpan timerInterval = TimeSpan.FromMilliseconds(66); //15fps
            _frameProcessingTimer = ThreadPoolTimer.CreatePeriodicTimer(new TimerElapsedHandler(ProcessCurrentVideoFrame), timerInterval);
            VideoProperties = _mediaCapture.VideoDeviceController.GetMediaStreamProperties(MediaStreamType.VideoPreview) as VideoEncodingProperties;
        }

        private async void ProcessCurrentVideoFrame(ThreadPoolTimer timer)
        {
            if (_mediaCapture.CameraStreamState != Windows.Media.Devices.CameraStreamState.Streaming || !_frameProcessingSemaphore.Wait(0))
            {
                return;
            }

            try
            {
                using (VideoFrame previewFrame = new VideoFrame(BitmapPixelFormat.Bgra8, (int)VideoProperties.Width, (int)VideoProperties.Height))
                {
                    await _mediaCapture.GetPreviewFrameAsync(previewFrame);

                    // Evaluate the image
                    // await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => StatusText.Text = $"Analyzing frame {DateTime.Now.ToLongTimeString()}");
                    await Task.Run(async () =>
                    {
                        await EvaluateVideoFrameAsync(previewFrame);
                    });

                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception with ProcessCurrentVideoFrame: " + ex);
            }
            finally
            {
                _frameProcessingSemaphore.Release();
            }
        }


        private async Task EvaluateVideoFrameAsync(VideoFrame frame)
        {
            if (frame != null)
            {
                try
                {

                    MyCustomVisionModelInput inputData = new MyCustomVisionModelInput
                    {
                        data = frame
                    };
                    var results = await _model.EvaluateAsync(inputData);
                    var loss = results.loss.ToList().OrderByDescending(x => x.Value);

                    var lossStr = string.Join(",  ", loss.Select(l => l.Key + " " + (l.Value * 100.0f).ToString("#0.00") + "%"));
                    var message = $" Predictions: {lossStr}";

                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => StatusText.Text = message);
                    Debug.WriteLine(message);

                    List<string> linesToDisplay = loss.Take(4).Select(a => $"{a.Key} {(a.Value * 100.0f):#0.00}%").ToList();
                    _spiDisplay.WriteLinesToScreen(linesToDisplay);

                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"error: {ex.Message}");
                }
            }
        }

        private async Task LoadModelAsync()
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => StatusText.Text = $"Loading {_modelFileName}");

            var modelFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri($"ms-appx:///Assets/{_modelFileName}"));
            _model = await MyCustomVisionModel.CreateMyCustomVisionModel(modelFile);

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => StatusText.Text = $"Loaded {_modelFileName}");
        }
    }
}
