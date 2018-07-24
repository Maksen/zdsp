using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Repository;

public class UI_SkillUISKLHelper : MonoBehaviour {

    public List<GameObject> m_Prefabs;
    // 0 --> level
    // 1 --> Stat lines
    // 3 --> cool down
    // 4 --> separator

    private GameObjectPoolManager SKLPoolLv;
    private GameObjectPoolManager SKLPoolDescriptor;
    private GameObjectPoolManager SKLPoolCD;
    private GameObject SKLLine;

    private List<SKLChunk> m_Chunks = new List<SKLChunk>();

    private struct SKLChunk
    {
        public HUD_ItemDetail_TextColonValue m_Level;
        public Text m_Descriptor; // --> 2 lines, take note
        public HUD_SkillCoolDown m_CoolDown;
    }

    public void Initialise(Transform parent)
    {
        SKLPoolLv = new GameObjectPoolManager(3, parent, m_Prefabs[0]);
        SKLPoolDescriptor = new GameObjectPoolManager(3, parent, m_Prefabs[1]);
        SKLPoolCD = new GameObjectPoolManager(3, parent, m_Prefabs[2]);
        SKLLine = Instantiate(m_Prefabs[3]);
    }

    public void GenerateChunk(UI_SkillButton reference)
    {
        if (reference.m_SkillLevel == 0)
        {
            ChunkHelper(SkillRepo.GetSkillByGroupIDOfNextLevel(reference.m_ID, 0), 1);
        }
        else
        {
            // remove previous chunks
            //foreach(SKLChunk chunk in m_Chunks)
            //{
            //    chunk.m_Level.transform.parent = null;
            //    chunk.m_Descriptor.transform.parent = null;
            //    chunk.m_CoolDown.transform.parent = null;
            //}
            //SKLLine.transform.parent = null;

            //m_Chunks.Clear();

            RemoveChunks();

            // load level higher version
            ChunkHelper(reference.m_SkillData, reference.m_SkillLevel);

            SkillData skill = SkillRepo.GetSkillByGroupIDOfNextLevel(reference.m_SkillData.skillgroupJson.id, reference.m_SkillData.skillJson.level);

            // check for valid level
            if (!SkillRepo.IsSkillMaxLevel(reference.m_SkillData.skillgroupJson.id, reference.m_SkillLevel))
            {
                SKLLine.transform.parent = this.transform;
                SKLLine.transform.localPosition = new Vector3(SKLLine.transform.localPosition.x, SKLLine.transform.localPosition.y, 0);
                SKLLine.transform.localScale = new Vector3(1, 1, 1);
                ChunkHelper(skill, reference.m_SkillLevel + 1);
                SKLLine.gameObject.SetActive(true);
            }
            else
                SKLLine.gameObject.SetActive(false);
        }
    }

    public void ChunkHelper(SkillData skilldata, int level)
    {
        SKLChunk chunk = new SKLChunk();

        // generate level text
        HUD_ItemDetail_TextColonValue lv = SKLPoolLv.RequestObject().GetComponent<HUD_ItemDetail_TextColonValue>();
        chunk.m_Level = lv;
        chunk.m_Level.Value = level.ToString() + "/" + "10";
        chunk.m_Level.transform.parent = this.transform;
        chunk.m_Level.transform.localPosition = new Vector3(chunk.m_Level.transform.localPosition.x, chunk.m_Level.transform.localPosition.y, 0);
        chunk.m_Level.transform.localScale = new Vector3(1, 1, 1);

        // fetch the level stuff from somewhere???
        // generate descriptor
        Text des = SKLPoolDescriptor.RequestObject().GetComponentInChildren<Text>();
        des.text = "對目標造成XXXXXX點物理傷害"; // stand in text only
        des.text += "\n對目標造成XXXXXX點物理傷害";
        chunk.m_Descriptor = des;
        chunk.m_Descriptor.transform.parent = this.transform;
        chunk.m_Descriptor.transform.localPosition = new Vector3(chunk.m_Descriptor.transform.localPosition.x, chunk.m_Descriptor.transform.localPosition.y, 0);
        chunk.m_Descriptor.transform.localScale = new Vector3(1, 1, 1);

        // generate cooldown
        HUD_SkillCoolDown cd = SKLPoolCD.RequestObject().GetComponent<HUD_SkillCoolDown>();
        chunk.m_CoolDown = cd;
        chunk.m_CoolDown.m_CDName.text = "冷卻時間";
        chunk.m_CoolDown.m_CDValue.text = skilldata.skillJson.cooldown.ToString();
        chunk.m_CoolDown.m_UseName.text = "技能消耗";
        chunk.m_CoolDown.m_UseValue.text = skilldata.skillJson.cost.ToString();
        chunk.m_CoolDown.transform.parent = this.transform;
        chunk.m_CoolDown.transform.localPosition = new Vector3(chunk.m_CoolDown.transform.localPosition.x, chunk.m_CoolDown.transform.localPosition.y, 0);
        chunk.m_CoolDown.transform.localScale = new Vector3(1, 1, 1);

        m_Chunks.Add(chunk);
    }

    public void RemoveChunks()
    {
        foreach(SKLChunk chunk in m_Chunks)
        {
            chunk.m_Level.transform.parent = null;
            chunk.m_Descriptor.transform.parent = null;
            chunk.m_CoolDown.transform.parent = null;
            SKLPoolLv.ReturnObject(chunk.m_Level.gameObject);
            SKLPoolDescriptor.ReturnObject(chunk.m_Descriptor.gameObject);
            SKLPoolCD.ReturnObject(chunk.m_CoolDown.gameObject);
        }
        SKLLine.transform.parent = null;
        m_Chunks.Clear();
    }
}
