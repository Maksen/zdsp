using UnityEngine;
using Zealot.Entities;

public class SafeZoneJson : ServerEntityJson
{
    public Vector3 size { get; set; }
    public float safeZoneRadius { get; set; }
    //public string uniqueName { get; set; }
}
