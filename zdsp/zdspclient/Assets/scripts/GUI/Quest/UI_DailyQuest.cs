using UnityEngine;
using System.Collections.Generic;
using Kopio.JsonContracts;
using Zealot.Repository;
using UnityEngine.UI;
using Zealot.Common;

public class UI_DailyQuest : MonoBehaviour
{
    [SerializeField]
    Text Bonus;

    [SerializeField]
    Text Progress;

    [SerializeField]
    Transform SignboardContent;

    [SerializeField]
    GameObject SignboardData;

    private List<GameObject> mSignboardObj = new List<GameObject>();
    private QuestClientController mQuestController;
    private List<int> mSignboardList;
    private List<int> mCompletedList;
    private List<int> mOngoingList;
    private List<int> mSubmitableList;
    private List<int> mAvailableList;

    public void OnEnable()
    {
        if (GameInfo.gLocalPlayer == null)
        {
            return;
        }

        mQuestController = GameInfo.gLocalPlayer.QuestController;
        mSignboardList = mQuestController.GetUnlockSignboardList();
        RefreshSignboard();
    }

    private void OrderSignboardList()
    {
        mCompletedList = new List<int>();
        mOngoingList = new List<int>();
        mSubmitableList = new List<int>();
        mAvailableList = new List<int>();

        if (mQuestController == null)
        {
            return;
        }

        foreach (int signboardid in mSignboardList)
        {
            QuestSignboardJson signboardJson = QuestRepo.GetSignboardQuestBySignboardId(signboardid);
            if (signboardJson != null)
            {
                bool completed = mQuestController.IsQuestCompleted(signboardJson.questid);
                bool submitable = mQuestController.IsQuestCanSubmit(signboardJson.questid);
                CurrentQuestData questData = mQuestController.GetQuestData(QuestType.Signboard, signboardJson.questid);
                if (completed)
                {
                    mCompletedList.Add(signboardid);
                    continue;
                }
                else
                {
                    if (questData == null)
                    {
                        mAvailableList.Add(signboardid);
                        continue;
                    }
                    else
                    {
                        if (submitable)
                        {
                            mSubmitableList.Add(signboardid);
                            continue;
                        }
                        else
                        {
                            mOngoingList.Add(signboardid);
                            continue;
                        }
                    }
                }
            }
        }
    }

    public void RefreshSignboard()
    {
        OrderSignboardList();
        Bonus.text = mQuestController.GetSignboardRewardBoost().ToString();
        Progress.text = mCompletedList.Count + "/" + mQuestController.GetSignboardLimit();
        Clean();
        RefreshSignboardData(mSubmitableList);
        RefreshSignboardData(mOngoingList);
        RefreshSignboardData(mAvailableList);
        RefreshSignboardData(mCompletedList);
    }

    private void RefreshSignboardData(List<int> singboardlist)
    {
        if (mQuestController == null)
        {
            return;
        }

        foreach(int signboardid in singboardlist)
        {
            QuestSignboardJson signboardJson = QuestRepo.GetSignboardQuestBySignboardId(signboardid);
            if (signboardJson != null)
            {
                GameObject newsignboard = Instantiate(SignboardData);
                newsignboard.GetComponent<UI_DailyQuestData>().Init(signboardJson, mQuestController);
                newsignboard.transform.SetParent(SignboardContent, false);
                mSignboardObj.Add(newsignboard);
            }
        }
    }

    private void OnDestroy()
    {
        Clean();
    }

    private void Clean()
    {
        foreach(GameObject obj in mSignboardObj)
        {
            Destroy(obj);
        }
        mSignboardObj = new List<GameObject>();
    }
}
