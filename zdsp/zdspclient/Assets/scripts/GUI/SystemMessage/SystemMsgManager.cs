using System;
using System.Collections.Generic;
using UnityEngine;
using Zealot.Common;

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
        SystemMsgComponent component;
        if (sysMsgMap.TryGetValue(SystemMsgType.PartyMessage, out component))
            component.GetComponent<UI_PartyMessage>().ShowPartyMessage(message, okCallback, timeOutCallback, duration);
    }

    public void ShowEventNotification(string message)
    {
        SystemMsgComponent component;
        if (sysMsgMap.TryGetValue(SystemMsgType.EventNotification, out component))
            component.GetComponent<UI_EventNotification>().ShowMessage(message);
    }

    public void ShowAchievementMessage(AchievementKind type, int id)
    {
        SystemMsgComponent component;
        if (sysMsgMap.TryGetValue(SystemMsgType.Achievement, out component))
            component.GetComponent<UI_AchievementMessage>().ShowMessage(type, id);
    }

    public void EnableShowAchievementMessages(bool value)
    {
        SystemMsgComponent component;
        if (sysMsgMap.TryGetValue(SystemMsgType.Achievement, out component))
            component.GetComponent<UI_AchievementMessage>().EnableShowMessages(value);
    }

    private void OnDestroy()
    {
        sysMsgMap.Clear();
        sysMsgController = null;
        hourglass = null;
        UIManager.DestroySystemMsgManager();
    }
}
