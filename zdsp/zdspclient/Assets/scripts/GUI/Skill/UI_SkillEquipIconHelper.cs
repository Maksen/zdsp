using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zealot.Common;

public class UI_SkillEquipIconHelper : MonoBehaviour {

    public enum ePanelType : byte
    {
        ePlayerSkill,
        eAutoSkill
    }

    public GameObject m_ContentRect;
    public GameObject m_TextContentRect;
    public GameObject m_SkillIconPrefab;
    public GameObject m_SkillIconTextPrefab;
    public int m_PoolSize;
    [SerializeField]
    private ePanelType m_SelectedPanel; 
    private UI_SkillTree.GameObjectPoolManager m_DataPool;
    private UI_SkillTree.GameObjectPoolManager m_TextPool;
    private Dictionary<ePanelType, List<Pair<GameObject, GameObject>>> m_ButtonCache;
    private UI_SkillSelectButton m_CurrentSelected;
    private UI_SkillTree m_Parent;

    private List<UI_SkillSelectButton> m_EquipList;

	// Use this for initialization
	void Start () {
        
	}

    public void Init(UI_SkillTree parent)
    {
        m_DataPool = new UI_SkillTree.GameObjectPoolManager(m_PoolSize, this.transform, m_SkillIconPrefab);
        m_TextPool = new UI_SkillTree.GameObjectPoolManager(m_PoolSize, this.transform, m_SkillIconTextPrefab);
        m_Parent = parent;
        m_ButtonCache = new Dictionary<ePanelType, List<Pair<GameObject, GameObject>>>();
        m_EquipList = new List<UI_SkillSelectButton>();
    }

    public void CreateSkillEquipPanel(ePanelType type, int size)
    {
        if (m_ButtonCache.ContainsKey(type)) return;
        m_ButtonCache.Add(type, new List<Pair<GameObject, GameObject>>());

        for(int i = 0; i < size; ++i)
        {
            GameObject go = m_DataPool.RequestObject();
            GameObject gotext = m_TextPool.RequestObject();

            m_ButtonCache[type].Add(Pair.Create(go, gotext));
            m_ButtonCache[type][i].Item2.GetComponent<UnityEngine.UI.Text>().text = (i + 1).ToString();
            UI_SkillSelectButton skillbutton = m_ButtonCache[type][i].Item1.GetComponent<UI_SkillSelectButton>();
            skillbutton.m_skgID = i;
            skillbutton.AddListener(OnSelect);
            m_EquipList.Add(skillbutton);

            m_ButtonCache[type][i].Item1.transform.SetParent(m_ContentRect.transform);
            m_ButtonCache[type][i].Item2.transform.SetParent(m_TextContentRect.transform);
            m_ButtonCache[type][i].Item1.SetActive(false);
            m_ButtonCache[type][i].Item2.SetActive(false);
        }
    }

    public void DisplayPanelOption(ePanelType panel)
    {
        if(m_SelectedPanel != panel)
        {
            
            foreach(var obj in m_ButtonCache[m_SelectedPanel])
            {
                obj.Item1.SetActive(false);
                obj.Item2.SetActive(false);
                
            }
        }

        m_EquipList.Clear();

        if (m_ButtonCache.ContainsKey(panel))
        {
            m_SelectedPanel = panel;
            foreach(var obj in m_ButtonCache[panel])
            {
                obj.Item1.SetActive(true);
                obj.Item2.SetActive(true);
                m_EquipList.Add(obj.Item1.GetComponent<UI_SkillSelectButton>());
            }
        }
    }

    /// <summary>
    /// Remember to call DisplayPanelOption Before this unless you are sure the correct panel is selected
    /// </summary>
    public void ShowPanel()
    {
        Zealot.Common.Datablock.CollectionHandler<object> collection = null;
        int slotgrp = -1;

        switch (m_SelectedPanel)
        {
            case ePanelType.ePlayerSkill:
                collection = GameInfo.gLocalPlayer.SkillStats.EquippedSkill;
                slotgrp += m_Parent.GetEquipGroup();
                break;
            case ePanelType.eAutoSkill:
                collection = GameInfo.gLocalPlayer.SkillStats.AutoSkill;
                slotgrp += m_Parent.GetAutoGroup();
                break;
        }

        foreach(var obj in m_ButtonCache[m_SelectedPanel])
        {
            UI_SkillSelectButton button = obj.Item1.GetComponent<UI_SkillSelectButton>();
            button.EquipSkill((int)collection[m_ButtonCache[m_SelectedPanel].Count * slotgrp + button.m_skgID]);
        }

        if (m_CurrentSelected != null) m_CurrentSelected.m_Toggle.isOn = false;
    }

    public void ClosePanel()
    {
        if (m_CurrentSelected != null)
        {
            m_CurrentSelected.m_Toggle.isOn = false;
            m_CurrentSelected = null;
        }
    }

    public void OnSelect(UI_SkillButtonBase button)
    {
        if (m_CurrentSelected != null && m_CurrentSelected != (UI_SkillSelectButton)button)
            m_CurrentSelected.m_Toggle.isOn = false;

        m_CurrentSelected = (UI_SkillSelectButton)button;
        m_Parent.m_SelectSkillDDL.GenerateSkillList();
        m_Parent.m_SelectSkillDDL.SortList(m_CurrentSelected.m_Skillid);
        m_Parent.CloseWindows(true);
        m_Parent.m_SelectSkillDDL.gameObject.SetActive(true);
        //// the selected one
        //if (button.m_Toggle.isOn)
        //{
        //    if (m_CurrentSelected != null && (button.m_skgID != m_CurrentSelected.m_skgID && m_CurrentSelected.m_Toggle.isOn))
        //        m_CurrentSelected.m_Toggle.isOn = false;
        //    m_CurrentSelected = (UI_SkillSelectButton)button;
        //    m_Parent.m_SelectSkillDDL.GenerateSkillList();
        //    m_Parent.m_SelectSkillDDL.SortList(m_CurrentSelected.m_Skillid);
        //    m_Parent.CloseWindows(true);
        //    m_Parent.m_SelectSkillDDL.gameObject.SetActive(true);
        //}
    }

    public void OnSelectEquip(int id)
    {
        int index = 0;
        //find duplicates
        foreach(var button in m_EquipList)
        {
            if(button == m_CurrentSelected) continue;
            if(button.m_Skillid == id)
            {
                if(m_CurrentSelected.m_Skillid != 0)
                {
                    int _id = m_CurrentSelected.m_Skillid;
                    button.EquipSkill(_id);

                    if (m_SelectedPanel == ePanelType.ePlayerSkill)
                        RPCFactory.NonCombatRPC.EquipSkill(_id, button.m_skgID, m_Parent.m_EquipSlotGroup);
                    else
                        RPCFactory.NonCombatRPC.AutoEquipSkill(_id, button.m_skgID, m_Parent.m_AutoSlotGroup);
                    break;
                }
                else
                {
                    button.EquipSkill(0);
                    
                    if(GameInfo.gLocalPlayer != null)
                    {
                        if (m_SelectedPanel == ePanelType.ePlayerSkill)
                            RPCFactory.NonCombatRPC.RemoveEquipSkill(index, m_Parent.m_EquipSlotGroup);
                        else
                            RPCFactory.NonCombatRPC.RemoveAutoEquipSkill(index, m_Parent.m_AutoSlotGroup);
                    }
                }

            }
            ++index;
        }

        m_CurrentSelected.EquipSkill(id);
        m_CurrentSelected.m_Toggle.isOn = false;
        if (GameInfo.gLocalPlayer != null)
        {
            if (m_SelectedPanel == ePanelType.ePlayerSkill)
                RPCFactory.NonCombatRPC.EquipSkill(id, m_CurrentSelected.m_skgID, m_Parent.m_EquipSlotGroup);
            else
                RPCFactory.NonCombatRPC.AutoEquipSkill(id, m_CurrentSelected.m_skgID, m_Parent.m_AutoSlotGroup);
        }
    }
}
