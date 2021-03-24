/*============================================================================== 
Copyright (c) 2020 PTC Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other 
countries.   
==============================================================================*/


using UnityEngine;
using UnityEngine.UI;
using Vuforia;

public class AboutManagerTestApp : MonoBehaviour
{
    public Text AboutText;

    void Start()
    {
        UpdateAboutText();
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Return))
        {
            // Treat 'Return' key as pressing the Close button and dismiss the About Screen
            OnStartAR();
        }
        else if (Input.GetKeyUp(KeyCode.JoystickButton0))
        {
            // Similar to above except detecting the first Joystick button
            // Allows external controllers to dismiss the About Screen
            // On an ODG R7 this is the select button
            OnStartAR();
        }

        if (Input.GetKeyUp(KeyCode.Escape))
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #elif UNITY_ANDROID 
            // On Android, the Back button is mapped to the Esc key
            // Exit app
            Application.Quit();
            #endif
        }
    }

    void UpdateAboutText()
    {
        if (!AboutText)
        {
            return;
        }

        var vuforiaVersion = VuforiaUnity.GetVuforiaLibraryVersion();
        var unityVersion = Application.unityVersion;
        
        var about =
            "\n<size=26>Description:</size>" +
            "\nThe Area Targets Test app allows you to detect " +
            "and track Area Targets in your scanned area." +

            "\n" +
            "\n<size=26>Key Functionality:</size>" +
            "\n• Detection and tracking of Area Targets" +

            "\n• Tracker reset" +
            "\n" +
            "\n<size=26>Physical Targets:</size>" +
            "\n• AreaTarget: Area from which the dataset was scanned" +
            "\n" +
            "\n<size=26>Instructions:</size>" +
            "\n• Point camera at the scanned area to start tracking" +
            "\n" +
            "\n<size=26>Compatible Devices:</size>" +
            "\nhttps://library.vuforia.com/content/vuforia-library/en/articles/Solution/model-target-supported-devices.html" +
            "\n" +
            "\n<size=26>Build Version Info:</size>" +
            "\n• Vuforia " + vuforiaVersion +
            "\n• Unity " + unityVersion +

            "\n" +
            "\n<size=26>Developer Agreement:</size>" +
            "\nhttps://developer.vuforia.com/legal/vuforia-developer-agreement" +
            "\n" +

            "\n<size=26>Terms of Use:</size>" +
            "\nhttps://developer.vuforia.com/legal/EULA" +
            "\n" +
            "\n© 2019 PTC Inc. All Rights Reserved." +
            "\n";

        AboutText.text = about;

        Debug.Log("Vuforia " + vuforiaVersion + "\nUnity " + unityVersion);

    }
    
    public void OnStartAR()
    {
        VuforiaRuntime.Instance.InitVuforia(); //delay initialization due to issue with not being able to show permission requests at the same time.

        UnityEngine.SceneManagement.SceneManager.LoadScene("2-Loading");
    }
}
