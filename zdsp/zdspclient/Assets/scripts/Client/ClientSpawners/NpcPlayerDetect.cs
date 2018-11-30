using UnityEngine;
using System.Collections;
using Zealot.Client.Entities;

public class NpcPlayerDetect : MonoBehaviour
{
    private SphereCollider _collider;
    public StaticClientNPCAlwaysShow Target = null;

    public void Init(StaticClientNPCAlwaysShow target, float radius)
    {
        if (_collider == null)
            _collider = gameObject.AddComponent<SphereCollider>();

        Target = target;
        _collider.radius = radius;
        _collider.isTrigger = true;
    }
	
    IEnumerator NextFrameTrigger()
    {
        yield return null;
        if (Target.GetType() == typeof(StaticNPCGhost))
        {
            (Target as StaticNPCGhost).OnPlayerNear();
        }
        else if(Target.GetType() == typeof(StaticAreaGhost))
        {
            (Target as StaticAreaGhost).OnPlayerNear();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("LocalPlayer"))
        {
            //Debug.Log("Player Enter"); 
            StartCoroutine(NextFrameTrigger());
        }
    }

    void OnTriggerExit(Collider other)
    {        
        if (other.CompareTag("LocalPlayer"))
        {
            //Debug.Log("Player Exit");
            if (Target.GetType() == typeof(StaticNPCGhost))
            {
                (Target as StaticNPCGhost).OnPlayerAway();
            }
            else if (Target.GetType() == typeof(StaticAreaGhost))
            {
                (Target as StaticAreaGhost).OnPlayerAway();
            }
            else if (Target.GetType() == typeof(StaticTargetGhost))
            {
                (Target as StaticTargetGhost).OnPlayerAway();
            }
        }
    }
}


public class NpcPlayerBoxDetect : MonoBehaviour
{
    private BoxCollider _collider;
    public StaticClientNPCAlwaysShow Target = null;

    public void Init(StaticClientNPCAlwaysShow target, float radius)
    {
        if (_collider == null)
            _collider = gameObject.AddComponent<BoxCollider>();

        Target = target;
        _collider.size = new Vector3(radius, radius, radius);
        _collider.isTrigger = true;
    }

    IEnumerator NextFrameTrigger()
    {
        yield return null;
        if (Target.GetType() == typeof(StaticNPCGhost))
        {
            (Target as StaticNPCGhost).OnPlayerNear();
        }
        else if (Target.GetType() == typeof(StaticAreaGhost))
        {
            (Target as StaticAreaGhost).OnPlayerNear();
        }
        else if (Target.GetType() == typeof(StaticGuideGhost))
        {
            (Target as StaticGuideGhost).OnPlayerNear();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("LocalPlayer"))
        {
            //Debug.Log("Player Enter"); 
            StartCoroutine(NextFrameTrigger());
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("LocalPlayer"))
        {
            //Debug.Log("Player Exit");
            if (Target.GetType() == typeof(StaticNPCGhost))
            {
                (Target as StaticNPCGhost).OnPlayerAway();
            }
            else if (Target.GetType() == typeof(StaticAreaGhost))
            {
                (Target as StaticAreaGhost).OnPlayerAway();
            }
            else if (Target.GetType() == typeof(StaticTargetGhost))
            {
                (Target as StaticTargetGhost).OnPlayerAway();
            }
            else if (Target.GetType() == typeof(StaticGuideGhost))
            {
                (Target as StaticGuideGhost).OnPlayerAway();
            }
        }
    }
}