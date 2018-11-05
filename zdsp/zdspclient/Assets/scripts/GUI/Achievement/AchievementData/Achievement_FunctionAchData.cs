using Kopio.JsonContracts;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Repository;

public class Achievement_FunctionAchData : Achievement_FunctionData
{
    [SerializeField] Achievement_ObjectiveSimpleData achObjData;

    public override void Init(LISAExternalFunctionJson data, ToggleGroup toggleGroup, bool unlocked)
    {
        base.Init(data, toggleGroup, unlocked);
        InitAchievementDetails(data.triggervalue);
    }

    public override void Refresh()
    {
        AchievementStatsClient achStats = GameInfo.gLocalPlayer.AchievementStats;
        SetUnlocked(achStats.IsAchievementCompletedAndClaimed(triggerValue));
        AchievementElement elem = achStats.GetAchievementById(triggerValue);
        achObjData.UpdateProgress(elem == null ? 0 : elem.Count);
    }

    private void InitAchievementDetails(int id)
    {
        AchievementObjective obj = AchievementRepo.GetAchievementObjectiveById(id);
        if (obj != null)
        {
            nameText.text = obj.localizedName.Replace("{count}", obj.completeCount.ToString());
            AchievementElement elem = GameInfo.gLocalPlayer.AchievementStats.GetAchievementById(id);
            achObjData.Init(obj, elem == null ? 0 : elem.Count);
        }
    }
}