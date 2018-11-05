using UnityEngine;
using UnityEngine.UI;
using Zealot.Repository;

public class Achievement_ObjectiveSimpleData : Achievement_ObjectiveDataBase
{
    [SerializeField] Text achSubTypeText;

    public override void Init(AchievementObjective obj, int count)
    {
        achSubTypeText.text = AchievementRepo.GetAchievementSubTypeLocalizedName(obj.subType);
        SetProgressBarMaxText(GUILocalizationRepo.GetLocalizedString("ach_filter_completed"));  // need to set first since is disabled
        base.Init(obj, count);
    }
}