using Candlelight.UI;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;

public class HUD_ChatBox : MonoBehaviour
{
    [SerializeField]
    HyperText HypertxtName = null;
    [SerializeField]
    GameObject slotPortraitParent = null;
    [SerializeField]
    GameObject prefabGameiconPortrait = null;
    [SerializeField]
    GameObject ChatTxt = null;
    [SerializeField]
    HyperText HypertxtChat = null;
    [SerializeField]
    GameObject gamobjVoice = null;
    [SerializeField]
    Text txtRecordTime = null;
    [SerializeField]
    Shadow txtShadow = null;
    [SerializeField]
    GameObject voiceNotPlayed = null;
    [SerializeField]
    GameObject voicePlaying = null; 

    public string VoiceClipName { get; set; }
    
    string senderName = "";
    MessageType msgType = MessageType.World;
    int voiceDuration = 10;
    //GameIcon_Portrait gameiconPortrait = null;

    public void Init(MessageType msgType, string message, string formattedNameTxt="", string senderName="", int portraitId=0, 
                     byte jobSect=1, byte vipLvl=0, bool isVoiceChat=false, bool hasTxtShadow=false)
    {
        this.msgType = msgType;
        this.senderName = senderName;
        if (HypertxtName != null)
            HypertxtName.text = formattedNameTxt;

        if (portraitId != 0)
        {
            InitGameIconPortrait();
           // gameiconPortrait.SetData(portraitId, vipLvl, false);
            //if (HypertxtName != null)
            //    gameiconPortrait.SetOnClickCallback(() => { GameInfo.gCombat.InspectPlayer(senderName); });
            //else
            //    gameiconPortrait.SetButtonEnable(false);
        }

        if (txtShadow != null && hasTxtShadow)
            txtShadow.enabled = true;

        ChatTxt.SetActive(!isVoiceChat);
        if (gamobjVoice != null)
            gamobjVoice.SetActive(isVoiceChat);
        if (isVoiceChat)
        {
            string[] voiceNameAndTime = message.Split(';');
            VoiceClipName = voiceNameAndTime[0];
            voiceDuration = int.Parse(voiceNameAndTime[1]);
            txtRecordTime.text = string.Format("({0}\")", voiceNameAndTime[1]);
            SetPlaying(false);
        }
        else
        {
            VoiceClipName = "";
            voiceDuration = 0;
            HypertxtChat.text = message;
           // HypertxtChat.ClickedLink.AddListener((a, b) => { GameInfo.gCombat.OnClickHyperText(a, b); });
            /*if(msgType != MessageType.World && msgType != MessageType.System)
            {
                Color color;
                switch(msgType)
                {
                    case MessageType.Guild: color = new Color(191/255.0f,1,189/255.0f); break;
                    case MessageType.Party: color = new Color(140/255.0f,1,1); break;
                    case MessageType.Faction: color = new Color(181/255.0f,196/255.0f,253/255.0f); break;
                    case MessageType.Whisper: color = new Color(1,185/255.0f,1); break;
                    default: color = Color.white; break;
                }
                ChatTxt.GetComponent<Image>().color = color;
            }*/
        }
    }

    void OnDestroy()
    {
        HypertxtName = null;
        slotPortraitParent = null;
        prefabGameiconPortrait = null;
        ChatTxt = null;
        HypertxtChat = null;
        gamobjVoice = null;
        txtRecordTime = null;
        txtShadow = null;
        voiceNotPlayed = null;
        voicePlaying = null;
       // gameiconPortrait = null;
    }

    void InitGameIconPortrait()
    {
        
    }

    public void OnClickVoiceRecord()
    {
        //HUD_VoiceChat hudVoiceChat = UIManager.GetWidget(HUDWidgetType.Chat).GetComponent<HUD_VoiceChat>();
        //if (hudVoiceChat)
            //hudVoiceChat.OnClickPlayVoice(VoiceClipName);
    }

    public void OnClickWhisper()
    {
        GameInfo.gCombat.OnChatWhisper(senderName);
    }

    public void OnAutoPlayVoice()
    {
        //HUD_VoiceChat hudVoiceChat = UIManager.GetWidget(HUDWidgetType.Chat).GetComponent<HUD_VoiceChat>();
        //if (hudVoiceChat)
           // hudVoiceChat.HandleAutoPlayVoice(msgType, VoiceClipName, voiceDuration);
    }

    public void SetPlayed(bool played)
    {
        voiceNotPlayed.SetActive(!played);
    }

    public void SetPlaying(bool playing)
    {
        voicePlaying.SetActive(playing);
    }
}
