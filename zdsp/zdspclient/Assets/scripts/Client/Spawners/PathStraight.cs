using UnityEngine;
using System.Collections.Generic;
using Zealot.Entities;

namespace Zealot.Spawners
{
    [AddComponentMenu("Spawners at Server/PathStraight")]
    public class PathStraight : ServerEntity
    {
        public Color pathColor = Color.cyan;
        public List<Vector3> nodes = new List<Vector3>() { Vector3.zero, Vector3.zero };
        public int nodeCount;
        public bool pathVisible = true;
        //public long pauseInNode = 0;

        void Awake()
        {
            gameObject.tag = "EditorOnly";
        }

        void OnDrawGizmosSelected()
        {
            if (pathVisible)
            {
                if (nodes.Count > 0)
                {
                    iTween.DrawLine(nodes.ToArray(), pathColor);
                }
            }
        }

        public override ServerEntityJson GetJson()
        {
            PathStraightJson jsonclass = new PathStraightJson();
            GetJson(jsonclass);
            return jsonclass;
        }

        public void GetJson(PathStraightJson jsonclass)
        {         
            jsonclass.nodes = nodes.ToArray();
            //jsonclass.pauseInNode = pauseInNode;
            base.GetJson(jsonclass);
        }
    }
}