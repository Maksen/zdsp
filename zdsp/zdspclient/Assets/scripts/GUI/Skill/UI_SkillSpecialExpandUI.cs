using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Repository;

public class UI_SkillSpecialExpandUI : MonoBehaviour {

    [Header("Panel Display")]
    public Image m_Icon;
    public Text m_SkillName;
    public Text m_ActivePassive;

    [Header("Skill Field")]
    public UI_SkillUISKLHelper m_SkillDesc;

    public UI_SkillButtonBase m_Selected;
    public Button m_Close;

    private UI_SkillTree.GameObjectPoolManager m_ReqStatsPool;
    private List<UI_SkillUIRequirementHelper> m_ReqStatsLabels;

    public void Initialise(Transform parent)
    {
        m_ReqStatsLabels = new List<UI_SkillUIRequirementHelper>();

        m_SkillDesc.Initialise(parent);
    }

    public void Show(UI_SkillButtonBase selected)
    {
        m_Selected = selected;

        SkillData skill = SkillRepo.GetSkill(selected.m_Skillid);
        m_Icon.sprite = selected.m_Icon.sprite;
        m_SkillName.text = skill.skillgroupJson.localizedname;

        switch (skill.skillgroupJson.skilltype)
        {
            case Zealot.Common.SkillType.Active:
            case Zealot.Common.SkillType.BasicAttack:
                m_ActivePassive.text = "主動";
                break;
            case Zealot.Common.SkillType.Passive:
                m_ActivePassive.text = "被動";
                break;
        }
        m_SkillDesc.GenerateChunk(selected);
    }

    public void OnClosed()
    {
        foreach (var obj in m_ReqStatsLabels)
        {
            obj.gameObject.transform.parent = null;
            m_ReqStatsPool.ReturnObject(obj.gameObject);
        }
        m_ReqStatsLabels.Clear();
        m_SkillDesc.RemoveChunks();
    }

    public void CloseUI()
    {
        m_Close.onClick.Invoke();
    }
}
