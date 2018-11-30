using Zealot.Spawners;

public class InteractiveController {

    private int entityId = 0;
    public bool isInterruptible = false;
    private bool isArea = false;
    public bool isUsing = false;
    private InteractiveTrigger trigger;

    public InteractiveController()
    {
    }

    public void OnActionEnter(int mEntityId, bool interruptible, bool mIsArea, InteractiveTrigger mTrigger)
    {
        trigger = mTrigger;
        isInterruptible = interruptible;
        entityId = mEntityId;
        isUsing = true;
        isArea = mIsArea;
        RPCFactory.CombatRPC.OnInteractiveUse(mEntityId, true);
    }
    
    public void OnActionLeave()
    {
        trigger = null;
        isInterruptible = false;
        isUsing = false;
    }

    public void InterruptAction()
    {
        if(isArea)
            trigger.OpenUpdate();
        RPCFactory.CombatRPC.OnInteractiveUse(entityId, false);
        OnActionLeave();
    }

    public void OnActionCompeleted()
    {
        RPCFactory.CombatRPC.OnInteractiveTrigger(entityId);
        OnActionLeave();
    }
}