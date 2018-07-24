using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor.Animations;

[CustomEditor(typeof(ParticleAndAnimation))]
public class ParticleAndAnimationInspector : Editor 
{
	ParticleAndAnimation pa;
    SerializedProperty Objects;
    SerializedProperty Length;
    Dictionary<string, List<string>> options = new Dictionary<string, List<string>>();
    Dictionary<string, int> selectindex = new Dictionary<string, int>();

    void OnEnable()
    {
        pa = target as ParticleAndAnimation;
        pa.GetAnimatorObject();
        Objects = serializedObject.FindProperty("AnimatorObjects");
        Length = serializedObject.FindProperty("Length");

        OnUpdateDropDownMenu();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        if (GUILayout.Button("Play Once"))
            pa.PlayOnce();

        if (GUILayout.Button("Play Loop"))
            pa.PlayLoop();

        EditorGUILayout.Separator();

        Length.floatValue = EditorGUILayout.FloatField("Lenght", Length.floatValue);

        EditorGUILayout.Separator();

        EditorGUILayout.BeginVertical();
        int length = pa.AnimatorObjects.Length;
        for (int i = 0; i < length; i++)
        {
            EditorGUILayout.BeginHorizontal();
            string name = pa.AnimatorObjects[i].obj.GetComponent<Animator>().name;
            selectindex[name] = EditorGUILayout.Popup(name, selectindex[name], options[name].ToArray());
            pa.SetAnimationState(i, options[name][selectindex[name]]);
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();
    }

    void OnUpdateDropDownMenu()
    {
        int length = pa.AnimatorObjects.Length;
        for (int i = 0; i < length; i++)
        {
            AnimatorController ac = pa.AnimatorObjects[i].obj.GetComponent<Animator>().runtimeAnimatorController as AnimatorController;
            AnimatorStateMachine sm = ac.layers[0].stateMachine;

            List<string> states = new List<string>();
            foreach (ChildAnimatorState state in sm.states)
                states.Add(state.state.name);

            string name = pa.AnimatorObjects[i].obj.GetComponent<Animator>().name;
            options.Add(name, states);

            int index = 0;
            string selected = pa.GetAnimationState(i);
            for (int j = 0; j < states.Count; j++)
            {
                if (states[j] == selected)
                    index = j;
            }

            selectindex.Add(name, index);
        }
    }
}
