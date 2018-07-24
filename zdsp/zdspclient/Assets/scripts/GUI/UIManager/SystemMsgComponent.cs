using UnityEngine;
using System.Collections;

public class SystemMsgComponent : MonoBehaviour
{
    public SystemMsgType messageType;

    public void RegisterSystemMessage()
    {
        if (messageType != SystemMsgType.None)
        {
            UIManager.SystemMsgManager.RegisterSystemMessage(messageType, this);
        }
    }
}
