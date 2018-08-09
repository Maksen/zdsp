using AsPerSpec;
using Candlelight.UI;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using Zealot.Common;
using Zealot.Repository;

public class HUD_ChatroomMini : MonoBehaviour
{
    // Editor Linked Gameobjects
    [SerializeField]
    CarouselToggler carouselToggler = null;

    [SerializeField]
    GameObject[] openChatroomButtons = null;

    [SerializeField]
    HyperText[] hyperTxtChannels = null;

    MessageType CurrentChannel = MessageType.System;
    List<int> miniChatLogIdxs = new List<int>(new int[6]);
    List<float> miniChatLogDelays = new List<float>(new float[6]);   
    const float miniChatLogDelayAmt = 0.5f;

    Dictionary<string, string> hudItemRefList = null;
    Dictionary<string, int> hudItemDupRefList = null;
    IList<string> hudVoiceRefList = null;

    // Localized String
    string toStr = "";
    string sayStr = "";
    string youStr = "";
    string localizedStrSystem = "";
    string localizedStrWorld = "";
    string localizedStrGuild = "";
    string localizedStrParty = "";
    string localizedStrNearby = "";
    string localizedStrWhisper = "";

    // Use this for initialization
    void Awake()
    {
        // Init Chat Messages/mini chat delays
        List<List<ChatMessage>> chatLogs = HUD_Chatroom.chatLog;
        for (int i = 0; i < 6; ++i)
        {
            openChatroomButtons[i].SetActive(false);
            miniChatLogIdxs[i] = 0;
            miniChatLogDelays[i] = 0;
            chatLogs[i] = new List<ChatMessage>();
        }
        openChatroomButtons[0].SetActive(true);

        // For storing ref when trimming text in HUD mini chat
        hudItemRefList = new Dictionary<string, string>();
        hudItemDupRefList = new Dictionary<string, int>();
        hudVoiceRefList = new List<string>();

        // Initialize localized string
        toStr = GUILocalizationRepo.GetLocalizedString("crm_to");
        sayStr = GUILocalizationRepo.GetLocalizedString("crm_say");
        youStr = GUILocalizationRepo.GetLocalizedString("crm_you");
        localizedStrSystem = GUILocalizationRepo.GetLocalizedString("crm_system");
        localizedStrWorld = GUILocalizationRepo.GetLocalizedString("crm_world");
        localizedStrGuild = GUILocalizationRepo.GetLocalizedString("crm_guild");
        localizedStrParty = GUILocalizationRepo.GetLocalizedString("crm_party");
        localizedStrNearby = GUILocalizationRepo.GetLocalizedString("crm_nearby");
        localizedStrWhisper = GUILocalizationRepo.GetLocalizedString("crm_whisper");
    }

    // Update is called once per frame
    void LateUpdate()
    {
        // Timer for HUD chat to display chat logs
        for (int i = 0; i < 6; ++i)
        {
            if (miniChatLogDelays[i] >= 0)
                miniChatLogDelays[i] -= Time.deltaTime;
            else
            {
                List<ChatMessage> chatMessages = HUD_Chatroom.chatLog[i];
                if (miniChatLogIdxs[i] < chatMessages.Count)
                {
                    if (UpdateChatroomMiniLog((MessageType)i, chatMessages, miniChatLogIdxs[i]++))
                        miniChatLogDelays[i] = miniChatLogDelayAmt;
                }
            }
        }
    }

    string GetLocalizedCharMsgType(MessageType type)
    {
        switch (type)
        {
            case MessageType.System: return  localizedStrSystem;
            case MessageType.World: return   localizedStrWorld;
            case MessageType.Guild: return   localizedStrGuild;
            case MessageType.Party: return   localizedStrParty;
            case MessageType.Nearby: return  localizedStrNearby;
            case MessageType.Whisper: return localizedStrWhisper;   
            default: return "";
        }
    }

    void StoreRefToItemTag(string str)
    {
        if (!string.IsNullOrEmpty(str))
        {
            hudItemRefList.Clear();
            hudItemDupRefList.Clear();
            hudVoiceRefList.Clear();
            int strLen = str.Length;
            for (int i = 0; i < strLen; ++i)
            {
                if (i + 3 < strLen && str[i] == '<' && str[i + 1] == 'a' && str[i + 2] == ' ' && str[i + 3] == 'n')
                {
                    if (string.Compare(str, i + 9, "|pn:|", 0, 5) != 0)
                    {
                        if (string.Compare(str, i + 9, "|voice|", 0, 5) == 0) // Voice tag ref add
                        {
                            for (int j = i; j < strLen; ++j)
                            {
                                if (j + 3 < strLen && str[j] == '<' && str[j + 1] == '/' && str[j + 2] == 'a' && str[j + 3] == '>')
                                {
                                    hudVoiceRefList.Add(str.Substring(i, j - i + 4));
                                    i = j + 3;
                                    break;
                                }
                            }
                        }
                        else // Item tag ref add
                        {
                            for (int j = i; j < strLen; ++j)
                            {
                                if (str[j] == '>')
                                {
                                    for (int k = j; k < strLen; ++k)
                                    {
                                        if (k + 3 < strLen && str[k] == '<' && str[k + 1] == '/' && str[k + 2] == 'a' && str[k + 3] == '>')
                                        {
                                            string itemName = str.Substring(j + 1, k - j - 1);
                                            if (hudItemRefList.ContainsKey(itemName))
                                            {
                                                if (hudItemDupRefList.ContainsKey(itemName))
                                                    ++hudItemDupRefList[itemName];
                                                else
                                                    hudItemDupRefList[itemName] = 1;
                                                itemName = itemName + hudItemDupRefList[itemName];
                                            }
                                            hudItemRefList[itemName] = str.Substring(i, k - i + 4);
                                            i = k + 3;
                                            break; // 3nd loop
                                        }
                                    }
                                    break; // 2nd loop
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    string RestoreItemTagWithRef(string str)
    {
        if (!string.IsNullOrEmpty(str))
        {
            // Items
            int refIdx = hudItemRefList.Count;
            if (refIdx > 0)
            {
                StringBuilder sb = new StringBuilder();
                int strLen = str.Length;
                for (int i = strLen - 1; i >= 0; --i)
                {
                    bool found = false;
                    if (str[i] == ']')
                    {
                        for (int j = i - 1; j >= 0; --j)
                        {
                            if (str[j] == '[')
                            {
                                string currItemName = str.Substring(j, i - j + 1);
                                if (hudItemRefList.ContainsKey(currItemName))
                                {
                                    if (hudItemDupRefList.ContainsKey(currItemName))
                                    {
                                        int currIdx = hudItemDupRefList[currItemName];
                                        if (currIdx == 0)
                                            sb.Insert(0, hudItemRefList[currItemName]);
                                        else
                                            sb.Insert(0, hudItemRefList[currItemName + currIdx]);

                                        --hudItemDupRefList[currItemName];
                                    }
                                    else
                                        sb.Insert(0, hudItemRefList[currItemName]);
                                    i = j;
                                    found = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (!found)
                        sb.Insert(0, str[i]);
                }
                str = sb.ToString();
            }
            // Voice
            refIdx = hudVoiceRefList.Count;
            if (refIdx > 0)
            {
                StringBuilder sb = new StringBuilder();
                int strLen = str.Length;
                --refIdx;
                for (int i = strLen - 1; i >= 0; --i)
                {
                    if (i - 3 >= 0 && str[i] == ')' && str[i - 1] == '\'' && str[i - 3] == '(')
                    {
                        sb.Insert(0, hudVoiceRefList[refIdx--]);
                        i -= 3;
                    }
                    else
                        sb.Insert(0, str[i]);
                }
                return sb.ToString();
            }
        }
        return str;
    }

    int GetPrefixEndIndex(string str, int idx, int strLen, ref int newIIdx)
    {
        if (idx + 2 < strLen && str[idx] == '[')
        {
            int sIdx = idx + 1;
            // Check for channel
            if (IsChannelTagValid(str, sIdx))
            {
                int lastIdx = sIdx;
                for (int i = sIdx; i < strLen; ++i)
                {
                    char currChar = str[i];
                    if (currChar == ']')
                    {
                        if (i + 8 < strLen && str[i + 1] == '<' && str[i + 2] == '/' && str[i + 3] == 'c' && str[i + 4] == 'o' &&
                           str[i + 5] == 'l' && str[i + 6] == 'o' && str[i + 7] == 'r' && str[i + 8] == '>')
                        {
                            i += 8;
                        }
                        lastIdx = i;
                    }
                    else if (currChar == '：')
                    {
                        newIIdx = lastIdx;
                        return i;
                    }
                }
            }
        }
        return -1;
    }

    bool IsChannelTagValid(string str, int idx)
    {
        //return (string.Compare(str, idx, localizedStrWorld, 0, 1) == 0 || string.Compare(str, idx, localizedStrGuild, 0, 1) == 0 ||
        //        string.Compare(str, idx, localizedStrParty, 0, 1) == 0 || string.Compare(str, idx, localizedStrWhisper, 0, 1) == 0 ||
        //        string.Compare(str, idx, localizedStrFaction, 0, 1) == 0 || string.Compare(str, idx, localizedStrSystem, 0, 1) == 0);
        return true;
    }

    void GetMsgTypePrefixAndSuffix(string str, int idx, ref string prefix, ref string suffix)
    {
        int sIdx = idx + 1;
        /*if (string.Compare(str, sIdx, localizedStrWorld, 0, 1) == 0)
        {
            prefix = "<wr>";
            suffix = "</wr>";
        }
        else if (string.Compare(str, sIdx, localizedStrFaction, 0, 1) == 0)
        {
            prefix = "<co>";
            suffix = "</co>";
        }
        else if (string.Compare(str, sIdx, localizedStrGuild, 0, 1) == 0)
        {
            prefix = "<g>";
            suffix = "</g>";
        }
        else if (string.Compare(str, sIdx, localizedStrParty, 0, 1) == 0)
        {
            prefix = "<p>";
            suffix = "</p>";
        }
        else if (string.Compare(str, sIdx, localizedStrWhisper, 0, 1) == 0)
        {
            prefix = "<wh>";
            suffix = "</wh>";
        }
        else if (string.Compare(str, sIdx, localizedStrSystem, 0, 1) == 0)
        {
            prefix = "<sy>";
            suffix = "</sy>";
        }*/
    }

    string RepairBrokenHTStyle(string currStr, string notTrimStr, string youStr, string toStr)
    {
        // Repair name tags
        StringBuilder sb = new StringBuilder(currStr);
        int sbLen = sb.Length;
        string sbStr = sb.ToString();
        for (int i = 0; i < sbLen; ++i)
        {
            int newIIdx = 0;
            int retIdx = GetPrefixEndIndex(sbStr, i, sbLen, ref newIIdx);
            if (retIdx != -1)
            {
                i = newIIdx;
                int pCnt = retIdx - i - 3;
                if (pCnt > 0)
                {
                    string pname = sbStr.Substring(i + 2, pCnt);
                    // For whisper
                    int findTo = pname.IndexOf(toStr), offset = 0;
                    if (findTo != -1)
                    {
                        offset = pname.Length - findTo + 1;
                        pname = pname.Substring(0, findTo - 1);
                        if (pname.Equals(youStr)) // Replace "you" with real name
                            pname = GameInfo.gLocalPlayer.Name;
                    }
                    sb.Insert(i + 2, "<a name=\"|pn:|");
                    sb.Insert(i + 16, pname);
                    int pnameLen = pname.Length;
                    sb.Insert(i + 16 + pnameLen, "\">");
                    retIdx += 16 + pnameLen;
                    sb.Insert(retIdx - 1 - offset, "</a>");
                    sbLen = sb.Length; // Update to latest
                    sbStr = sb.ToString();
                    i += 20 + pnameLen + pnameLen;
                }
            }
        }
        // Repair color tags
        sbLen = sb.Length;
        int notTrimLen = notTrimStr.Length;
        bool prefixFound = false, isColorFix = false;
        for (int i = 0; i < sbLen; ++i)
        {
            if (i + 1 < sbLen && sb[i] == '<')
            {
                if (sb[i + 1] == '/' && i + 7 < sbLen)
                {
                    if (sb[i + 2] == 'c' && sb[i + 3] == 'o' && sb[i + 4] == 'l' &&
                       sb[i + 5] == 'o' && sb[i + 6] == 'r' && sb[i + 7] == '>')
                    {
                        if (prefixFound) break;
                        int tmpLen = notTrimLen - sbLen + i;
                        for (int j = tmpLen; j >= 0; --j)
                        {
                            if (notTrimStr[j] == '>')
                            {
                                for (int k = j; k >= 0; --k)
                                {
                                    if (notTrimStr[k] == '<')
                                    {
                                        string colorStr = notTrimStr.Substring(k, j - k + 1);
                                        int diff = tmpLen - j + 1;
                                        int insertIdx = (i - diff >= 0) ? diff : 0;
                                        sb.Insert(insertIdx, colorStr);
                                        isColorFix = true;
                                        break;
                                    }
                                }
                                if (isColorFix) break;
                            }
                        }
                        if (isColorFix) break;
                    }
                }
                else if (sb[i + 1] == 'c' && i + 6 < sbLen && sb[i + 2] == 'o' && sb[i + 3] == 'l' &&
                        sb[i + 4] == 'o' && sb[i + 5] == 'r' && sb[i + 6] == '=')
                {
                    if (prefixFound) break;
                    else prefixFound = true;
                }
            }
        }
        if (isColorFix)
        {
            sbLen = sb.Length;
            sbStr = sb.ToString();
        }
        // Repair channel tags
        if (!IsChannelTagValid(sbStr, 1))
        {
            // Trim text if name tag is broken
            int occurTag = currStr.IndexOf('[');
            int occurColon = currStr.IndexOf('：');
            if (occurColon != -1)
            {
                if ((occurTag != -1 && occurTag > occurColon) || occurTag == -1)
                    sb.Remove(occurColon, 2);
            }

            // Search message before trim for message type tag
            string prefix = "", suffix = "";
            for (int i = 0; i < notTrimLen; ++i)
            {
                if (notTrimStr[i] == '[')
                {
                    GetMsgTypePrefixAndSuffix(notTrimStr, i, ref prefix, ref suffix);
                    sb.Insert(0, prefix);
                    sbLen = sb.Length;
                    string newStr = sb.ToString();
                    for (int j = 0; j < sbLen; ++j)
                    {
                        if (newStr[j] == '\n' || j == sbLen - 1)
                        {
                            sb.Insert(j + 1, suffix);
                            break;
                        }
                    }
                    break;
                }
            }
        }
        return sb.ToString();
    }

    bool IsColorPrefixExist(string str, int strLen, int startIdx, int lastIdx)
    {
        if (str[startIdx] == '[')
        {
            for (int i = startIdx - 1; i >= lastIdx; --i)
            {
                if (str[i] == '>')
                {
                    for (int j = i - 1; j >= lastIdx; --j)
                    {
                        if (str[j] == '<' && j + 6 < strLen && str[j + 1] == 'c' && str[j + 2] == 'o' &&
                           str[j + 3] == 'l' && str[j + 4] == 'o' && str[j + 5] == 'r' && str[j + 6] == '=')
                            return true;
                    }
                }
            }
        }
        return false;
    }

    string AppendHTStyleExp(string strToAppend)
    {
        string result = strToAppend;
        int strLen = result.Length;
        string prefix = "", suffix = "";
        char localTo = toStr.ToCharArray()[0];
        int lastEndIdx = 0;
        for (int i = 0; i < strLen; ++i)
        {
            if (result[i] == '\n')
                lastEndIdx = i;
            if (result[i] == '[')
            {
                bool isInsert = true;
                if (IsColorPrefixExist(result, strLen, i, lastEndIdx))
                    isInsert = false;
                if (isInsert)
                {
                    GetMsgTypePrefixAndSuffix(result, i, ref prefix, ref suffix);
                    result = result.Insert(i, prefix);
                    i += prefix.Length;
                    strLen = result.Length;
                }
                for (int j = i; j < strLen; ++j)
                {
                    if (result[j] == ']')
                    {
                        if (isInsert)
                        {
                            result = result.Insert(j + 1, suffix);
                            j += suffix.Length;
                            strLen = result.Length;
                        }
                        i = j;
                        lastEndIdx = j + 1;
                        break;
                    }
                }
            }
            else if ((result[i] == localTo) || (result[i] == '：'))
            {
                result = result.Insert(i, prefix);
                i += prefix.Length;
                strLen = result.Length;
                for (int j = i; j < strLen; ++j)
                {
                    if ((j + 1 < strLen && result[j] == '\n' && result[j + 1] == '[') || j == strLen - 1)
                    {
                        int insertIdx = (j == strLen - 1) ? j + 1 : j;
                        result = result.Insert(insertIdx, suffix);
                        j += suffix.Length;
                        strLen = result.Length;
                        i = j;
                        lastEndIdx = insertIdx;
                        break;
                    }
                }
            }
        }
        return result;
    }

    public bool UpdateChatroomMiniLog(MessageType currentChannel, List<ChatMessage> chatMessages, int latestMsgIdx)
    {
        HyperText currentHyperTxt = hyperTxtChannels[(byte)currentChannel];

        bool startNextTimer = false;
        currentHyperTxt.text = ""; // Clear hypertext
        HyperTextProcessor textProc = new HyperTextProcessor();
        StringBuilder msgBuilder = new StringBuilder();
        for (int i = latestMsgIdx; i >= 0; --i)
        {
            string previousMsg = currentHyperTxt.text; // Compiled previous message
            ChatMessage chatMsg = chatMessages[i];
            msgBuilder.Length = 0; // Clear StringBuilder
            msgBuilder.AppendFormat("[{0}]", GetLocalizedCharMsgType(currentChannel));
            if (currentChannel != MessageType.System)
            {
                string senderStr = chatMsg.mSender;
                if (!string.IsNullOrEmpty(senderStr))
                {
                    msgBuilder.Append(" <a name=\"|pn:|");
                    msgBuilder.Append(senderStr);
                    msgBuilder.Append("\">");
                    //msgBuilder.Append((GameInfo.gLocalPlayer.Name == senderStr && msgType == MessageType.Whisper) ?
                    //             youStr : senderStr);
                    msgBuilder.Append(senderStr);
                    msgBuilder.Append("</a>");
                }
            }
            if (currentChannel == MessageType.Whisper)
            {
                msgBuilder.Append(toStr);
                //msgBuilder.Append((GameInfo.gLocalPlayer.Name == currMsg.mWhisperTo) ? youStr : currMsg.mWhisperTo);
                msgBuilder.Append(chatMsg.mWhisperTo);
                msgBuilder.Append(sayStr);
            }
            string messageTxt = chatMsg.mMessage;
            if (chatMsg.mIsVoiceChat)
            {
                string[] voiceSplit = messageTxt.Split(';');
                StringBuilder sb = new StringBuilder();
                sb.Append("<a name=\"");
                sb.Append("|voice|");
                sb.Append(voiceSplit[0]);
                sb.Append("\">(");
                sb.Append(voiceSplit[1]);
                sb.Append("\")</a>");
                messageTxt = sb.ToString();
            }
            // Message text to append
            string currentMsg = string.Format("{0}：{1}", msgBuilder.ToString(), messageTxt);
            currentHyperTxt.text = (i != latestMsgIdx) ? string.Format("{0}\n{1}", currentMsg, previousMsg) : currentMsg;

            Canvas.ForceUpdateCanvases(); // Update to read correct rect transfrom
            Vector2 extents = currentHyperTxt.rectTransform.rect.size;
            var settings = currentHyperTxt.GetGenerationSettings(extents);
            textProc.InputText = currentHyperTxt.text; // Process text with text processor
            currentHyperTxt.cachedTextGenerator.Populate(textProc.OutputText, settings);
            int lineCount = currentHyperTxt.cachedTextGenerator.lineCount;
            if (lineCount == 14)
                break;
            else if (lineCount > 14)
            {
                // Trim text to 2 lines
                int startCharIdx = currentHyperTxt.cachedTextGenerator.lines[lineCount-14].startCharIdx;
                textProc.InputText = currentHyperTxt.text; // Process text with text processor
                string newText = textProc.OutputText.Substring(startCharIdx);
                StoreRefToItemTag(currentHyperTxt.text); // Store item tags first
                string beforeTrimTxt = currentHyperTxt.text; // Store text before trimming
                currentHyperTxt.text = newText;
                currentHyperTxt.text = RestoreItemTagWithRef(currentHyperTxt.text);
                currentHyperTxt.text = RepairBrokenHTStyle(currentHyperTxt.text, beforeTrimTxt, youStr, toStr); // Repair if tag is broken
                startNextTimer = true;
                break;
            }
        }
        currentHyperTxt.text = AppendHTStyleExp(currentHyperTxt.text); // Append tag if missing
        return startNextTimer;
    }

    public void OnValueChangedChannels(int index)
    {
        if (CurrentChannel == (MessageType)index)
            return;

        openChatroomButtons[(byte)CurrentChannel].SetActive(false);

        carouselToggler.CenterOnToggledIndex(index);
        CurrentChannel = (MessageType)index;
        openChatroomButtons[index].SetActive(true);

        //Debug.LogFormat("Current Channel: {0}", CurrentChannel.ToString());
    }

    public void OnClickOpenChatroom()
    {
        UIManager.SetWidgetActive(HUDWidgetType.Chatroom, true);
    }
}
