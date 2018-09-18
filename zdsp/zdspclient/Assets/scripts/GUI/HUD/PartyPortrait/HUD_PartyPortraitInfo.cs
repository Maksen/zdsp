using Kopio.JsonContracts;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Repository;

public class HUD_PartyPortraitInfo : MonoBehaviour
{
    [SerializeField] Image portraitImage;
    [SerializeField] Text levelText;
    [SerializeField] UI_ProgressBarC hpBar;
    [SerializeField] UI_ProgressBarC mpBar;
    [SerializeField] Text nameText;
    [SerializeField] GameObject deadObj;
    [SerializeField] GameObject offlineObj;
    [SerializeField] GameObject leaderObj;
    [SerializeField] Transform portraitFuncParentTransform;
    [SerializeField] GameObject portraitFuncPrefab;

    private PartyMember thisMember;
    private HUD_PortraitFunctions portraitFunc;

    public void Init(PartyMember member)
    {
        thisMember = member;
        nameText.text = member.GetName();
        levelText.text = member.level.ToString();
        SetHP(member.hp);
        SetMP(member.mp);
        SetOnline(member.online);
        SetPortrait();
    }

    private void SetHP(float hp)
    {
        if (thisMember.IsHero())
            hpBar.Value = hpBar.Max;
        else
        {
            hpBar.Value = (long)(hp * hpBar.Max);  // max is 100
            deadObj.SetActive(hp <= 0);
        }
    }

    private void SetMP(float mp)
    {
        if (thisMember.IsHero())
            mpBar.Value = mpBar.Max;
        else
            mpBar.Value = (long)(mp * mpBar.Max);  // max is 100
    }

    private void SetPortrait()
    {
        portraitImage.sprite = ClientUtils.LoadIcon(thisMember.GetPortraitPath());
    }

    public void SetLeader(string leaderName)
    {
        leaderObj.SetActive(thisMember.name == leaderName);
    }

    private void SetOnline(bool value)
    {
        offlineObj.SetActive(!value);
    }

    public void OnClickPortrait()
    {
        if (!thisMember.IsHero())
        {
            if (portraitFunc == null)
            {
                GameObject obj = ClientUtils.CreateChild(portraitFuncParentTransform, portraitFuncPrefab);
                portraitFunc = obj.GetComponent<HUD_PortraitFunctions>();
            }

            portraitFunc.Init(thisMember.name, thisMember.guildName);
        }
    }

    public void ClosePortraitFunctions()
    {
        if (portraitFunc != null && portraitFunc.gameObject.activeInHierarchy)
            portraitFunc.ClosePanel();
    }

    public void CleanUp()
    {
        thisMember = null;
        deadObj.SetActive(false);
        leaderObj.SetActive(false);
        offlineObj.SetActive(false);
        ClientUtils.DestroyChildren(portraitFuncParentTransform);
        portraitFunc = null;
    }
}
