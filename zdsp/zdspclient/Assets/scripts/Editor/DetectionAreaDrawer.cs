using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using Zealot.Entities;

[CustomPropertyDrawer (typeof (DetectionArea))]
public class DetectionAreaDrawer : PropertyDrawer {
	public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
	{
		EditorGUI.BeginProperty (pos, label, prop);
		// Draw label
		//Rect right_pos = EditorGUI.PrefixLabel (pos, GUIUtility.GetControlID (FocusType.Passive), label);
		
		// Don't make child fields be indented
		//var indent = EditorGUI.indentLevel;
		//EditorGUI.indentLevel = 0;

		var type_prop = prop.FindPropertyRelative ("mType");
		var radius_prop = prop.FindPropertyRelative ("mRadius");
		var extent_prop = prop.FindPropertyRelative ("mExtents");
		Rect firstRowRect = new Rect(pos.x, pos.y, pos.width, pos.height*0.45f);
		Rect secondRowRect = new Rect(pos.x, firstRowRect.yMax + pos.height*0.1f, pos.width, pos.height*0.45f);

		EditorGUI.PropertyField (firstRowRect, type_prop, label);
		switch ((AreaType)type_prop.enumValueIndex)
		{
			case AreaType.Box:
				EditorGUI.PropertyField (secondRowRect, extent_prop, new GUIContent("Extents"));
				break;
			case AreaType.Sphere:
				EditorGUI.PropertyField (secondRowRect, radius_prop, new GUIContent("Radius"));
				break;
		}
		
		// Set indent back to what it was
		//EditorGUI.indentLevel = indent;
		EditorGUI.EndProperty ();
	}
	
	// Overriding the GetPropertyHeight gives us the possibility to specify the property height
	public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
	{
		float height = base.GetPropertyHeight(prop, label);
		height *= 2.2f;
		return height;
	}
}

