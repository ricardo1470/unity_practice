/*========================================================================
Copyright (c) 2020 PTC Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other
countries.
=========================================================================*/


using UnityEngine;
using UnityEngine.UI;

public class NotificationUI : MonoBehaviour
{
    public NavigationHandler SceneNavigationHandler;
    public Canvas NotificationCanvas;
    public Text MessageText;
    public Button CloseButton;

    
    public void ShowNotification(string message, bool exitOnClose = false)
    {
        if (!SceneNavigationHandler || !NotificationCanvas || !MessageText || !CloseButton)
        {
            Debug.LogError("NotificationUI has not been configured correctly. " +
                           "Make sure all the class fields have valid object references assigned to them.");
            return;
        }
        
        CloseButton.onClick.RemoveAllListeners();
    
        MessageText.text = message;
        if (exitOnClose)
            CloseButton.onClick.AddListener(SceneNavigationHandler.HandleBackButtonPressed);
        else
            CloseButton.onClick.AddListener(() => NotificationCanvas.enabled = false);

        NotificationCanvas.enabled = true;
    }
}
