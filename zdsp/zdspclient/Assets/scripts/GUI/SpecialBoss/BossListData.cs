using Kopio.JsonContracts;
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
    public int rewardIdGroupid;
    public Toggle BossListToggle;
    UI_SpecialBoss_Detail BossListClickData;
    UI_DamangeRankData UI_DamangeRankData;

    private long mDayWeeklyTime;
    private double mDoubleConvert;

    void Awake()
    {
        BossListToggle.onValueChanged.RemoveAllListeners();
        BossListToggle.onValueChanged.AddListener(BossListOnClick);
    }

    public long DayWeeklyTime
    {
        get { return mDayWeeklyTime; }
        set { mDayWeeklyTime = value; }
    }

    public double DoubleConvert
    {
        get { return mDoubleConvert; }
        set { mDoubleConvert = value; }
    }

    public string BossSpawnTime
    {
        get { return SpawnTime.text; }
        set
        {
            SpawnTime.text = value;
        }
    }

    public void RefreshWorldBossListData(UI_SpecialBoss_Detail uiSpecialBossDetails, UI_DamangeRankData uiDamangeRankData , Dictionary<int, SpecialBossStatus> specialBossStatus)
    {
        foreach (KeyValuePair<int, SpecialBossStatus> status in specialBossStatus)
        {
            InitBossList(uiSpecialBossDetails, uiDamangeRankData, status.Value);
        }

        UIManager.StopHourglass();
    }

    public void InitBossList(UI_SpecialBoss_Detail uiSpecialBossDetails, UI_DamangeRankData uiDamangeRankData, SpecialBossStatus bossStatus)
    {
        BossListClickData = uiSpecialBossDetails;

        UI_DamangeRankData = uiDamangeRankData;

        boss_id = bossStatus.id;
        var boss_info = SpecialBossRepo.GetInfoById(boss_id);

        //取得獎勵組ID
        rewardIdGroupid = boss_info.displayrewardgroupid;

        string scene_name = LevelReader.GetSpecialBossLocationData(boss_id).mLevel;
        var level_info = LevelRepo.GetInfoByName(scene_name);
        LevelName.text = level_info.localizedname;

        Sprite img = ClientUtils.LoadIcon(boss_info.icon);
        if (img != null)
            BossIcon.sprite = img;
        BossIcon = null;

        bool foundNext = false;

        if (bossStatus != null && bossStatus.isAlive)
        {
            SpawnTime.text = GUILocalizationRepo.GetLocalizedString("wb_isAlive");
        }
        else
        {
            switch (boss_info.spawntype)
            {
                case BossSpawnType.SpawnDaily:
                    DayWeeklyTime = (GameUtils.TimeToNextEventDailyFormat(GameInfo.GetSynchronizedServerDT(), boss_info.spawndaily, out foundNext, 5000)/1000);
                    mDoubleConvert = Convert.ToDouble(DayWeeklyTime);
                    BossSpawnTime = GUILocalizationRepo.GetShortLocalizedTimeString(mDoubleConvert);
                    break;
                case BossSpawnType.SpawnWeekly:
                    DayWeeklyTime = (GameUtils.TimeToNextEventWeeklyFormat(GameInfo.GetSynchronizedServerDT(), boss_info.spawnweekly, out foundNext, 5000)/1000);
                    mDoubleConvert = Convert.ToDouble(DayWeeklyTime);
                    BossSpawnTime = GUILocalizationRepo.GetShortLocalizedTimeString(mDoubleConvert);
                    break;
                case BossSpawnType.SpawnDuration:
                    if (bossStatus.nextSpawn.HasValue == true)
                    {
                        mDoubleConvert = (bossStatus.nextSpawn.Value - GameInfo.GetSynchronizedServerDT()).TotalSeconds;
                        BossSpawnTime = GUILocalizationRepo.GetShortLocalizedTimeString(mDoubleConvert);
                    }
                    break;
            }
        }
    }

    public void BossListOnClick(bool BossListClick)
    {
        if (BossListClick)
        {
            BossListClickData.GetBossListData(boss_id);
            BossListClickData.DrawRewardListGameIcons(rewardIdGroupid);
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