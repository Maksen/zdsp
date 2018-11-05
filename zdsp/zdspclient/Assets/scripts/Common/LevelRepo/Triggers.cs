using System.ComponentModel;
using UnityEngine;

namespace Zealot.Entities
{
    public class DelayTickerJson : ServerEntityWithEventJson
    {
        [DefaultValue(true)]
        public bool activeOnStartup { get; set; }
        public long delay { get; set; }
        public long ticker { get; set; }

        public override string GetServerClassName() { return "DelayTickerServer";}
    }

    public class CounterJson : ServerEntityWithEventJson
    {
        [DefaultValue(true)]
        public bool activeOnStartup { get; set; }
        public int count { get; set; }

        public override string GetServerClassName() { return "Counter"; }
    }

    public class MessageBroadcasterJson : ServerEntityJson
    {
        public byte type { get; set; }
        public bool emergency { get; set; }
        public string[] messages { get; set; }

        public override string GetServerClassName() { return "MessageBroadcaster";  }
    }

    public class GateSpawnerJson : ServerEntityJson
    {
        [DefaultValue(true)]
        public bool activeOnStartup { get; set; }
        public Vector3 forward { get; set; }
        [DefaultValue(1f)]
        public float width { get; set; }
        [DefaultValue(1f)]
        public float height { get; set; }
        [DefaultValue("")]
        public string prefab { get; set; }

        public override string GetServerClassName() { return "GateSpawner"; }
    }

    public class CutsceneBroadcasterJson : ServerEntityWithEventJson
    {
        public override string GetServerClassName() { return "CutsceneBroadcaster"; }
    }

    public class ColliderTriggerJson : ServerEntityWithEventJson
    {
        [DefaultValue(true)]
        public bool activeOnStartup { get; set; }
        [DefaultValue(1)]
        public int count { get; set; }

        public override string GetServerClassName() { return "ColliderTrigger"; }
    }

    public class RandomTriggerJson : ServerEntityWithEventJson
    {
        [DefaultValue(true)]
        public bool activeOnStartup { get; set; }
        public bool loop { get; set; }
        [DefaultValue(10)]
        public byte size { get; set; }

        public override string GetServerClassName() { return "RandomTrigger"; }
    }

    public class InteractiveTriggerJson : ServerEntityWithEventJson
    {
        [DefaultValue(true)]
        public bool activeObject;
        [DefaultValue(true)]
        public bool canTrigger;
        [DefaultValue(true)]
        public bool activeOnStartup { get; set; }
        [DefaultValue("")]
        public string archetype { get; set; }
        [DefaultValue("")]
        public string parentPath { get; set; }
        public Vector3 forward { get; set; }
        public int interactiveTime { get; set; }
        public bool interrupt { get; set; }
        public int counter { get; set; }
        public int keyId { get; set; }
        public bool isArea { get; set; }
        [DefaultValue(1)]
        public int min { get; set; }
        [DefaultValue(1)]
        public int max { get; set; }

        public override string GetServerClassName() { return "InteractiveTrigger"; }
    }
}
