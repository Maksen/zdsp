using UnityEngine;
using System.Collections.Generic;
using Zealot.Common.Actions;
using Zealot.Client.Actions;
using Pathfinding;

public enum PathFindingStates
{
    Error = -1,
    NotReady = 0,
    Success = 1
}

public static class PathFinder
{
    static Seeker seeker;
    static List<Vector3> mCacheWayPoints;
    public static void Init()
    {
        GameObject go = new GameObject();
        go.name = "PathFinder";
        seeker = go.AddComponent<Seeker>();
        go.AddComponent<StripStraightPointsModifier>();
        seeker.drawGizmos = false;
    }
    public static bool IsSamePosition(Vector3 v1, Vector3 v2, float delta = 0.0025f)
    {
        v1.y = 0; v2.y = 0;
        float d2 = (v1 - v2).sqrMagnitude;
        return (d2 < delta);
    }

    public static PathFindingStates PlotPath(Vector3 startpoint, Vector3 endpoint, out List<Vector3> waypoints)
    {
        Path p = seeker.StartPath(startpoint, endpoint);
        AstarPath.WaitForPath(p);
        // UnityEngine.Object.Destroy(go);

        waypoints = p.vectorPath;
        mCacheWayPoints = waypoints;
        PathCompleteState pstate = p.CompleteState;

        if (pstate == PathCompleteState.Complete)
        {
            return PathFindingStates.Success;
        }
        else if (pstate == PathCompleteState.NotCalculated)
        {
            return PathFindingStates.NotReady;
        }
        else
            return PathFindingStates.Error;
    }

    public static List<Vector3> GetWayPoints()
    {
        return mCacheWayPoints;
    }
}
