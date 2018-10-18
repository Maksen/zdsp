using UnityEngine;
using Zealot.Spawners;
using Zealot.Client.Entities;
using Zealot.Common.Entities;

public class InteractiveEntities : MonoBehaviour {

    InteractiveTrigger parentTrigger;
    bool canClick = false;

	void Start () {
        parentTrigger = transform.parent.GetComponent<InteractiveTrigger>();
        if (parentTrigger.interactiveType == InteractiveType.Target)
            canClick = true;
    }

    public void OnClickEntity()
    {
        PlayerGhost player = GameInfo.gLocalPlayer;
        Vector3 distance = new Vector3(player.Position.x - transform.position.x, 0, player.Position.z - transform.position.z);
        if (distance.sqrMagnitude > 2 * 2)
            player.PathFindToTarget(transform.position, -1, 2, true, false, Interactive);
        else
            Interactive();
    }

    void Interactive()
    {
        if (parentTrigger.GetPlayer() == null && canClick)
        {
            parentTrigger.InitController(GameInfo.gLocalPlayer);
            parentTrigger.OnInteractiveTrigger();
        }
        else
        {
            UIManager.SystemMsgManager.ShowSystemMessage("Event is be using！", true);
        }
    }
}