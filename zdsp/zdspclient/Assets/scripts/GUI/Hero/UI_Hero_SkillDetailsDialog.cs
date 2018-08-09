using Kopio.JsonContracts;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Repository;

public class UI_Hero_SkillDetailsDialog : BaseWindowBehaviour
{
    [SerializeField] Transform skillIconTransform;
    [SerializeField] GameObject skillIconPrefab;
    [SerializeField] Text skillNameText;
    [SerializeField] Text skillTypeText;
    [SerializeField] Text skillPtsText;
    [SerializeField] Button levelUpBtn;

    [Header("Skill Details")]
    [SerializeField] Transform skillDetailsTransform;
    [SerializeField] ScrollRect scrollRect;
    [SerializeField] GameObject[] dataPrefabs; // 0 - level, 1 - sdg, 2 - cooldown, 3 - line

    private List<HeroSkillDetails> skillDetails = new List<HeroSkillDetails>();
    private Hero_SkillDetailsIconData skillIcon;
    private int heroId;
    private int skillNo;

    public override void OnCloseWindow()
    {
        base.OnCloseWindow();
        scrollRect.verticalNormalizedPosition = 1f;
    }

    private HeroSkillDetails CreateSkillDetail()
    {
        HeroSkillDetails skillDetails = new HeroSkillDetails();
        for (int i = 0; i < dataPrefabs.Length; i++)
        {
            GameObject obj = ClientUtils.CreateChild(skillDetailsTransform, dataPrefabs[i]);
            Hero_SkillText skillText = obj.GetComponent<Hero_SkillText>();
            if (skillText != null)
                skillDetails.skillText.Add(skillText);
            else
                skillDetails.lineObj = obj;
        }
        return skillDetails;
    }

    private void SetupUIElements()
    {
        if (skillIcon == null)
        {
            ClientUtils.DestroyChildren(skillDetailsTransform);
            GameObject obj = ClientUtils.CreateChild(skillIconTransform, skillIconPrefab);
            skillIcon = obj.GetComponent<Hero_SkillDetailsIconData>();
        }

        if (skillDetails.Count == 0)
        {
            ClientUtils.DestroyChildren(skillDetailsTransform);
            for (int i = 0; i < 2; i++)
                skillDetails.Add(CreateSkillDetail());
        }
    }

    public void Init(Hero hero, int skillNum, Sprite sprite)
    {
        SetupUIElements();

        heroId = hero.HeroId;
        skillNo = skillNum;

        skillIcon.Init(sprite, skillNo > 1);
        skillTypeText.text = GetSkillTypeText(skillNo);

        int skillGroupId = 0, currentLevel = 0;
        hero.GetSkillGroupAndCurrentLevel(skillNo, out skillGroupId, out currentLevel);
        bool isSkillMaxed = SkillRepo.IsSkillMaxLevel(skillGroupId, currentLevel);

        SkillGroupJson skillgroup = SkillRepo.GetSkillGroupById(skillGroupId);
        if (skillgroup != null)
        {
            skillNameText.text = skillgroup.localizedname;

            skillDetails[0].Init(skillGroupId, currentLevel, true);
            if (!isSkillMaxed)
                skillDetails[1].Init(skillGroupId, currentLevel + 1, false);
            else
                skillDetails[1].Hide();
        }

        skillPtsText.text = hero.SkillPoints.ToString();
        levelUpBtn.interactable = !isSkillMaxed && hero.SkillPoints > 0;
    }

    public void Refresh(Hero hero)
    {
        int skillGroupId = 0, currentLevel = 0;
        hero.GetSkillGroupAndCurrentLevel(skillNo, out skillGroupId, out currentLevel);
        bool isSkillMaxed = SkillRepo.IsSkillMaxLevel(skillGroupId, currentLevel);

        skillDetails[0].Init(skillGroupId, currentLevel, true);
        if (!isSkillMaxed)
            skillDetails[1].Init(skillGroupId, currentLevel + 1, false);
        else
            skillDetails[1].Hide();

        skillPtsText.text = hero.SkillPoints.ToString();
        levelUpBtn.interactable = !isSkillMaxed && hero.SkillPoints > 0;
    }

    private string GetSkillTypeText(int skillNo)
    {
        string localizedname = "";
        switch (skillNo)
        {
            case 1:
                localizedname = "skl_active"; break;
            case 2:
                localizedname = "skl_passive_summon"; break;
            case 3:
                localizedname = "skl_passive"; break;
        }
        return GUILocalizationRepo.GetLocalizedString(localizedname);
    }

    public void OnClickLevelUp()
    {
        RPCFactory.CombatRPC.LevelUpHeroSkill(heroId, skillNo);
    }
}

public class HeroSkillDetails
{
    public List<Hero_SkillText> skillText = new List<Hero_SkillText>();
    public GameObject lineObj;

    private StringBuilder sb = new StringBuilder();

    public void Init(int skillGroupId, int skillLevel, bool showLine)
    {
        SkillData skill = SkillRepo.GetSkillByGroupIDOfLevel(skillGroupId, skillLevel);
        if (skill != null)
        {
            // skill level
            int maxLevel = SkillRepo.GetSkillGroupMaxLevel(skillGroupId);
            sb.AppendFormat("{0}/{1}", skillLevel, maxLevel);
            skillText[0].SetText(sb.ToString());
            skillText[0].gameObject.SetActive(true);

            sb.Length = 0;
            // sideeffect description
            skillText[1].SetText("SDG NOT READY!");
            skillText[1].gameObject.SetActive(true);


            // cooldown
            if (skill.skillgroupJson.skilltype == SkillType.Active)
            {
                skillText[2].SetText(skill.skillJson.cooldown.ToString());
                skillText[2].gameObject.SetActive(true);
            }
            else
                skillText[2].gameObject.SetActive(false);

            lineObj.SetActive(showLine && skillLevel != maxLevel);
        }
        else
        {
            Hide();
        }
    }

    public void Hide()
    {
        for (int i = 0; i < skillText.Count; i++)
            skillText[i].gameObject.SetActive(false);
        lineObj.SetActive(false);
    }
}