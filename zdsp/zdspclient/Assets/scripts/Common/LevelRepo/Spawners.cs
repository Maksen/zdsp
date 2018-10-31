using System.ComponentModel;
using UnityEngine;

namespace Zealot.Entities
{
    public class ResourceSpawnerJson : ServerEntityWithEventJson
    {
        [DefaultValue(true)]
        public bool activeOnStartup { get; set; }
        [DefaultValue("")]
        public string archetype { get; set; }
        public Vector3 forward { get; set; }

        public override string GetServerClassName() { return "ResourceSpawner"; }
    }

    public class ObjectSpawnerJson : ServerEntityJson
    {
        [DefaultValue(true)]
        public bool activeOnStartup { get; set; }
        [DefaultValue("")]
        public string prefab { get; set; }
        public Vector3 forward { get; set; }

        public override string GetServerClassName() { return "ObjectSpawner"; }
    }
}
