# Mood PI #

Welcome to the Mood PI challenge. In this challenge you are going to make the PI is reflect you mood.

> **Challenge**:   
> Make a program that runs on your Rapsberry PI and reflects your emotions in colors.

### Requirements 
- Use Emotions API
- Use the motion sensor to trigger to detection
- Use the RGB LED to show the emotion
  - Green = Smile
  - Blue = Neutral
  - Red = Anger

| . | . |
| ---- | ---- |
| **Difficulty** | Easy |
| **Duration** | 1-2 hours |
| **Challenge Points** | 10 points |
| **Modules** | Emotion API |
| **Sensors**| RGB LED / Motion Sensor / Camera |  

## Part 1 - Create a face API

* Login to the Azure Portal
* Create a Face API Endpoint [Create endpoint](https://portal.azure.com/#create/Microsoft.CognitiveServicesFace)
![alt text](assets/img_2001.jpg)

* Open your newly created Face API and copy the "API Key" and remember the region
![alt text](assets/img_2002.jpg)

# Part 2: Build the UWP App

### Create the app
![alt text](assets/img_2002.jpg)
* File > New Project
* Select: Visual C > Windows Universial > Blank App (Universial App)
* Select: Build 17134 (If you don't see this version please go back to the requirements for this workshop)

## 2.1 The camera

### Enable the Camera
![alt text](assets/img_3011.jpg)

* Open the "Package.appxmanifest" file
* Open the tab: "Capabilities"
* Check the checkbox "Webcam"

### Showing the camera feed

* Open the file: "MainPage.xaml"
* Add the code below between the "grid" tags:
```         
<StackPanel>
   <TextBlock x:Name="StatusText" FontWeight="Bold" TextWrapping="Wrap" Text="...."/>
   <CaptureElement Name="PreviewControl" Stretch="Uniform"/>
</StackPanel> 
```
* Open the file: "MainPage.xaml.cs
* Add this code to the class: "MainPage"
```
private readonly DisplayRequest _displayRequest = new DisplayRequest();

private readonly MediaCapture _mediaCapture = new MediaCapture();

private async Task StartVideoPreviewAsync()
{
   await _mediaCapture.InitializeAsync();
   _displayRequest.RequestActive();

   PreviewControl.Source = _mediaCapture;
   await _mediaCapture.StartPreviewAsync();
}
```
* Call the StartVideoPreviewAsync method from the constructor
* Run the application and validate you can see the camera feed

## 2.2 Analyze the camera feed

### Grabbing the frames from the camera

* Open the file: "MainPage.xaml.cs
* Add this code to the class: "MainPage"
```
private readonly SemaphoreSlim _frameProcessingSemaphore = new SemaphoreSlim(1);

private ThreadPoolTimer _frameProcessingTimer;

public VideoEncodingProperties VideoProperties;
```
* Add this lines to the "StartVideoPreviewAsync" method
```
TimeSpan timerInterval = TimeSpan.FromMilliseconds(66); //15fps
_frameProcessingTimer = ThreadPoolTimer.CreatePeriodicTimer(new TimerElapsedHandler(ProcessCurrentVideoFrame), timerInterval);
VideoProperties = _mediaCapture.VideoDeviceController.GetMediaStreamProperties(MediaStreamType.VideoPreview) as VideoEncodingProperties;
```
* Add this method:
```
private async void ProcessCurrentVideoFrame(ThreadPoolTimer timer)
{
   if (_mediaCapture.CameraStreamState != Windows.Media.Devices.CameraStreamState.Streaming || !_frameProcessingSemaphore.Wait(0))
   {
       return;
   }

   try
   {
        // Evaluate the image
        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => StatusText.Text = $"Analyzing frame {DateTime.Now.ToLongTimeString()}");

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

private async Task<MemoryStream> ConvertFromInMemoryRandomAccessStream(InMemoryRandomAccessStream inputStream)
{
    var reader = new DataReader(inputStream.GetInputStreamAt(0));
    var bytes = new byte[inputStream.Size];
    await reader.LoadAsync((uint)inputStream.Size);
    reader.ReadBytes(bytes);

    var outputStream = new MemoryStream(bytes);
    return outputStream;
}

```
![alt text](assets/img_3013.jpg)
* Run the application and validate that every second a frame is analyzed


## Add the face API
* Add the NuGet package Microsoft.ProjectOxford.Face
* 




## Part 3 - Run it on the RaspBerry PI 3
