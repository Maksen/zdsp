using UnityEngine;
using UnityEditor;
using System.IO;

class AssetContainerPreProcesser : UnityEditor.AssetModificationProcessor
{
    static void OnWillSaveAssets(string[] assetPaths)
    {
        foreach(string path in assetPaths)
        {
            var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            if(asset != null && asset is IAssetContainer)
            {
                IAssetContainer container = asset as IAssetContainer;
                container.OnWillSaveAssets();

                Debug.LogFormat("AssetContainerPreProcesser: Compiling assets in [{0}]", asset.name);
            }
        }
    }

    //Cannot receive event, switch to using post processor
    //public static AssetDeleteResult OnWillDeleteAsset(string assetPath, RemoveAssetOptions option)
    //{
    //    return AssetDeleteResult.DidDelete;
    //}
}

class AssetContainerPostProcesser : AssetPostprocessor
{
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach (var str in deletedAssets)
        {
            //assumes these are asset containers            
            if (Path.GetDirectoryName(str) == "Assets/AssetContainers" && str.EndsWith(".asset"))
            {
                string containerName = Path.GetFileNameWithoutExtension(str);
                Debug.LogFormat("Deleting Asset Container [{0}] from Database", containerName);
                //KopioAssetManager.DeleteAssetContainer(containerName);
            }
        }
    }
}