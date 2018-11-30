using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Repository;

public class UI_SkillSelectDropDownList : MonoBehaviour {

	public UI_SkillTree m_PanelPanel { get; set; }
    public GameObject m_ExpandData;
    public Transform m_ContentRect;
    public Button m_Close;
    public Dictionary<int, HUD_ExpandDataHelper> m_ActiveSkills = new Dictionary<int, HUD_ExpandDataHelper>();

    private UI_SkillTree.GameObjectPoolManager m_DataPool;
    
    public void Initialise(Transform parent)
    {
        m_DataPool = new UI_SkillTree.GameObjectPoolManager(5, parent, m_ExpandData, m_ContentRect);
    }

    public void GenerateSkillList()
    {
        Dictionary<int, int> skill = null;
        if (GameInfo.gLocalPlayer != null)
            skill = GameInfo.gLocalPlayer.mSkillInventory;

        List<KeyValuePair<int, int>> temp = new List<KeyValuePair<int, int>>();

        SkillData basic = SkillRepo.GetSkill(GameInfo.gLocalPlayer.SkillStats.basicAttack1SId);
        temp.Add(new KeyValuePair<int, int>(basic.skillgroupJson.id, basic.skillJson.id));

        foreach(var item in skill)
        {
            SkillData skd = SkillRepo.GetSkill(item.Value);
            if (skd.skillgroupJson.skilltype == Zealot.Common.SkillType.Active)// &&
                //skd.skillgroupJson.skillclass == Zealot.Common.SkillClass.Normal)
                temp.Add(item);
        }

        for (int i = 0; i < temp.Count; ++i)
        {
            if (!m_ActiveSkills.ContainsKey(temp[i].Key))
            {
                SkillData skd = SkillRepo.GetSkill(temp[i].Value);
                HUD_ExpandDataHelper data = m_DataPool.RequestObject().GetComponent<HUD_ExpandDataHelper>();
                data.Init(skd);
                data.AddListener(OnSelected);
                m_ActiveSkills.Add(temp[i].Key, data);
                data.transform.SetParent(m_ContentRect, false);
                data.transform.localScale = new Vector3(1, 1, 1);
            }
            else
            {
                if(temp[i].Value != m_ActiveSkills[temp[i].Key].m_Skill.skillJson.id)
                {
                    SkillData skd = SkillRepo.GetSkill(temp[i].Value);
                    m_ActiveSkills[temp[i].Key].Init(skd);
                    
                }
            }
        }
    }

    public void SortList(int top = -1)
    {
        // if top is null meaning the selected cell is empty
        if (top >= 0)
        {
            // top is on the top followed by following group id
            int childindex = 1;
            SortedList<int, HUD_ExpandDataHelper> sortedlist = new SortedList<int, HUD_ExpandDataHelper>();
            foreach(var item in m_ActiveSkills)
            {
                if(item.Value.m_Skill.skillJson.id == top)
                {
                    item.Value.transform.SetSiblingIndex(0);
                    item.Value.m_Toggle.isOn = true;
                    continue;
                }
                item.Value.m_Toggle.isOn = false;
                sortedlist.Add(item.Value.m_Skill.skillgroupJson.id, item.Value);
            }
            foreach(HUD_ExpandDataHelper item in sortedlist.Values)
            {
                item.transform.SetSiblingIndex(childindex++);
            }
        }
        else
        {
            int childindex = 0;
            SortedList<int, HUD_ExpandDataHelper> sortedlist = new SortedList<int, HUD_ExpandDataHelper>();
            foreach (var item in m_ActiveSkills)
            {
                sortedlist.Add(item.Value.m_Skill.skillgroupJson.id, item.Value);
            }
            foreach (HUD_ExpandDataHelper item in sortedlist.Values)
            {
                item.transform.SetSiblingIndex(childindex++);
            }
        }
    }
    
    public void OnSelected(UI_SkillButtonBase selected)
    {
        selected.m_Toggle.isOn = false;
        m_PanelPanel.OnEquipSkill(selected.m_Skillid);
    }

    public void OnEquipedSkill(int id)
    {
        // sort selected to top
        m_ActiveSkills[SkillRepo.GetSkill(id).skillgroupJson.id].transform.SetAsFirstSibling();
        gameObject.SetActive(false);
    }

    public void OnDeselected()
    {
        m_PanelPanel.OnCloseSelectEquipSkill();

        ClearActiveList();
    }

    public void ClearActiveList()
    {
        foreach (var item in m_ActiveSkills)
        {
            item.Value.transform.parent = null;
            m_DataPool.ReturnObject(item.Value.gameObject);
        }

        m_ActiveSkills.Clear();
    }

    public void SkillChanged(int skillid, int level)
    {
        // update the skill list
        // find the skill group id
        SkillData skd = SkillRepo.GetSkill(skillid);
        int skgid = skd.skillgroupJson.id;
        m_ActiveSkills[skgid].Init(skd);
    }

    public void CloseUI()
    {
        ClearActiveList();
        m_Close.onClick.Invoke();
    }
}
