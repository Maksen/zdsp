using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Repository;

public class UI_SkillSelectButton : UI_SkillButtonBase{

    public delegate void OnSelectedCallback(UI_SkillSelectButton param);

    public void Init(OnSelectedCallback function)
    {
        m_Toggle.onValueChanged.AddListener(delegate { function(this); });
    }

    public void EquipSkill(int skillid, bool isPlayerEquip)
    {
        m_Skillid = skillid;
        if (m_Skillid == 0)
        {
            m_Icon.sprite = ClientUtils.LoadIcon("UI_ZDSP_Icons/Skill/00_ActiveEmpty.png");
        }
        else
        {
            m_Icon.sprite = ClientUtils.LoadIcon(SkillRepo.GetSkill(skillid).skillgroupJson.icon);
            if(isPlayerEquip)
                GameInfo.gLocalPlayer.SkillStats.EquippedSkill[m_skgID * m_parentPanel.GetEquipGroup()] = skillid;
            else
                GameInfo.gLocalPlayer.SkillStats.AutoSkill[m_skgID * m_parentPanel.GetEquipGroup()] = skillid;
        }
    }
}
