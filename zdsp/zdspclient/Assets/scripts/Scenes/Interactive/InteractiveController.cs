using Zealot.Spawners;
using System.Collections.Generic;
using Kopio.JsonContracts;
using Zealot.Common;
using Zealot.Common.Entities;
using Zealot.Repository;

public class InteractiveController {

    public InteractiveTriggerInventoryData InteractiveTriggerInventory;

    public InteractiveController()
    {
        InteractiveTriggerInventory = new InteractiveTriggerInventoryData();
        InteractiveTriggerInventory.InitDefault();
    }

    public void InitFromStats(InteractiveTriggerStats interactiveTriggerStats)
    {
        InteractiveTriggerInventory.InitFromStats(interactiveTriggerStats);
    }

    public bool isArea = true;
    bool interruptible = false;
    InteractiveTrigger interactiveTrigger = null;
    
    public bool isUsing = false;
    public bool isCompleted = false;

    public void Init(bool area, bool interrupt, InteractiveTrigger interactive)
    {
        isArea = area;
        interruptible = interrupt;
        interactiveTrigger = interactive;
    }

    public bool GetInterrupt()
    {
        return interruptible;
    }

    public void Interrupt()
    {
        if (interactiveTrigger != null)
        {
            interactiveTrigger.InterruptActrion();
            isUsing = false;
        }
    }
    public void StartProgress()
    {
        isUsing = true;
        isCompleted = false;
    }

    public bool IsProgressing()
    {
        return isUsing;
    }

    public void InitProgress()
    {
        isUsing = false;
        isCompleted = false;
    }

    public void CompletedProgress()
    {
        if (isUsing && !isCompleted)
        {
            isUsing = false;
            isCompleted = true;
        }
        else
        {
            UIManager.SystemMsgManager.ShowSystemMessage("Progress is abnormal. Check code or player cheat.", false);
        }
    }
}