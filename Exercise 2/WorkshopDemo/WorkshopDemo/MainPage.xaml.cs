using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Timers;
using Windows.Devices.Gpio;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using RaspberryModules.App.Modules;
using WorkshopDemo.Modules;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace WorkshopDemo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {

            this.InitializeComponent();
            Init_LEDS();
            InitScreen();

            // Yello LED
            yellowLEd.Init(26);
            yellowLEd.TurnOff();

            // RGB LED Disco
            _rgbLed.Init();
            var aTimer = new System.Timers.Timer();
            aTimer.Elapsed += DoDisco;
            aTimer.Interval = 2500;
            aTimer.Enabled = true;

        }

        private async Task InitScreen()
        {
            await _spiDisplay.InitAll();

            _spiDisplay.WriteLinesToScreen(new List<string> { "Hello World" });

            var displayTimer = new System.Timers.Timer();
            displayTimer.Elapsed += UpdateScreen;
            displayTimer.Interval = 1000;
            displayTimer.Enabled = true;
        }

        private GpioPin _yellowpin;

        private GpioPin _motionPin;

        private DispatcherTimer timer;

        private SPIDisplay _spiDisplay = new SPIDisplay();

        private RGBLed _rgbLed = new RGBLed();

        private SingleLED yellowLEd = new SingleLED();

        private void Init_LEDS()
        {
            // Init the motion Sensor
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
            }
        }

        private void DoDisco(object source, ElapsedEventArgs e)
        {
            Array values = Enum.GetValues(typeof(LedStatus));
            Random random = new Random();
            LedStatus randomBar = (LedStatus)values.GetValue(random.Next(values.Length-1));

            _rgbLed.TurnOnLed(randomBar);
        }

        private void UpdateScreen(object source, ElapsedEventArgs e)
        {
            _spiDisplay.WriteLinesToScreen(new List<string> { $"{DateTime.Now.ToLongTimeString()}"});
        }

        private void PirSensorChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            bool motion = (args.Edge == GpioPinEdge.RisingEdge);

            if (motion)
            {
                Debug.WriteLine("Motion Detected");
                yellowLEd.TurnOn();
            }
            else
            {
                Debug.WriteLine("No Motion Detected");
                yellowLEd.TurnOff();
            }
        }
    }
}
