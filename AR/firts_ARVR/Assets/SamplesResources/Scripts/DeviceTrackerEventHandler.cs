/*==============================================================================
Copyright (c) 2020 PTC Inc. All Rights Reserved.

Confidential and Proprietary - Protected under copyright and other laws.
==============================================================================*/


using UnityEngine;
using Vuforia;

public class DeviceTrackerEventHandler : MonoBehaviour
{
    public GameObject ResetPopup;

    DeviceTrackerHelper mDeviceTrackerHelper;
    bool mRelocalizing;
    float mRelocalizingTime;

    const float RELOCALIZING_THRESHOLD = 10;

    void Awake()
    {
        mDeviceTrackerHelper = FindObjectOfType<DeviceTrackerHelper>();
    }

    void Start()
    {
        DeviceTrackerARController.Instance.RegisterDevicePoseStatusChangedCallback(OnDevicePoseStatusChanged);
    }

    void Update()
    {
        if (!mRelocalizing) return;
        
        mRelocalizingTime += Time.deltaTime;
        if (mRelocalizingTime >= RELOCALIZING_THRESHOLD)
        {
            mDeviceTrackerHelper.ResetDeviceTracker();
            RelocalizationStopped();
        }
    }
    
    void OnDestroy()
    {
        DeviceTrackerARController.Instance.UnregisterDevicePoseStatusChangedCallback(OnDevicePoseStatusChanged);
    }

    void OnDevicePoseStatusChanged(TrackableBehaviour.Status status, TrackableBehaviour.StatusInfo statusInfo)
    {
        if (statusInfo == TrackableBehaviour.StatusInfo.RELOCALIZING)
            RelocalizationStarted();
        else
            RelocalizationStopped();
    }

    void RelocalizationStarted()
    {
        mRelocalizing = true;
        mRelocalizingTime = 0;
        ResetPopup.SetActive(true);
    }

    void RelocalizationStopped()
    {
        mRelocalizing = false;
        mRelocalizingTime = 0;
        ResetPopup.SetActive(false);
    }
}