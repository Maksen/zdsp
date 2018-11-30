using UnityEngine;
using Zealot.Spawners;
using Zealot.Client.Entities;

public class InteractiveEntities : MonoBehaviour {
    
    public InteractiveTrigger parentTrigger;
    bool canClick = false;
    PlayerGhost player;

    public void Init()
    {
        parentTrigger = transform.parent.GetComponent<InteractiveTrigger>();
        if (parentTrigger.interactiveType == InteractiveType.Target)
        {
            canClick = true;
        }
    }

    public void OnClickEntity()
    {
        player = GameInfo.gLocalPlayer;
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

    private void Interactive()
    {
        if (canClick && !parentTrigger.GetUsing())
        {
            int entityId = parentTrigger.EntityId;
            player.InteractiveController.OnActionEnter(entityId,
                parentTrigger.interrupt, false, parentTrigger.GetComponent<InteractiveTrigger>());
        }
    }

    public void SetCanUse(bool canUse)
    {
        parentTrigger.SetCanUse(canUse);
        gameObject.layer = LayerMask.NameToLayer((canUse) ? "Entities" : "Character");
        if(!canUse)
            GameInfo.gCombat.OnSelectEntity(null);
    }
}