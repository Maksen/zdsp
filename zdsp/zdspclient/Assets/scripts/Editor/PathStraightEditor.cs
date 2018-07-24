using UnityEngine;
using UnityEditor;
using System.Collections;
using Zealot.Spawners;

[CustomEditor(typeof(PathStraight))]
public class PathStraightEditor : Editor
{
    PathStraight _target;
    GUIStyle style = new GUIStyle();
    public static int count = 0;

    void OnEnable()
    {
        //i like bold handle labels since I'm getting old:
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = Color.white;
        _target = (PathStraight)target;
    }

    public override void OnInspectorGUI()
    {
        //draw the path?
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Path Visible");
        _target.pathVisible = EditorGUILayout.Toggle(_target.pathVisible);
        EditorGUILayout.EndHorizontal();

        //path color:
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Path Color");
        _target.pathColor = EditorGUILayout.ColorField(_target.pathColor);
        EditorGUILayout.EndHorizontal();

        //EditorGUILayout.BeginHorizontal();
        //EditorGUILayout.PrefixLabel("Pause In Node");
        //_target.pauseInNode = EditorGUILayout.LongField(_target.pauseInNode);
        //EditorGUILayout.EndHorizontal();

        //exploration segment count control:
        EditorGUILayout.BeginHorizontal();
        //EditorGUILayout.PrefixLabel("Node Count");
        _target.nodeCount = Mathf.Max(2, EditorGUILayout.IntField("Node Count", _target.nodeCount));
        //_target.nodeCount =  Mathf.Clamp(EditorGUILayout.IntSlider(_target.nodeCount, 0, 10), 2,100);
        EditorGUILayout.EndHorizontal();

        //add node?
        if (_target.nodeCount > _target.nodes.Count)
        {
            for (int i = 0; i < _target.nodeCount - _target.nodes.Count; i++)
            {
                _target.nodes.Add(Vector3.zero);
            }
        }

        //remove node?
        if (_target.nodeCount < _target.nodes.Count)
        {
            if (EditorUtility.DisplayDialog("Remove path node?", "Shortening the node list will permantently destory parts of your path. This operation cannot be undone.", "OK", "Cancel"))
            {
                int removeCount = _target.nodes.Count - _target.nodeCount;
                _target.nodes.RemoveRange(_target.nodes.Count - removeCount, removeCount);
            }
            else
            {
                _target.nodeCount = _target.nodes.Count;
            }
        }

        //node display:
        EditorGUI.indentLevel = 4;
        for (int i = 0; i < _target.nodes.Count; i++)
        {
            _target.nodes[i] = EditorGUILayout.Vector3Field("Node " + (i + 1), _target.nodes[i]);
        }

        //update and redraw:
        if (GUI.changed)
        {
            EditorUtility.SetDirty(_target);
        }
    }

    void OnSceneGUI()
    {
        if (_target.pathVisible)
        {
            if (_target.nodes.Count > 0)
            {
                //allow path adjustment undo:
                Undo.RecordObject(_target, "Adjust iTween Path");

                //path begin and end labels:
                Handles.Label(_target.nodes[0], "Path Begin", style);
                Handles.Label(_target.nodes[_target.nodes.Count - 1], "Path End", style);

                //node handle display:
                for (int i = 0; i < _target.nodes.Count; i++)
                {
                    _target.nodes[i] = Handles.PositionHandle(_target.nodes[i], Quaternion.identity);
                }
            }
        }
    }
}
