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

    public override void OnOpenWindow()
    {
        base.OnOpenWindow();

        PlayerGhost player = GameInfo.gLocalPlayer;

        var versionList = AchievementRepo.GetExternalFunctionsByTriggerType(LISAFunctionTriggerType.AchievementLV);
        for (int i = 0; i < versionList.Count; ++i)
        {
            GameObject go = ClientUtils.CreateChild(versionDataParent, versionDataPrefab);
            go.GetComponent<Achievement_FunctionData>().Init(versionList[i], toggleGroup,
                player.PlayerSynStats.AchievementLevel >= versionList[i].triggervalue, null);
        }

        var achList = AchievementRepo.GetExternalFunctionsByTriggerType(LISAFunctionTriggerType.AchievementID);
        for (int i = 0; i < achList.Count; ++i)
        {
            GameObject go = ClientUtils.CreateChild(achDataParent, achDataPrefab);
            go.GetComponent<Achievement_FunctionData>().Init(achList[i], toggleGroup,
                player.AchievementStats.IsAchievementCompletedAndClaimed(versionList[i].triggervalue), OnDataSelected);
        }
    }

    private void OnDataSelected(int value)
    {
        
    }

    public override void OnCloseWindow()
    {
        base.OnCloseWindow();
        ClientUtils.DestroyChildren(versionDataParent);
        ClientUtils.DestroyChildren(achDataParent);
        versionScrollRect.verticalNormalizedPosition = 1f;
        achScrollRect.verticalNormalizedPosition = 1f;
    }
}