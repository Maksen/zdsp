using System.Collections.Generic;
using UnityEngine;
using Zealot.Common;

public class HUD_PartyPortrait : MonoBehaviour
{
    [SerializeField] Transform iconsTransform;
    [SerializeField] GameObject partyPortraitPrefab;
    [SerializeField] GameObject addButton;

    private List<HUD_PartyPortraitInfo> portraitList = new List<HUD_PartyPortraitInfo>();
    private int memberCount = 0;

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        for (int i = 0; i < PartyData.MAX_MEMBERS; i++)
        {
            GameObject portrait = ClientUtils.CreateChild(iconsTransform, partyPortraitPrefab);
            HUD_PartyPortraitInfo info = portrait.GetComponent<HUD_PartyPortraitInfo>();
            portrait.SetActive(false);
            portraitList.Add(info);
        }
    }

    public void ResetAll()
    {
        // self
        GameObject playerPortrait = UIManager.GetWidget(HUDWidgetType.PlayerPortrait);
        if (playerPortrait != null)
            playerPortrait.GetComponent<HUD_PlayerPortrait>().SetPartyLeader(false);

        // other members
        for (int i = 0; i < portraitList.Count; i++)
        {
            portraitList[i].gameObject.SetActive(false);
            portraitList[i].CleanUp();
        }
        addButton.SetActive(true);
        memberCount = 0;
    }

    public void AddPartyMember(PartyMember member)
    {
        if (GameInfo.gLocalPlayer.Name == member.name)  // ignore self
            return;

        HUD_PartyPortraitInfo portrait = portraitList[member.slotIdx];
        if (!portrait.gameObject.activeInHierarchy)  // ensure is inactive, prevent adding membercount when changed scene
        {
            portrait.Init(member);
            portrait.gameObject.SetActive(true);
            memberCount++;
            if (memberCount >= PartyData.MAX_MEMBERS - 1)
                addButton.SetActive(false);
        }
    }

    public void UpdatePartyMember(PartyMember member)
    {
        if (GameInfo.gLocalPlayer.Name == member.name)  // ignore self
            return;

        HUD_PartyPortraitInfo portrait = portraitList[member.slotIdx];
        if (portrait.gameObject.activeInHierarchy)  // ensure is active member
            portrait.Init(member);
    }

    public void RemovePartyMember(PartyMember member)
    {
        if (GameInfo.gLocalPlayer.Name == member.name)  // ignore self
            return;

        HUD_PartyPortraitInfo portrait = portraitList[member.slotIdx];
        if (portrait.gameObject.activeInHierarchy)  // ensure is active member
        {
            portrait.gameObject.SetActive(false);
            portrait.CleanUp();
            memberCount--;
            if (memberCount < PartyData.MAX_MEMBERS - 1)
                addButton.SetActive(true);
        }
    }

    public void SetPartyLeader(string leaderName)
    {
        // self
        GameObject playerPortrait = UIManager.GetWidget(HUDWidgetType.PlayerPortrait);
        if (playerPortrait != null)
            playerPortrait.GetComponent<HUD_PlayerPortrait>().SetPartyLeader(GameInfo.gLocalPlayer.Name == leaderName);

        // other members
        for (int i = 0; i < portraitList.Count; i++)
        {
            if (portraitList[i].gameObject.activeInHierarchy)
                portraitList[i].SetLeader(leaderName);
        }
    }

    public void OnMemberOffline(PartyMember member)
    {
        HUD_PartyPortraitInfo portrait = portraitList[member.slotIdx];
        portrait.ClosePortraitFunctions();
    }
}
