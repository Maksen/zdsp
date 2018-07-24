using UnityEngine;
using System.Collections.Generic;

namespace Zealot.Entities
{
	public class EntityLinkServer
	{
		public int mReceiver;
		public string mTrigger;
		
		public EntityLinkServer(int receiver, string trigger)
		{
			mReceiver = receiver;
			mTrigger = trigger;
		}
	}
	
	public class ServerEntityJson
	{
		public int ObjectID;
		public Vector3 position;
        public bool ShowInMap;
        public virtual string GetServerClassName(){return "";}
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