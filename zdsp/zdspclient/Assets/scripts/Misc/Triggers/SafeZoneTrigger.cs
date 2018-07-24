using UnityEngine;
using Zealot.Spawners;
using Zealot.Entities;
using System.Collections;
using Zealot.Client.Entities;

public class SafeZoneTrigger : ServerEntity
{
    public enum SafeZoneAreaType
    {
        Sphere,
        Box
    }

    public int myid;
    [HideInInspector]
    public float safeZoneRadius;//for sphere
    [HideInInspector]
    public Vector3 boxSize;//for box
    public SafeZoneAreaType myAreaType;

    SphereCollider SC;
    BoxCollider BC;
    bool myszexport;
    bool isEnter;

    void Start()
    {     
        if (Application.isPlaying)
        {
            if (myAreaType == SafeZoneAreaType.Sphere)
            {
                SC = gameObject.AddComponent<SphereCollider>();
                SC.isTrigger = true;
                SC.radius = safeZoneRadius;

            }
            if (myAreaType == SafeZoneAreaType.Box)
            {
                BC = gameObject.AddComponent<BoxCollider>();
                BC.isTrigger = true;
                BC.size = boxSize;

            }
        }
        else
            myszexport = false;
    }

    public override ServerEntityJson GetJson()
    {
        SafeZoneJson jsonclass = new SafeZoneJson();
        GetJson(jsonclass);
        return jsonclass;
    }

    public void SetMySafeZoneFlag(bool _flag)
    {
        myszexport = _flag;
    }

    public void GetJson(SafeZoneJson jsonclass)
    {
              
        if (myAreaType == SafeZoneAreaType.Sphere)
        {
            SphereCollider temp = gameObject.AddComponent<SphereCollider>();
            temp.radius = safeZoneRadius;
            jsonclass.safeZoneRadius = safeZoneRadius;
            jsonclass.size = Vector3.zero;
        }
        else if (myAreaType == SafeZoneAreaType.Box)
        {
            BoxCollider temp = gameObject.AddComponent<BoxCollider>();
            temp.size = boxSize;
           
            jsonclass.safeZoneRadius = 0;
            jsonclass.size = boxSize;
        }
        if (myszexport == true)
        {
            base.GetJson(jsonclass);
            myid = jsonclass.ObjectID;
            Debug.Log("i export");
        }
        else
        {
            jsonclass.position = transform.position;
            jsonclass.ObjectID = myid;
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("LocalPlayer"))
        {
            isEnter = true;
            RPCFactory.CombatRPC.OnSafeZone(true, myid , (int)myAreaType);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("LocalPlayer"))
        {
            isEnter = false;
            RPCFactory.CombatRPC.OnSafeZone(false, myid ,(int)myAreaType);
        }
    }

    IEnumerator delay(float _time)
    {
        yield return new WaitForSeconds(_time);
        while(true)
        {
            PlayerGhost localplayer = GameInfo.gLocalPlayer;
            if (localplayer != null && localplayer.LocalCombatStats != null && localplayer.LocalCombatStats.IsInSafeZone != isEnter)
            {
                yield return new WaitForSeconds(0.1f);
                RPCFactory.CombatRPC.OnSafeZone(isEnter, myid, (int)myAreaType);
                Debug.Log("in sz");
            }
            else
            {
                yield break;
            }
        }
       
    }

    void OnDrawGizmos()
    {
        if (myAreaType == SafeZoneAreaType.Sphere)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, safeZoneRadius);
        }

        if (myAreaType == SafeZoneAreaType.Box)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, boxSize);
        }
    }
}
