using System;
using System.ComponentModel;
using UnityEngine;

namespace Zealot.Entities
{
    public enum AreaType
    {
        Sphere,
        Box
    } 

    [Serializable]
    public class DetectionArea
    {
        public AreaType mType = AreaType.Sphere;
        public float mRadius = 2;
        public Vector3 mExtents = Vector3.one;
    }

    public class PortalEntryJson : ServerEntityJson
    {
        //public bool activeOnStartup;
        [DefaultValue("")]
        public string myName { get; set; }
        [DefaultValue("")]
        public string exitName { get; set; }
        //public bool partyTeleport;
        //public DetectionArea detectionArea;
    }

    public class PortalExitJson : ServerEntityJson
    {
        [DefaultValue("")]
        public string myName { get; set; }
        public Vector3 forward = Vector3.forward;
    }
}
