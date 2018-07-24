using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Repository;

public class UI_Party_PartyInfo : MonoBehaviour
{
    [SerializeField] Text locationText;
    [SerializeField] Text levelRangeText;
    [SerializeField] Text leaderNameText;
    [SerializeField] Text memberCountText;
    [SerializeField] Text partyNotesText;
    [SerializeField] Button joinButton;

    private UI_Party_Join parent;
    private int partyId;

    public void Init(UI_Party_Join myParent, PartyInfo info)
    {
        parent = myParent;
        partyId = info.partyId;
        locationText.text = PartyRepo.GetLocationName(info.locationId);
        levelRangeText.text = PartyRepo.FormatLevelRange(info.minLevel, info.maxLevel);
        leaderNameText.text = info.leader;
        memberCountText.text = info.memberCount + "/" + PartyData.MAX_MEMBERS;
        partyNotesText.text = info.notes;
        joinButton.onClick.AddListener(OnClickJoin);
    }

    public void OnClickJoin()
    {
        parent.OnClickJoinParty(partyId);
        joinButton.interactable = false;
    }
}
