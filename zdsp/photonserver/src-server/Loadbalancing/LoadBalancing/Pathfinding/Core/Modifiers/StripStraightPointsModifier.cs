using UnityEngine;
using System.Collections.Generic;

using Pathfinding;
using Pathfinding.Util;

namespace Pathfinding
{
    [AddComponentMenu("Pathfinding/Modifiers/Strip Waypoints in Straight Line")]
    [System.Serializable]
    /** Modifier which strips away unnecessary waypoints in a straight line when grid graph is used.
     * \nThis is to reduce the number of waypoints and therefore number of walk actions
     * \nthat AI need to take to reach destination. In this way, there is less chance
     * \nof pauses at the client when monster moves by path following. Also, add original end point
     * \nto the final path, if last waypoint is some distance away from original end point.
     * */
    public class StripStraightPointsModifier : MonoModifier
    {

        public override ModifierData input
        {
            get { return ModifierData.All; }
        }

        public override ModifierData output
        {
            get { return ModifierData.VectorPath; }

        }

        public override void Apply(Path p, ModifierData source)
        {

            //This should never trigger unless some other modifier has messed stuff up
            if (p.vectorPath == null)
            {
                Debug.LogWarning("Can't process NULL path (has another modifier logged an error?)");
                return;
            }

            List<Vector3> path = null;

            path = StripWaypointsInStraightLine(p.vectorPath);

            if (path != p.vectorPath)
            {
                ListPool<Vector3>.Release(p.vectorPath);
                p.vectorPath = path;
            }            
        }

        private List<Vector3> StripWaypointsInStraightLine(List<Vector3> path)
        {
            List<Vector3> optimizedPath = new List<Vector3>();

            optimizedPath.Add(path[0]);

            UnityEngine.Vector3 prevDir = new Vector3(99, 99, 99); //some invalid direction
            for (int i = 1; i < path.Count - 1; i++)
            {                
                Vector3 currWaypoint = path[i];
                Vector3 nextWaypoint = path[i + 1];                            
                Vector3 currDir = (nextWaypoint - currWaypoint).normalized;
                if (currDir.x != prevDir.x || currDir.y != prevDir.y || currDir.z != prevDir.z) //strip away waypoints if same direction
                {                    
                    prevDir = currDir;
                    optimizedPath.Add(currWaypoint);                 
                }                
            }

            if (path.Count > 1) //if input path at least 2 points
            {
                Vector3 C = path[path.Count - 1];

                if (optimizedPath.Count > 1) //if result path has at least 2 points
                {
                    Vector3 B = optimizedPath[optimizedPath.Count - 1];
                    Vector3 A = optimizedPath[optimizedPath.Count - 2];

                    Vector3 BC = (C - B).normalized;
                    Vector3 AB = (B - A).normalized;
                    if (AB.x == BC.x || AB.y == BC.y || AB.z == BC.z) //then B is redundant
                    {
                        optimizedPath.RemoveAt(optimizedPath.Count - 1);
                    }
                }
                optimizedPath.Add(C); //We always add the final waypoint            
            }

            return optimizedPath;
        }
        
    }
}