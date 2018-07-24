using UnityEngine;
using Zealot.Common;

public class UI_Party : BaseWindowBehaviour
{
    [SerializeField] UI_Party_Join uiJoinParty;
    [SerializeField] UI_Party_MyParty uiMyParty;

    public void SetUp(bool hasParty)
    {
        if (hasParty)
        {
            uiJoinParty.Hide();
            uiMyParty.Init();
        }
        else
        {
            uiMyParty.Hide();
            uiJoinParty.Init();
        }
    }

    public override void OnRegister()
    {
        base.OnRegister();
        uiJoinParty.SetupUIElements();
    }

    public override void OnCloseWindow()
    {
        base.OnCloseWindow();
        uiJoinParty.CleanUp();
        uiMyParty.CleanUp();
    }

    public void OnGetPartyList(string result)
    {
        uiJoinParty.OnGetPartyList(result);
    }

    public void OnUpdatePartySetting(PartySetting setting)
    {
        uiMyParty.SetPartySettingsText(setting);
    }

    public void OnUpdatePartyLeader(string leaderName)
    {
        uiMyParty.OnUpdatePartyLeader(leaderName);
    }

    public void AddPartyMember(PartyMember member)
    {
        uiMyParty.AddPartyMember(member);
    }

    public void UpdatePartyMember(PartyMember member)
    {
        uiMyParty.UpdatePartyMember(member);
    }

    public void RemovePartyMember(PartyMember member)
    {
        uiMyParty.RemovePartyMember(member);
    }

    public void OnMemberOffline(PartyMember member)
    {
        uiMyParty.OnMemberOffline(member);
    }
}