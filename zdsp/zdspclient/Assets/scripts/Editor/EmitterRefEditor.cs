using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections;

[CustomEditor(typeof(EmitterRef))]
[CanEditMultipleObjects]
public class EmitterRefEditor : Editor 
{
	SerializedProperty Refbone;
	SerializedProperty Offset;
	SerializedProperty Rotation;
	SerializedProperty Scale;
	SerializedProperty WorldSpace;

	string[] childrenoptions = new string[0];
	string boneName = null;
	int selectedbone;
	Texture2D Icon;

	public void OnEnable()
	{
		Icon =	EditorGUIUtility.Load ("Icon/child.png") as Texture2D;

		Refbone = serializedObject.FindProperty ("Refbone");
		Offset = serializedObject.FindProperty ("Offset");
		Rotation = serializedObject.FindProperty ("Rotation");
		Scale = serializedObject.FindProperty ("Scale");
		WorldSpace = serializedObject.FindProperty ("WorldSpace");

		boneName = ((EmitterRef)target).GetRefboneName ();
		OnUpdateBoneDropDownMenu ();
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update ();

		EditorGUILayout.BeginHorizontal();
		if (childrenoptions.Length > 0) {
			int boneindex = EditorGUILayout.Popup ("Refbone", selectedbone, childrenoptions);
			if (boneindex != selectedbone) {
				selectedbone = boneindex;
				OnRefboneSelected ();
			}
			if (GUILayout.Button ("Refresh", GUILayout.MaxWidth (80f))) {
				OnUpdateBoneDropDownMenu ();
			}
		}
		EditorGUILayout.EndHorizontal();

		WorldSpace.boolValue = EditorGUILayout.Toggle ("World Space", WorldSpace.boolValue);
		
		GUILayout.Label( "Offset", GUILayout.MaxWidth( 100f ) );
		EditorGUILayout.PropertyField(Offset, GUIContent.none, GUILayout.MinWidth(50f));
		
		GUILayout.Label( "Rotation", GUILayout.MaxWidth( 100f ) );
		EditorGUILayout.PropertyField(Rotation, GUIContent.none, GUILayout.MinWidth(50f));
		
		GUILayout.Label( "Scale", GUILayout.MaxWidth( 100f ) );
		EditorGUILayout.PropertyField(Scale, GUIContent.none, GUILayout.MinWidth(50f));

		if (GUILayout.Button("Copy Transfrom Value")){
			((EmitterRef)target).OnCopyTransformValue();
		}
		
		if (GUILayout.Button("Reset Transfrom Value")){
			((EmitterRef)target).OnResetTransformValue();
		}

		DrawIcon ();
		serializedObject.ApplyModifiedProperties ();
	}

	private void OnUpdateBoneDropDownMenu()
	{
		Transform[] children = ((EmitterRef)target).GetActorChildren();
		if (children != null)
		{
			childrenoptions = new string[children.Length];
			for (int i=0; i<children.Length; i++) {
				childrenoptions [i] = children [i].name;
				if (children [i].name == boneName)
					selectedbone = i;
			}
		}
	}

	private void OnRefboneSelected()
	{
		Refbone.stringValue = childrenoptions [selectedbone];
	}

	private void DrawIcon ()
	{
		GameObject selectedobject = ((EmitterRef)target).gameObject;
		SetCustomIcon(selectedobject, Icon);
	}

	private void SetCustomIcon(GameObject obj, Texture2D tex)
	{
		BindingFlags bindingFlags = BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.NonPublic;
		object[] args = new object[] { obj, tex };
		typeof(EditorGUIUtility).InvokeMember("SetIconForObject", bindingFlags, null, null, args);
	}
}
