using System.Collections.Generic;
using UnityEngine;

namespace Zealot.Entities
{
    public class EntityLinkServer
    {
        public int mReceiver { get; set; }
        public string mTrigger { get; set; }

        public EntityLinkServer(int receiver, string trigger)
        {
            mReceiver = receiver;
            mTrigger = trigger;
        }
    }

    public class ServerEntityJson
    {
        public int ObjectID { get; set; }
        public Vector3 position { get; set; }
        public bool ShowInMap { get; set; }

        public virtual string GetServerClassName() { return ""; }
    }

    public class ServerEntityWithEventJson : ServerEntityJson
    {
        public Dictionary<string, List<EntityLinkServer>> EntityLinks_Server;
    }

    public class PlayerSpawnerJson : ServerEntityJson
    {
        public Vector3 forward = Vector3.forward;
    }

    public class PathStraightJson : ServerEntityJson
    {
        public Vector3[] nodes;
    }
}
