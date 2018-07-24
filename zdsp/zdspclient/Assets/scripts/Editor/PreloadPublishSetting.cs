using UnityEngine;
using UnityEditor;
using System.IO;

[InitializeOnLoad]
public class PreloadPublishSetting
{
    static PreloadPublishSetting()
    {
        PlayerSettings.Android.keystoreName = Application.dataPath + "/zdsp.keystore";
        PlayerSettings.Android.keystorePass = "password$1";
        PlayerSettings.Android.keyaliasName = "zdsp";
        PlayerSettings.Android.keyaliasPass = "password$1";
    }
}
