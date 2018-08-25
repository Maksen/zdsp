using Kopio.JsonContracts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Repository;

public class UI_SpecialBoss_Detail : MonoBehaviour
{
    //Boss Info
    public Text BossName;
    public Text BossLevel;
    public Model_3DAvatar m3DAvatar;

    //Last Kill Info
    public Text LastKillHeader;
    public Text LastKill;
    public Button InspectRank;

    //Reward Icon
    public UI_RewardDisplay RewardDisplay;

    //Boss list
    public Transform BossListContent;
    public GameObject BossListPrefab;
    public ToggleGroup BossListToggleGroup;

    private Dictionary<int, SpecialBossStatus> mBossStatusAll;
    private BossType mBossCategorySelected = BossType.BigBoss;
    private SpecialBossStatus mBossStatusSelected;
    private SpecialBossJson mBossSelected;

    void Awake()
    {
        mBossStatusAll = new Dictionary<int, SpecialBossStatus>();
        Clear();
        LastKillHeader.text = GUILocalizationRepo.GetLocalizedString("wb_PartyScore");
    }

    void OnEnable()
    {
        UIManager.StartHourglass();
        RPCFactory.CombatRPC.GetWorldBossList();
    }

    public void OnToggleBigBossType(bool ison)
    {
        if (!ison)
            return;
        mBossCategorySelected = BossType.BigBoss;
        LastKillHeader.text = GUILocalizationRepo.GetLocalizedString("wb_PartyScore");
        RefreshAll();
    }

    public void OnToggleSmallBossType(bool ison)
    {
        if (!ison)
            return;
        mBossCategorySelected = BossType.BigBoss;
        LastKillHeader.text = GUILocalizationRepo.GetLocalizedString("wb_DamageScore");
        RefreshAll();
    }

    public void Init(Dictionary<int, SpecialBossStatus> specialBossStatus)
    {
        mBossStatusAll = specialBossStatus;
        RefreshAll();
    }

    public void RefreshAll()
    {
        Clear();

        List<SpecialBossJson> idList = SpecialBossRepo.GetOrderedBossIdsByType(mBossCategorySelected);
        int count = idList.Count;
        for (int index = 0; index < count; index++)
        {
            SpecialBossStatus status;
            if (mBossStatusAll.TryGetValue(idList[index].id, out status))
            {
                GameObject bossListData = Instantiate(BossListPrefab);
                bossListData.transform.SetParent(BossListContent, false);
                bossListData.GetComponent<UI_WorldBossList_Data>().Init(this, idList[index], status, BossListToggleGroup);
            }
        }
        StartCoroutine(TurnOnFirst());
    }

    private IEnumerator TurnOnFirst()
    {
        yield return null;
        if (BossListContent.childCount > 0)
        {
            Transform firstBoss = BossListContent.GetChild(0);
            if (firstBoss != null)
                firstBoss.GetComponent<UI_WorldBossList_Data>().Toggle.isOn = true;
        }
    }

    public void OnBossSelectionChanged(SpecialBossJson bossJson, SpecialBossStatus status)
    {
        mBossStatusSelected = status;
        mBossSelected = bossJson;
        CombatNPCJson npcInfo = CombatNPCRepo.GetNPCById(mBossSelected.archetypeid);
        BossName.text = npcInfo.localizedname;
        BossLevel.text = npcInfo.level.ToString();
        m3DAvatar.Change(npcInfo.containerprefabpath, OnModelLoaded);
        if (status != null)
        {
            if (!string.IsNullOrEmpty(status.killer))
            {
                LastKill.text = status.killer + "\n" + status.score;
                InspectRank.interactable = true;
            }
            else
            {
                LastKill.text = "";
                InspectRank.interactable = false;
            }
        }
        RewardDisplay.Init(RewardListRepo.GetRewardItemsByGrpIDJobID(mBossSelected.displayrewardgroupid, -1));
    }

    private void OnModelLoaded(GameObject model)
    {
        float[] camera = StaticNPCRepo.ParseCameraPosInTalk(mBossSelected.cameraposintalk);
        Vector3 pos = model.transform.localPosition;
        model.transform.localPosition = new Vector3(camera[0], camera[1], pos.z);
        model.transform.localRotation = Quaternion.Euler(new Vector3(0, camera[2], 0));
        model.transform.localScale = new Vector3(camera[3], camera[3], camera[3]);
    }

    public void OnClick_InspectRank()
    {
        if (mBossStatusSelected != null)
        {
            if (!string.IsNullOrEmpty(mBossStatusSelected.killer))
            {
                UIManager.StartHourglass();
                RPCFactory.CombatRPC.GetWorldBossDmgList(mBossStatusSelected.id);
            }
        }
    }

    void Clear()
    {
        BossName.text = "";
        BossLevel.text = "";
        m3DAvatar.Cleanup();
        LastKill.text = "";
        InspectRank.interactable = false;
        RewardDisplay.Init(null);
        foreach (Transform child in BossListContent)
            Destroy(child.gameObject);
        mBossStatusSelected = null;
        mBossSelected = null;
    }
}
