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

    public void Init(string description, bool ongoing, bool completed)
    {
        OngoingQuest.SetActive(ongoing);
        CompletedQuest.SetActive(completed);
        Objective.text = description;
        if (ongoing)
        {
            Objective.ClickedLink.AddListener(OnClickHyperlink);
        }
    }

    public void OnClickHyperlink(HyperText hyperText, HyperText.LinkInfo linkInfo)
    {

    }
}
