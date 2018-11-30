using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UIWidgets;
using Zealot.Repository;

public class HUD_ExpandDataHelper : MonoBehaviour
{

    public Image m_Icon;
    public Text m_SkillName;

    public SkillData m_Skill; 

    public Toggle m_Toggle;
    public Spinner m_Level;

    [Header("To hide on Basic Attack")]
    [SerializeField]private GameObject hlevel;
    [SerializeField]private GameObject hSpinner;

    public UI_SkillButtonBase m_Button;

    public void Init(SkillData skill)
    {
        m_Icon.sprite = ClientUtils.LoadIcon(skill.skillgroupJson.icon);
        m_SkillName.text = skill.skillgroupJson.localizedname;
        m_Skill = skill;
        if (skill.skillgroupJson.skilltype == Zealot.Common.SkillType.BasicAttack)
        {
            //no level needed
            hlevel.SetActive(false);
            hSpinner.SetActive(false);
        }
        else
        {
            Dictionary<int, int> skills = GameInfo.gLocalPlayer.mSkillInventory;
            if (skills.ContainsKey(m_Skill.skillgroupJson.id))
            {
                SkillData skd = SkillRepo.GetSkill(skills[m_Skill.skillgroupJson.id]);
                m_Level.Max = skd.skillJson.level;
            }
            m_Level.Value = skill.skillJson.level;
        }
        
        m_Button.m_Skillid = m_Skill.skillJson.id;
    }

    public void LevelUp()
    {
        m_Skill = SkillRepo.GetSkillByGroupIDOfNextLevel(m_Skill.skillgroupJson.id, m_Level.Value);
    }

    public void LevelDown()
    {
        m_Skill = SkillRepo.GetSkillByGroupIDOfPreviousLevel(m_Skill.skillgroupJson.id, m_Level.Value);
    }

    public void AddListener(UI_SkillButtonBase.OnSelectedCallback func)
    {
        m_Button.AddListener(func);
    }
}
