/*==============================================================================
Copyright (c) 2020 PTC Inc. All Rights Reserved.

Confidential and Proprietary - Protected under copyright and other laws.
==============================================================================*/


using UnityEngine;
using UnityEngine.UI;

public class DeviceTrackerResetPopup : MonoBehaviour
{
    public Text TimerText;

    float mElapsedTime;

    const float RESET_TIME = 10f;
    const string RESET_TIMER_LABEL = "The Tracker will reset automatically in ";
    
    void Update()
    {
        mElapsedTime += Time.deltaTime;
        TimerText.text = RESET_TIMER_LABEL + (Mathf.CeilToInt(RESET_TIME - mElapsedTime) + "s");
        
        if (mElapsedTime >= RESET_TIME)
        {
            gameObject.SetActive(false);
        }
    }

    void OnEnable()
    {
        mElapsedTime = 0;
    }
}