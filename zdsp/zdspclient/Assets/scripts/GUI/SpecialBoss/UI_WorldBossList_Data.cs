using Kopio.JsonContracts;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Repository;

public class UI_WorldBossList_Data : MonoBehaviour
{
    public Sprite[] BossTypeSprites;

    public Toggle Toggle;
    public Image MiniIcon;
    public Image BossIcon;
    public Text LevelName;
    public Text SpawnTime;

    private SpecialBossJson mBossJson;
    private SpecialBossStatus mBossStatus;
    private UI_SpecialBoss_Detail mBossDetail;

    public void Init(UI_SpecialBoss_Detail uiSpecialBossDetails, SpecialBossJson boss_info, SpecialBossStatus bossStatus, ToggleGroup group)
    {
        mBossJson = boss_info;
        mBossStatus = bossStatus;
        mBossDetail = uiSpecialBossDetails;

        Toggle.group = group;
        var level_info = LevelRepo.GetInfoById(boss_info.location);
        LevelName.text = level_info.localizedname;
        MiniIcon.sprite = boss_info.bosstype == BossType.MiniBoss ? BossTypeSprites[0] : BossTypeSprites[1];
        BossIcon.sprite = ClientUtils.LoadIcon(boss_info.icon);

        if (bossStatus.isAlive)
            SpawnTime.text = GUILocalizationRepo.GetLocalizedString("wb_isAlive");
        else
        {
            bool foundNext;
            switch (boss_info.spawntype)
            {
                case BossSpawnType.SpawnDaily:
                    {
                        long timetoNextEvent = GameUtils.TimeToNextEventDailyFormat(GameInfo.GetSynchronizedServerDT(), boss_info.spawndaily, out foundNext, 0);
                        if (foundNext)
                            SpawnTime.text = GUILocalizationRepo.GetShortLocalizedTimeString(timetoNextEvent / 1000);
                        else
                            SpawnTime.text = "";
                        break;
                    }
                case BossSpawnType.SpawnWeekly:
                    {
                        long timetoNextEvent = GameUtils.TimeToNextEventWeeklyFormat(GameInfo.GetSynchronizedServerDT(), boss_info.spawnweekly, out foundNext, 0);
                        if (foundNext)
                            SpawnTime.text = GUILocalizationRepo.GetShortLocalizedTimeString(timetoNextEvent / 1000);
                        else
                            SpawnTime.text = "";
                        break;
                    }
                case BossSpawnType.SpawnDuration:
                    if (bossStatus.nextSpawn.HasValue == true)
                    {
                        var totalSeconds = (bossStatus.nextSpawn.Value - GameInfo.GetSynchronizedServerDT()).TotalSeconds;
                        SpawnTime.text = GUILocalizationRepo.GetShortLocalizedTimeString(totalSeconds);
                    }
                    else
                        SpawnTime.text = "";
                    break;
            }
        }
    }

    public void OnToggleChanged(bool ison)
    {
        if (!ison)
            return;
        mBossDetail.OnBossSelectionChanged(mBossJson, mBossStatus);
    }
}