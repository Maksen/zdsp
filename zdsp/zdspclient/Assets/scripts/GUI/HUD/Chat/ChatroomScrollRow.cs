using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;

public class ChatroomScrollRow : MonoBehaviour
{
    [SerializeField]
    LayoutElement layoutElement = null;

    [Header("Prefabs")]
    [SerializeField]
    GameObject prefabChatboxMy = null;
    [SerializeField]
    GameObject prefabChatboxOther = null;
    [SerializeField]
    GameObject prefabChatboxSystem = null;

    HUD_Chatbox hudChatbox = null;

    public void Init(HUD_Chatroom hudChatroom, int rowIndex)
    {
        Clear();

        MessageType msgType = hudChatroom.CurrentChannelTab;
        List<ChatMessage> chatMessages = HUD_Chatroom.chatLog[(byte)msgType];
        ChatMessage chatMsg = chatMessages[rowIndex];
        GameObject gameobjChatbox = null;
        if (msgType == MessageType.System)
            gameobjChatbox = Instantiate(prefabChatboxSystem);
        else
            gameobjChatbox = (chatMsg.mSender == GameInfo.gLocalPlayer.Name) 
                ? Instantiate(prefabChatboxMy) : Instantiate(prefabChatboxOther);

        gameobjChatbox.transform.SetParent(gameObject.transform, false);
        layoutElement.preferredHeight = -1; // Reset preferred height
        hudChatbox = gameobjChatbox.GetComponent<HUD_Chatbox>();
        hudChatbox.Init(chatMsg.mSender, chatMsg.mMessage);
    }

    public float GetRowHeight()
    {
        float height = (hudChatbox != null) 
            ? hudChatbox.GetComponent<RectTransform>().rect.height : -1;
        layoutElement.preferredHeight = height;
        return height;
    }

    public void Clear()
    {
        if (hudChatbox != null)
        {
            Destroy(hudChatbox.gameObject);
            hudChatbox = null;
        }
    }
}
