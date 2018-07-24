using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

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
        public string myName = "";
        public string exitName = "";
        //public bool partyTeleport;
        //public DetectionArea detectionArea;
    }

	public class PortalExitJson : ServerEntityJson
	{
		public string myName = "";
        public Vector3 forward = Vector3.forward;
	}
}

