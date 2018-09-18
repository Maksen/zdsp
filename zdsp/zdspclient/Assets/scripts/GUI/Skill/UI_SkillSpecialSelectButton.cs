using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Repository;

public class UI_SkillSpecialSelectButton : UI_SkillButtonBase {

    public delegate void OnSelectedCallback(UI_SkillSpecialSelectButton param);

    public void Init(SkillData skill, UnityEngine.Events.UnityAction<bool> function)
    {
        m_Toggle = GetComponent<Toggle>();
        m_Toggle.onValueChanged.AddListener(function);

        //init
        m_skgID = skill.skillgroupJson.id;
        m_Skillid = skill.skillJson.id;
        m_SkillLevel = skill.skillJson.level;
        m_Icon.sprite = ClientUtils.LoadIcon(skill.skillgroupJson.icon);
    }

    public void OnSelected(OnSelectedCallback functor)
    {
        functor(this);
    }
}
