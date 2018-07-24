using UnityEngine;

namespace Zealot.Entities
{
	public class DelayTickerJson : ServerEntityWithEventJson
	{
		public bool activeOnStartup;
		public long delay;
		public long ticker;
		
		public override string GetServerClassName(){return "DelayTickerServer";}
	}

    public class CounterJson : ServerEntityWithEventJson
    {
        public bool activeOnStartup;
        public int count;

        public override string GetServerClassName() { return "Counter"; }
    }

    public class MessageBroadcasterJson : ServerEntityJson
    {
        public byte type;
        public bool emergency;
        public string[] messages;

        public override string GetServerClassName() { return "MessageBroadcaster";  }
    }

    public class GateSpawnerJson : ServerEntityJson
    {
        public bool activeOnStartup = true;
        public Vector3 forward;
        public float width = 4f;
        public float height = 2f;
        public string prefab = "";

        public override string GetServerClassName() { return "GateSpawner"; }
    }

    public class CutSceneBroadcasterJson : ServerEntityJson
    {
        public override string GetServerClassName() { return "CutSceneBroadcaster"; }
    }

    public class ColliderTriggerJson : ServerEntityWithEventJson
    {
        public bool activeOnStartup = true;
        public int count = 1;

        public override string GetServerClassName() { return "ColliderTrigger"; }
    }
}