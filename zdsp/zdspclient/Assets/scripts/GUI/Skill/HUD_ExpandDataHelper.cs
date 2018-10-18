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

        if (skill.skillgroupJson.skilltype == Zealot.Common.SkillType.BasicAttack)
        {
            //no level needed
            hlevel.SetActive(false);
            hSpinner.SetActive(false);
        }
        else
        {
            //m_Level.Max = SkillRepo.GetSkillGroupMaxUpgrade(skill.skillgroupJson.id);
            Zealot.Common.Datablock.CollectionHandler<object> skills = GameInfo.gLocalPlayer.SkillStats.SkillInv;
            for (int i = 0; i < skills.Count >> 1; i += 2)
            {
                if ((int)skills[i] == 0)
                {
                    break;
                }
                if ((int)skills[i] == skill.skillgroupJson.id)
                {
                    SkillData m_SkillData = SkillRepo.GetSkill((int)skills[i + 1]);
                    m_Level.Max = m_SkillData.skillJson.level;
                }
            }
            m_Level.Value = skill.skillJson.level;
        }
        m_Skill = skill;
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
