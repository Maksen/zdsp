using UnityEngine;  
using UnityEditor;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class GenerateNavData: MonoBehaviour
{
    [MenuItem("Tools/Generate NavData")]
    static void GenerateDefault()
    {
        GameObject astar = GameObject.Find("A*");
        if (astar == null)
        {
            astar = new GameObject("A*");
            //AstarPath astarpath = astar.AddComponent<AstarPath>();
            astar.AddComponent<AstarPath>();
            List<GameObject> sel = new List<GameObject>();
            sel.Add(astar);
            Selection.objects = sel.ToArray();
        }
        else
        {
            EditorUtility.DisplayDialog("Generate A*", "The GameObject A* already exists. Only one A* is allowed per scene.", "Ok");
        }
    }
}

