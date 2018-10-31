using System.Collections.Generic;
using System.Linq;
using Zealot.Entities;

namespace Zealot.Bot
{
    public class WorldMapGraphWithDijkstra
    {
        private List<MapNode> GraphNodes = null;
        private bool bSetupDone = false;

        public WorldMapGraphWithDijkstra()
        {              
        }

        public void Setup()
        {
            if (LevelReader.IsClientInitialized && !bSetupDone)
            {
                GraphNodes = new List<MapNode>();
                ///TODO: only need levels with portal entities. e.g. WorldMap levels.
                foreach (KeyValuePair<string, LevelInfo> entry in LevelReader.levels)
                {
                    LevelInfo levelinfo = entry.Value;
                    string levelName = entry.Key;
                    MapNode node = new MapNode();
                    var portals = PortalInfos.GetLevelPortals(levelName);
                    var portalExits = PortalInfos.GetLevelPortalExits(levelName);
                    node.Init(levelName, portals.ToList(), portalExits.ToList());
                    GraphNodes.Add(node);
                }
                bSetupDone = true;
            }
        }

        public Dictionary<string, string> LastRouterByPortal;//levelname, portalname
        public void DoRouter(string startlevel, string endlevel, out bool bfound)
        {
            LastRouterByPortal = new Dictionary<string, string>();
            Dictionary<string, string> path = new Dictionary<string, string>();
            List<MapNode> queue = new List<MapNode>();
            foreach (MapNode node in GraphNodes)
            {
                if (node.level == startlevel)
                {
                    node.Reset(0);
                }
                else
                    node.Reset(9999);
                queue.Add(node);
            }
            while (queue.Count > 0)
            {
                queue.Sort(delegate (MapNode n1, MapNode n2)
                {
                    return n1.dist < n2.dist ? -1 : 1;
                });
                MapNode node = queue[0];
                queue.RemoveAt(0);
                if (node.level == endlevel)
                {

                    break;
                }
                foreach (MapNode neighbornode in queue)
                {
                    if (node.GetDistance(neighbornode) == 1)
                    {
                        int dist = node.dist + 1; 
                        if (dist < neighbornode.dist)
                        {
                            neighbornode.dist = dist;
                            neighbornode.prevLinks.Add(node);
                        }
                    }
                }
            }

            List<MapNode> resultPaths = new List<MapNode>();
            MapNode destNode = null;
            foreach (MapNode node in GraphNodes)
            {
                if (node.level == endlevel)
                {
                    destNode = node;
                    break;
                }
            }
            if (destNode.dist < 9999)
            {
                resultPaths.Insert(0, destNode);
            }
            while(destNode.prevLinks.Count > 0)
            {
                resultPaths.Insert(0, destNode.prevLinks[0]);
                destNode = destNode.prevLinks[0];
            }
            string str = "";
            if (resultPaths.Count > 0 && resultPaths[resultPaths.Count - 1].level == endlevel)
            {
                bfound = true;
                for (int i = 0; i < resultPaths.Count; i++)
                {
                    str += " => " + resultPaths[i].level;
                    if (i < resultPaths.Count - 1)
                    {
                        string portalname = resultPaths[i].GetPortalName(resultPaths[i + 1]);
                        LastRouterByPortal.Add(resultPaths[i].level, portalname);
                    }
                }
            }
            else
            {
                bfound = false;
            }
            
            UnityEngine.Debug.Log("path is :" + str);
        }
    
        private class MapNode
        {
            public string level;
            public int dist = 0;//used by pathfind
            public List<MapNode> prevLinks;//used by pathfind
            private List<PortalEntryData> portalEntries;
            private List<LocationData> portalExits;

            public MapNode()
            {
                level = "uninitedname"; 
            }

            public void Init(string name, List<PortalEntryData> listentry, List<LocationData> listexit)
            {
                level = name;
                portalEntries = listentry;
                portalExits = listexit;
            }

            public void Reset(int pdist)
            {
                dist = pdist;
                prevLinks = new List<MapNode>();
            }

            public int GetDistance(MapNode node2)
            {  
                var portals1 = portalEntries;
                var portal2Exits = node2.portalExits;

                foreach (LocationData ldata in portal2Exits)
                    foreach (PortalEntryData entry in portals1)
                    {
                        //handle one enter portal => multiply exit port. 
                        string[] exitportals = entry.mExitName.Split(';');
                        for(int i =0; i < exitportals.Length; i++)
                        {
                            if (exitportals[i] == ldata.mName)
                            {
                                return 1;
                            }
                        }
                    }
                return 0; //0 means no link.
            }

            public string GetPortalName(MapNode node2)
            {
                var portals1 = portalEntries;
                var portal2Exits = node2.portalExits;

                foreach (LocationData ldata in portal2Exits)
                    foreach (PortalEntryData entry in portals1)
                    {
                        if (entry.mExitName == ldata.mName)
                        {
                            return entry.mName;
                        }
                    }
                return ""; 
            }
        }
    }
}
