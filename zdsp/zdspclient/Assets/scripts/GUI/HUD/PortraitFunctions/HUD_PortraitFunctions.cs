using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zealot.Client.Entities;
using Zealot.Common.Entities.Social;
using Zealot.Common.Entities;
using Zealot.Common;
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

    [Header("Social")]
    [SerializeField] GameObject addToFriendListBtn;
    [SerializeField] GameObject addToBlackListBtn;

    private PlayerGhost localPlayer;
    private string playerName;
    private List<RaycastResult> raycastHits = new List<RaycastResult>();

    private void Start()
    {
        int parentLayer = transform.parent.gameObject.layer;
        if (gameObject.layer != parentLayer)
            ClientUtils.SetLayerRecursively(gameObject, parentLayer);
    }

    public string PlayerName { get { return playerName; } }

    public void Init(string charName, string guildName)
    {
        gameObject.SetActive(true);
        localPlayer = GameInfo.gLocalPlayer;
        playerName = charName;

        charNameText.text = charName;
        guildNameText.text = !string.IsNullOrEmpty(guildName) ? guildName : GUILocalizationRepo.GetLocalizedString("com_noguild");

        SetPartyFunctions();
        SetSocialFunctions();
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

    const bool SocialHideALL = false;
    private void SetSocialFunctions()
    {
        if (localPlayer == null)
            return;

        var stats = localPlayer.SocialStats;
        if (stats == null)
            return;

        var data = stats.data;
        if (data == null)
            return;

        if (SocialHideALL)
        {
            if (data.goodFriendStates != null && data.blackFriendStates != null)
            {
                bool contains = data.goodFriendStates.ContainsPlayerName(playerName) || data.blackFriendStates.ContainsPlayerName(playerName);
                addToFriendListBtn.SetActive(!contains);
                addToBlackListBtn.SetActive(!contains);
            }
            else
            {
                addToFriendListBtn.SetActive(false);
                addToBlackListBtn.SetActive(false);
            }
        }
        else
        {
            if (data.goodFriendStates != null)
                addToFriendListBtn.SetActive(!data.goodFriendStates.ContainsPlayerName(playerName));
            if (data.blackFriendStates != null)
                addToBlackListBtn.SetActive(!data.blackFriendStates.ContainsPlayerName(playerName));
        }
    }

    bool socialLocked = false;

    public void SocialUnlock()
    {
        socialLocked = false;
    }


    private SocialData GetSocialData()
    {
        SocialStats stats;
        if(localPlayer!=null)
        {
            stats = localPlayer.SocialStats;
            if (stats != null)
            {
                return stats.data;
            }
        }
        return null;
    }

    public void OnClickAddToFriendList()
    {
        SocialData data;
        if (!string.IsNullOrEmpty(playerName) && (data = GetSocialData()) != null)
        {
            if (data.blackFriendStates.ContainsPlayerName(playerName))
                SocialController.AddSystemMessage("ret_social_RaiseRequest_TargetBlacked", "name;" + playerName);
            else if (!socialLocked)
            {
                socialLocked = true;

                RPCFactory.NonCombatRPC.SocialRaiseRequest(playerName,false);
            }
        }
        ClosePanel();
    }

    public void OnClickAddToBlackList()
    {
        SocialData data;
        if (!string.IsNullOrEmpty(playerName) && (data = GetSocialData()) != null)
        {
            if (data.goodFriendStates.ContainsPlayerName(playerName))
                SocialController.AddSystemMessage("ret_social_AddBlack_RemoveGoodFriendFirst", "name;" + playerName);
            else if (!socialLocked)
            {
                socialLocked = true;
                RPCFactory.NonCombatRPC.SocialAddBlack(playerName);
            }
        }
        ClosePanel();
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