using UnityEngine;
using UnityEditor;
using System.Collections;

public class PlayerPrefsWindow : EditorWindow
{

    string prefsKey;

    [MenuItem("Window/PlayerPrefs Window")]
    static void Init()
    {
        PlayerPrefsWindow window = (PlayerPrefsWindow)EditorWindow.GetWindow(typeof(PlayerPrefsWindow));
        window.Show();
    }
    void OnGUI()
    {
        prefsKey = EditorGUILayout.TextField("playerprefs key", prefsKey);

        if (GUILayout.Button("delete playerprefs"))
        {
            PlayerPrefs.DeleteKey(prefsKey);
            Debug.Log("deleted " + prefsKey + " key");
        }
    }
}
