using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Client.Entities;
using Zealot.Common;
using Zealot.Repository;

public class UI_Achievement_ExternalFunctionsDialog : BaseWindowBehaviour
{
    [SerializeField] ToggleGroup toggleGroup;
    [SerializeField] Transform versionDataParent;
    [SerializeField] GameObject versionDataPrefab;
    [SerializeField] Transform achDataParent;
    [SerializeField] GameObject achDataPrefab;
    [SerializeField] ScrollRect versionScrollRect;
    [SerializeField] ScrollRect achScrollRect;

    private List<Achievement_FunctionData> versionDataList = new List<Achievement_FunctionData>();
    private List<Achievement_FunctionAchData> achDataList = new List<Achievement_FunctionAchData>();
    private bool initialized;

    public override void OnOpenWindow()
    {
        base.OnOpenWindow();

        PlayerGhost player = GameInfo.gLocalPlayer;

        if (!initialized)
        {
            var versionList = AchievementRepo.GetExternalFunctionsByTriggerType(LISAFunctionTriggerType.AchievementLV);
            for (int i = 0; i < versionList.Count; ++i)
            {
                GameObject go = ClientUtils.CreateChild(versionDataParent, versionDataPrefab);
                Achievement_FunctionData data = go.GetComponent<Achievement_FunctionData>();
                data.Init(versionList[i], toggleGroup, player.PlayerSynStats.AchievementLevel >= versionList[i].triggervalue);
                versionDataList.Add(data);
            }

            var achList = AchievementRepo.GetExternalFunctionsByTriggerType(LISAFunctionTriggerType.AchievementID);
            for (int i = 0; i < achList.Count; ++i)
            {
                GameObject go = ClientUtils.CreateChild(achDataParent, achDataPrefab);
                Achievement_FunctionAchData data = go.GetComponent<Achievement_FunctionAchData>();
                data.Init(achList[i], toggleGroup, player.AchievementStats.IsAchievementCompletedAndClaimed(achList[i].triggervalue));
                achDataList.Add(data);
            }

            initialized = true;
        }
        else
        {
            for (int i = 0; i < versionDataList.Count; ++i)
                versionDataList[i].Refresh();

            for (int i = 0; i < achDataList.Count; ++i)
                achDataList[i].Refresh();
        }
    }

    public override void OnCloseWindow()
    {
        base.OnCloseWindow();
        versionScrollRect.verticalNormalizedPosition = 1f;
        achScrollRect.verticalNormalizedPosition = 1f;

        bool allowSwitchOff = toggleGroup.allowSwitchOff;
        toggleGroup.allowSwitchOff = true;
        for (int i = 0; i < versionDataList.Count; ++i)
            versionDataList[i].SetToggleOn(false);
        for (int i = 0; i < achDataList.Count; ++i)
            achDataList[i].SetToggleOn(false);
        toggleGroup.allowSwitchOff = allowSwitchOff;
    }
}