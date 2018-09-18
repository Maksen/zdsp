using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zealot.Common;
using Zealot.Repository;
using Kopio.JsonContracts;


public class UI_SkillSpecialUI : MonoBehaviour {

    [Header("Prefabs")]
    public GameObject m_SpecialSkillRow;
    public GameObject m_SkillIconData;

    [Header("Content Rect")]
    public GameObject m_ContentRect;

    [Header("Skill Details Panel")]
    public UI_SkillSpecialExpandUI m_SkillDescriptor;

    public UI_SkillTree m_Parent { get; set; }
    private GameObjectPoolManager m_SpecialRowPool;
    private GameObjectPoolManager m_SkillIconPool;

    private int m_RowCount = 0;
    private GameObject m_CurrentRow;

    [SerializeField]
    private UnityEngine.UI.Button m_Close;

    private UI_SkillSpecialSelectButton m_CurrentActive;

    public void Initialise(Transform parent)
    {
        m_SpecialRowPool = new GameObjectPoolManager(3, parent, m_SpecialSkillRow);
        m_SkillIconPool = new GameObjectPoolManager(9, parent, m_SkillIconData);
        m_SkillDescriptor.Initialise(this.transform);
        m_SkillDescriptor.gameObject.SetActive(false);
    }

    private UI_SkillSpecialSelectButton AddSkillToList(SkillData skill)
    {
        UI_SkillSpecialSelectButton button = null;
        if (m_RowCount == 3 || m_RowCount == 0)
        {
            m_RowCount = 0;
            m_CurrentRow = m_SpecialRowPool.RequestObject();
            
            m_CurrentRow.transform.parent = m_ContentRect.transform;
            m_CurrentRow.transform.localPosition = new Vector3(0, 0, 1);
            m_CurrentRow.transform.localScale = new Vector3(1, 1, 1);
        }
        if (m_RowCount < 3)
        {
            GameObject obj = m_SkillIconPool.RequestObject();
            obj.transform.parent = m_CurrentRow.transform;
            obj.transform.localPosition = new Vector3(0, 0, 1);
            obj.transform.localScale = new Vector3(1, 1, 1);
            button = obj.GetComponent<UI_SkillSpecialSelectButton>();
            ++m_RowCount;
        }
        return button;
    }

    public void GenerateList(JobType job)
    {
        // get list of special skills from repo
        List<int> skills = SkillRepo.GetSpecialSkillGivenJob(job);
        foreach(var skill in skills)
        {
            SkillData skd = SkillRepo.GetSkill(skill);
            UI_SkillSpecialSelectButton button = AddSkillToList(skd);
            button.Init(skd, delegate { button.OnSelected(OnSelectSkill); });
        }
    }

    public void OnSelectSkill(UI_SkillSpecialSelectButton button)
    {
        if (button.m_Toggle.isOn && m_CurrentActive != button && m_CurrentActive != null)
            //if(m_CurrentActive != button && m_CurrentActive != null)
            m_CurrentActive.m_Toggle.isOn = false;

        if (button.m_Toggle.isOn)
        {
            m_CurrentActive = button;
            m_SkillDescriptor.gameObject.SetActive(true);
            m_SkillDescriptor.Show(m_CurrentActive);

        }
        else if (!button.m_Toggle.isOn && button == m_CurrentActive)
        {
            m_SkillDescriptor.OnClosed();
            //m_SkillDescriptor.CloseUI();
        }
    }

    public void OnClose()
    {
        if(m_CurrentActive != null)
            m_CurrentActive.m_Toggle.isOn = false;
        m_CurrentActive = null;
    }

    public void CloseUI()
    {
        m_Close.onClick.Invoke();
    }
}
