/*========================================================================
Copyright (c) 2020 PTC Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other
countries.
=========================================================================*/


using UnityEngine;
using Vuforia;

public class DeviceTrackerHelper : MonoBehaviour
{
    NotificationUI mNotificationUI;
    
    const string ERROR_DEVICE_TRACKER_NOT_INITIALIZED = "Positional Device Tracker is not initialized.\nPlease consult the User Guide.";
    const string ERROR_DEVICE_TRACKER_PLATFORM = "Area Targets requires a device with platform tracker such as ARCore or ARKit, depending on the platform. ";

    void Awake()
    {
        mNotificationUI = FindObjectOfType<NotificationUI>();
    }

    void Start()
    {
        VuforiaARController.Instance.RegisterVuforiaStartedCallback(OnVuforiaStarted);
    }
    
    void OnVuforiaStarted()
    {
        if (!VuforiaRuntimeUtilities.IsSimulatorMode())
        {
            if (VuforiaRuntimeUtilities.GetActiveFusionProvider() != FusionProviderType.PLATFORM_SENSOR_FUSION)
            {
                if (mNotificationUI)
                    mNotificationUI.ShowNotification(ERROR_DEVICE_TRACKER_PLATFORM, true);
                return;
            }

            if (TrackerManager.Instance.GetTracker<PositionalDeviceTracker>() == null)
            {
                if (mNotificationUI)
                    mNotificationUI.ShowNotification(ERROR_DEVICE_TRACKER_NOT_INITIALIZED, true);
            }
        }
    }
    
    void OnDestroy()
    {
        VuforiaARController.Instance.UnregisterVuforiaStartedCallback(OnVuforiaStarted);
    }
    
    public void ResetDeviceTracker()
    {
        var deviceTracker = TrackerManager.Instance.GetTracker<PositionalDeviceTracker>();

        if (deviceTracker != null && deviceTracker.Reset())
        {
            Debug.Log("Successfully reset Device Tracker");
        }
        else
        {
            Debug.LogError("Failed to reset Device Tracker");
        }
    }
}
