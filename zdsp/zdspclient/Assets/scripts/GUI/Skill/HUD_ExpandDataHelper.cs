using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Repository;

public class HUD_ExpandDataHelper : MonoBehaviour {

    public Image m_Icon;
    public Text m_SkillName;
    public Text m_SkillLevel;

    public int m_ID;

    public Toggle m_Toggle;

    public void Init(SkillData skill, int level)
    {
        m_Icon.sprite = ClientUtils.LoadIcon(skill.skillgroupJson.icon);
        m_SkillName.text = skill.skillJson.name;
        m_SkillLevel.text = level.ToString();
    }
}
