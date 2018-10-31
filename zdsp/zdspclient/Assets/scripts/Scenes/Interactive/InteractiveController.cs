using Zealot.Spawners;

public class InteractiveController {

    public InteractiveController()
    {

    }
    
    public bool isInterruptible = false;
    public bool isUsing = false;
    
    InteractiveTrigger interactiveTrigger;

    public void OnActionEnter(InteractiveTrigger trigger, bool interruptible)
    {
        interactiveTrigger = trigger;
        isInterruptible = interruptible;
        isUsing = true;
    }

    public void OnActionLeave()
    {
        interactiveTrigger.InterruptAction();
        interactiveTrigger = null;
        isInterruptible = false;
        isUsing = false;
    }

    public void ActionInterupted()
    {
        OnActionLeave();
    }

    public bool IsUsing()
    {
        return isUsing;
    }
}