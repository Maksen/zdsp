using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Repository;

public class UI_SkillSpecialSelectButton : UI_SkillButtonBase {
    
    public void Init(SkillData skill)
    {
        m_Toggle = GetComponent<Toggle>();

        //init
        m_skgID = skill.skillgroupJson.id;
        m_Skillid = skill.skillJson.id;
        m_SkillLevel = skill.skillJson.level;
        m_Icon.sprite = ClientUtils.LoadIcon(skill.skillgroupJson.icon);
    }

    public void OnValueUpdate(SkillData skill)
    {
        m_skgID = skill.skillgroupJson.id;
        m_Skillid = skill.skillJson.id;
        m_SkillLevel = skill.skillJson.level;
        m_Icon.sprite = ClientUtils.LoadIcon(skill.skillgroupJson.icon);
    }
}
