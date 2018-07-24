using Candlelight.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Repository;

public class HUD_Chatroom : MonoBehaviour
{
    // Editor Linked Gameobjects
    [SerializeField]
    ScrollRect scrollrectChatBox = null;
    [SerializeField]
    InputField inputfieldChat = null;
    [SerializeField]
    HyperText hypertxtInputfield = null;
    [SerializeField]
    GameObject gameobjBottomControl = null;
    [SerializeField]
    GameObject buttonChannels = null;
    [SerializeField]
    Toggle toggleAutoPlayVoice = null;
    [SerializeField]
    GameObject gameobjFriends = null;
    [SerializeField]
    ScrollRect scrollrectFriends = null;
    [SerializeField]
    GameObject panelEmoticon = null;
    [SerializeField]
    GameObject panelChatBag = null;
    //[SerializeField]
    //UI_CharacterBag chatBagStuff = null;
    [SerializeField]
    Text txtBagTotalpages = null;

    [Header("Prefabs")]
    [SerializeField]
    GameObject prefabChatboxMe = null;
    [SerializeField]
    GameObject prefabChatboxPlayer = null;
    [SerializeField]
    GameObject prefabChatboxSysMsger = null;
    [SerializeField]
    GameObject prefabFriendsdata = null;

    public static List<ChatMessage> chatLog = new List<ChatMessage>();
    public static int currHUDChatIdx = -1;

    List<HUD_ChatBox> chatBoxList =  new List<HUD_ChatBox>();
    Dictionary<string, HUD_ChatBox> chatBoxVoiceRefList;
    bool isChatLogDirty = false;
    MessageType currChannelToShow;
    MessageType currChannelToChat;
    string prevInputValue = "";
    bool isEditedOnInputfieldChanged = false;
    Dictionary<int, GameTimer> channelTimers;
    bool isScrollViewMouseDown = false;
    List<RaycastResult> rayResult = null;
    GraphicRaycaster gfxRaycaster = null;
    IList<string> itemCodeList = null;
    Dictionary<string, string> hudItemRefList = null;
    Dictionary<string, int> hudItemDupRefList = null;
    IList<string> hudVoiceRefList = null;
    const float totalMiniChatDelay = 0.5f;
    float currMiniChatDelay = 0;
    GameTimer friendRefreshTimer;

    // Localized Strings
    string toStr = "";
    string youStr = "";
    string sayStr = "";
    string localizedStrWorld = "";
    string localizedStrFaction = "";
    string localizedStrGuild = "";
    string localizedStrParty = "";
    string localizedStrRecruit = "";
    string localizedStrWhisper = "";
    string localizedStrSystem = "";
    string localizedCharWorld = "";
    string localizedCharFaction = "";
    string localizedCharGuild = "";
    string localizedCharParty = "";
    string localizedCharRecruit = "";
    string localizedCharWhisper = "";
    string localizedCharFactionDragon = "";
    string localizedCharFactionTiger = "";
    string localizedCharFactionPig = "";
    string localizedCharFactionLeopard = "";

    // Use this for initialization
    void Awake()
    {
        rayResult = new List<RaycastResult>(); // Create list to receive all raycast results
        gfxRaycaster = UIManager.UIHud.GetComponent<GraphicRaycaster>();
    }

    public void Init()
    {
        chatBoxList = new List<HUD_ChatBox>();
        chatBoxVoiceRefList = new Dictionary<string, HUD_ChatBox>();

        itemCodeList = new List<string>(); // For storing item code when filter text
        // For storing ref when trimming text in HUD mini chat
        hudItemRefList = new Dictionary<string, string>();
        hudItemDupRefList = new Dictionary<string, int>();
        hudVoiceRefList = new List<string>();

        // Channel cooldown timers
        channelTimers = new Dictionary<int, GameTimer>();
        channelTimers.Add((int)MessageType.World, null);
        channelTimers.Add((int)MessageType.Faction, null);
        channelTimers.Add((int)MessageType.Guild, null);
        channelTimers.Add((int)MessageType.Party, null);
        channelTimers.Add((int)MessageType.Whisper, null);

        // Init localized string
        //youStr = GUILocalizationRepo.GetLocalizedString("com_You");
        toStr = GUILocalizationRepo.GetLocalizedString("com_To");
        sayStr = GUILocalizationRepo.GetLocalizedString("com_Say");
        localizedStrWorld = GUILocalizationRepo.GetLocalizedString("com_World");
        localizedStrFaction = GUILocalizationRepo.GetLocalizedString("com_Faction");
        localizedStrGuild = GUILocalizationRepo.GetLocalizedString("com_Guild");
        localizedStrParty = GUILocalizationRepo.GetLocalizedString("com_Party");
        localizedStrRecruit = GUILocalizationRepo.GetLocalizedString("com_Recruit");
        localizedStrWhisper = GUILocalizationRepo.GetLocalizedString("com_Whisper");
        localizedStrSystem = GUILocalizationRepo.GetLocalizedString("com_System");
        localizedCharWorld = localizedStrWorld[0].ToString();
        localizedCharFaction = localizedStrFaction[0].ToString();
        localizedCharGuild = localizedStrGuild[0].ToString();
        localizedCharParty = localizedStrParty[0].ToString();
        localizedCharRecruit = localizedStrRecruit[0].ToString();
        localizedCharWhisper = localizedStrWhisper[0].ToString();
        localizedCharFactionDragon = GUILocalizationRepo.GetLocalizedString("Faction_Dragon")[0].ToString();
        localizedCharFactionTiger = GUILocalizationRepo.GetLocalizedString("Faction_Tiger")[0].ToString();
        localizedCharFactionPig = GUILocalizationRepo.GetLocalizedString("Faction_Pig")[0].ToString();
        localizedCharFactionLeopard = GUILocalizationRepo.GetLocalizedString("Faction_Leopard")[0].ToString();
    }

    void OnEnable()
    {
        // Reset active UI panels
        currChannelToShow = currChannelToChat = MessageType.World;
        panelChatBag.SetActive(false);
        panelEmoticon.SetActive(false);
        gameobjFriends.SetActive(false);
        //ToggleAutoPlayVoice();
        gameobjBottomControl.SetActive(true);

        // Set to default toggle buttons for channels
        buttonChannels.GetComponent<ToggleGroup>().SetAllTogglesOff();
        Toggle[] toggleButs = buttonChannels.GetComponentsInChildren<Toggle>(true);
        foreach (Toggle t in toggleButs)
            t.isOn = false;
        toggleButs[0].isOn = true;

        // Chat input Field initialization
        CanvasRenderer txtCmptRender = inputfieldChat.textComponent.gameObject.GetComponent<CanvasRenderer>();
        txtCmptRender.SetAlpha(0);
        prevInputValue = "";
        isEditedOnInputfieldChanged = false;
        inputfieldChat.text = "";

        //InitScrollViewFriends();

        // Update to current chat log
        //UpdateChatWindowText();

        //PlayerGhost player = GameInfo.gLocalPlayer;
        //chatBagStuff.Init(OnClickChatBagItem, false);
        Dictionary<string, string> param = new Dictionary<string, string>();
        param.Add("page", ((int)InventorySlot.MAXSLOTS / 20).ToString());
        txtBagTotalpages.text = GUILocalizationRepo.GetLocalizedString("charinfo_bagpage", param);
    }

    void OnDisable()
    {
        //chatBagStuff.Cleanup();
        foreach (Transform child in scrollrectFriends.content.transform)
            Destroy(child.gameObject);
    }

    void OnDestroy()
    {
        chatBoxList.Clear();
        chatBoxList = null;
        chatBoxVoiceRefList.Clear();
        chatBoxVoiceRefList = null;
        channelTimers.Clear();
        channelTimers = null;
        rayResult.Clear();
        rayResult = null;
        gfxRaycaster = null;
        friendRefreshTimer = null;

        scrollrectChatBox = null;
        inputfieldChat = null;
        hypertxtInputfield = null;

        prefabChatboxMe = null;
        prefabChatboxPlayer = null;
        prefabChatboxSysMsger = null;
        prefabFriendsdata = null;
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
        UpdateMiniChat();
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
        scrollrectChatBox.verticalNormalizedPosition = 0; // Scroll to latest
    }

    public void UpdateMiniChat()
    {
        // Timer for HUD chat to display
        /*if (currMiniChatDelay >= 0)
            currMiniChatDelay -= Time.deltaTime;
        else
        {
            if (currHUDChatIdx < chatLog.Count - 1)
                if (UpdateChatHUDText(++currHUDChatIdx))
                    currMiniChatDelay = totalMiniChatDelay;
        }*/
    }
}
 