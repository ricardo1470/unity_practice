/*==============================================================================
Copyright (c) 2020 PTC Inc. All Rights Reserved.

Confidential and Proprietary - Protected under copyright and other laws.
==============================================================================*/


using UnityEngine;
using Vuforia;

/// <summary>
/// A custom handler that implements the DefaultTrackableEventHandler class.
/// </summary>
public class AreaTargetTrackableEventHandler : DefaultTrackableEventHandler
{
    [HideInInspector]
    public OutlineSlider Slider;

    MeshRenderer[] mRenderers;
    
    readonly Color LIMITED_COLOR = new Color(1,0.6f,0, 1);
    readonly string OUTLINE_COLOR_PROPERTY = "_SilhouetteColor";
    readonly string OUTLINE_SIZE_PROPERTY = "_SilhouetteSize";
    
    void Awake()
    {
        if (Slider == null)
        {
            Slider = Resources.FindObjectsOfTypeAll<OutlineSlider>()[0];
            if (!Slider)
            {
                Debug.LogError("Outline Slider not found in the scene.");
            }
        }
    }

    protected override void Start()
    {
        base.Start();
        
        // Inherited from DefaultTrackableEventHandler
        mTrackableBehaviour.RegisterOnTrackableStatusChanged(OnTrackableStatusChanged);
        
        mRenderers = GetComponentsInChildren<MeshRenderer>(true);
        
        AdjustOutlineSize(Slider.GetValue());
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        mTrackableBehaviour.UnregisterOnTrackableStatusChanged(OnTrackableStatusChanged);
    }

    void OnTrackableStatusChanged(TrackableBehaviour.StatusChangeResult statusChangeResult)
    {
        if (statusChangeResult.NewStatus == TrackableBehaviour.Status.LIMITED)
        {
            SetOutlineColor(LIMITED_COLOR);
        }
        else if (statusChangeResult.PreviousStatus == TrackableBehaviour.Status.LIMITED)
        {
            SetOutlineColor(Color.white);
        }
    }
    
    protected override void OnTrackingFound()
    {
        base.OnTrackingFound();
        Slider.RegisterOnSliderValueChangedCallback(AdjustOutlineSize);
    }

    protected override void OnTrackingLost()
    {
        base.OnTrackingLost();
        Slider.UnregisterOnSliderValueChangedCallback(AdjustOutlineSize);
    }

    void SetOutlineColor(Color newColor)
    {
        foreach (var mr in mRenderers)
        {
            if (mr.material &&
                mr.material.HasProperty(OUTLINE_COLOR_PROPERTY))
            {
                mr.material.SetColor(OUTLINE_COLOR_PROPERTY, newColor);
            }
        }
    }

    void AdjustOutlineSize(float refSize)
    {
        foreach (var mr in mRenderers)
        {
            if (mr.material &&
                mr.material.HasProperty(OUTLINE_SIZE_PROPERTY))
            {
                var meshObject = mr.gameObject;
                var meshLocalScale = meshObject.transform.localScale;
                var meshWorldScale = meshObject.transform.TransformVector(meshLocalScale);
                mr.material.SetFloat(OUTLINE_SIZE_PROPERTY, refSize * meshWorldScale.magnitude);
            }
        }
    }
}
