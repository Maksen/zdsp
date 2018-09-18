using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zealot.Client.Entities;
using Zealot.Repository;

public class HUD_PortraitFunctions : MonoBehaviour
{
    [SerializeField] GameObject clickBlocker;
    [SerializeField] Text charNameText;
    [SerializeField] Text guildNameText;

    [Header("Party")]
    [SerializeField] GameObject followMemberBtn;
    [SerializeField] GameObject cancelFollowMemberBtn;
    [SerializeField] GameObject setAsLeaderBtn;

    private PlayerGhost localPlayer;
    private string playerName;
    private List<RaycastResult> raycastHits = new List<RaycastResult>();

    private void Start()
    {
        int parentLayer = transform.parent.gameObject.layer;
        if (gameObject.layer != parentLayer)
            ClientUtils.SetLayerRecursively(gameObject, parentLayer);
    }

    public void Init(string charName, string guildName)
    {
        gameObject.SetActive(true);
        localPlayer = GameInfo.gLocalPlayer;
        playerName = charName;

        charNameText.text = charName;
        guildNameText.text = !string.IsNullOrEmpty(guildName) ? guildName : GUILocalizationRepo.GetLocalizedString("com_noguild");

        SetPartyFunctions();
    }

    private void SetPartyFunctions()
    {
        followMemberBtn.SetActive(false);
        cancelFollowMemberBtn.SetActive(false);
        setAsLeaderBtn.SetActive(false);

        if (localPlayer == null)
            return;
        if (localPlayer.IsInParty())
        {
            PartyStatsClient partyStats = localPlayer.PartyStats;
            if (partyStats.IsMember(playerName) && partyStats.IsMemberOnline(playerName))  // this player is my party member
            {
                if (string.IsNullOrEmpty(PartyFollowTarget.TargetName) || PartyFollowTarget.TargetName != playerName)
                    followMemberBtn.SetActive(true);
                else
                    cancelFollowMemberBtn.SetActive(true);
                setAsLeaderBtn.SetActive(partyStats.IsLeader(localPlayer.Name)); // show change leader button if I'm leader
            }
        }
    }

    public void OnClickChangePartyLeader()
    {
        ClosePanel();
        Dictionary<string, string> param = new Dictionary<string, string>();
        param.Add("name", playerName);
        string message = GUILocalizationRepo.GetLocalizedString("party_confirmChangeLeader", param);
        UIManager.OpenYesNoDialog(message, () => RPCFactory.CombatRPC.ChangePartyLeader(playerName));
    }

    public void OnClickFollowMember()
    {
        ClosePanel();
        RPCFactory.CombatRPC.FollowPartyMember(playerName);
    }

    public void OnClickCancelFollowMember()
    {
        ClosePanel();
        if (localPlayer != null && localPlayer.PartyStats != null)
            localPlayer.PartyStats.StopFollowTarget();
    }

    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }

    private void Update()
    {
        Vector2 pointerPosition = Vector2.zero;

#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetMouseButtonUp(0))
            pointerPosition = Input.mousePosition;
#elif UNITY_IOS || UNITY_ANDROID
        int touchCount = Input.touchCount;
        for (int i = 0; i < touchCount; i++)
        {
            if (Input.GetTouch(i).phase == TouchPhase.Ended)
            {
                pointerPosition = Input.GetTouch(i).position;
                break;
            }
        }
#endif
        if (pointerPosition != Vector2.zero)
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = pointerPosition;
            EventSystem.current.RaycastAll(eventData, raycastHits);
            bool clickOutside = true;
            for (int i = 0; i < raycastHits.Count; i++)
            {
                if (raycastHits[i].gameObject == clickBlocker)
                {
                    clickOutside = false;
                    break;
                }
            }
            if (clickOutside)
                ClosePanel();
        }
    }
}