using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using Zealot.Spawners;
using System.Collections.Generic;

[CustomPropertyDrawer (typeof (EntityLink))]
public class EntityLinkDrawer : PropertyDrawer {
	public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
	{
		EditorGUI.BeginProperty (pos, label, prop);
		// Draw label
		//pos = EditorGUI.PrefixLabel (pos, GUIUtility.GetControlID (FocusType.Passive), label);

		// Don't make child fields be indented
		var indent = EditorGUI.indentLevel;
		EditorGUI.indentLevel = 0;

		ServerEntityWithEvent target = prop.serializedObject.targetObject as ServerEntityWithEvent;
		var event_prop = prop.FindPropertyRelative ("mEvent");
		var receiver_prop = prop.FindPropertyRelative ("mReceiver");
		var trigger_prop = prop.FindPropertyRelative ("mTrigger");
		

		Rect event_rect = new Rect (pos.x + 40, pos.y, pos.width - 40, pos.height * 0.4f); 
		if (target.Events.Length > 0) {
			EditorGUI.LabelField (pos, "Event");
			int event_selected = Array.IndexOf(target.Events, event_prop.stringValue);
			event_selected = EditorGUI.Popup(event_rect, (event_selected == -1) ? 0 : event_selected, target.Events);
			event_prop.stringValue = target.Events[event_selected];

			var receiver = receiver_prop.objectReferenceValue as GameObject;
			Rect receiverRect = new Rect (pos.x, event_rect.yMax + 2, 100, pos.height * 0.4f ); 
			Rect triggerRect = new Rect (pos.x + 100, event_rect.yMax + 2, pos.width - 100, pos.height * 0.4f);
			if (receiver != null) {
				var spawner = receiver.GetComponent<ServerEntity> ();
				if (spawner != null && spawner.Triggers.Length > 0) {
					int selected = Array.IndexOf(spawner.Triggers, trigger_prop.stringValue);
					selected = EditorGUI.Popup(triggerRect, (selected == -1) ? 0 : selected, spawner.Triggers);
					trigger_prop.stringValue = spawner.Triggers[selected];
				} 
			}
			EditorGUI.PropertyField (receiverRect, receiver_prop, GUIContent.none);
			//EditorGUI.PropertyField (triggerRect, trigger_prop, GUIContent.none);
		}

		// Set indent back to what it was
		EditorGUI.indentLevel = indent;
		EditorGUI.EndProperty ();
	}

	// Overriding the GetPropertyHeight gives us the possibility to specify the property height
	public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
	{
		float height = 0f;
		ServerEntityWithEvent target = prop.serializedObject.targetObject as ServerEntityWithEvent;
		if (target.Events.Length > 0) {
			height = base.GetPropertyHeight (prop, label);
			height *= 2.5f;
		}
		return height;
	}
}