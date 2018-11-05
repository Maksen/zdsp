using UnityEngine;
using Zealot.Spawners;
using Zealot.Client.Entities;

public class InteractiveEntities : MonoBehaviour {
    
    public InteractiveTrigger parentTrigger;
    bool canClick = false;

	void Start () {
        parentTrigger = transform.parent.GetComponent<InteractiveTrigger>();
        if (parentTrigger.interactiveType == InteractiveType.Target)
        {
            canClick = true;
        }
    }

    public void OnClickEntity()
    {
        PlayerGhost player = GameInfo.gLocalPlayer;
        Vector3 distance = new Vector3(player.Position.x - transform.position.x, 0, player.Position.z - transform.position.z);
        if (distance.sqrMagnitude > 2 * 2)
        {
            player.PathFindToTarget(transform.position, -1, 2, true, false, Interactive);
        }
        else
        {
            Interactive();
        }
    }

    void Interactive()
    {
        if (canClick && parentTrigger.GetStep() == Zealot.Common.InteractiveTriggerStep.None)
        {
            int entityId = parentTrigger.EntityId;
            GameInfo.gLocalPlayer.InteractiveController.OnActionEnter(entityId,
                parentTrigger.interrupt, false, parentTrigger.GetComponent<InteractiveTrigger>());
        }
    }

    public void RefreshInteractiveStats(bool canUse, bool active, int step)
    {
        parentTrigger.Init(canUse, active, step);
    }
}