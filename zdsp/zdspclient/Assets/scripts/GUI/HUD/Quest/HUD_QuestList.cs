using Kopio.JsonContracts;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zealot.Common;
using Zealot.Repository;

public class HUD_QuestList : MonoBehaviour
{
    [SerializeField]
    Transform QuestListContent;

    [SerializeField]
    GameObject QuestData;

    private QuestClientController mQuestController;
    private Dictionary<int, GameObject> mQuestList = new Dictionary<int, GameObject>();

    public void OnClickOpenQuest()
    {
        UIManager.OpenWindow(WindowType.Quest);
    }

    public void UpdateQuestData(CurrentQuestData newQuestData, CurrentQuestData oldQuestData)
    {
        if (newQuestData == null && oldQuestData != null)
        {
            DeleteQuestObject(oldQuestData.QuestId);
        }
        else if (newQuestData != null)
        {
            UpdateQuestObject(newQuestData);
        }
    }

    private void DeleteQuestObject(int questid)
    {
        if (mQuestList.ContainsKey(questid))
        {
            Destroy(mQuestList[questid]);
            mQuestList.Remove(questid);
        }
    }

    private void UpdateQuestObject(CurrentQuestData questData)
    {
        QuestClientController questController = GameInfo.gLocalPlayer.QuestController;
        if (questController.IsQuestTracked(questData.QuestId))
        {
            GameObject questobj = null;
            if (mQuestList.ContainsKey(questData.QuestId))
            {
                questobj = mQuestList[questData.QuestId];
            }
            else
            {
                questobj = Instantiate(QuestData);
                questobj.transform.SetParent(QuestListContent, false);
                mQuestList.Add(questData.QuestId, questobj);
            }

            questobj.GetComponent<UI_TrackQuestData>().UpdateQuestData(questData, questController);
        }
    }

    public void UpdateTrackingList(List<int> trackinglist)
    {
        List<int> questlist = mQuestList.Keys.ToList();
        foreach (int questid in questlist)
        {
            if (!trackinglist.Contains(questid))
            {
                DeleteQuestObject(questid);
            }
        }

        foreach(int questid in trackinglist)
        {
            if (!mQuestList.ContainsKey(questid))
            {
                QuestJson questJson = QuestRepo.GetQuestByID(questid);
                if (questJson != null)
                {
                    CurrentQuestData questData = GameInfo.gLocalPlayer.QuestController.GetQuestData(questJson.type, questid);
                    if (questData != null)
                    {
                        UpdateQuestObject(questData);
                    }
                }
            }
        }
    }
}
