using System;
using System.IO;
using UnityEditor;

public class BuildAssetBundles : Editor {
    [MenuItem("Assets/Build AssetBundles")]
    private static void BuildAllAssetBundles() {
        string assetBundlesDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "AssetBundles");

        if (!Directory.Exists(assetBundlesDirectory))
            Directory.CreateDirectory(assetBundlesDirectory);
        
        foreach (string name in AssetDatabase.GetAllAssetBundleNames()) {
            string directory = Path.Combine(assetBundlesDirectory, $"{name}_bundle");

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            
            BuildPipeline.BuildAssetBundles(directory, new [] { new AssetBundleBuild {
                assetBundleName = name,
                assetNames = AssetDatabase.GetAssetPathsFromAssetBundle(name)
            } }, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneWindows);
        }
    }
}
