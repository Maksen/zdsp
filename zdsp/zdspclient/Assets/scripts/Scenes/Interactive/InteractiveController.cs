using Zealot.Client.Entities;
using Zealot.Spawners;

public class InteractiveController {

    public InteractiveController()
    {

    }

    private int pid = 0;
    public bool isInterruptible = false;
    private bool isArea = false;
    public bool isUsing = false;
    private InteractiveTrigger trigger;

    public void OnActionEnter(int mPid, bool interruptible, bool mIsArea, InteractiveTrigger mTrigger)
    {
        trigger = mTrigger;
        isInterruptible = interruptible;
        pid = mPid;
        isUsing = true;
        isArea = mIsArea;
        RPCFactory.CombatRPC.OnInteractiveUse(mPid, true);
    }

    public void OnActionLeave()
    {
        RPCFactory.CombatRPC.OnInteractiveUse(pid, false);
        pid = 0;
        trigger = null;
        isInterruptible = false;
        isUsing = false;
    }

    public void InterruptAction()
    {
        if(isArea)
            trigger.OpenUpdate();
        OnActionLeave();
    }

    public void SetEntityStats(int pid, bool canUse, bool active, int step)
    {
        InteractiveGhost ghost = GameInfo.gLocalPlayer.EntitySystem.GetEntityByPID(pid) as InteractiveGhost;
        if(ghost != null)
            ghost.entityObj.GetComponent<InteractiveEntities>().RefreshInteractiveStats(canUse, active, step);
    }

    public static void Init()
    {
        RPCFactory.CombatRPC.OnInteractiveInit();
    }
}