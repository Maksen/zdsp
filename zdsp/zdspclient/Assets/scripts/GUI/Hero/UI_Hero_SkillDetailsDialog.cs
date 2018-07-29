using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Repository;

public class UI_Hero_SkillDetailsDialog : BaseWindowBehaviour
{
    [SerializeField] Transform skillIconTransform;
    [SerializeField] GameObject skillIconPrefab;
    [SerializeField] Text skillNameText;
    [SerializeField] Text skillTypeText;

    [Header("Skill Details")]
    [SerializeField] Transform skillDetailsTransform;
    [SerializeField] ScrollRect scrollRect;
    [SerializeField] GameObject[] detailPrefabs;



    public override void OnRegister()
    {
        base.OnRegister();
    }

}

public class HeroSkillDetails
{
    public Hero_SkillText level;
    public Hero_SkillText description;
    public Hero_SkillText cooldown;

    public HeroSkillDetails(int skillGroupId, int skillLevel)
    {
        SkillData skill = SkillRepo.GetSkillByGroupIDOfLevel(skillGroupId, skillLevel);
        if (skill != null)
        {
            level.SetText(skillLevel + "/" + SkillRepo.GetSkillGroupMaxLevel(skillGroupId));

        }
    }
}