using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Zealot.Entities
{
    public class ResourceSpawnerJson : ServerEntityWithEventJson
    {
        public bool activeOnStartup;
        public string archetype;
        public Vector3 forward;

        public override string GetServerClassName() { return "ResourceSpawner"; }
    }

    public class ObjectSpawnerJson : ServerEntityJson
    {
        public bool activeOnStartup;
        public string prefab = "";
        public Vector3 forward;

        public override string GetServerClassName() { return "ObjectSpawner"; }
    }
}
