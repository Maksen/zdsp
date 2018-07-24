using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class LoginServerSelector
{
    const string kPrivateLogin = "Server/Login Server/ServerPrivate";
    const string kStagingLogin = "Server/Login Server/Staging Server";

    public const string kEditorPrefLogin = "LoginServerIp";
    static LoginServerSelector()
    {
        if (!EditorPrefs.HasKey(kEditorPrefLogin))
        {
            Debug.Log("Init default Login Server IP in editor preferences");
            int login = (int)LoginServerIp.Private;
            EditorPrefs.SetInt(kEditorPrefLogin, login);
        }
    }

    //SERVER PRIVATE
    [MenuItem(kPrivateLogin, false, 10000)]
    public static void SetServerPrivate()
    {
        EditorPrefs.SetInt(kEditorPrefLogin, (int)LoginServerIp.Private);
    }

    [MenuItem(kPrivateLogin, true)]
    public static bool ToggleSetServerPrivate()
    {
        if (EditorPrefs.HasKey(kEditorPrefLogin))
        {
            int login = EditorPrefs.GetInt(kEditorPrefLogin);
            Menu.SetChecked(kPrivateLogin, login == (int)LoginServerIp.Private);
            return true;
        }
        return false;
    }


    //STAGING
    [MenuItem(kStagingLogin, false, 10000)]
    public static void SetStagingServer()
    {
        EditorPrefs.SetInt(kEditorPrefLogin, (int)LoginServerIp.Staging);
    }

    [MenuItem(kStagingLogin, true)]
    public static bool ToggleSetStagingServer()
    {
        if (EditorPrefs.HasKey(kEditorPrefLogin))
        {
            int login = EditorPrefs.GetInt(kEditorPrefLogin);
            Menu.SetChecked(kStagingLogin, login == (int)LoginServerIp.Staging);
            return true;
        }
        return false;
    }
}