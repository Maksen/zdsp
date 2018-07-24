using System;
using System.Collections.Generic;
using UIAddons;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Client.Entities;
using Zealot.Common;
using Zealot.Repository;

public class UI_Party_MyParty : MonoBehaviour
{
    [SerializeField] UI_Party_Avatar[] avatars;
    [SerializeField] Text locationNameText;
    [SerializeField] Text levelRangeText;
    [SerializeField] GameObject leaderButtonsObj;
    [SerializeField] GameObject memberButtonsObj;
    [SerializeField] Button inviteButton;
    [SerializeField] Button recruitButton;
    [SerializeField] Toggle autoFollowCheckbox;

    private PlayerGhost player;
    private PartyStatsClient partyStats;
    private bool isDirty = false;

    public void Init()
    {
        gameObject.SetActive(true);
        player = GameInfo.gLocalPlayer;

        if (player.PartyStats != null)
        {
            partyStats = player.PartyStats;
            SetupBottomPanel();
            SetupMemberAvatars();
        }
        else
        {
            Debug.LogError("player partystats is null!");
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        CleanUp();
    }

    public void CleanUp()
    {
        SaveAutoFollowSetting();
        ClearAvatars();
        player = null;
        partyStats = null;
    }

    private void SaveAutoFollowSetting()
    {
        if (isDirty && partyStats != null)
        {
            RPCFactory.CombatRPC.SaveAutoFollowSetting(partyStats.GetPartyMember(player.Name).rejectFollow);
            isDirty = false;
        }
    }

    public void OnMemberOffline(PartyMember member)
    {
        UI_Party_Avatar avatar = avatars[member.slotIdx];
        avatar.ClosePortraitFunctions();
    }

    private void ClearAvatars()
    {
        for (int i = 0; i < avatars.Length; i++)
        {
            avatars[i].gameObject.SetActive(false);
            avatars[i].CleanUp();
        }
    }

    private void SetupMemberAvatars()
    {
        ClearAvatars();
        var memberList = partyStats.GetPartyMemberList().Values;
        foreach (var member in memberList)
            AddPartyMember(member);
    }

    public void AddPartyMember(PartyMember member)
    {
        if (partyStats != null)
        {
            bool isLeader = partyStats.IsLeader(player.Name);
            bool showLeaderIcon = member.name == partyStats.leader;
            bool showKickBtn = isLeader && member.name != partyStats.leader;
            avatars[member.slotIdx].gameObject.SetActive(true);
            avatars[member.slotIdx].Init(this, member, showLeaderIcon, showKickBtn);
            SetButtonsInteractable(partyStats.MemberCount());
        }
    }

    public void UpdatePartyMember(PartyMember member)
    {
        UI_Party_Avatar avatar = avatars[member.slotIdx];
        if (avatar != null)
            avatar.UpdateAvatar(this, member);
    }

    public void RemovePartyMember(PartyMember member)
    {
        UI_Party_Avatar avatar = avatars[member.slotIdx];
        if (avatar != null)
        {
            avatar.CleanUp();
            avatar.gameObject.SetActive(false);
        }
        SetButtonsInteractable(partyStats.MemberCount());
    }

    #region Bottom panel setup
    private void SetupBottomPanel()
    {
        if (partyStats != null)
        {
            ShowLeaderMemberPanel(player.IsPartyLeader());
            SetPartySettingsText(partyStats.mPartySetting);
            SetButtonsInteractable(partyStats.MemberCount());
            InitAutoFollowCheckBox(!partyStats.GetPartyMember(player.Name).rejectFollow);
        }
    }

    private void SetButtonsInteractable(int memberCount)
    {
        inviteButton.interactable = memberCount < PartyData.MAX_MEMBERS;
        recruitButton.interactable = memberCount < PartyData.MAX_MEMBERS;
    }

    private void ShowLeaderMemberPanel(bool isLeader)
    {
        leaderButtonsObj.SetActive(isLeader);
        memberButtonsObj.SetActive(!isLeader);
    }

    public void SetPartySettingsText(PartySetting setting)
    {
        locationNameText.text = PartyRepo.GetLocationName(setting.locationId);
        levelRangeText.text = PartyRepo.FormatLevelRange(setting.minLevel, setting.maxLevel);
    }

    private void InitAutoFollowCheckBox(bool isOn)
    {
        autoFollowCheckbox.onValueChanged.RemoveListener(OnToggleAutoFollow);
        autoFollowCheckbox.isOn = isOn;
        CustomToggle customToggle = autoFollowCheckbox.GetComponent<CustomToggle>();
        customToggle.offState.SetActive(!autoFollowCheckbox.isOn);
        customToggle.onState.SetActive(autoFollowCheckbox.isOn);
        autoFollowCheckbox.onValueChanged.AddListener(OnToggleAutoFollow);
    }

    private void OnToggleAutoFollow(bool isOn)
    {
        if (partyStats != null)
        {
            partyStats.GetPartyMember(player.Name).rejectFollow = !isOn;
            isDirty = true;
        }
    }

    #endregion

    public void OnUpdatePartyLeader(string leaderName)
    {
        for (int i = 0; i < avatars.Length; i++)
            avatars[i].OnUpdateLeader(leaderName);
        if (player != null)
            ShowLeaderMemberPanel(player.IsPartyLeader());
    }

    public void OnClickKickMember(PartyMember member)
    {
        Dictionary<string, string> param = new Dictionary<string, string>();
        param.Add("name", member.GetName());
        string message = GUILocalizationRepo.GetLocalizedString("party_confirmKick", param);
        UIManager.OpenYesNoDialog(message, () => RPCFactory.CombatRPC.KickPartyMember(member.name));
    }

    public void OnClickLeaveParty()
    {
        string sysMsgName = player.IsPartyLeader() ? "party_confirmDisband" : "party_confirmLeave";
        string message = GUILocalizationRepo.GetLocalizedString(sysMsgName);
        UIManager.OpenYesNoDialog(message, () =>
        {
            RPCFactory.CombatRPC.LeaveParty();
            isDirty = false;  // prevent saving setting since already leaving party
        });
    }

    public void OnClickSettings()
    {
        UIManager.OpenDialog(WindowType.DialogPartySettings);
    }

    public void OnClickRecruitMembers()
    {
        RPCFactory.CombatRPC.SendPartyRecruitment();
    }

    public void OnClickFollowMe()
    {
        if (partyStats != null && partyStats.MemberCount() > 1)
            RPCFactory.CombatRPC.InviteToFollow();
    }

    public void OnClickRequestList()
    {
        UIManager.OpenDialog(WindowType.DialogPartyRequestList);
    }

    public void OnClickInviteMember()
    {
        if (player == null)
            return;

        if (player.IsPartyLeader())
        {
            if (partyStats != null && !partyStats.IsPartyFull())
                UIManager.OpenDialog(WindowType.DialogPartyInvite);
            else
                UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("sys_party_PartyFull"));
        }
        else
            UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("sys_party_OnlyLeaderCanInvite"));
    }

}