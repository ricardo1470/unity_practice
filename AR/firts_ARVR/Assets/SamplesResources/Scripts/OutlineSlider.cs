/*==============================================================================
Copyright (c) 2020 PTC Inc. All Rights Reserved.

Confidential and Proprietary - Protected under copyright and other laws.
==============================================================================*/


using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class OutlineSlider : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Slider used to control the size of the outline.")]
    Slider Slider = null;
    
    [SerializeField]
    [Tooltip("Panel containing the Slider.")]
    GameObject SliderPanel = null;
    
    HashSet<UnityAction<float>> mListeners = new HashSet<UnityAction<float>>();

    void Awake()
    {
        if (Slider)
            Slider.onValueChanged.AddListener(OnValueChanged);
    }

    void OnDestroy()
    {
        if (Slider)
            Slider.onValueChanged.RemoveListener(OnValueChanged);
    }

    public void RegisterOnSliderValueChangedCallback(UnityAction<float> callback)
    {
        mListeners.Add(callback);
        
        if (mListeners.Count == 1)
        {
            SliderPanel.SetActive(true);
        }
    }

    public void UnregisterOnSliderValueChangedCallback(UnityAction<float> callback)
    {
        mListeners.Remove(callback);

        if (mListeners.Count == 0)
        {
            SliderPanel.SetActive(false);
        }
    }

    public float GetValue()
    {
        return Slider.value;
    }

    void OnValueChanged(float value)
    {
        foreach (var listener in mListeners)
        {
            listener.Invoke(value);
        }
    }
}