using Kopio.JsonContracts;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Repository;

public class UI_DialogueSelection : MonoBehaviour
{
    [SerializeField]
    Text Answer;

    enum SelectionType
    {
        Talk = 0,
        Quest = 1,
        Group = 2,
    }

    private UI_Dialogue mParent;
    private QuestSelectDetailJson mSelectionJson;
    private int mQuestId;
    private SelectionType mType;
    private bool mOngoing;

    public void Init(UI_Dialogue parent, QuestSelectDetailJson selectionjson, int questid, bool ongoing)
    {
        mParent = parent;
        mSelectionJson = selectionjson;
        mQuestId = questid;
        mOngoing = ongoing;
        mType = mOngoing ? SelectionType.Talk : SelectionType.Group;
        Answer.text = selectionjson.answer;
    }

    public void Init(UI_Dialogue parent, int questid)
    {
        mParent = parent;
        mQuestId = questid;
        mType = SelectionType.Quest;
        QuestJson questJson = QuestRepo.GetQuestByID(questid);
        if (questJson != null)
        {
            Answer.text = questJson.questname;
        }
    }

    public void OnClickedAnswer()
    {
        if (mType == SelectionType.Quest)
        {
            mParent.SelectQuest(mQuestId);
        }
        else if (mType == SelectionType.Group)
        {
            mParent.SelectedStartQuest(mQuestId, mSelectionJson.actionid, mSelectionJson.questtalkid > 0 ? mSelectionJson.questtalkid : -1);
        }
        else
        {
            if (mSelectionJson.actiontype == QuestSelectionActionType.SubmitObjective)
            {
                mParent.SelectedInteractChoice(mQuestId, mSelectionJson.id, mSelectionJson.questtalkid > 0 ? mSelectionJson.questtalkid : -1);
            }
            else
            {
                mParent.UpdateTalkId(mSelectionJson.questtalkid);
            }
        }
    }
}
