using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zealot.Repository;

public class UI_SkillSelectButton : UI_SkillButtonBase{

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
            //if (isPlayerEquip)
            //    //GameInfo.gLocalPlayer.SkillStats.EquippedSkill[m_skgID * m_parentPanel.GetEquipGroup()] = skillid;
            //    RPCFactory.NonCombatRPC.EquipSkill(skillid, m_skgID, m_parentPanel.GetEquipGroup());
            //else
            //    //GameInfo.gLocalPlayer.SkillStats.AutoSkill[m_skgID * m_parentPanel.GetEquipGroup()] = skillid;
            //    RPCFactory.NonCombatRPC.AutoEquipSkill(skillid, m_skgID, m_parentPanel.GetEquipGroup());
        }
    }
}
