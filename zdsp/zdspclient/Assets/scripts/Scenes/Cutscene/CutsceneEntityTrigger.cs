using UnityEngine;
using System.Collections;

public enum TriggerColliderType
{
    Sphere,
    Box,
}

public class CutsceneEntityTrigger : MonoBehaviour
{
    public TriggerColliderType triggerColliderType;

    [HideInInspector]
    public float radius;
    [HideInInspector]
    public Vector3 boxSize;

    public CutsceneEntity   cutsceneEntity;
    public int              tutorialId;

    // Use this for initialization
    void Start ()
    {
        if(Application.isPlaying)
        {
            if (triggerColliderType == TriggerColliderType.Sphere)
            {
                var SC = gameObject.AddComponent<SphereCollider>();
                SC.isTrigger = true;
                SC.radius = radius;
            }
            else if (triggerColliderType == TriggerColliderType.Box)
            {
                var BC = gameObject.AddComponent<BoxCollider>();
                BC.isTrigger = true;
                BC.size = boxSize;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("LocalPlayer"))
        {
            if(tutorialId >= 1)
            {
                RPCFactory.NonCombatRPC.OnTriggerTutorial(tutorialId);
            }

            cutsceneEntity.PlayCutscene();
            gameObject.SetActive(false);
        }
    }

    void OnDrawGizmos()
    {
        if (triggerColliderType == TriggerColliderType.Sphere)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, radius);
        }

        if (triggerColliderType == TriggerColliderType.Box)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, boxSize);
        }
    }
}
