using Kopio.JsonContracts;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Repository;

public class Hero_SkillIconData : MonoBehaviour
{
    [SerializeField] Image iconImage;
    [SerializeField] Text levelText;

    private int skillNo;
    private Hero hero;
    private string iconPath;

    public void Init(Hero hero, int skillNum)
    {
        this.hero = hero;
        if (hero == null)
            return;

        skillNo = skillNum;

        int skillGroup = 0, skillLevel = 0;
        hero.GetSkillGroupAndCurrentLevel(skillNo, out skillGroup, out skillLevel);

        levelText.text = skillLevel > 1 ? skillLevel.ToString() : "";

        SkillGroupJson skillgroup = SkillRepo.GetSkillGroupById(skillGroup);
        if (skillgroup != null && iconPath != skillgroup.icon)
        {
            iconImage.sprite = ClientUtils.LoadIcon(skillgroup.icon);
            iconPath = skillgroup.icon;
        }
    }

    public void OnClick()
    {
        if (hero == null)
            return;

        UIManager.OpenDialog(WindowType.DialogHeroSkillDetails,
            (window) => window.GetComponent<UI_Hero_SkillDetailsDialog>().Init(hero, skillNo, iconImage.sprite));
    }
}