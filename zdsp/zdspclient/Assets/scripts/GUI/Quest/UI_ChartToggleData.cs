using Kopio.JsonContracts;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Repository;

public class UI_ChartToggleData : MonoBehaviour
{
    [SerializeField]
    GameObject Ongoing;

    [SerializeField]
    GameObject Completed;

    [SerializeField]
    GameObject Selected;

    [SerializeField]
    Text Description;

    [SerializeField]
    RectTransform TopPoint;

    [SerializeField]
    RectTransform BottomPoint;

    [SerializeField]
    RectTransform LeftPoint;

    [SerializeField]
    RectTransform RightPoint;

    private UI_DestinyQuest mParent;
    private int mDestinyId;

    public void Init(QuestDestinyJson destinydata, UI_DestinyQuest parent)
    {
        if (GameInfo.gLocalPlayer == null)
        {
            return;
        }

        mParent = parent;
        mDestinyId = destinydata.id;
        Ongoing.SetActive(GameInfo.gLocalPlayer.QuestController.IsQuestOngoing(QuestType.Destiny, destinydata.questid));
        Completed.SetActive(GameInfo.gLocalPlayer.QuestController.IsAdventureLocked(destinydata.questid));
        Selected.SetActive(parent.GetSelectedDestinyId() == destinydata.destinyid);
        QuestJson questJson = QuestRepo.GetQuestByID(destinydata.questid);
        Description.text = questJson.questname;
    }

    public Vector2 GetLinkPoint(ChartDirection direction, RectTransform parentRect)
    {
        if (direction == ChartDirection.Right)
        {
            return new Vector2(RightPoint.localPosition.x, transform.localPosition.y - (parentRect.rect.height / 2));
        }
        else if (direction == ChartDirection.Left)
        {
            return new Vector2(LeftPoint.localPosition.x, transform.localPosition.y - (parentRect.rect.height / 2));
        }
        else if (direction == ChartDirection.Top)
        {
            return new Vector2(0, (transform.localPosition.y - (parentRect.rect.height / 2)) + TopPoint.localPosition.y);
        }
        else
        {
            return new Vector2(0, (transform.localPosition.y - (parentRect.rect.height / 2)) + BottomPoint.localPosition.y);
        }
    }

    public void OnClickedChartData()
    {
        Selected.SetActive(GetComponent<Toggle>().isOn ? true : false);
        mParent.OnDestinyChanged(GetComponent<Toggle>().isOn ? mDestinyId : -1);
    }
}
