using Kopio.JsonContracts;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UIAddons;
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
    //public GameObject DropPrefab;

    public GameObject mConsumablePrefab;
    public GameObject mEquipIconPrefab;
    public GameObject mGeneIconPrefab;
    public GameObject mMaterialPrefab;

    //Boss Status Display List
    public Toggle BigBossToggle;
    public Toggle MiniBossToggle;

    public GameObject BossListContent;
    public GameObject BossListPrefab;

    private Dictionary<int, SpecialBossStatus> worldBossList;
    private List<GameObject> bigBossObjList;
    private List<GameObject> miniBossObjList;

    UI_DamangeRankData UI_DamangeRankData;

    Model_3DAvatar m3DAvatar;

    public bool BigBossCategory;
    public bool MiniBossCategory;

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
        DrawRewardListGameIcons(bigBossData.rewardIdGroupid);

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
            BigBossToggle.isOn = true;
            BigBossCategory = true;
            MiniBossCategory = false;

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

            GameObject bigBossDataObj = bigBossObjList[0];
            BossListData bigBossData = bigBossDataObj.GetComponent<BossListData>();
            GetBossListData(bigBossData.boss_id);
        }
    }

    public void MiniBossOnClick(bool MiniBossClick)
    {
        if (MiniBossClick)
        {
            ClearBigBossListData();
            ClearMiniBossListData();
            MiniBossToggle.isOn = true;
            BigBossCategory = false;
            MiniBossCategory = true;

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

            GameObject MiniBossDataObj = miniBossObjList[0];
            BossListData bigBossData = MiniBossDataObj.GetComponent<BossListData>();
            GetBossListData(bigBossData.boss_id);
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

    public void DrawRewardListGameIcons(int rewardIdGroupid)
    {
        ClearRewardIcons();

        //取得獎勵組的所有獎勵
        var rewardGroupDic = RewardListRepo.GetRewardDicByGrpId(rewardIdGroupid);

        if (rewardGroupDic != null)
        {
            foreach (var reward in rewardGroupDic.Values)
            {
                for (int i = 0; i < reward.itemRewardLst.Count; ++i)
                {
                    ItemBaseJson itemData = GameRepo.ItemFactory.GetItemById(reward.itemRewardLst[i].id);

                    switch (itemData.bagtype)
                    {
                        case BagType.Consumable:
                            GameObject obj = CreateIcon(mConsumablePrefab);
                            GameIcon_MaterialConsumable mcIcon = obj.GetComponent<GameIcon_MaterialConsumable>();
                            //mcIcon.Init(itemData.itemid, reward.itemRewardLst.Count , false);
                            break;
                        case BagType.DNA:
                            GameObject obj2 = CreateIcon(mConsumablePrefab);
                            GameIcon_DNA mcIcon2 = obj2.GetComponent<GameIcon_DNA>();
                            //mcIcon2.Init(itemData.itemid,0,0);
                            break;
                        case BagType.Equipment:
                            GameObject obj3 = CreateIcon(mConsumablePrefab);
                            GameIcon_Equip mcIcon3 = obj3.GetComponent<GameIcon_Equip>();
                            //mcIcon3.Init(itemData.itemid);
                            break;
                        case BagType.Gem:
                            GameObject obj4 = CreateIcon(mConsumablePrefab);
                            GameIcon_DNA mcIcon4 = obj4.GetComponent<GameIcon_DNA>();
                            //mcIcon4.Init(itemData.itemid, 0, 0);
                            break;
                        case BagType.Material:
                            GameObject obj5 = CreateIcon(mConsumablePrefab);
                            GameIcon_MaterialConsumable mcIcon5 = obj5.GetComponent<GameIcon_MaterialConsumable>();
                            //mcIcon5.Init(itemData.itemid, reward.itemRewardLst.Count, false);
                            break;
                    }
                }
            }
        }
        
    }

    private GameObject CreateIcon(GameObject prefab)
    {
        GameObject obj = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        obj.transform.SetParent(DropContent.transform, false);
        return obj;
    }

    private void ClearRewardIcons()
    {
        if (DropContent != null)
        {
            foreach (Transform t in DropContent.transform)
                Destroy(t.gameObject);
        }
    }

    void OnDisable()
    {
        BigBossToggle.isOn = true;
        MiniBossToggle.isOn = false;
    }

    void OnDestroy()
    {
        BossName = null;
        DropContent = null;
        BossListContent = null;
        BossListPrefab = null;
    }
}
