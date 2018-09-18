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


    private GameObjectPoolManager m_ReqStatsPool;

    private List<UI_SkillUIRequirementHelper> m_ReqStatsLabels;

    public void Initialise(Transform parent)
    {
        //create pool of required stats holder for use
        m_ReqStatsPool = new GameObjectPoolManager(5, parent, m_ReqStatsPrefab);
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

        m_SkillName.text = skill.skillgroupJson.name;
        //m_ActivePassive = selected.m_SkillData.skillgroupJson.ac

        if (selected.IsUpgradable())
            m_Upgrade.interactable = true;
        else
            m_Upgrade.interactable = false;

        string[] req = skill.skillJson.progressskill.Split(';');
        //show required skill points
        //check current player skill
        int skp = 1000;
        int my = 1000;
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
        m_ReqStatsLabels[m_ReqStatsLabels.Count - 1].SetData("技能點數 ", skillpoint.ToString() + "/"  + skp.ToString());
        obj.transform.parent = m_ReqStatsParent.transform;
        obj.transform.localPosition = new Vector3(obj.transform.localPosition.x, obj.transform.localPosition.y, 0);
        obj.transform.localScale = new Vector3(1, 1, 1);

        Color color;
        if (skp < skillpoint)
            color = Color.red;
        else
            color = Color.white;
        m_ReqStatsLabels[m_ReqStatsLabels.Count - 1].SetColor(color);

        obj = m_ReqStatsPool.RequestObject();
        m_ReqStatsLabels.Add(obj.GetComponent<UI_SkillUIRequirementHelper>());
        m_ReqStatsLabels[m_ReqStatsLabels.Count - 1].SetData("Money (temp) ", money.ToString() + "/" + my.ToString());
        obj.transform.parent = m_ReqStatsParent.transform;
        obj.transform.localPosition = new Vector3(obj.transform.localPosition.x, obj.transform.localPosition.y, 0);
        obj.transform.localScale = new Vector3(1, 1, 1);

        if (my < money)
            color = Color.red;
        else
            color = Color.white;
        m_ReqStatsLabels[m_ReqStatsLabels.Count - 1].SetColor(color);

        obj = m_ReqStatsPool.RequestObject();
        m_ReqStatsLabels.Add(obj.GetComponent<UI_SkillUIRequirementHelper>());
        m_ReqStatsLabels[m_ReqStatsLabels.Count - 1].SetData("等級需 ", level.ToString());
        obj.transform.parent = m_ReqStatsParent.transform;
        obj.transform.localPosition = new Vector3(obj.transform.localPosition.x, obj.transform.localPosition.y, 0);
        obj.transform.localScale = new Vector3(1, 1, 1);

        if(lv < level)
            color = Color.red;
        else
            color = Color.white;
        m_ReqStatsLabels[m_ReqStatsLabels.Count - 1].SetColor(color);

        switch (skill.skillgroupJson.skilltype)
        {
            case Zealot.Common.SkillType.Active:
                m_ActivePassive.text = "主動";
                break;
            case Zealot.Common.SkillType.Passive:
                m_ActivePassive.text = "被動";
                break;
        }

        if (req[0].CompareTo("#unnamed#") != 0 && req[0].CompareTo("") != 0)
            foreach(string requirement in req)
            {
                // show required skill level
                GameObject reqObj = m_ReqStatsPool.RequestObject();
                m_ReqStatsLabels.Add(reqObj.GetComponent<UI_SkillUIRequirementHelper>());
                int id = 0;
                System.Int32.TryParse(requirement, out id);
                string name = SkillRepo.GetSkill(id).skillJson.name;
                m_ReqStatsLabels[m_ReqStatsLabels.Count - 1].SetData(name);
                reqObj.transform.parent = m_ReqStatsParent.transform;
                reqObj.transform.localPosition = new Vector3(0, 0, 1);
                reqObj.transform.localScale = new Vector3(1, 1, 1);
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
        m_Close.onClick.Invoke();
    }
}
