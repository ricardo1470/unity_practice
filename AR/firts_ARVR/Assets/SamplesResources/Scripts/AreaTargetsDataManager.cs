/*========================================================================
Copyright (c) 2020 PTC Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other
countries.
=========================================================================*/


using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Vuforia;

public class AreaTargetsDataManager : MonoBehaviour
{
    [Tooltip("The material to be used for rendering the loaded 3D models.")]
    public Material ModelMaterial;
    
    [Tooltip("The Slider used to control the size of the model's outline.")]
    public OutlineSlider OutlineSlider;

    Dictionary<string, GameObject> mAreaTargetAugmentations = new Dictionary<string, GameObject>();
    
    void Start()
    {
        mAreaTargetAugmentations.Clear();
    }
    
    public DataSet LoadAndActivateDatabase(string dataSetPath)
    {
        DataSet newDataSet = null;

        if (string.IsNullOrEmpty(dataSetPath))
        {
            Debug.LogError("Invalid dataset path!");
            return newDataSet;
        }

        if (dataSetPath.EndsWith("/"))
        {
            Debug.LogError("The path [" + dataSetPath + "] is a directory, not a dataset file!");
            return newDataSet;
        }

        if (!(dataSetPath.EndsWith(".xml") || dataSetPath.EndsWith(".XML")))
        {
            // Append Vuforia Database extension
            dataSetPath += ".xml";
        }

        if (!File.Exists(dataSetPath))
        {
            Debug.LogError("File not found: " + dataSetPath);
            return newDataSet;
        }

        var areaTracker = TrackerManager.Instance.GetTracker<AreaTracker>();

        // Create new dataset and load/activate it
        newDataSet = areaTracker.CreateDataSet();

        if (newDataSet != null 
            && newDataSet.Load(dataSetPath, VuforiaUnity.StorageType.STORAGE_ABSOLUTE))
        {
            Debug.Log("Successfully loaded dataset " + newDataSet.Path);

            if (areaTracker.ActivateDataSet(newDataSet))
            {
                Debug.Log("Successfully activated dataset " + newDataSet.Path);
            }
            else
            {
                Debug.LogError("Failed to activate dataset " + dataSetPath);
            }
        }
        else
        {
            Debug.LogError("Failed to load dataset " + dataSetPath);
            return null;
        }

        return newDataSet;
    }

    public bool LoadDatasetModel(IEnumerable<Trackable> trackableList, string pathToGlbFile)
    {
        if (trackableList == null)
        {
            Debug.LogError("Invalid dataset: it does not contain Trackable data.");
            return false;
        }
        
        if (string.IsNullOrEmpty(pathToGlbFile))
        {
            Debug.LogError("Invalid path: path to .glb path is empty.");
            return false;
        }
        
        foreach (var areaTarget in FindObjectsOfType<AreaTargetBehaviour>().Where((atb) => trackableList.Contains(atb.Trackable)))
        {
            var eventHandler = areaTarget.GetComponent<AreaTargetTrackableEventHandler>();
            if (!eventHandler)
            {
                eventHandler = areaTarget.gameObject.AddComponent<AreaTargetTrackableEventHandler>();
            }
                
            // Add outline slider object
            if (OutlineSlider != null)
            {
                eventHandler.Slider = OutlineSlider;
            }

            GameObject augmentation; 
            if (!mAreaTargetAugmentations.ContainsKey(areaTarget.TrackableName))
            {
                GLBHelper.ExtractGLBResources(pathToGlbFile, out var json, out var binaryFile);

                if (binaryFile == null)
                {
                    Debug.LogError("Error while extracting model from .glb file: binary file is null.");
                    return false;
                }
                
                var binCache = new Dictionary<string, byte[]>
                               {
                                   [pathToGlbFile] = binaryFile
                               };
                var model = GLTFModelCreator.ExtractModel(pathToGlbFile, json, binCache, new Material(ModelMaterial), false);

                if (model == null)
                {
                    Debug.LogError("Error while extracting model from .glb file: GLTF model is null.");
                    return false;
                }
                
                // Add augmentation
                augmentation = model;
                mAreaTargetAugmentations[areaTarget.TrackableName] = augmentation;
            }
            else
            {
                augmentation = mAreaTargetAugmentations[areaTarget.TrackableName];
            }
            
            augmentation.transform.parent = areaTarget.transform;
            augmentation.transform.localScale = Vector3.one;
            augmentation.transform.localPosition = Vector3.zero;
            augmentation.transform.localRotation = Quaternion.identity;
            augmentation.SetActive(true);
        }

        return true;
    }
}
