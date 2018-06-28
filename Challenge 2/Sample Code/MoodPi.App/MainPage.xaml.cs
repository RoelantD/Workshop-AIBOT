using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage.Streams;
using Windows.System.Display;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using RaspberryModules.App.Modules;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MoodPi.App
{

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            StartVideoPreviewAsync();
            InitPirSensor();
            _rgbLed.Init();
        }

        private readonly RGBLed _rgbLed = new RGBLed();

        private readonly DisplayRequest _displayRequest = new DisplayRequest();

        private readonly MediaCapture _mediaCapture = new MediaCapture();

        private readonly SemaphoreSlim _frameProcessingSemaphore = new SemaphoreSlim(1);

        private readonly FaceServiceClient _faceServiceClient = new FaceServiceClient("<API KEY>", "https://<REGION>.api.cognitive.microsoft.com/face/v1.0");

        private ThreadPoolTimer _frameProcessingTimer;

        public VideoEncodingProperties VideoProperties;
        
        private bool _motion;

        private GpioPin _motionPin;

        private async Task StartVideoPreviewAsync()
        {
            await _mediaCapture.InitializeAsync();
            _displayRequest.RequestActive();

            PreviewControl.Source = _mediaCapture;
            await _mediaCapture.StartPreviewAsync();

            TimeSpan timerInterval = TimeSpan.FromMilliseconds(100); //10fps
            _frameProcessingTimer = ThreadPoolTimer.CreatePeriodicTimer(new TimerElapsedHandler(ProcessCurrentVideoFrame), timerInterval);
            VideoProperties = _mediaCapture.VideoDeviceController.GetMediaStreamProperties(MediaStreamType.VideoPreview) as VideoEncodingProperties;

            Debug.WriteLine("Camera ready.");
        }

        private async void ProcessCurrentVideoFrame(ThreadPoolTimer timer)
        {
            if (_mediaCapture.CameraStreamState != Windows.Media.Devices.CameraStreamState.Streaming || !_frameProcessingSemaphore.Wait(0))
            {
                return;
            }

            if (!_motion)
            {
                Debug.WriteLine("No motion detected.");
                _frameProcessingSemaphore.Release();
                return;
            }

            try
            {
                var stream = new InMemoryRandomAccessStream();
                await _mediaCapture.CapturePhotoToStreamAsync(ImageEncodingProperties.CreateJpeg(), stream);
                MemoryStream memStream = await ConvertFromInMemoryRandomAccessStream(stream);
                
                Face[] result = await _faceServiceClient.DetectAsync(memStream, false, false, new[] { FaceAttributeType.Emotion });

                string displayText = $"{result.Length} faces found | {DateTime.Now.ToLongTimeString()}";

                if (result.Any())
                {
                    List<EmotionResult> emotions = new List<EmotionResult>
                    {
                        new EmotionResult() { Name = "Anger", Score = result.First().FaceAttributes.Emotion.Anger, LedStatus = LedStatus.Red },
                        new EmotionResult() { Name = "Happiness",Score = result.First().FaceAttributes.Emotion.Happiness, LedStatus = LedStatus.Green },
                        new EmotionResult() { Name = "Neutral", Score = result.First().FaceAttributes.Emotion.Neutral, LedStatus = LedStatus.Blue }
                    };

                    displayText += string.Join(", ", emotions.Select(a => $"{a.Name}: {(a.Score * 100.0f).ToString("#0.00")}"));

                    _rgbLed.TurnOnLed(emotions.OrderByDescending(a => a.Score).First().LedStatus);
                }

                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => StatusText.Text = displayText);
                Debug.WriteLine(displayText);
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


        private void InitPirSensor() {

            try
            {
                var gpio = GpioController.GetDefault();
                _motionPin = gpio.OpenPin(21);
                if (_motionPin != null)
                {
                    _motionPin.SetDriveMode(GpioPinDriveMode.Input);
                    _motionPin.ValueChanged += PirSensorChanged;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("No GPIO ports found");
                _motion = true;
            }

        }

        private void PirSensorChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            _motion = (args.Edge == GpioPinEdge.RisingEdge);
        }

        private async Task<MemoryStream> ConvertFromInMemoryRandomAccessStream(InMemoryRandomAccessStream inputStream)
        {
            var reader = new DataReader(inputStream.GetInputStreamAt(0));
            var bytes = new byte[inputStream.Size];
            await reader.LoadAsync((uint)inputStream.Size);
            reader.ReadBytes(bytes);

            var outputStream = new MemoryStream(bytes);
            return outputStream;
        }

    }


    public class EmotionResult
    {
        public string Name { get; set; }

        public float Score { get; set; }

        public LedStatus LedStatus { get; set; }
    }
}
