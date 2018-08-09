using Kopio.JsonContracts;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Repository;

public class Hud_DestinyNews : MonoBehaviour
{
    [SerializeField]
    Text Message;

    private ActivatedClueData mClueData;

    public void UpdateClue(ActivatedClueData clueData)
    {
        mClueData = clueData;
        ClueType type = (ClueType)clueData.ClueType;
        DestinyClueJson clueJson = DestinyClueRepo.GetDestinyClueById(mClueData.ClueId);
        if (clueJson != null)
        {
            Message.text = clueJson.message;
        }
    }

    public void OnClickOk()
    {
        if (mClueData != null)
        {
            UIManager.OpenWindow(WindowType.Destiny);
        }
        mClueData = null;
    }
}
