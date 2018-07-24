using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public class ZealotBuildWindow : EditorWindow
{
    [MenuItem("Build/Builder/Build Player", false, 10200)]
    public static void ShowBuildPlayerWindow()
    {
        var window = EditorWindow.GetWindowWithRect<ZealotBuildWindow>(new Rect(50, 50, 400, 350), true, "Build Player", true);
        window.ShowPopup();
    }

    bool startBuild = false;
    bool isBuilding = false;
    BuildTarget mBuildTarget;

    bool bDevelopmentBuild = true;
    string gameVersion;
    int androidVersionCode;
    string iOSBuildNumber;
    LoginServerIp loginServerIp;

    GUIStyle commonBtnStyle;
    BuildOptions buildOptions = BuildOptions.ShowBuiltPlayer;

    GUIStyle boldstyle;

    GameVersion gameVersionResource = null;

    void OnEnable()
    {
        gameVersion = PlayerSettings.bundleVersion;
        androidVersionCode = PlayerSettings.Android.bundleVersionCode;
        iOSBuildNumber = PlayerSettings.iOS.buildNumber;

        if (commonBtnStyle == null)
        {
            commonBtnStyle = new GUIStyle(EditorStyles.miniButton) { margin = new RectOffset(30, 30, 30, 2) };
            commonBtnStyle.fontSize = 12;
            commonBtnStyle.fixedHeight = 30;
        }

        if(boldstyle == null)
        {
            boldstyle = new GUIStyle(EditorStyles.textField) { fontStyle = FontStyle.Bold };
        }

        if (EditorPrefs.HasKey(LoginServerSelector.kEditorPrefLogin))
        {
            int login = EditorPrefs.GetInt(LoginServerSelector.kEditorPrefLogin);
            loginServerIp = (LoginServerIp)login;
        }
        else
        {
            loginServerIp = LoginServerIp.Private;
        }
        
    }

    void OnGUI()
    {
        var buildTarget = EditorUserBuildSettings.activeBuildTarget;

        EditorGUILayout.LabelField("Current Config", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");

#if MYCARD
         EditorGUILayout.LabelField("MYCARD");
#endif

#if ZEALOT_DEVELOPMENT
        EditorGUILayout.LabelField("ZEALOT_DEVELOPMENT");
#endif

//#if ZEALOT_CONSOLECOMMAND
//        EditorGUILayout.LabelField("ZEALOT_CONSOLECOMMAND");
//#endif

#if ZEALOT_SimplifiedChinese
        EditorGUILayout.LabelField("ZEALOT_SimplifiedChinese");
#endif

#if USE_ASSETBUNDLE
        EditorGUILayout.LabelField("USE_ASSETBUNDLE");
#endif

        EditorGUILayout.EndVertical();
        EditorGUILayout.Separator();

        //build options
        EditorGUILayout.LabelField("Build Options", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");
        bDevelopmentBuild = EditorGUILayout.Toggle("Development Build", bDevelopmentBuild);
        EditorGUILayout.EndVertical();

        EditorGUILayout.Separator();

        //server ip
        EditorGUILayout.LabelField("Server IP", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");
        loginServerIp = (LoginServerIp)EditorGUILayout.EnumPopup("Login Server", loginServerIp);
        EditorGUILayout.EndVertical();

        EditorGUILayout.Separator();

        //target platform
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Current Target Platform : ", GUILayout.Width(150));
        EditorGUILayout.LabelField(buildTarget.ToString(), EditorStyles.boldLabel);
        EditorGUILayout.EndHorizontal();

        //game version
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Game Version : ", GUILayout.Width(150));
        gameVersion = EditorGUILayout.TextField(gameVersion, boldstyle, GUILayout.Width(100));
        EditorGUILayout.EndHorizontal();

        //android Google version code
#if UNITY_ANDROID
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Android Version Code : ", GUILayout.Width(150));
        androidVersionCode = EditorGUILayout.IntField(androidVersionCode, boldstyle, GUILayout.Width(100));
        EditorGUILayout.EndHorizontal();

#elif UNITY_IOS //iOS build number
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("IOS Build Number : ", GUILayout.Width(150));
        iOSBuildNumber = EditorGUILayout.TextField(iOSBuildNumber, boldstyle, GUILayout.Width(100));
        EditorGUILayout.EndHorizontal();
#endif

        EditorGUILayout.Separator();

        GUI.enabled = !isBuilding && !EditorApplication.isCompiling;

        if (GUI.changed)
        {
            if (gameVersion != PlayerSettings.bundleVersion)
                PlayerSettings.bundleVersion = gameVersion;

            if (androidVersionCode != PlayerSettings.Android.bundleVersionCode)
                PlayerSettings.Android.bundleVersionCode = androidVersionCode;

            if (iOSBuildNumber != PlayerSettings.iOS.buildNumber)
                PlayerSettings.iOS.buildNumber = iOSBuildNumber;
            
            PhotonNetwork.PhotonServerSettings.SetLoginServerTypeForBuild(loginServerIp);
            
        }

        if (buildTarget == BuildTarget.Android)
        {
#if MYCARD
            if (GUILayout.Button("Build Android MyCard Player (.apk)", commonBtnStyle))
#else
            if (GUILayout.Button("Build Android Player (.apk)", commonBtnStyle))
#endif
                StartBuild(BuildTarget.Android);
        }
        else if(buildTarget == BuildTarget.iOS)
        {
            if (GUILayout.Button("Build iOS Player (.app)", commonBtnStyle))
                StartBuild(BuildTarget.iOS);
        }
        else if(buildTarget == BuildTarget.StandaloneWindows64)
        {
            if (GUILayout.Button("Build Windows Player (.exe)", commonBtnStyle))
                StartBuild(BuildTarget.StandaloneWindows64);
        }

        GUI.enabled = true;
    }

    void StartBuild(BuildTarget target)
    {
        isBuilding = true;
        mBuildTarget = target;
        startBuild = true;
    }

    void OnInspectorUpdate()
    {
        if (startBuild)
        {
            if (loginServerIp == LoginServerIp.Live)
            {
                string warning = "";
//#if ZEALOT_CONSOLECOMMAND
//                warning += "[console command]" + Environment.NewLine;
//#endif

#if !USE_ASSETBUNDLE
                warning += "[not using asset bundle]" + Environment.NewLine;
#endif
                if (bDevelopmentBuild)
                    warning += "[development build checked]" + Environment.NewLine;

                if (!string.IsNullOrEmpty(warning))
                {
                    string msg = string.Format("This LIVE build has the following{1}{0}{1}Do you still want to continue?", warning, Environment.NewLine);
                    if (!EditorUtility.DisplayDialog("Live Build Warning", msg, "Ok", "Cancel"))
                    {
                        startBuild = false;
                        isBuilding = false;
                        return;
                    }
                }
            }

            startBuild = false;
            
            switch(mBuildTarget)
            {
                case BuildTarget.Android:
                    ZealotBuild.BuildAndroidPlayer(GetBuildOptions(), loginServerIp);
                    break;

                case BuildTarget.iOS:
                    ZealotBuild.BuildiOSPlayer(GetBuildOptions(), loginServerIp);
                    break;

                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    ZealotBuild.BuildWindowsPlayer(GetBuildOptions(), loginServerIp);
                    break;
            }

            isBuilding = false;
        }
    }

    BuildOptions GetBuildOptions()
    {
        BuildOptions options = BuildOptions.ShowBuiltPlayer;

        if (bDevelopmentBuild)
            options |= BuildOptions.Development;

        return options;
    }
}