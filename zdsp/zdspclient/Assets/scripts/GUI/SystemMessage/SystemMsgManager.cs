using System;
using System.Collections.Generic;
using UnityEngine;

public class SystemMsgManager : MonoBehaviour
{
    private Dictionary<SystemMsgType, SystemMsgComponent> sysMsgMap = new Dictionary<SystemMsgType, SystemMsgComponent>();
    private SystemMessageController sysMsgController;
    private UI_HourGlass hourglass;

    private void Awake()
    {
        UIManager.RegisterSystemMsgManager(this);

        var sysMsgComponents = gameObject.GetComponentsInChildren<SystemMsgComponent>(true);
        for (int i = 0; i < sysMsgComponents.Length; i++)
            sysMsgComponents[i].RegisterSystemMessage();
    }

    public void RegisterSystemMessage(SystemMsgType msgType, SystemMsgComponent component)
    {
        if (sysMsgMap.ContainsKey(msgType))
            Debug.LogErrorFormat("Duplicate msgtype {0} registered with SystemMsgManager", msgType);
        else
        {
            sysMsgMap.Add(msgType, component);
            if (msgType == SystemMsgType.SystemMessage)
                sysMsgController = component.gameObject.GetComponent<SystemMessageController>();
            else if (msgType == SystemMsgType.Hourglass)
                hourglass = component.gameObject.GetComponent<UI_HourGlass>();
        }
    }

    public void ShowSystemMessage(string message, bool addToChatLog = false)
    {
        if (sysMsgController != null)
            sysMsgController.AddSystemMessage(message, addToChatLog);
    }

    public void StartHourglass(float duration, string message, Action timeoutcallback)
    {
        if (hourglass != null)
            hourglass.Show(duration, message, timeoutcallback);
    }

    public void StopHourglass()
    {
        if (hourglass != null)
            hourglass.Stop();
    }

    public void ShowPartyMessage(string message, Action okCallback, Action timeOutCallback, float duration)
    {
        if (sysMsgMap.ContainsKey(SystemMsgType.PartyMessage))
            sysMsgMap[SystemMsgType.PartyMessage].GetComponent<UI_PartyMessage>().ShowPartyMessage(message, okCallback, timeOutCallback, duration);
    }

    public void ShowEventNotification(string message)
    {
        if (sysMsgMap.ContainsKey(SystemMsgType.EventNotification))
            sysMsgMap[SystemMsgType.EventNotification].GetComponent<UI_EventNotification>().ShowMessage(message);
    }

    private void OnDestroy()
    {
        sysMsgMap.Clear();
        sysMsgController = null;
        hourglass = null;
        UIManager.DestroySystemMsgManager();
    }
}
