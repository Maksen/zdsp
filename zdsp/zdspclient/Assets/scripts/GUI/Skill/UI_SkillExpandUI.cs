using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Repository;

public class UI_SkillExpandUI : MonoBehaviour {

    [Header("Panel Display")]
    public Image m_Icon;
    public Text m_SkillName;
    public Text m_ActivePassive;

    [Header("Required Stats Field")]
    public GameObject m_ReqStatsPrefab;
    public GameObject m_ReqStatsParent;

    [Header("Skill Field")]
    public UI_SkillUISKLHelper m_SkillDesc;

    public Button m_Close;
    public UI_SkillButton m_Button;
    public Button m_Upgrade;


    private UI_SkillTree.GameObjectPoolManager m_ReqStatsPool;

    private List<UI_SkillUIRequirementHelper> m_ReqStatsLabels;

    public void Initialise(Transform parent)
    {
        //create pool of required stats holder for use
        m_ReqStatsPool = new UI_SkillTree.GameObjectPoolManager(5, parent, m_ReqStatsPrefab);
        m_ReqStatsLabels = new List<UI_SkillUIRequirementHelper>();

        m_SkillDesc.Initialise(parent);
    }

    public void Show(UI_SkillButton selected)
    {
        m_Button = selected;

        SkillData skill;
        if (selected.m_SkillLevel == 0)
            skill = SkillRepo.GetSkillByGroupIDOfNextLevel(selected.m_skgID, selected.m_SkillLevel);
        else
            skill = selected.m_SkillData;

        m_Icon.sprite = selected.m_Icon.sprite;

        m_SkillName.text = skill.skillgroupJson.localizedname;
        //m_ActivePassive = selected.m_SkillData.skillgroupJson.ac

        if (selected.IsUpgradable())
            m_Upgrade.interactable = true;
        else
            m_Upgrade.interactable = false;

        string[] req = skill.skillJson.progressskill.Split(';');
        //show required skill points
        //check current player skill
        int skp = 0;
        int my = 0;
        int lv = 1;
        if (GameInfo.gLocalPlayer != null)
        {
            skp = (int)GameInfo.gLocalPlayer.LocalCombatStats.SkillPoints;
            my = (int)GameInfo.gLocalPlayer.SecondaryStats.Money;
            lv = (int)GameInfo.gLocalPlayer.PlayerStats.Level;
        }
        int skillpoint = skill.skillJson.learningsp;
        int money = skill.skillJson.learningcost;
        int level = skill.skillJson.requiredlv;

        GameObject obj = m_ReqStatsPool.RequestObject();
        m_ReqStatsLabels.Add(obj.GetComponent<UI_SkillUIRequirementHelper>());
        m_ReqStatsLabels[m_ReqStatsLabels.Count - 1].SetData(GUILocalizationRepo.GetLocalizedString("hro_skill_points"), skillpoint.ToString() + "/"  + skp.ToString());
        obj.transform.SetParent(m_ReqStatsParent.transform, false);
        //obj.transform.localPosition = new Vector3(obj.transform.localPosition.x, obj.transform.localPosition.y, 0);
        obj.transform.localScale = new Vector3(1, 1, 1);

        Color color;
        if (skp < skillpoint)
            color = Color.red;
        else
            color = Color.white;
        m_ReqStatsLabels[m_ReqStatsLabels.Count - 1].SetColor(color);


        if (money > 0)
        {
            obj = m_ReqStatsPool.RequestObject();
            m_ReqStatsLabels.Add(obj.GetComponent<UI_SkillUIRequirementHelper>());
            m_ReqStatsLabels[m_ReqStatsLabels.Count - 1].SetData(GUILocalizationRepo.GetLocalizedString("currency_Money"), money.ToString() + "/" + my.ToString());
            obj.transform.parent.SetParent(m_ReqStatsParent.transform, false);
            //obj.transform.localPosition = new Vector3(obj.transform.localPosition.x, obj.transform.localPosition.y, 0);
            obj.transform.localScale = new Vector3(1, 1, 1);

            if (my < money)
                color = Color.red;
            else
                color = Color.white;
            m_ReqStatsLabels[m_ReqStatsLabels.Count - 1].SetColor(color);
        }

        if (level > 0)
        {
            obj = m_ReqStatsPool.RequestObject();
            m_ReqStatsLabels.Add(obj.GetComponent<UI_SkillUIRequirementHelper>());
            m_ReqStatsLabels[m_ReqStatsLabels.Count - 1].SetData(GUILocalizationRepo.GetLocalizedString("skl_skill_level"), level.ToString());
            obj.transform.SetParent(m_ReqStatsParent.transform, false);
            //obj.transform.localPosition = new Vector3(obj.transform.localPosition.x, obj.transform.localPosition.y, 0);
            obj.transform.localScale = new Vector3(1, 1, 1);

            if (lv < level)
                color = Color.red;
            else
                color = Color.white;
            m_ReqStatsLabels[m_ReqStatsLabels.Count - 1].SetColor(color);
        }

        switch (skill.skillgroupJson.skilltype)
        {
            case Zealot.Common.SkillType.Active:
                m_ActivePassive.text = GUILocalizationRepo.GetLocalizedString("skl_active");
                break;
            case Zealot.Common.SkillType.Passive:
                m_ActivePassive.text = GUILocalizationRepo.GetLocalizedString("skl_passive");
                break;
        }

        m_SkillDesc.GenerateChunk(selected);
    }

    public void Reload(int newskillpoint, int newmoney, UI_SkillButton selected)
    {
        m_Button = selected;

        SkillData skill;
        if (selected.m_SkillLevel == 0)
            skill = SkillRepo.GetSkillByGroupIDOfNextLevel(selected.m_skgID, selected.m_SkillLevel);
        else
            skill = selected.m_SkillData;

        m_Icon.sprite = selected.m_Icon.sprite;

        m_SkillName.text = skill.skillgroupJson.localizedname;
        //m_ActivePassive = selected.m_SkillData.skillgroupJson.ac

        if (selected.IsUpgradable())
            m_Upgrade.interactable = true;
        else
            m_Upgrade.interactable = false;

        string[] req = skill.skillJson.progressskill.Split(';');
        //show required skill points
        //check current player skill
        int skp = newskillpoint;
        int my = newmoney;
        int lv = 1;
        if (GameInfo.gLocalPlayer != null)
        {
            lv = (int)GameInfo.gLocalPlayer.PlayerStats.Level;
        }
        int skillpoint = skill.skillJson.learningsp;
        int money = skill.skillJson.learningcost;
        int level = skill.skillJson.requiredlv;

        GameObject obj = m_ReqStatsPool.RequestObject();
        m_ReqStatsLabels.Add(obj.GetComponent<UI_SkillUIRequirementHelper>());
        m_ReqStatsLabels[m_ReqStatsLabels.Count - 1].SetData(GUILocalizationRepo.GetLocalizedString("hro_skill_points"), skillpoint.ToString() + "/" + skp.ToString());
        obj.transform.SetParent(m_ReqStatsParent.transform, false);
        //obj.transform.localPosition = new Vector3(obj.transform.localPosition.x, obj.transform.localPosition.y, 0);
        obj.transform.localScale = new Vector3(1, 1, 1);

        Color color;
        if (skp < skillpoint)
            color = Color.red;
        else
            color = Color.white;
        m_ReqStatsLabels[m_ReqStatsLabels.Count - 1].SetColor(color);


        if (money > 0)
        {
            obj = m_ReqStatsPool.RequestObject();
            m_ReqStatsLabels.Add(obj.GetComponent<UI_SkillUIRequirementHelper>());
            m_ReqStatsLabels[m_ReqStatsLabels.Count - 1].SetData(GUILocalizationRepo.GetLocalizedString("currency_Money"), money.ToString() + "/" + my.ToString());
            obj.transform.SetParent(m_ReqStatsParent.transform, false);
            //obj.transform.localPosition = new Vector3(obj.transform.localPosition.x, obj.transform.localPosition.y, 0);
            obj.transform.localScale = new Vector3(1, 1, 1);

            if (my < money)
                color = Color.red;
            else
                color = Color.white;
            m_ReqStatsLabels[m_ReqStatsLabels.Count - 1].SetColor(color);
        }

        if (level > 0)
        {
            obj = m_ReqStatsPool.RequestObject();
            m_ReqStatsLabels.Add(obj.GetComponent<UI_SkillUIRequirementHelper>());
            m_ReqStatsLabels[m_ReqStatsLabels.Count - 1].SetData(GUILocalizationRepo.GetLocalizedString("skl_skill_level"), level.ToString());
            obj.transform.SetParent(m_ReqStatsParent.transform, false);
            //obj.transform.localPosition = new Vector3(obj.transform.localPosition.x, obj.transform.localPosition.y, 0);
            obj.transform.localScale = new Vector3(1, 1, 1);

            if (lv < level)
                color = Color.red;
            else
                color = Color.white;
            m_ReqStatsLabels[m_ReqStatsLabels.Count - 1].SetColor(color);
        }

        switch (skill.skillgroupJson.skilltype)
        {
            case Zealot.Common.SkillType.Active:
                m_ActivePassive.text = GUILocalizationRepo.GetLocalizedString("skl_active");
                break;
            case Zealot.Common.SkillType.Passive:
                m_ActivePassive.text = GUILocalizationRepo.GetLocalizedString("skl_passive");
                break;
        }

        m_SkillDesc.GenerateChunk(selected);
    }

    public void OnLevelUpPressed()
    {
        // notify button to level up
        m_Button.OnLevelUpSkill();
        //m_SkillDesc.GenerateChunk(m_Button);
    }

    public void OnClosed()
    {
        foreach(var obj in m_ReqStatsLabels)
        {
            obj.gameObject.transform.parent = null;
            m_ReqStatsPool.ReturnObject(obj.gameObject);
        }
        m_ReqStatsLabels.Clear();
        m_SkillDesc.RemoveChunks();
    }

    public void CloseUI()
    {
        OnClosed();
        m_Close.onClick.Invoke();
    }
}
