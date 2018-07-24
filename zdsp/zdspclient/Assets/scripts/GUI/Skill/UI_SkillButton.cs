using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Kopio.JsonContracts;
using Zealot.Repository;

public class UI_SkillButton : MonoBehaviour {

    [Flags]
    private enum STATUS : byte {
        // 0000 0001 --> 1st bit for unlock status
        // 0000 0011 --> 2nd bit for upgrade status
        eLOCKED = 0, // --> 0000 0000 -> nothing to it... lol
        eUNLOCKED = 1 << 0, // --> 0000 0001 -> skill is now unlocked
        eLEVELUP = 1 << 1, // --> 0000 0011 -> level up avaliable
        eACQUIRED = 1 << 2, // --> 0000 0111 -> skill is learnt
        eMAX = 1 << 3, // --> 0000 1111 -> Maxed
    }

    [Header("Information")]
    public int m_ID;
    public Text m_SkillLevelText;
    public Transform m_Anchor;
    public SkillData m_SkillData;
    public Image m_Icon;
    public SkillTreeJson m_Json;
    [SerializeField]
    private GameObject m_LevelUpIcon;

    public int m_Row = 0, m_Col = 0;
    public Toggle m_Toggle;

    public UI_SkillTree m_parentPanel { get; set; }

    private STATUS m_Status;
    public int m_SkillLevel = 0;

    public List<KeyValuePair<int, int>> m_RequiredSkills = new List<KeyValuePair<int, int>>();

    public void Init(SkillTreeJson info)
    {
        m_ID = info.skillgroupid;
        //load icon
        m_SkillData = SkillRepo.GetSkillByGroupID(m_ID);
        string icon = m_SkillData.skillgroupJson.icon;
        m_Icon.sprite = ClientUtils.LoadIcon(icon);
        m_Toggle = GetComponent<Toggle>();
        m_Toggle.onValueChanged.AddListener(delegate { OnSelected(); });
        m_SkillData = null;

        //check current player skill
        if (GameInfo.gLocalPlayer != null)
        {
            Zealot.Common.Datablock.CollectionHandler<object> skills = GameInfo.gLocalPlayer.SkillStats.SkillInv;
            //if (skills.ContainsKey(m_ID))
            //{
            //    m_SkillData = SkillRepo.GetSkill(skills[m_ID]);
            //    m_SkillLevel = m_SkillData.skillJson.level;
            //}
            for(int i = 0; i < skills.Count >> 1; i += 2)
            {
                if((int)skills[i] == 0)
                {
                    break;
                }
                if((int)skills[i] == m_ID)
                {
                    m_SkillData = SkillRepo.GetSkill((int)skills[i + 1]);
                    m_SkillLevel = m_SkillData.skillJson.level;
                }
            }
        }

        UpdateButton();
    }

    public void UpdateButton()
    {
        m_LevelUpIcon.SetActive(false);
        if (m_SkillLevel != 0)
        {
            // is unlocked
            m_Status = STATUS.eUNLOCKED | STATUS.eACQUIRED;
            // init the level
            m_SkillLevelText.text = m_SkillLevel.ToString() + " / " + SkillRepo.GetSkillGroupMaxLevel(m_SkillData.skillgroupJson.id).ToString();
            // check if can be upgraded
        }
        else
        {
            // get the unlockable skill first
            SkillData fskill = SkillRepo.GetSkillByGroupIDOfNextLevel(m_ID, 0);

            int level = 0;
            // not unlocked previously so check for unlock
            if (GameInfo.gLocalPlayer != null)
                level = (int)GameInfo.gLocalPlayer.PlayerSynStats.Level;
            else
                level = 1;
            if (level >= fskill.skillJson.requiredlv && m_parentPanel.IsRequiredSkillsUnlocked(this))
                m_Status |= STATUS.eUNLOCKED;
            else
                m_Status |= STATUS.eLOCKED;

            m_SkillLevelText.text = m_SkillLevel + " / " + SkillRepo.GetSkillGroupMaxLevel(fskill.skillgroupJson.id).ToString();
        }

        // find the amount of skill points avaliable
        int skillpoint = 1000, money = 1000;
        if (GameInfo.gLocalPlayer != null)
        {
            skillpoint = GameInfo.gLocalPlayer.LocalCombatStats.SkillPoints;
            //money = GameInfo.gLocalPlayer.SecondaryStats.money;
        }

        SkillData skill = SkillRepo.GetSkillByGroupIDOfNextLevel(m_ID, m_SkillLevel);
        if (skill == null)
        {
            m_Status = STATUS.eMAX | STATUS.eACQUIRED;
            return;
        }
        if ((m_Status & STATUS.eUNLOCKED) == STATUS.eUNLOCKED)
        {
            if (skillpoint >= skill.skillJson.learningsp && money >= skill.skillJson.learningcost)
            {
                m_Status |= STATUS.eLEVELUP;
                m_LevelUpIcon.SetActive(true);
            }
        }
    }

    public void OnSelected()
    {
        UpdateButton();
        m_parentPanel.OnSelectSkill(this);
    }

    public void OnLevelUpSkill()
    {
        // check if leveling is possible
        if ((m_Status & STATUS.eLEVELUP) == STATUS.eLEVELUP)
        {
            // level up possible
            int level = m_SkillLevel;

            // with current level find next level skill
            SkillData data = SkillRepo.GetSkillByGroupIDOfNextLevel(m_ID, m_SkillLevel);

            if (data != null && !SkillRepo.IsSkillMaxLevel(m_ID, m_SkillLevel))
            {
                m_SkillData = data;
                m_SkillLevel = m_SkillData.skillJson.level;
                //UpdateButton();

                RPCFactory.NonCombatRPC.AddToSkillInventory(m_SkillData.skillJson.id, m_SkillData.skillgroupJson.id);
                
                m_Status |= STATUS.eACQUIRED;
                //GameInfo.gLocalPlayer.
            }

            // inform parent of level up
            m_parentPanel.OnLevelUpSkillWithID(m_ID, level);
        }
    }

    public bool IsUpgradable()
    {
        if ((m_Status & STATUS.eMAX) == STATUS.eMAX)
            return false;
        if ((m_Status & STATUS.eLEVELUP) == STATUS.eLEVELUP)
            return true;
        return false;
    }

    public bool IsUnlocked()
    {
        if ((m_Status & STATUS.eACQUIRED) == STATUS.eACQUIRED)
            return true;
        return false;
    }
}
