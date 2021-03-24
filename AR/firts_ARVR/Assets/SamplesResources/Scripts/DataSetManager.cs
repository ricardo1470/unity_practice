/*========================================================================
Copyright (c) 2020 PTC Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other
countries.
=========================================================================*/

using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Vuforia;

public class DataSetManager : MonoBehaviour
{
    public AreaTargetsDataManager DataMgr;

    [Tooltip("The Directory where the Area Target DataSets are stored.")]
    public string AreaTargetsDataDir = "AreaTargetData";

    [Tooltip("The CanvasGroup containing the DataSet menu.")]
    public CanvasGroup MenuCanvasGroup;

    [Tooltip("The Button prefab used in the scroll view.")]
    public Transform DatasetBtn;

    [Tooltip("The UI Image for the file loading progress.")]
    public UnityEngine.UI.Image LoadingProgress;

    public Transform ButtonContainer;

    [SerializeField] 
    [Tooltip("The UI for the file loading progress.")]
    Canvas LoadingProgressCanvas = null;

    NotificationUI mNotificationUI;
    float mTimeSinceLastScan;
    Dictionary<string, bool> mActiveDataSets = new Dictionary<string, bool>();
    Dictionary<string, string> mDataSets = new Dictionary<string, string>();
    
    const int DATASET_SCAN_TIME_SPAN = 10;
    const string DEFAULT_DATASET_NAME = "Default Area Target";
    const string ERROR_DATASET_NOT_VALID = "Invalid dataset. Please use a valid Area Target Dataset.";
    const string ERROR_DATASET_LOADING_FAILED = "Dataset loading failed. Please check if you are using a valid Area Target Dataset!";
    const string ERROR_AREA_TRACKER_NOT_INITIALIZED = "Area Tracker is not initialized.\nPlease consult the User Guide.";

    void Awake()
    {
        VuforiaARController.Instance.RegisterVuforiaInitializedCallback(OnVuforiaInitialized);

        mNotificationUI = FindObjectOfType<NotificationUI>();
    }
    
    void Update()
    {
        if (MenuCanvasGroup && MenuCanvasGroup.alpha > 0)
        {
            mTimeSinceLastScan += Time.deltaTime;
            if (mTimeSinceLastScan > DATASET_SCAN_TIME_SPAN)
            {
                mTimeSinceLastScan = 0;
                UpdateDatasetList();
            }
        }
    }

    void OnDestroy()
    {
        VuforiaARController.Instance.UnregisterVuforiaInitializedCallback(OnVuforiaInitialized);
    }

    void OnVuforiaInitialized()
    {
        //If no AreaTarget is in the scene, the AreaTracker will not be automatically started. Start it here for this application.
        if (TrackerManager.Instance.GetTracker<AreaTracker>() == null)
            TrackerManager.Instance.InitTracker<AreaTracker>();
    }
    
    public void ShowDataSetsMenu(bool active)
    {
        if (MenuCanvasGroup)
        {
            MenuCanvasGroup.alpha = active ? 1 : 0;

            UpdateDatasetList();
        }
    }

    public void OnFileSelected(bool selected)
    {
        if (!DataMgr)
        {
            Debug.LogError("Area Target Data Manager not set!");
            return;
        }

        var selectedButton = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;

        // Ignore the dataset list button
        if (!selectedButton || selectedButton.name.Contains("DataSetButton"))
        {
            return;
        }
        
        var selectedDataSet = selectedButton.GetComponentInChildren<Text>(true)?.text;
        Debug.Log(selectedDataSet + " selected: " + selected);

        if (!string.IsNullOrEmpty(selectedDataSet) && selectedDataSet != DEFAULT_DATASET_NAME)
        {
            if (mActiveDataSets[selectedDataSet] && !selected)
            {
                // Deactivate DataSet
                DeactivateDataset(GetDataSetXmlFullPath(selectedDataSet));
                DisableAreaTarget(selectedDataSet);
                mActiveDataSets[selectedDataSet] = false;
            }
            else if (!mActiveDataSets[selectedDataSet] && selected)
            {
                // Activate DataSet
                StartCoroutine(LoadAreaTargetDataAsync(selectedDataSet, () => DataSetLoadFailed(selectedDataSet, selectedButton)));
            }
        }
    }

    IEnumerator LoadAreaTargetDataAsync(string datasetName, Action callback)
    {
        yield return new WaitForEndOfFrame();

        if (!string.IsNullOrEmpty(datasetName) || mDataSets.ContainsKey(datasetName))
        {
            var datasetXmlFullPath = GetDataSetXmlFullPath(datasetName);

            SetLoadingProgress(0.25f);

            yield return new WaitForSeconds(0.1f);

            if (!IsAreaTargetDataSet(datasetXmlFullPath))
            {
                callback.Invoke();
                yield break;
            }

            SetLoadingProgress(0.5f);

            yield return new WaitForSeconds(0.1f);

            SetLoadingProgress(0.75f);

            if (!LoadAreaTarget(mDataSets[datasetName], datasetXmlFullPath))
            {
                callback.Invoke();
                yield break;
            }
            
            SetLoadingProgress(0.9f);
        }
        else
        {
            callback.Invoke();
            Debug.LogError("Invalid dataset name");
            yield return new WaitForSeconds(0.25f);
            yield break;
        }
        
        SetLoadingProgress(1);
        mActiveDataSets[datasetName] = true;
        yield return new WaitForSeconds(0.25f);
        SetLoadingProgress(0, false);
    }

    void DataSetLoadFailed(string datasetName, GameObject uiButton)
    {
        mActiveDataSets[datasetName] = false;
        if (mNotificationUI)
            mNotificationUI.ShowNotification(ERROR_DATASET_LOADING_FAILED);
        uiButton.GetComponent<Toggle>().isOn = false;
        SetLoadingProgress(0, false);
    }

    string GetDataSetXmlFullPath(string datasetName)
    {
        var datasetFileXML = DirectoryScanner.FindFileWithExtension(mDataSets[datasetName], "*.xml");
        return Path.Combine(mDataSets[datasetName], datasetFileXML);
    }

    bool IsAreaTargetDataSet(string datasetXmlFullPath)
    {
        var xmlDoc = new XmlDocument();
        var dataSetXmlContent = File.ReadAllText(datasetXmlFullPath);

        if (!string.IsNullOrEmpty(dataSetXmlContent))
        {
            xmlDoc.LoadXml(dataSetXmlContent);

            // A valid AreaTarget Database XML must contain the AreaTarget node:
            var areaNode = FindXmlNodeByName(xmlDoc, "AreaTarget");

            if (areaNode == null)
            {
                Debug.LogError("Invalid AreaTarget Database XML: does not contain the AreaTarget node.");
                if (mNotificationUI)
                    mNotificationUI.ShowNotification(ERROR_DATASET_NOT_VALID);
                return false;
            }

            return true;
        }

        Debug.LogError("Invalid AreaTarget Database XML: file is empty.");
        return false;
    }

    bool LoadAreaTarget(string dirFullPath, string datasetXmlFullPath)
    {
        bool loadSuccessful = false;

        var areaTracker = TrackerManager.Instance.GetTracker<AreaTracker>();
        areaTracker.Stop();

        var newDataset = DataMgr.LoadAndActivateDatabase(datasetXmlFullPath);

        if (newDataset != null)
        {
            var datasetFileGLB = DirectoryScanner.FindFileWithExtension(dirFullPath, "*.glb");
            var datasetGLBFullPath = Path.Combine(dirFullPath, datasetFileGLB);
            datasetGLBFullPath = datasetGLBFullPath.Replace("\\", "/");

            loadSuccessful = DataMgr.LoadDatasetModel(newDataset.GetTrackables(), datasetGLBFullPath);
        }

        // Restart Tracker
        if (!areaTracker.Start() && !VuforiaRuntimeUtilities.IsSimulatorMode())
        {
            if (mNotificationUI)
                mNotificationUI.ShowNotification(ERROR_AREA_TRACKER_NOT_INITIALIZED, true);
            return false;
        }

        return loadSuccessful;
    }

    void UpdateDatasetList()
    {
        if (!DirectoryScanner.GetDataSetsInFolder(AreaTargetsDataDir, out mDataSets))
        {
            return;
        }

        foreach (var dataSet in mDataSets.Keys)
        {
            if (mActiveDataSets.Count == 0 || !mActiveDataSets.ContainsKey(dataSet))
            {
                // Set text in UI label
                var newBtn = Instantiate(DatasetBtn, ButtonContainer, false);

                newBtn.localScale = new Vector3(1, 1, 1);
                newBtn.gameObject.SetActive(true);
                newBtn.GetComponentInChildren<Text>(true).text = dataSet;
                newBtn.GetComponentInChildren<Toggle>().isOn = false;

                mActiveDataSets.Add(dataSet, false);
            }
        }
    }

    void DisableAreaTarget(string dataset)
    {
        var areaTargets = FindObjectsOfType<AreaTargetBehaviour>();

        foreach (var areaTarget in areaTargets)
        {
            if (areaTarget.TrackableName == dataset)
            {
                areaTarget.enabled = false;
                areaTarget.gameObject.SetActive(false);
            }
        }
    }
    
    void DeactivateDataset(string dataSetFullPath)
    {
        var areaTracker = TrackerManager.Instance.GetTracker<AreaTracker>();

        // Deactivate selected dataset
        foreach (var ds in areaTracker.GetActiveDataSets().ToList())
        {
            if (ds.Path == dataSetFullPath)
            {
                Debug.Log("Deactivating dataset: " + ds.Path);
                areaTracker.DeactivateDataSet(ds);
            }
        }
    }

    /// <summary>
    /// Search recursively for an XML node with the given name.
    /// </summary>
    XmlNode FindXmlNodeByName(XmlNode rootNode, string nodeName)
    {
        if (rootNode.Name == nodeName)
        {
            return rootNode;
        }

        foreach (var childNode in rootNode.ChildNodes)
        {
            var matchingNode = FindXmlNodeByName(childNode as XmlNode, nodeName);
            if (matchingNode != null)
            {
                return matchingNode;
            }
        }

        // no match found
        return null;
    }

    void SetLoadingProgress(float progress, bool showLoadingProgress = true)
    {
        if (LoadingProgressCanvas)
        {
            LoadingProgressCanvas.enabled = showLoadingProgress;

            if (LoadingProgress)
            {
                LoadingProgress.fillAmount = progress;
            }
        }
    }
}