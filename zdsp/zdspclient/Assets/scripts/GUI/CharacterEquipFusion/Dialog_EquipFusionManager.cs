using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Dialog_EquipFusionManager : MonoBehaviour {

    [SerializeField]
    private GameObject confirmPrefab;

    [SerializeField]
    private FusionData_ConfirmItem beforeFusionDataConfirm;
    [SerializeField]
    private FusionData_ConfirmItem afterFusionDataConfirm;

    public Button confirmChangeFusion;
    public Button cancelConfirm;

    public void EnterUI(int itemId, List<string> itemStats, List<string> beforeStats, List<string> afterStats)
    {
        beforeFusionDataConfirm.Init(confirmPrefab, itemId, itemStats[0], itemStats[1], itemStats[2], itemStats[3], beforeStats);
        afterFusionDataConfirm.Init(confirmPrefab, itemId, itemStats[0], itemStats[1], itemStats[2], itemStats[3], afterStats);
    }
}
