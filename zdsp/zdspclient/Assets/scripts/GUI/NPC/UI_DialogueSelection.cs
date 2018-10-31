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
        Answer = 0,
        StartQuest = 1,
        QuestGroup = 2,
        Function = 3,
    }

    private UI_Dialogue mParent;
    private QuestSelectDetailJson mSelectionJson;
    private int mQuestId;
    private SelectionType mType;
    private bool mOngoing;
    private int mFunctionType;
    private int mFunctionGroup;

    public void Init(UI_Dialogue parent, QuestSelectDetailJson selectionjson, int questid, bool ongoing)
    {
        mParent = parent;
        mSelectionJson = selectionjson;
        mQuestId = questid;
        mOngoing = ongoing;
        mType = mOngoing ? SelectionType.Answer : SelectionType.QuestGroup;
        Answer.text = selectionjson.answer;
    }

    public void Init(UI_Dialogue parent, int questid)
    {
        mParent = parent;
        mQuestId = questid;
        mType = SelectionType.StartQuest;
        QuestJson questJson = QuestRepo.GetQuestByID(questid);
        if (questJson != null)
        {
            Answer.text = questJson.questname;
        }
    }

    public void Init(UI_Dialogue parent, int functiontype, int group)
    {
        mParent = parent;
        mFunctionType = functiontype;
        mFunctionGroup = group;
        mType = SelectionType.Function;
        //QuestJson questJson = QuestRepo.GetQuestByID(questid);
        //if (questJson != null)
        //{
        //    Answer.text = questJson.questname;
        //}
    }

    public void OnClickedAnswer()
    {
        if (mType == SelectionType.StartQuest)
        {
            mParent.SelectStartQuest(mQuestId);
        }
        else if (mType == SelectionType.QuestGroup)
        {
            mParent.SelectedStartQuestGroup(mQuestId, mSelectionJson.actionid, mSelectionJson.questtalkid > 0 ? mSelectionJson.questtalkid : -1);
        }
        else if (mType == SelectionType.Function)
        {
            mParent.SelectedFunction(mFunctionType, mFunctionGroup);
        }
        else
        {
            if (mSelectionJson.actiontype == QuestSelectionActionType.SubmitObjective)
            {
                mParent.SelectedInteractChoice(mQuestId, mSelectionJson.id, mSelectionJson.questtalkid > 0 ? mSelectionJson.questtalkid : -1);
            }
            else if (mSelectionJson.actiontype == QuestSelectionActionType.Job)
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
