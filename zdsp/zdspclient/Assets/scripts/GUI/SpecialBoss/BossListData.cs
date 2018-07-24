using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Entities;
using Zealot.Repository;

public class BossListData : BaseWindowBehaviour
{
    public Text LevelName;
    public Text SpawnTime;
    public Image BossIcon;
    public Material DeadMaterial;

    public int boss_id;
    public int rewardid;
    public Toggle BossListToggle;
    private UI_SpecialBoss_Detail BossListClickData;

    //public GameObject SpawnTimeObj;

    void Awake()
    {
        BossListToggle.onValueChanged.RemoveAllListeners();
        BossListToggle.onValueChanged.AddListener(BossListOnClick);
    }

    public void RefreshWorldBossListData(UI_SpecialBoss_Detail  uiSpecialBossDetails, Dictionary<int, SpecialBossStatus> specialBossStatus)
    {
        foreach (KeyValuePair<int, SpecialBossStatus> status in specialBossStatus)
        {
            InitBossList(uiSpecialBossDetails, status.Value);
        }

        UIManager.StopHourglass();
    }

    public void InitBossList(UI_SpecialBoss_Detail uiSpecialBossDetails, SpecialBossStatus bossStatus)
    {
        BossListClickData = uiSpecialBossDetails;

        boss_id = bossStatus.id;
        var boss_info = SpecialBossRepo.GetInfoById(boss_id);

        rewardid = boss_info.displayrewardgroupid;
        BossListClickData.DrawRewardListGameIcons(rewardid);

        string scene_name = LevelReader.GetSpecialBossLocationData(boss_id).mLevel;
        var level_info = LevelRepo.GetInfoByName(scene_name);
        LevelName.text = level_info.localizedname;

        Sprite img = ClientUtils.LoadIcon(boss_info.icon);
        if (img != null)
            BossIcon.sprite = img;
        BossIcon = null;

        if (bossStatus != null && bossStatus.isAlive)
        {
            SpawnTime.text = "活躍中";
        }
        else
        {
            if (bossStatus.nextSpawn.HasValue == true)
            {
                SpawnTime.text = GUILocalizationRepo.GetLocalizedDateTime(bossStatus.nextSpawn.Value , 3);
            }
        }
    }

    public void BossListOnClick(bool BossListClick)
    {
        if (BossListClick)
        {
            BossListClickData.GetBossListData(boss_id);
        }
    }

    void OnDestroy()
    {
        LevelName = null;
        SpawnTime = null;
        BossIcon = null;
        DeadMaterial = null;
    }
}
