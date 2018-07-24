using Kopio.JsonContracts;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Repository;

public class Hero_SkillIconData : MonoBehaviour
{
    [SerializeField] Image iconImage;
    [SerializeField] Text levelText;

    private int skillNo;

    public void Init(int skillno, int skillGrpId, int skillLevel)
    {
        skillNo = skillno;
        SkillGroupJson skillgroup = SkillRepo.GetSkillGroupById(skillGrpId);
        if (skillgroup != null)
        {
            iconImage.sprite = ClientUtils.LoadIcon(skillgroup.icon);
        }

        levelText.text = skillLevel > 0 ? skillLevel.ToString() : "";
    }

    public void OnClick()
    {
        print("click on skill " + skillNo);
    }
}