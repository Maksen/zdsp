using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EffectRef))]
[CanEditMultipleObjects]
public class Setting : Editor
{
    SerializedProperty UseRefCamera;
    SerializedProperty SpawnAvatar;
    SerializedProperty Refmodel;
    SerializedProperty RefAnimation;
    SerializedProperty Offset;
    SerializedProperty Rotation;
    SerializedProperty Scale;
    SerializedProperty Attached;
    SerializedProperty WeaponTrailStartDelay;
    SerializedProperty WeaponTrailLifeTime;
    SerializedProperty AudioClip;

    string[] animationoptions = new string[0];
    string animationName = null;
    int selectedanimation;
    Texture2D Icon;

    public void OnEnable()
    {
        Icon = EditorGUIUtility.Load("Icon/parent.png") as Texture2D;

        UseRefCamera = serializedObject.FindProperty("UseRefCamera");
        SpawnAvatar = serializedObject.FindProperty("SpawnAvatar");
        Refmodel = serializedObject.FindProperty("Refmodel");
        RefAnimation = serializedObject.FindProperty("RefAnimation");
        Offset = serializedObject.FindProperty("Offset");
        Rotation = serializedObject.FindProperty("Rotation");
        Scale = serializedObject.FindProperty("Scale");
        Attached = serializedObject.FindProperty("Attached");
        WeaponTrailStartDelay = serializedObject.FindProperty("WeaponTrailStartDelay");
        WeaponTrailLifeTime = serializedObject.FindProperty("WeaponTrailLifeTime");
        AudioClip = serializedObject.FindProperty("AudioClip");
        animationName = ((EffectRef)target).GetRefanimationName();
        OnUpdateDropDownMenu();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        UseRefCamera.boolValue = EditorGUILayout.Toggle("Use Game Camera", UseRefCamera.boolValue);
        SpawnAvatar.boolValue = EditorGUILayout.Toggle("Spawn Ref Model", SpawnAvatar.boolValue);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(Refmodel, GUILayout.MinWidth(50f));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        int animindex = EditorGUILayout.Popup("Refanimation", selectedanimation, animationoptions);
        if (animindex != selectedanimation)
        {
            selectedanimation = animindex;
            OnRefanimationSelected();
        }
        if (GUILayout.Button("Refresh", GUILayout.MaxWidth(80f)))
        {
            OnUpdateAnimationDropDownMenu();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (!Attached.boolValue)
        {
            if (GUILayout.Button("Attach", GUILayout.MaxWidth(80f)))
            {
                OnAttachEffect();
            }
        }
        else
        {
            if (GUILayout.Button("Detach", GUILayout.MaxWidth(80f)))
            {
                OnDetachEffect();
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.PropertyField(Offset, GUILayout.MinWidth(50f));
        EditorGUILayout.PropertyField(Rotation, GUILayout.MinWidth(50f));
        EditorGUILayout.PropertyField(Scale, GUILayout.MinWidth(50f));

        if (!EditorApplication.isPlaying)
        {
            if (GUILayout.Button("Copy Transfrom Value"))
            {
                ((EffectRef)target).OnCopyTransformValue();
            }

            if (GUILayout.Button("Reset Transfrom Value"))
            {
                ((EffectRef)target).OnResetTransformValue();
            }
        }

        EditorGUILayout.LabelField("Weapon Trail", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(WeaponTrailStartDelay, new GUIContent("Trail Start Delay"), GUILayout.MinWidth(50f));
        EditorGUILayout.PropertyField(WeaponTrailLifeTime, new GUIContent("Trail Life Time"), GUILayout.MinWidth(50f));

        EditorGUILayout.LabelField("Sound", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(AudioClip, GUILayout.MinWidth(50f));

        if (EditorApplication.isPlaying)
        {
            if (GUILayout.Button("Reset Playback"))
            {
                EffectRef _target = (EffectRef)target;
                ParticleHoming homing = _target.GetComponent<ParticleHoming>();
                if (homing != null)
                    homing.SetTarget_Editor();
                _target.Restart();
            }
        }

        DrawIcon();
        serializedObject.ApplyModifiedProperties();
    }

    private void OnUpdateDropDownMenu()
    {
        OnUpdateAnimationDropDownMenu();
    }

    private void OnUpdateAnimationDropDownMenu()
    {
        if (Refmodel.objectReferenceValue != null)
        {
            AnimationClip[] clips = ((EffectRef)target).GetAnimationClips();
            if (clips.Length > 0)
            {
                animationoptions = new string[clips.Length];
                for (int i = 0; i < clips.Length; i++)
                {
                    if (clips[i] != null)
                    {
                        animationoptions[i] = clips[i].name;
                        if (clips[i].name == animationName)
                            selectedanimation = i;
                    }
                }
            }
        }
        else
        {
            Debug.Log("refmodel is empty");
        }
    }

    private void OnRefanimationSelected()
    {
        RefAnimation.stringValue = animationoptions[selectedanimation];
    }

    private void OnAttachEffect()
    {
        ((EffectRef)target).Attach_Editor();
    }

    private void OnDetachEffect()
    {
        ((EffectRef)target).Detach_Editor();
    }

    private void DrawIcon()
    {
        GameObject selectedobject = ((EffectRef)target).gameObject;
        SetCustomIcon(selectedobject, Icon);
    }

    private void SetCustomIcon(GameObject obj, Texture2D tex)
    {
        BindingFlags bindingFlags = BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.NonPublic;
        object[] args = new object[] { obj, tex };
        typeof(EditorGUIUtility).InvokeMember("SetIconForObject", bindingFlags, null, null, args);
    }
}