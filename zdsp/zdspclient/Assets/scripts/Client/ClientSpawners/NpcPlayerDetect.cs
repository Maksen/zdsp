using UnityEngine;
using System.Collections;
using Zealot.Client.Entities;
public class NpcPlayerDetect : MonoBehaviour {

    // Use this for initialization
    private SphereCollider _collider;
    public StaticClientNPCAlwaysShow Target = null;

    public void Init(StaticClientNPCAlwaysShow target, float radius)
    {
        if (_collider == null)
        {
            _collider = gameObject.AddComponent<SphereCollider>();
        }

        Target = target;
        _collider.radius = radius;
        _collider.isTrigger = true;
    }
	
    IEnumerator NextFrameTrigger()
    {
        yield return null;
        if (Target.GetType() == typeof(QuestNPC))
        {
            (Target as QuestNPC).OnPlayerNear();
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
            if (Target.GetType() == typeof(QuestNPC))
            {
                (Target as QuestNPC).OnPlayerAway();
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
