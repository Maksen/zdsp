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

    public UI_SkillTree m_Parent { get; set; }
    private GameObjectPoolManager m_SpecialRowPool;
    private GameObjectPoolManager m_SkillIconPool;

    public void Initialise(Transform parent)
    {
        m_SpecialRowPool = new GameObjectPoolManager(3, parent, m_SpecialSkillRow);
        m_SkillIconPool = new GameObjectPoolManager(9, parent, m_SkillIconData);
    }

    public void GenerateList(JobType job)
    {
        Zealot.Common.Datablock.CollectionHandler<object> skill = null;
        if (GameInfo.gLocalPlayer != null)
            skill = GameInfo.gLocalPlayer.SkillStats.SkillInv;

        List<KeyValuePair<int, int>> temp = new List<KeyValuePair<int, int>>();

        for (int i = 0; i < skill.Count >> 1; i += 2)
        {
            if ((int)skill[i] == 0) break;
            SkillData skd = SkillRepo.GetSkill((int)skill[i + 1]);
            if (skd.skillgroupJson.skilltype == Zealot.Common.SkillType.Active &&
                skd.skillgroupJson.skillclass == SkillClass.Special)
                temp.Add(new KeyValuePair<int, int>((int)skill[i], (int)skill[i + 1]));

            // Add condition checking for class skill
        }
    }
}
