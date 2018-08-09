using Candlelight.UI;
using UnityEngine;

public class UI_ObjectiveListData : MonoBehaviour
{
    [SerializeField]
    GameObject OngoingQuest;

    [SerializeField]
    GameObject CompletedQuest;

    [SerializeField]
    HyperText Objective;

    private QuestClientController mQuestController;
    private int mQuestId;

    public void Init(string description, bool ongoing, bool completed, QuestClientController controller, int questid)
    {
        mQuestId = questid;
        OngoingQuest.SetActive(ongoing);
        CompletedQuest.SetActive(completed);
        Objective.text = description;
        mQuestController = controller;
        if (ongoing)
        {
            Objective.raycastTarget = true;
            Objective.ClickedLink.AddListener(OnClickHyperlink);
        }
    }

    public void OnClickHyperlink(HyperText hyperText, HyperText.LinkInfo linkInfo)
    {
        mQuestController.ProcessObjectiveHyperLink(linkInfo.Name, mQuestId);
    }
}
