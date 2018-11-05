using UnityEngine;
using UnityEditor;

class AssetContainerCreator
{
    [MenuItem("Assets/Create/Asset Container/Prefab Container")]
    public static void CreatePrefabContainer()
    {
        CreateAssetContainer<PrefabContainer>("Prefab");
    }

    [MenuItem("Assets/Create/Asset Container/AvatarPart Container")]
    public static void CreateAvatarPartContainer()
    {
        CreateAssetContainer<AvatarPartContainer>("AvatarPart");
    }

    [MenuItem("Assets/Create/Asset Container/Sprite Container")]
    public static void CreateSpriteContainer()
    {
        CreateAssetContainer<SpriteContainer>("Sprite");
    }

    [MenuItem("Assets/Create/Asset Container/Material Container")]
    public static void CreateMaterialContainer()
    {
        CreateAssetContainer<MaterialContainer>("Texture");
    }

    [MenuItem("Assets/Create/Asset Container/Texture Container")]
    public static void CreateTextureContainer()
    {
        CreateAssetContainer<TextureContainer>("Texture");
    }

    [MenuItem("Assets/Create/Asset Container/AudioClip Container")]
    public static void CreateAudioClipContainer()
    {
        CreateAssetContainer<AudioClipContainer>("AudioClip");
    }
    [MenuItem("Assets/Create/Asset Container/TextAsset Container")]
    public static void CreateTextAssetContainer()
    {
        CreateAssetContainer<TextAssetContainer>("TextAsset");
    }
    [MenuItem("Assets/Create/Asset Container/VideoClip Container")]
    public static void CreateVideoClipContainer()
    {
        CreateAssetContainer<VideoClipContainer>("VideoClip");
    }

    public static void CreateAssetContainer<T>(string type) where T : ScriptableObject
    {
        var selectedObjects = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets);
        if (selectedObjects.Length == 0)
        {
            Debug.LogFormat("Create {0} Container Failed: No folder selected", type);
        }
        else if (selectedObjects.Length > 1)
        {
            Debug.LogFormat("Create {0} Container Failed: Please select only 1 folder", type);
            return;
        }

        string folderPath = AssetDatabase.GetAssetPath(selectedObjects[0]);
        if (AssetDatabase.IsValidFolder(folderPath))
        {
            //remove "Assets/" from path
            folderPath = folderPath.Substring(7);

            string containerName = folderPath.Replace('/', '_');
            string targetPath = "Assets/AssetContainers/" + containerName + ".asset";

            if (AssetDatabase.LoadAssetAtPath<Object>(targetPath) == null)
            {
                T asset = ScriptableObject.CreateInstance<T>();

                var newContainer = asset as BaseAssetContainer;
                newContainer.containerAssetsPath = folderPath;
                newContainer.Preload = true;
                newContainer.Build = true;
                newContainer.IndividualAssetBundle = false;
                newContainer.AddSubFolder = true;

                AssetDatabase.CreateAsset(asset, targetPath);

                ProjectWindowUtil.ShowCreatedAsset(asset);

                Debug.LogFormat("Created {1} Container [{0}]", targetPath, type);
            }
            else
            {
                EditorUtility.DisplayDialog("Error Creating Container", targetPath + " already exists", "OK");
            }
        }
        else
        {
            Debug.LogFormat("Create {0} Container Failed: Invalid folder path", type);
        }
    }
}
