using Candlelight.UI;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Repository;

public class HUD_Chatroom : MonoBehaviour
{
    // Editor Linked Gameobjects
    [Header("Top Section")]
    [SerializeField]
    ChatroomScrollView chatroomScrollView = null;

    [Header("Bottom Section")]
    [SerializeField]
    InputField inputfieldChatMessage = null;
    [SerializeField]
    HyperText hypertxtInputfield = null;

    public static List<List<ChatMessage>> chatLog = new List<List<ChatMessage>>(new List<ChatMessage>[6]);

    public MessageType CurrentChannelTab { get; private set; }

    MessageType CurrentChannelToSend = MessageType.System;

    Dictionary<int, GameTimer> channelCooldowns;
    List<string> itemCodeList = null;
    
    bool isChatLogDirty = false;
    bool isScrollViewMouseDown = false;
    List<RaycastResult> rayResult = null;
    GraphicRaycaster gfxRaycaster = null;
    
    bool isEditedOnInputfieldChanged = false;
    string prevInputfieldValue = "";
    TextGenerationSettings txtGenSettingHypertxtInputfield;

    // Localized Strings
    string toStr = "對";

    // Use this for initialization
    void Awake()
    {
        rayResult = new List<RaycastResult>(); // Create list to receive all raycast results
        gfxRaycaster = UIManager.UIHud.GetComponent<GraphicRaycaster>();

        Vector2 extents = hypertxtInputfield.rectTransform.rect.size;
        txtGenSettingHypertxtInputfield = hypertxtInputfield.GetGenerationSettings(extents);

        itemCodeList = new List<string>(); // For storing item code when filter send text

        // Channel cooldown timers
        channelCooldowns = new Dictionary<int, GameTimer>();
        for (int i = 1; i < 6; ++i)
            channelCooldowns.Add(i, null);
    }

    public void Init()
    {    
    }

    void OnEnable()
    {
        // Init chat inputfield message
        CanvasRenderer txtCmptRender = inputfieldChatMessage.textComponent.gameObject.GetComponent<CanvasRenderer>();
        txtCmptRender.SetAlpha(0);
        inputfieldChatMessage.text = "";
        isEditedOnInputfieldChanged = false;
        prevInputfieldValue = "";

        CurrentChannelTab = MessageType.System;
        CurrentChannelToSend = MessageType.World;
        chatroomScrollView.InitScrollView(this);
        StartCoroutine(chatroomScrollView.PopulateRows());
    }

    void OnDisable()
    {
    }

    void OnDestroy()
    {
    }

    // Update is called once per frame
    void Update()
    {
        isScrollViewMouseDown = false;
        if (Input.GetMouseButton(0) && gfxRaycaster != null)
        {
            PointerEventData pEventData = new PointerEventData(null); //Create the PointerEventData with null for the EventSystem
            pEventData.position = Input.mousePosition; //Set required parameters, in this case, mouse position
            rayResult.Clear(); // Receive all raycast results
            gfxRaycaster.Raycast(pEventData, rayResult); // Raycast it
            int retCount = rayResult.Count;
            for (int i = 0; i < retCount; ++i)
            {
                if (rayResult[i].gameObject.name.Equals("ScrollviewB_Chat"))
                {
                    isScrollViewMouseDown = true;
                    break;
                }
            }
        }
    }

    void LateUpdate()
    {
        // Update content in scrollview
        if (isChatLogDirty && !isScrollViewMouseDown)
        {
            StartCoroutine(ScrollToLatestMsg());
            isChatLogDirty = false; // Set to false after updated
        }
    }

    IEnumerator ScrollToLatestMsg()
    {
        yield return null;
        //scrollrectChatMessages.verticalNormalizedPosition = 0; // Scroll to latest
    }

    public void AddToChatLog(byte msgType, string msg, string sender, string whisperTo, int portraitId = 0, byte jobSect = 1,
                             byte vipLvl = 0, byte faction = 1, bool isVoiceChat = false)
    {
        if (GameInfo.mInspectMode)
            return;

        msg = SwapSymbolWithTag(msg);
        bool hasItemTag = CheckItemTagExist(msg);
        //if (chatLog.Count >= 100) // Maximum 100 logs
        //{
        //    chatLog.RemoveAt(0);
            //if (chatboxList.Count >= 100)
            //{
                // Remove voice chat first
                //string voiceClipName = chatBox.VoiceClipName;
                //if (!string.IsNullOrEmpty(voiceClipName) && voiceElements.ContainsKey(voiceClipName))
                //{
                //    voiceElements.Remove(voiceClipName);
                //    VoiceChatManager.Instance.RemoveVoice(voiceClipName);
                //}
            //}
            //if (currHUDChatIdx > 0)
            //    --currHUDChatIdx;
        //}
        chatLog[msgType].Add(new ChatMessage((MessageType)msgType, msg, sender, whisperTo, 
                             portraitId, jobSect, vipLvl, faction, isVoiceChat, 0, hasItemTag));

        if (gameObject.activeInHierarchy)
            StartCoroutine(chatroomScrollView.PopulateRows());
    }

    bool ValidateWhisperTo(string msg, out string whisperTo, out int msgStartIdx)
    {
        if (msg.Length >= 4)
        {
            if (msg[0] == toStr[0] && msg[1] == '[')
            {
                msgStartIdx = msg.IndexOf(']');
                if (msgStartIdx != -1)
                {
                    ++msgStartIdx; // Real index
                    whisperTo = msg.Substring(2, msgStartIdx - 3);
                    return true;
                }
            }
        }
        msgStartIdx = -1;
        whisperTo = "";
        return false;
    }

    public bool CheckMsgValidity(ref string message, ref byte msgType, ref string whisperTo, bool isVoiceChat)
    {
        bool skipGetWhisperTo = false;
        string processedMsg = message;
        if (message.StartsWith("/"))
        {
            CurrentChannelToSend = MessageType.Whisper;
            msgType = (byte)CurrentChannelToSend;
        }
        else
        {
            int msgStartIdx = 0;
            if (ValidateWhisperTo(message, out whisperTo, out msgStartIdx))
            {
                CurrentChannelToSend = MessageType.Whisper;
                msgType = (byte)CurrentChannelToSend;
                if (message.Length > msgStartIdx)
                    processedMsg = message.Substring(msgStartIdx);
                else if (message.Length == msgStartIdx)
                    return false;
                skipGetWhisperTo = true;
            }
        }

        // Skip sending chat if still in cooldown
        if (channelCooldowns.ContainsKey(msgType) && channelCooldowns[msgType] != null)
        {
            UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("ret_OnCooldown"));
            return false;
        }

        message = processedMsg; // Set to processed message
        switch (msgType)
        {
            case (byte)MessageType.Guild:
                if (GameInfo.gLocalPlayer.PlayerSynStats.guildName == "")
                {
                    UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("ret_Chat_NoGuild"));
                    return false;
                }
                break;
            case (byte)MessageType.Party:
                if (!GameInfo.gLocalPlayer.IsInParty())
                {
                    UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("ret_Chat_NoParty"));
                    return false;
                }
                break;
            case (byte)MessageType.Whisper:
                if (!skipGetWhisperTo)
                {
                    int charIdx = message.IndexOf(' ');
                    if (charIdx != -1)
                    {
                        whisperTo = message.Substring(1, charIdx - 1);
                        message = message.Substring(charIdx + 1);
                    }
                    else return false;
                }
                if (string.IsNullOrEmpty(whisperTo) || (!isVoiceChat && string.IsNullOrEmpty(message)))
                    return false;
                else if (!string.IsNullOrEmpty(whisperTo))
                {
                    if (whisperTo == GameInfo.gLocalPlayer.Name)
                    {
                        UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("ret_Chat_NoWhisperYourself"));
                        return false;
                    }
                    //OnClickFriendsData(whisperTo);
                }
                break;
        }
        return true;
    }

    public void SetChannelCooldown(byte msgType)
    {
        long coolDownTime = 0;
        switch (msgType)
        {
            case (byte)MessageType.World:   coolDownTime = 5000; break;
            case (byte)MessageType.Guild:   coolDownTime = 5000; break;
            case (byte)MessageType.Party:   coolDownTime = 300; break;
            case (byte)MessageType.Nearby:  coolDownTime = 300; break;
            case (byte)MessageType.Whisper: coolDownTime = 300; break;
        }
        if (coolDownTime > 0)     
            channelCooldowns[msgType] =
                GameInfo.gCombat.mTimers.SetTimer(coolDownTime, (arg) => { channelCooldowns[msgType] = null; }, null);
    }

    string SwapTagWithSymbol(string str)
    {
        int strLen = str.Length;
        StringBuilder sb = new StringBuilder();
        itemCodeList.Clear();
        int times = 0, startIdx = 0;
        bool skipOnce = false;
        for (int i = 0; i < strLen; ++i)
        {
            char c1 = str[i];
            if ((times == 0 && c1 == '<') || (times == 1 && c1 == '\"'))
            {
                for (int j = i + 1; j < strLen; ++j)
                {
                    char c2 = str[j];
                    if ((times == 0 && c2 == '\"') || (times == 1 && c2 == '>'))
                    {
                        if (c2 == '>' && !skipOnce)
                        {
                            skipOnce = true;
                            continue;
                        }
                        sb.Length = 0; // Clear StringBuilder
                        sb.Append(str);
                        sb.Remove(i, j - i + 1);
                        if (times == 0)
                        {
                            sb.Insert(i, "`|");
                            startIdx = i;
                            str = sb.ToString();
                            strLen = str.Length;
                            i += 2;
                            times = 1;
                        }
                        else if (times == 1)
                        {
                            sb.Insert(i, "|`");
                            str = sb.ToString();
                            itemCodeList.Add(str.Substring(startIdx, i - startIdx + 2));
                            sb.Remove(startIdx, i - startIdx + 2);
                            sb.Insert(startIdx, "{}");
                            str = sb.ToString();
                            strLen = str.Length;
                            i = startIdx + 1;
                            times = 0;
                        }
                        skipOnce = false;
                        break;
                    }
                }
            }
        }
        return str;
    }

    string RestoreItemCode(string str)
    {
        StringBuilder sb = new StringBuilder();
        int strLen = str.Length, itemIdx = 0;
        for (int i = 0; i < strLen; ++i)
        {
            if (str[i] == '{' && (i + 1 < strLen) && str[i + 1] == '}')
            {
                sb.Append(itemCodeList[itemIdx]);
                ++itemIdx;
                ++i;
            }
            else
                sb.Append(str[i]);
        }
        return sb.ToString();
    }

    string SwapSymbolWithTag(string str)
    {
        if (string.IsNullOrEmpty(str))
            return str;
        int strLen = str.Length;
        StringBuilder sb = new StringBuilder();
        StringBuilder sb2 = new StringBuilder();
        int nameStart = 0;
        for (int i = 0; i < strLen; ++i)
        {
            if (str[i] == '`' && (i + 1 < strLen) && str[i + 1] == '|')
            {
                sb.Length = 0; // Clear StringBuilder
                sb.Append(str);
                sb.Remove(i, 2);
                sb.Insert(i, "<a name=\"");
                i += 9;
                str = sb.ToString();
                strLen = str.Length;
                nameStart = i;
            }
            else if (str[i] == '|' && (i + 1 < strLen) && str[i + 1] == '`')
            {
                sb.Length = 0; // Clear StringBuilder
                sb.Append(str);
                sb.Remove(i, 2);
                sb.Insert(i, "\" class=\"");
                string itemCode = sb.ToString(nameStart, i - nameStart);
                IInventoryItem item = GameRepo.ItemFactory.GetItemFromCode(itemCode, true);
                i += 9;
                string rarityStr = item.JsonObject.rarity.ToString().ToLower();
                sb.Insert(i, rarityStr);
                i += rarityStr.Length;
                sb.Insert(i, "\">");
                i += 2;
                // Insert name here  
                sb2.Length = 0; // Clear StringBuilder
                sb2.Append("[");
                sb2.Append(GameRepo.ItemFactory.GetItemById(item.ItemID).localizedname);
                sb2.Append("]");
                string localName = sb2.ToString();
                sb.Insert(i, localName);
                i += localName.Length;
                sb.Insert(i, "</a>");
                i += 4;
                str = sb.ToString();
                strLen = str.Length;
            }
        }
        return str;
    }

    bool CheckItemTagExist(string str)
    {
        if (string.IsNullOrEmpty(str))
            return false;

        int idx = str.IndexOf("<a name=\"");
        if (idx != -1)
        {
            idx = str.IndexOf("class=\"", idx + 9);
            if (idx != -1)
            {
                if (str.IndexOf("</a>", idx + 7) != -1)
                    return true;
            }
        }
        return false;
    }

    string RemoveExcessSpaceFromEnd(string str)
    {
        if (string.IsNullOrEmpty(str))
            return str;

        int strLen = str.Length;
        StringBuilder sb = new StringBuilder(str);
        for (int i = strLen - 1; i >= 0; --i)
        {
            if (sb[i] == ' ')
            {
                sb.Remove(i, 1);
                strLen = sb.Length;
            }
            else break;
        }
        return sb.ToString();
    }

    string TrimToCharLimitFromStart(string txt, string processedTxt, int charLimit)
    {
        if (string.IsNullOrEmpty(txt))
            return txt;
        int procLen = processedTxt.Length;
        if (procLen <= charLimit)
            return txt;

        StringBuilder sb = new StringBuilder(txt);
        for (int i = 0; i < sb.Length; ++i)
        {
            if (i + 16 < sb.Length && sb[i] == '<' && sb[i+1] == 'a' && sb[i+2] == ' ' && sb[i+3] == 'n' &&
                sb[i+4] == 'a' && sb[i+5] == 'm' && sb[i+6] == 'e' && sb[i+7] == '=' && sb[i+8] == '\"')
            {
                for (int j = i + 8; j < sb.Length; ++j)
                {
                    if (j + 6 < sb.Length && sb[j] == '\"' && sb[j+1] == '>')
                    {
                        bool hasRemoved = false;
                        for (int k = j + 1; k < sb.Length; ++k)
                        {
                            if (k + 3 < sb.Length && sb[k] == '<' && sb[k+1] == '/' && sb[k+2] == 'a' && sb[k+3] == '>')
                            {
                                int tagEnd = k + 3;
                                sb.Remove(i, tagEnd - i + 1);
                                procLen -= k - j - 2;
                                --i;
                                hasRemoved = true;
                                break;
                            }
                        }
                        if (hasRemoved)
                            break;
                    }
                }
            }
            else // Just a normal character
            {
                sb.Remove(i, 1);
                --procLen;
                --i;
            }
            if (procLen <= charLimit)
                break;
        }
        return sb.ToString();
    }

    string TrimToCharLimitFromEnd(string txt, string processedTxt, int charLimit, bool trimOnce = false)
    {
        if (string.IsNullOrEmpty(txt))
            return txt;
        int procLen = processedTxt.Length;
        if (!trimOnce && procLen <= charLimit)
            return txt;

        StringBuilder sb = new StringBuilder(txt);
        for (int i = sb.Length - 1; i >= 0; --i)
        {
            if (i >= 16 && sb[i] == '>' && sb[i-1] == 'a' && sb[i-2] == '/' && sb[i-3] == '<')
            {
                for (int j = i - 4; j >= 0; --j)
                {
                    if (j >= 11 && sb[j] == '>' && sb[j - 1] == '\"')
                    {
                        bool hasRemoved = false;
                        for (int k = j-2; k >= 0; --k)
                        {
                            if (k >= 8 && sb[k] == '\"' && sb[k-1] == '=' && sb[k-2] == 'e' && sb[k-3] == 'm' &&
                               sb[k-4] == 'a' && sb[k-5] == 'n' && sb[k-6] == ' ' && sb[k-7] == 'a' && sb[k-8] == '<')
                            {
                                int tagStart = k - 8;
                                sb.Remove(tagStart, i - tagStart + 1);
                                if (trimOnce)
                                    return sb.ToString();
                                procLen -= i - 4 - j;
                                i = tagStart;
                                hasRemoved = true;
                                break;
                            }
                        }
                        if (hasRemoved)
                            break;
                    }
                }
            }
            else // Just a normal character
            {
                sb.Remove(i, 1);
                --procLen;
            }
            if (procLen <= charLimit)
                break;
        }
        return sb.ToString();
    }

    public void OnValueChangedChannels(int index)
    {
        if (CurrentChannelTab == (MessageType)index)
            return;

        MessageType msgType = (MessageType)index;
        CurrentChannelTab = msgType;
        CurrentChannelToSend = (msgType != MessageType.System) ? msgType : MessageType.World;

        StartCoroutine(chatroomScrollView.PopulateRows());
    }

    public void OnValueChangedInputfieldMessage(string value)
    {
        if (isEditedOnInputfieldChanged) // Return if inputfield is manually edited during this function
        {
            isEditedOnInputfieldChanged = false;
            return;
        }

        // Remove special tags when backspace
        if (prevInputfieldValue.Length - value.Length == 1)
        {
            if (prevInputfieldValue.EndsWith(">") && !value.EndsWith(">"))
            {
                value = TrimToCharLimitFromEnd(prevInputfieldValue, "", 0, true);
                isEditedOnInputfieldChanged = true;
                inputfieldChatMessage.text = value;
            }
            else if (prevInputfieldValue.StartsWith(string.Format("{0}[", toStr)) &&
                     prevInputfieldValue.EndsWith("]") && !value.EndsWith("]"))
            {
                isEditedOnInputfieldChanged = true;
                inputfieldChatMessage.text = value = "";
                inputfieldChatMessage.caretPosition = 0;
            }
        }

        if (value.StartsWith("/") && !value.StartsWith("/w ") && !value.StartsWith("/f ") &&
            !value.StartsWith("/g ") && !value.StartsWith("/p "))
        {
            int whitespaceIdx = value.IndexOf(' ');
            if (whitespaceIdx > 1 && whitespaceIdx == value.Length - 1)
            {
                value = string.Format("{0}[{1}]", toStr, value.Substring(1, whitespaceIdx - 1));
                isEditedOnInputfieldChanged = true;
                inputfieldChatMessage.text = value;
                inputfieldChatMessage.caretPosition = value.Length;
            }
        }

        // Process hypertext for inputfield
        HyperTextProcessor hyperTxtProc = new HyperTextProcessor();
        hyperTxtProc.InputText = value; // Process text with text processor
        string hyperTxtProcessedTxt = hyperTxtProc.OutputText;
        if (hyperTxtProcessedTxt.Length > 46)
        {
            value = TrimToCharLimitFromEnd(value, hyperTxtProcessedTxt, 46);
            value = RemoveExcessSpaceFromEnd(value);
            isEditedOnInputfieldChanged = true;
            inputfieldChatMessage.text = value;
            hyperTxtProc.InputText = value; // Process text with text processor
            hyperTxtProcessedTxt = hyperTxtProc.OutputText;
        }

        hypertxtInputfield.text = value;     
        hypertxtInputfield.cachedTextGenerator.Populate(hyperTxtProcessedTxt, txtGenSettingHypertxtInputfield);

        // Trim hypertext to inputfield width
        int lenLimit = hypertxtInputfield.cachedTextGenerator.characterCountVisible;
        lenLimit = (lenLimit == hyperTxtProcessedTxt.Length) ? lenLimit : lenLimit+1;
        value = TrimToCharLimitFromStart(value, hyperTxtProcessedTxt, lenLimit);
        hypertxtInputfield.text = value;   
        prevInputfieldValue = value;
    }

    public void OnClickCloseChatroom()
    {
        UIManager.SetWidgetActive(HUDWidgetType.Chatroom, false);
    }

    public void OnClickSendChatMessage()
    {
        string chatMessage = inputfieldChatMessage.text;
        inputfieldChatMessage.text = ""; // Clear input field
        if (string.IsNullOrEmpty(chatMessage)) // No text no talk
            return;

        byte msgType = (byte)CurrentChannelToSend;
        string whisperTo = "";
        if (!CheckMsgValidity(ref chatMessage, ref msgType, ref whisperTo, false))
        {
            inputfieldChatMessage.text = chatMessage; // Set back to inputfield
            return;
        }

        chatMessage = SwapTagWithSymbol(chatMessage); // Swap out tag before sending msg
        // Filter text here 
        //string filteredTxt = "";
        //if (WordFilterRepo.FilterString(message, '*', DirtyWordType.Chat, out filteredTxt))
        //    message = filteredTxt;
        //SetChannelCooldown(msgType);

        if (itemCodeList.Count > 0)
            chatMessage = RestoreItemCode(chatMessage);

        RPCFactory.CombatRPC.ClientSendChatMessage(msgType, chatMessage, whisperTo, false);
    }
}
 