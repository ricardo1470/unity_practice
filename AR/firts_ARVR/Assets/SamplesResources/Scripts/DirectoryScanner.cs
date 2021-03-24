/*========================================================================
Copyright (c) 2020 PTC Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other
countries.
=========================================================================*/


using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Vuforia;

public class DirectoryScanner
{
    public static bool GetDataSetsInFolder(string folderName, out Dictionary<string, string> directories)
    {
        // Clear previous file list
        directories = new Dictionary<string, string>();

        // Get the base dir (SD card on Android, or App /Documents folder on iOS)
        var storageRoot = GetStorageRoot(folderName);
        var scanDir = new DirectoryInfo(storageRoot);

        if (!scanDir.Exists)
        {
            Debug.LogError("Cannot scan non-existing directory: " + storageRoot);
            return false;
        }
        
        var dirs = scanDir.GetDirectories();

        foreach (var directoryInfo in dirs)
        {
            var datasetFileXML = FindFileWithExtension(directoryInfo.FullName, "*.xml");
            var datasetFileDat = FindFileWithExtension(directoryInfo.FullName, "*.dat");
            var datasetFileGLTF = FindFileWithExtension(directoryInfo.FullName, "*.gltf");
            var datasetFileGLB = FindFileWithExtension(directoryInfo.FullName, "*.glb");


            // Only display datasets that have both an .xml and .dat file
            if (datasetFileDat != null && datasetFileXML != null 
                                       && (datasetFileGLTF != null || datasetFileGLB != null))
            {
                // Keep track of full path by directory name
                directories[directoryInfo.Name] = directoryInfo.FullName;
            }
            else
            {
                Debug.LogError("Could not find dataset files in directory " + directoryInfo.Name);
            }
        }

        return true;
    }
    
    public static string GetStorageRoot(string folderName)
    {
        if (VuforiaRuntimeUtilities.IsPlayMode())
            return Path.Combine(Application.streamingAssetsPath, folderName);

        // On Android, Application.persistentDataPath should look like:
        // '/sdcard/Android/data/com.myorg.myapp/files'
        // 
        // On iOS, Application.persistentDataPath points to the 
        // 'Documents' folder of the App
        return Path.Combine(Application.persistentDataPath , folderName);
    }

    public static string FindFileWithExtension(string dirPath, string extension)
    {
        if (!Directory.Exists(dirPath))
        {
            return null;
        }

        var files = new DirectoryInfo(dirPath).GetFiles(extension, SearchOption.AllDirectories);
        if (files.Length == 0)
        {
            return null;
        }

        return files[0].Name; 
    }
}
