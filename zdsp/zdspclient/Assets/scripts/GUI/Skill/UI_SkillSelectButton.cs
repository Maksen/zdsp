using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Repository;

public class UI_SkillSelectButton : MonoBehaviour {

    public Toggle m_Toggle;
    public int m_ID;
    public Image m_Icon;
    public int m_Skillid;

    public UI_SkillTree m_parentPanel { get; set; }

    public void Init()
    {
        m_Toggle = GetComponent<Toggle>();
        // check if skill is equipped
        if(GameInfo.gLocalPlayer != null)
        {
            EquipSkill((int)GameInfo.gLocalPlayer.SkillStats.EquippedSkill[m_ID]);
        }

        m_Toggle.onValueChanged.AddListener(delegate { OnSelected(); });
    }

    public void EquipSkill(int skillid)
    {
        m_Skillid = skillid;
        if (m_Skillid == 0)
        {
            m_Icon.sprite = ClientUtils.LoadIcon("UI_ZDSP_Icons/Skill/00_ActiveEmpty.png");
        }
        else
        {
            m_Icon.sprite = ClientUtils.LoadIcon(SkillRepo.GetSkill(skillid).skillgroupJson.icon);
            GameInfo.gLocalPlayer.SkillStats.EquippedSkill[m_ID] = skillid;
        }
    }

    public void OnSelected()
    {
        m_parentPanel.OnSelectEquipSkill(this);
    }
}
