using CinemaDirector;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

class ZealotCutsceneWindow : EditorWindow
{
    [MenuItem("Window/Cinema Suite/Zealot Cutscene Window", false, 10000)]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(ZealotCutsceneWindow));
    }

    AudioListener[] audioListeners;
    Cutscene[] cutsceneList;
    Cutscene currentPlaying;
    AudioListener audioListener;
    bool isPlaying;

    Dictionary<Cutscene, bool> cutsceneCache;
    string statusMessage = "";

    public void Awake()
    {
        base.titleContent = new GUIContent("Cinema Player");

        this.minSize = new Vector2(400f, 300f);
        isPlaying = false;
    }

    void OnEnable()
    {
        EditorApplication.playmodeStateChanged = (EditorApplication.CallbackFunction)System.Delegate.Combine(EditorApplication.playmodeStateChanged, new EditorApplication.CallbackFunction(this.PlaymodeStateChanged));
        RefreshScene();
    }

    public void OnGUI()
    {
        EditorGUILayout.Separator();

        if (GUILayout.Button(new GUIContent("Refresh Scene"), GUILayout.Width(120)))
        {
            RefreshScene();
        }

        EditorGUILayout.Separator();

        if (cutsceneList.Length > 0)
        {
            foreach (var cutscene in cutsceneList)
            {
                if (cutscene != null)
                {
                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.SelectableLabel(cutscene.gameObject.name);

                    if (GUILayout.Button("Select", GUILayout.Width(80)))
                    {
                        Selection.activeObject = cutscene.gameObject;

                        DirectorWindow window = EditorWindow.GetWindow(typeof(DirectorWindow)) as DirectorWindow;
                        window.FocusCutscene(cutscene);
                    }

                    if (isPlaying || !Application.isPlaying)
                    {
                        GUI.enabled = false;
                    }
                    if (GUILayout.Button("Play", GUILayout.Width(60)))
                    {
                        PlayCutscene(cutscene);
                    }
                    GUI.enabled = true;

                    if (!isPlaying || cutscene != currentPlaying)
                    {
                        GUI.enabled = false;
                    }
                    if (GUILayout.Button("Stop", GUILayout.Width(60)))
                    {
                        if (cutscene == currentPlaying)
                        {
                            StopCutscene(currentPlaying);
                        }
                    }
                    GUI.enabled = true;

                    EditorGUILayout.EndHorizontal();
                }
            }
        }
        else
            GUILayout.Label("No cut scene found");

        Rect rect = new Rect(2, position.size.y - 30, position.size.x - 5, 30);
        GUILayout.BeginArea(rect);
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label(statusMessage);
        EditorGUILayout.EndVertical();
        GUILayout.EndArea();
    }

    //finds all Cutscene in scene
    void RefreshScene()
    {
        cutsceneList = GameObject.FindObjectsOfType<Cutscene>();

        audioListeners = GameObject.FindObjectsOfType<AudioListener>();
        if(audioListeners.Length > 0)
        {
            statusMessage = string.Format("Warning: more than 1 ({0}) audiolisteners found", audioListeners.Length);
        }
    }

    void PlayCutscene(Cutscene cutscene)
    {
        if (currentPlaying != null)
            StopCutscene(currentPlaying);

        //cutscene.Optimize();
        cutscene.CutsceneFinished += OnCutsceneFinished;

        currentPlaying = cutscene;
        isPlaying = true;
        CacheCutscenesActive(currentPlaying);
        cutscene.Play();
    }


    void StopCutscene(Cutscene cutscene)
    {        
        cutscene.Stop();
        currentPlaying = null;
        isPlaying = false;
        RestoreCutscenesActive();
    }

    void OnCutsceneFinished(object sender, CutsceneEventArgs e)
    {
        currentPlaying = null;
        isPlaying = false;
        RestoreCutscenesActive();
        Repaint();
    }

    public void PlaymodeStateChanged()
    {
        if(isPlaying)
        {
            StopCutscene(currentPlaying);
        }
        RefreshScene();
    }

    void CacheCutscenesActive(Cutscene playingCutscene)
    {
        cutsceneCache = new Dictionary<Cutscene, bool>();
        foreach (var cutscene in cutsceneList)
        {
            if (cutscene != playingCutscene)
            {
                cutsceneCache.Add(cutscene, cutscene.gameObject.activeSelf);
                if (cutscene.gameObject.activeSelf)
                    cutscene.gameObject.SetActive(false);
            }
        }
    }

    void RestoreCutscenesActive()
    {
        if(audioListener != null)
        {
            Destroy(audioListener.gameObject);
            audioListener = null;
        }

        foreach(var kvp in cutsceneCache)
        {
            if (kvp.Value)
                kvp.Key.gameObject.SetActive(true);
        }
    }

    void CreateAudioListener()
    {
        if(audioListeners.Length == 0)
        {
            var go = new GameObject();
            audioListener = go.AddComponent<AudioListener>();
        }
    }
}
