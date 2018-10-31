using Kopio.JsonContracts;
using UnityEngine;
using UnityEngine.UI;

public class Hud_QuestStart : MonoBehaviour
{
    [SerializeField]
    Text QuestName;

    private QuestJson mQuestJson;

    public void Init(QuestJson questJson)
    {
        mQuestJson = questJson;
        QuestName.text = questJson.questname;
    }

    public void OnOpenDestiny()
    {
        UIManager.OpenWindow(WindowType.Quest, (window) => { window.GetComponent<UI_Quest>().OpenDestinyTab(mQuestJson); });
    }
}
