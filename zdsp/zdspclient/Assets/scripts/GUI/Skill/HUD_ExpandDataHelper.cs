using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UIWidgets;
using Zealot.Repository;

public class HUD_ExpandDataHelper : MonoBehaviour {

    public Image m_Icon;
    public Text m_SkillName;

    public SkillData m_Skill; 

    public Toggle m_Toggle;
    public Spinner m_Level;

    public void Init(SkillData skill)
    {
        m_Icon.sprite = ClientUtils.LoadIcon(skill.skillgroupJson.icon);
        m_SkillName.text = skill.skillgroupJson.localizedname;
        m_Level.Max = SkillRepo.GetSkillGroupMaxUpgrade(skill.skillgroupJson.id);
        m_Level.Value = skill.skillJson.level;
        m_Skill = skill;
    }

    public void LevelUp()
    {
        m_Skill = SkillRepo.GetSkillByGroupIDOfNextLevel(m_Skill.skillgroupJson.id, m_Level.Value);
    }

    public void LevelDown()
    {
        m_Skill = SkillRepo.GetSkillByGroupIDOfPreviousLevel(m_Skill.skillgroupJson.id, m_Level.Value);
    }
}
