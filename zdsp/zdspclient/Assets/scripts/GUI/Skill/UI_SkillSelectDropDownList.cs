using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Repository;

public class UI_SkillSelectDropDownList : MonoBehaviour {

	public UI_SkillTree m_PanelPanel { get; set; }
    public GameObject m_ExpandData;
    public Transform m_ContentRect;
    public Dictionary<int, HUD_ExpandDataHelper> m_ActiveSkills = new Dictionary<int, HUD_ExpandDataHelper>();

    private GameObjectPoolManager m_DataPool;
    
    public void Initialise(Transform parent)
    {
        m_DataPool = new GameObjectPoolManager(5, parent, m_ExpandData, m_ContentRect);
    }

    public void GenerateSkillList()
    {
        Zealot.Common.Datablock.CollectionHandler<object> skill = null;
        if (GameInfo.gLocalPlayer != null)
            skill = GameInfo.gLocalPlayer.SkillStats.SkillInv;

        List<KeyValuePair<int, int>> temp = new List<KeyValuePair<int, int>>();

        for(int i = 0; i < skill.Count >> 1; i += 2)
        {
            if ((int)skill[i] == 0) break;
            SkillData skd = SkillRepo.GetSkill((int)skill[i + 1]);
            if (skd.skillgroupJson.skilltype == Zealot.Common.SkillType.Active)
                temp.Add(new KeyValuePair<int, int>((int)skill[i], (int)skill[i + 1]));
        }

        for (int i = 0; i < temp.Count; ++i)
        {
            if (!m_ActiveSkills.ContainsKey(temp[i].Key))
            {
                SkillData skd = SkillRepo.GetSkill(temp[i].Value);
                HUD_ExpandDataHelper data = m_DataPool.RequestObject().GetComponent<HUD_ExpandDataHelper>();
                data.Init(skd, skd.skillJson.level);
                data.m_ID = temp[i].Value;
                data.m_Toggle.onValueChanged.AddListener(delegate { OnSelected(data.m_ID); });
                m_ActiveSkills.Add(temp[i].Key, data);
                data.transform.parent = m_ContentRect;
            }
            else
            {
                if(temp[i].Value != m_ActiveSkills[temp[i].Key].m_ID)
                {
                    SkillData skd = SkillRepo.GetSkill(temp[i].Value);
                    m_ActiveSkills[temp[i].Key].Init(skd, skd.skillJson.level);
                    m_ActiveSkills[temp[i].Key].m_ID = temp[i].Value;
                }
            }
        }
    }
    
    public void OnSelected(int id)
    {
        m_PanelPanel.OnEquipSkill(id);
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
        m_ActiveSkills.Clear();
    }

    public void SkillChanged(int skillid, int level)
    {
        // update the skill list
        // find the skill group id
        SkillData skd = SkillRepo.GetSkill(skillid);
        int skgid = skd.skillgroupJson.id;
        m_ActiveSkills[skgid].m_ID = skillid;
        m_ActiveSkills[skgid].Init(skd, level);
    }
}
