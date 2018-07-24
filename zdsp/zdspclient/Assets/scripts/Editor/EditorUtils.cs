using Kopio.JsonContracts;
using Newtonsoft.Json;
using System.IO;
using UnityEditor;
using UnityEngine;

public class EditorUtils
{
    public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
    {
        DirectoryInfo dir = new DirectoryInfo(sourceDirName);
        DirectoryInfo[] dirs = dir.GetDirectories();

        // If the source directory does not exist, throw an exception.
        if (!dir.Exists)
        {
            throw new DirectoryNotFoundException(
                "Source directory does not exist or could not be found: "
                + sourceDirName);
        }

        // If the destination directory does not exist, create it.
        if (!Directory.Exists(destDirName))
        {
            Directory.CreateDirectory(destDirName);
        }


        // Get the file contents of the directory to copy.
        FileInfo[] files = dir.GetFiles();

        foreach (FileInfo file in files)
        {
            // Create the path to the new copy of the file.
            string temppath = Path.Combine(destDirName, file.Name);

            // Copy the file.
            file.CopyTo(temppath, true);
        }

        // If copySubDirs is true, copy the subdirectories.
        if (copySubDirs)
        {

            foreach (DirectoryInfo subdir in dirs)
            {
                // Create the subdirectory.
                string temppath = Path.Combine(destDirName, subdir.Name);

                // Copy the subdirectories.
                DirectoryCopy(subdir.FullName, temppath, copySubDirs);
            }
        }
    }

    public static void DeleteDirectory(string path)
    {
        FileUtil.DeleteFileOrDirectory(path);
    }

    public static GameDBRepo GetGameDB()
    {
        TextAsset gamedb = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/GameData/GameRepo/gamedata.json");
        if (gamedb != null)
        {
            return JsonConvert.DeserializeObject<GameDBRepo>(gamedb.text);
        }
        return null;
    }

    public static bool ParseAssetPath(string path, ref string containerName, ref string assetPath)
    {
        string[] assetNames = path.Split('/');
        int seperator = path.IndexOf('/');
        if (seperator > -1)
        {
            containerName = path.Substring(0, seperator);
            assetPath = path.Substring(seperator + 1);
            return true;
        }
        return false;
    }

    public static IAssetContainer LoadAssetContainer(string containerName)
    {
        string containerPath = Path.Combine("Assets/AssetContainers", containerName + ".asset");
        return AssetDatabase.LoadAssetAtPath<BaseAssetContainer>(containerPath);
    }
}
