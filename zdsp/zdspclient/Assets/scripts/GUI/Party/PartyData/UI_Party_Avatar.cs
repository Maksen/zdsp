using UnityEngine;
using UnityEngine.UI;
using Zealot.Client.Entities;
using Zealot.Common;
using Zealot.Repository;

public class UI_Party_Avatar : MonoBehaviour
{
    [SerializeField] Model_3DAvatar modelAvatar;
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] GameObject leaderObj;
    [SerializeField] GameObject kickButtonObj;
    [SerializeField] Transform portraitFuncTransform;
    [SerializeField] GameObject portraitFuncPrefab;
    [SerializeField] UI_DragEvent uiDragEvent;

    private UI_Party_MyParty parent;
    private PartyMember thisMember;
    private HUD_PortraitFunctions portraitFunction;

    private void Awake()
    {
        uiDragEvent.onClicked = OnClickAvatar;
    }

    public void Init(UI_Party_MyParty myParent, PartyMember member, bool showLeaderIcon, bool showKickBtn)
    {
        parent = myParent;
        thisMember = member;
        PlayerGhost localPlayer = GameInfo.gLocalPlayer;
        nameText.text = localPlayer.Name == member.name ? ClientUtils.FormatStringColor(member.name, "#59DD7BFF") : member.GetName();
        int charlevel = localPlayer.Name == member.name ? localPlayer.PlayerSynStats.Level : member.level;
        levelText.text = GUILocalizationRepo.GetLocalizedString("com_level") + charlevel;
        leaderObj.SetActive(showLeaderIcon);
        kickButtonObj.SetActive(showKickBtn);

        if (localPlayer.Name == member.name)  // self
            modelAvatar.Change(localPlayer.mEquipmentInvData, (JobType)localPlayer.PlayerSynStats.jobsect, localPlayer.mGender);
        else
        {
            if (member.IsHero())
                modelAvatar.ChangeHero(member.heroId, member.heroTier);
            else // is other player
                modelAvatar.Change(member.avatar.equipInvData, member.avatar.jobType, member.avatar.gender);
        }
    }

    public void UpdateAvatar(UI_Party_MyParty myParent, PartyMember member)
    {
        parent = myParent;
        thisMember = member;
        PlayerGhost localPlayer = GameInfo.gLocalPlayer;
        nameText.text = localPlayer.Name == member.name ? ClientUtils.FormatStringColor(member.name, "#59DD7BFF") : member.GetName();
        levelText.text = GUILocalizationRepo.GetLocalizedString("com_level") + member.level;
        if (localPlayer.Name != member.name)  // others only
        {
            if (member.IsHero())
                modelAvatar.ChangeHero(member.heroId, member.heroTier);
            else
                modelAvatar.Change(member.avatar.equipInvData, member.avatar.jobType, member.avatar.gender);
        }
    }

    public void OnClickKickMember()
    {
        parent.OnClickKickMember(thisMember);
    }

    public void OnUpdateLeader(string leaderName)
    {
        if (thisMember != null)
        {
            bool thisIsLeader = thisMember.name == leaderName;
            bool showKickBtn = GameInfo.gLocalPlayer.Name == leaderName && !thisIsLeader;  // show if local player is leader and this avatar is a member
            leaderObj.SetActive(thisIsLeader);
            kickButtonObj.SetActive(showKickBtn);
        }
        else  // in case not init yet
        {
            leaderObj.SetActive(false);
            kickButtonObj.SetActive(false);
        }
    }

    public void OnClickAvatar()
    {
        if (GameInfo.gLocalPlayer.Name != thisMember.name && !thisMember.IsHero())
        {
            if (portraitFunction == null)
            {
                GameObject obj = ClientUtils.CreateChild(portraitFuncTransform, portraitFuncPrefab);
                portraitFunction = obj.GetComponent<HUD_PortraitFunctions>();
            }

            portraitFunction.Init(thisMember.name, thisMember.guildName);
        }
    }

    public void ClosePortraitFunctions()
    {
        if (portraitFunction != null && portraitFunction.gameObject.activeInHierarchy)
            portraitFunction.ClosePanel();
    }

    public void CleanUp()
    {
        parent = null;
        thisMember = null;
        leaderObj.SetActive(false);
        kickButtonObj.SetActive(false);
        modelAvatar.Cleanup();
        ClientUtils.DestroyChildren(portraitFuncTransform);
        portraitFunction = null;
    }
}