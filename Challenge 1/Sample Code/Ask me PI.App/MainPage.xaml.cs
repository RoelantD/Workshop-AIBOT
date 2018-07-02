using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Media.SpeechRecognition;
using Windows.Media.SpeechSynthesis;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Microsoft.Cognitive.LUIS;
using RaspberryModules.App.Modules;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Ask_me_PI.App
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        private readonly string _luiskey = "<INSERT LUIS KEY>";

        private readonly string _appId = "<INSERT LUIS APP ID>";

        private readonly RGBLed _rgbLed  = new RGBLed();

        private readonly SpeechSynthesizer _synthesizer = new SpeechSynthesizer();

        private readonly MediaPlayer _speechPlayer = new MediaPlayer();

        private SpeechRecognizer _contSpeechRecognizer;


        public MainPage()
        {
            this.InitializeComponent();
            _rgbLed.Init();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            _contSpeechRecognizer = new SpeechRecognizer();
            await _contSpeechRecognizer.CompileConstraintsAsync();
            _contSpeechRecognizer.ContinuousRecognitionSession.ResultGenerated += ContinuousRecognitionSession_ResultGenerated;
            _contSpeechRecognizer.ContinuousRecognitionSession.Completed += ContinuousRecognitionSession_Completed;
            await _contSpeechRecognizer.ContinuousRecognitionSession.StartAsync();

            var voice = SpeechSynthesizer.AllVoices.FirstOrDefault(i => i.Gender == VoiceGender.Female) ?? SpeechSynthesizer.DefaultVoice;
            _synthesizer.Voice = voice;           
        }

        public async Task SayAsync(string text)
        {
            using (var stream = await _synthesizer.SynthesizeTextToStreamAsync(text))
            {
                _speechPlayer.Source = MediaSource.CreateFromStream(stream, stream.ContentType);
            }
            _speechPlayer.Play();
        }

        private async void ContinuousRecognitionSession_Completed(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionCompletedEventArgs args)
        {
            Debug.WriteLine($"Completed > Restart listening");
            await _contSpeechRecognizer.ContinuousRecognitionSession.StartAsync();
        }

        private async void ContinuousRecognitionSession_ResultGenerated(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {
            string speechResult = args.Result.Text;
            Debug.WriteLine($"Heared: {speechResult}");

            if (speechResult.ToLower().StartsWith("hey"))
            {
                LuisClient client = new LuisClient(_appId,_luiskey);
                LuisResult result = await client.Predict(speechResult);

                HandleLuisResult(result);

                Debug.WriteLine($"LUIS Result: {result.Intents.First().Name} {string.Join(",", result.Entities.Select(a => $"{a.Key}:{a.Value.First().Value}"))}");
            }
        }

        public void HandleLuisResult(LuisResult result)
        {
            if (!result.Intents.Any())
            {
                return;
            }

            switch (result.Intents.First().Name)
            {
                case "ControlLED":

                    if (result.Entities.Any(a => a.Key == "LedState"))
                    {
                        string ledState = result.Entities.First(a => a.Key == "LedState").Value.First().Value;

                        if (ledState == "on")
                        {
                            if (result.Entities.Any(a => a.Key == "LedColor"))
                            {
                                string ledColor = result.Entities.First(a => a.Key == "LedColor").Value.First().Value;

                                SayAsync($"Turning on the {ledColor} light.");

                                switch (ledColor)
                                {
                                    case "red":
                                        _rgbLed.TurnOnLed(LedStatus.Red);
                                        break;

                                    case "green":
                                        _rgbLed.TurnOnLed(LedStatus.Green);
                                        break;

                                    case "blue":
                                        _rgbLed.TurnOnLed(LedStatus.Blue);
                                        break;

                                    case "purple":
                                        _rgbLed.TurnOnLed(LedStatus.Purple);
                                        break;
                                }
                            }
                        }
                        else if (ledState == "off")
                        {
                            SayAsync("Turning the light off.");
                            _rgbLed.TurnOffLed();
                        }
                    }
                    break;
            }
        }
    }
}
