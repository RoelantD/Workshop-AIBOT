# Ask me PI


## Part 1 - Language Understanding

### Setup LUIS
* Go to [LUIS.AI](https://www.luis.ai) 
* Login with your Microsoft Pasport.

![](Assets/img_luis_1001.jpg)

* Click "Create a new app"

![](Assets/img_luis_1002.jpg)

* Enter a name for your app
* Click "create"

![](Assets/img_luis_1003.jpg)
* Click "Create new intent"
* Type the intent name: "ControlLED"

![](Assets/img_luis_1004.jpg)
* Enter the following examples:
    * Turn the light on
    * Turn the light off
    * Turn on the light
    * Turn the green light on
    * Put the light on
    * Put the red light off
    
![](Assets/img_luis_1005.jpg)
* Click in the left menu "Entities"
* Click "Create new entity"
* Enter a name: "LedState"
* Click "done"
* Create another Entity "LedColor"

![](Assets/img_luis_1006.jpg)
* Open "Intents"
* Open the intent "ControlLED"
* Click on the word "on"
* Select the Entity "LedState" in the dropdown
* Repeat this for all the words: "on" and "off"
* Select all the colors and link them to the entity "LedColor"

![](Assets/img_luis_1007.jpg)
* Your project should look the picture above.

* Click the "train" button in the top right
* When the training is done click the "test" button

![](Assets/img_luis_1008.jpg)
* Type the sentence: "turn the yellow light on"
* Notice that the word "yellow" and "on" are mapped to the corresponding entities.
* Try some other sentence

![](Assets/img_luis_1008.jpg)
* Open the "publish" section
* Publish the app to production
* Scroll down en copy the API key
* Open the "settings" section 
* Copy the application id


## Part 2 - Build the UWP App

### 2.1 Create the app

![](Assets/img_3010.jpg)
* File > New Project
* Select: Visual C > Windows Universial > Blank App (Universial App)
* Select: Build 17134 (If you don't see this version please go back to the requirements for this workshop)

### 2.2 Enable the Microphone

![](Assets/img_3011.jpg)

* Open the "Package.appxmanifest" file
* Open the tab: "Capabilities"
* Check the checkbox "Webcam"
* Check the checkbox "Microphone"

### 2.3 Listen

* Open the file: "MainPage.xaml.cs
* Add this code to the class: "MainPage"
```
private SpeechRecognizer _contSpeechRecognizer;

protected override async void OnNavigatedTo(NavigationEventArgs e)
{
   _contSpeechRecognizer = new SpeechRecognizer();
   await _contSpeechRecognizer.CompileConstraintsAsync();
   _contSpeechRecognizer.ContinuousRecognitionSession.ResultGenerated +=ContinuousRecognitionSession_ResultGenerated;
   _contSpeechRecognizer.ContinuousRecognitionSession.Completed += ContinuousRecognitionSession_Completed;
   await _contSpeechRecognizer.ContinuousRecognitionSession.StartAsync();
}

private async void ContinuousRecognitionSession_Completed(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionCompletedEventArgs args)
{
   Debug.WriteLine($"Completed > Restart listening");
   await _contSpeechRecognizer.ContinuousRecognitionSession.StartAsync();
}

private async void ContinuousRecognitionSession_ResultGenerated(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs args)
{
   string speechResult = args.Result.Text;
   Debug.WriteLine($"Text: {speechResult}");
}
```
* Run the application and validate that you see what say in the debug output
![](Assets/img_chall_app_001.JPG)

### 2.4 Understand


### 2.5 Speak


### 2.6 Act

### 
* Add the nuget package: "Microsoft.Cognitive.LUIS"


## Part 3 - Run it on the RaspBerry PI 3
![alt text](Assets/challenge_1_bb.png)  

* Connect the display to the RaspBerry
* Connect all the wires exactly the same as in the schema below.

*Don't forget to remove the power*


