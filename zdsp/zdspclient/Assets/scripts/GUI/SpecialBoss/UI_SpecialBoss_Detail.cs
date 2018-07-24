using Kopio.JsonContracts;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Entities;
using Zealot.Repository;

public class UI_SpecialBoss_Detail : BaseWindowBehaviour {

    //Boss Info
    public Text BossName;
    public Text BossGrade;

    public Text ScoreName;
    public Text PlayerScoreName;
    public Button DamageScoreDetails;

    //Reward Icon
    public GameObject DropContent;
    public GameObject DropPrefab;

    //Boss Status Display List
    public Toggle BigBossToggle;
    public Toggle MiniBossToggle;

    public GameObject BossListContent;
    public GameObject BossListPrefab;

    private GameObject[] mDrop_Children;
    private Dictionary<int, GameObject> mBossList_Children;

    private Dictionary<int, SpecialBossStatus> worldBossList;
    private List<GameObject> bigBossObjList;
    private List<GameObject> miniBossObjList;

    UI_DamangeRankData UI_DamangeRankData;

    Model_3DAvatar m3DAvatar;

    void OnEnable()
    {
        BigBossToggle.onValueChanged.RemoveAllListeners();
        BigBossToggle.onValueChanged.AddListener(BigBossOnClick);

        MiniBossToggle.onValueChanged.RemoveAllListeners();
        MiniBossToggle.onValueChanged.AddListener(MiniBossOnClick);

        RPCFactory.CombatRPC.GetWorldBossList();

        UIManager.StartHourglass();
    }

    private void GetWorldBossList()
    {
        RPCFactory.CombatRPC.GetWorldBossList();
    }

    public void InitWorldBossList(UI_DamangeRankData ui_DamangeRankData , Dictionary<int, SpecialBossStatus> specialBossStatus, Model_3DAvatar model_3DAvatar)
    {
        UI_DamangeRankData = ui_DamangeRankData;

        worldBossList = specialBossStatus;

        m3DAvatar = model_3DAvatar;

        BigBossOnClick(true);

        GameObject bigBossDataObj = bigBossObjList[0];
        BossListData bigBossData = bigBossDataObj.GetComponent<BossListData>();
        GetBossListData(bigBossData.boss_id);

        UIManager.StopHourglass();
    }

    public void GetBossListData(int boss_id)
    {
        var boss_info = SpecialBossRepo.GetInfoById(boss_id);
        var npc_info = NPCRepo.GetArchetypeById(boss_info.archetypeid);
        BossName.text = npc_info.localizedname;
        BossGrade.text = npc_info.level.ToString();
        m3DAvatar.Change(npc_info.modelprefabpath);
    }

    public void BossRankOnClick()
    {
        UIManager.OpenDialog(WindowType.DialogWorldBossRanking);
        UI_DamangeRankData.InitDamangeRank();
    }

    public void BigBossOnClick(bool BigBossClick)
    {
        if(BigBossClick)
        {
            ClearBigBossListData();
            ClearMiniBossListData();
            ScoreName.text = GUILocalizationRepo.GetLocalizedString("wb_PartyScore");

            List<SpecialBossStatus> bigBosses = new List<SpecialBossStatus>();
            foreach(KeyValuePair<int, SpecialBossStatus> status in worldBossList)
            {
                SpecialBossJson boss = SpecialBossRepo.GetInfoById(status.Value.id);

                if (boss.category == BossCategory.BIGBOSS)
                {
                    bigBosses.Add(status.Value);
                }
            }

            for(int i = 0; i < bigBosses.Count; ++i)
            {
                GameObject newWorldBossListObj = Instantiate(BossListPrefab);
                newWorldBossListObj.transform.SetParent(BossListContent.transform, false);

                BossListData bossListData = newWorldBossListObj.GetComponent<BossListData>();
                bossListData.InitBossList(this, bigBosses[i]);

                bigBossObjList.Add(newWorldBossListObj);
            }
        }
    }

    public void MiniBossOnClick(bool MiniBossClick)
    {
        if (MiniBossClick)
        {
            ClearBigBossListData();
            ClearMiniBossListData();
            ScoreName.text = GUILocalizationRepo.GetLocalizedString("wb_DamangeScore");

            List<SpecialBossStatus> miniBosses = new List<SpecialBossStatus>();
            foreach (KeyValuePair<int, SpecialBossStatus> status in worldBossList)
            {
                SpecialBossJson boss = SpecialBossRepo.GetInfoById(status.Value.id);

                if (boss.category == BossCategory.BOSS)
                {
                    miniBosses.Add(status.Value);
                }
            }

            for (int i = 0; i < miniBosses.Count; ++i)
            {
                GameObject newMiniBossListObj = Instantiate(BossListPrefab);
                newMiniBossListObj.transform.SetParent(BossListContent.transform, false);

                BossListData bossListData = newMiniBossListObj.GetComponent<BossListData>();
                bossListData.InitBossList(this, miniBosses[i]);

                miniBossObjList.Add(newMiniBossListObj);
            }
        }
    }

    void ClearBigBossListData()
    {
        if (bigBossObjList == null)
        {
            bigBossObjList = new List<GameObject>();
            return;
        }

        for (int i = 0; i < bigBossObjList.Count; ++i)
        {
            Destroy(bigBossObjList[i]);
            bigBossObjList[i] = null;
        }
        bigBossObjList.Clear();
    }

    void ClearMiniBossListData()
    {
        if (miniBossObjList == null)
        {
            miniBossObjList = new List<GameObject>();
            return;
        }

        for (int i = 0; i < miniBossObjList.Count; ++i)
        {
            Destroy(miniBossObjList[i]);
            miniBossObjList[i] = null;
        }
        miniBossObjList.Clear();
    }

    public void DrawRewardListGameIcons(int rewardid)
    {
        GameRepo.ItemFactory.GetItemById(rewardid);
    }

    void OnDestroy()
    {
        BossName = null;
        DropContent = null;
        DropPrefab = null;
        BossListContent = null;
        BossListPrefab = null;
    }
}
