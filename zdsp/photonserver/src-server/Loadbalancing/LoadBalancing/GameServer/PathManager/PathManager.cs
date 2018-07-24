namespace Photon.LoadBalancing.GameServer
{
    using System;
    using System.Collections.Generic;      
    using Pathfinding;

    public enum PathFindState
    {
        Error = -1,
        NotReady = 0,
        Success = 1,
        Partial = 2
    }

    public enum PathPostProcess
    {
        None = 0,
        Simple = 1,
        Bezier = 2,
        StripStraightPoints = 3
    }

    public static class PathManager
    {        
        public struct PathFindRequest{
            public UnityEngine.Vector3 start, end;
        }
        public static AstarPath astarPathFinder = new AstarPath();
        public static Dictionary<string, Dictionary<int, Seeker>> pathSeekers = new Dictionary<string, Dictionary<int, Seeker>>();

        //private static Timer updateTimer = null;

        public static void StartUpdate()
        {
            //updateTimer = new Timer(Update, null, 100, 100);            
            astarPathFinder.Update();        //threading inside astarpath.cs    
        }

        private static void Update(Object stateopbj)
        {
            astarPathFinder.Update();
        }

        private static void LoadNavData()
        {
            astarPathFinder.Init();
            string path = System.IO.Path.Combine(GameApplication.AssemblyDirectory, "../navdata/");
            string[] files = System.IO.Directory.GetFiles(path, "*.bytes");
            foreach (string fpath in files)
            {
                string levelname = System.IO.Path.GetFileNameWithoutExtension(fpath);              
                astarPathFinder.LoadGraphData(levelname, fpath);                
            }
        }

        public static void InitNavData()
        {
            LoadNavData();
        }        

        //testing method only!!!!!!!!
        //public static void TestPath()
        //{                        
        //    InitNavData();
        //    //Path path = pathseeker.StartPath(new Vector3(12.5f, 0.0f, 29.9f), new Vector3(30.1f, 0.0f, 38.8f));
        //    Path path = pathseeker.StartPath(new UnityEngine.Vector3(133.4f, 0.0f, 131.3f), new UnityEngine.Vector3(77.5f, 16.4f, 27.5f));
        //    //astarpath.TestComputePath(path);
        //    //---
        //    //Max number of ticks before yielding/sleeping
        //    //long maxTicks = 10000;
        //    //long targetTick = System.DateTime.UtcNow.Ticks + maxTicks;

        //    //path.Prepare();
        //    //if (!path.IsDone())
        //    //{
        //    //    path.Initialize();                
        //    //    while (!path.IsDone())
        //    //    {
        //    //        path.CalculateStep(targetTick);                    
        //    //        path.searchIterations++;                    

        //    //        // If the path has finished calculation, we can break here directly instead of sleeping
        //    //        // Improves latency
        //    //        if (path.IsDone()) break;                    
        //    //        //yield return null;                    

        //    //        targetTick = System.DateTime.UtcNow.Ticks + maxTicks;
        //    //    }
        //    //}

        //    //// Cleans up node tagging and other things
        //    //path.Cleanup();
        //    ////---

        //    List<UnityEngine.Vector3> waypoints = path.vectorPath;
        //    String errlog = path.errorLog;
        //}

        //public static void TestPath2(){
        //    Path path = pathseeker.StartPath(new UnityEngine.Vector3(133.4f, 0.0f, 131.3f), new UnityEngine.Vector3(77.5f, 16.4f, 27.5f));
        //    //ABPath path = ABPath.Construct(new UnityEngine.Vector3(133.4f, 0.0f, 131.3f), new UnityEngine.Vector3(77.5f, 16.4f, 27.5f));
        //    //AstarPath.StartPath(path);
        //    List<UnityEngine.Vector3> waypoints = path.vectorPath;
        //    String errlog = path.errorLog;
        //}

        public static void SeekPath(string levelname, string roomid, int pid, UnityEngine.Vector3 startpoint, UnityEngine.Vector3 endpoint, PathPostProcess ppp = PathPostProcess.None)
        {
            if (!pathSeekers.ContainsKey(roomid))
            {
                pathSeekers.Add(roomid, new Dictionary<int, Seeker>());
            }                
            if (!pathSeekers[roomid].ContainsKey(pid))
            {
                Seeker seeker = new Seeker(levelname);
                switch (ppp)
                {
                    case PathPostProcess.StripStraightPoints:
                        StripStraightPointsModifier sspmod = new StripStraightPointsModifier();
                        seeker.RegisterModifier(sspmod);
                        break;
                    case PathPostProcess.Simple:
                        SimpleSmoothModifier smod = new SimpleSmoothModifier();
                        seeker.RegisterModifier(smod);
                        break;
                    case PathPostProcess.Bezier:
                        SimpleSmoothModifier bmod = new SimpleSmoothModifier();
                        bmod.smoothType = SimpleSmoothModifier.SmoothType.Bezier;
                        seeker.RegisterModifier(bmod);
                        break;
                    default:
                        break;
                }
                seeker.Init();
                pathSeekers[roomid].Add(pid, seeker);
            }
            Path path = pathSeekers[roomid][pid].StartPath(startpoint, endpoint);                
        }


        //this method only retrieve current available path find result computed in the last update,
        //which might not be updated yet to the current path find request
        public static List<UnityEngine.Vector3> GetWaypoints(string roomid, int pid, out PathFindState pathState)
        {            
            if (pathSeekers.ContainsKey(roomid) && pathSeekers[roomid].ContainsKey(pid))
            {
                Path p = pathSeekers[roomid][pid].GetCurrentPath();
                if (p != null)
                {
                    switch (p.CompleteState)
                    {
                        case PathCompleteState.NotCalculated:
                            pathState = PathFindState.NotReady;
                            break;
                        case PathCompleteState.Error:
                            pathState = PathFindState.Error;
                            return null;
                        case PathCompleteState.Complete:
                            pathState = PathFindState.Success;
                            break;
                        case PathCompleteState.Partial:
                            pathState = PathFindState.Partial;
                            break;
                        default:
                            pathState = PathFindState.Error;
                            return null;
                    }
                    if (p.vectorPath == null || p.vectorPath.Count < 1) pathState = PathFindState.Error;
                    return p.vectorPath;
                }

            }            
            pathState = PathFindState.Error;
            return null;            
        }

        public static void RemoveSeeker(string roomid, int pid){
            if (pathSeekers.ContainsKey(roomid) && pathSeekers[roomid].ContainsKey(pid)) {
                pathSeekers[roomid].Remove(pid);
            }
        }
    }
}
