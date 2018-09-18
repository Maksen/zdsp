using Kopio.JsonContracts;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Repository;

public class Hud_DestinyStart : MonoBehaviour
{
    [SerializeField]
    Text QuestName;

    public void Init(int questid)
    {
        QuestJson questJson = QuestRepo.GetQuestByID(questid);
        if (questJson != null)
        {
            QuestName.text = questJson.questname;
        }
    }
}
