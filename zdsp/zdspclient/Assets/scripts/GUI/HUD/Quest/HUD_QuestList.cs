using Kopio.JsonContracts;
using System;
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

    [SerializeField]
    Transform[] OngoingGroupContent;

    [SerializeField]
    Transform[] UnlockGroupContent;

    private QuestClientController mQuestController;
    private Dictionary<int, GameObject> mQuestList = new Dictionary<int, GameObject>();

    public void OnClickOpenQuest()
    {
        UIManager.OpenWindow(WindowType.Quest);
    }

    private Transform GetOngoingGroup(QuestType type)
    {
        return OngoingGroupContent[(int)type];
    }

    private Transform GetUnlockGroup(QuestType type)
    {
        return UnlockGroupContent[(int)type];
    }

    private void AddQuestData(QuestType type, List<int> trackinglist, QuestClientController controller)
    {
        List<CurrentQuestData> questDataList = GetQuestDataByType(type, trackinglist);
        Transform ongoingcontent = GetOngoingGroup(type);
        ongoingcontent.gameObject.SetActive(questDataList.Count == 0 ? false : true);
        foreach(CurrentQuestData questdata in questDataList)
        {
            AddQuestObject(questdata, ongoingcontent, controller);
        }

        List<QuestJson> questJsonList = GetQuestJsonByType(type, trackinglist);
        Transform unlockcontent = GetUnlockGroup(type);
        unlockcontent.gameObject.SetActive(questDataList.Count == 0 ? false : true);
        foreach (QuestJson questjson in questJsonList)
        {
            AddQuestObject(questjson, unlockcontent, controller);
        }
    }

    public void UpdateQuestData(CurrentQuestData newQuestData, CurrentQuestData oldQuestData, QuestClientController controller)
    {
        if (newQuestData == null && oldQuestData != null)
        {
            DeleteQuestObject(oldQuestData.QuestId);
        }
        else if (newQuestData != null)
        {
            UpdateQuestObject(newQuestData, controller);
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

    private void AddQuestObject(CurrentQuestData questData, Transform parent, QuestClientController controller)
    {
        GameObject questobj = null;
        if (mQuestList.ContainsKey(questData.QuestId))
        {
            questobj = mQuestList[questData.QuestId];
        }
        else
        {
            questobj = Instantiate(QuestData);
            mQuestList.Add(questData.QuestId, questobj);
        }
        questobj.transform.SetParent(parent, false);
        questobj.GetComponent<UI_TrackQuestData>().UpdateQuestData(questData, controller);
    }

    private void AddQuestObject(QuestJson questJson, Transform parent, QuestClientController controller)
    {
        GameObject questobj = null;
        if (mQuestList.ContainsKey(questJson.questid))
        {
            questobj = mQuestList[questJson.questid];
        }
        else
        {
            questobj = Instantiate(QuestData);
            mQuestList.Add(questJson.questid, questobj);
        }
        questobj.transform.SetParent(parent, false);
        questobj.GetComponent<UI_TrackQuestData>().UpdateQuestData(questJson, controller);        
    }

    private void UpdateQuestObject(CurrentQuestData questData, QuestClientController controller)
    {
        if (controller.IsQuestTracked(questData.QuestId))
        {
            GameObject questobj = null;
            if (mQuestList.ContainsKey(questData.QuestId))
            {
                questobj = mQuestList[questData.QuestId];
            }
            else
            {
                Transform content = GetOngoingGroup((QuestType)questData.QuestType);
                content.gameObject.SetActive(true);

                questobj = Instantiate(QuestData);
                questobj.transform.SetParent(content, false);
                mQuestList.Add(questData.QuestId, questobj);
            }

            questobj.GetComponent<UI_TrackQuestData>().UpdateQuestData(questData, controller);
        }
    }

    public void UpdateTrackingList(List<int> trackinglist, QuestClientController controller)
    {
        List<int> questlist = mQuestList.Keys.ToList();
        foreach (int questid in questlist)
        {
            if (!trackinglist.Contains(questid))
            {
                DeleteQuestObject(questid);
            }
        }

        foreach (QuestType type in Enum.GetValues(typeof(QuestType)))
        {
            AddQuestData(type, trackinglist, controller);
        }
    }

    private List<QuestJson> GetQuestJsonByType(QuestType type, List<int> trackinglist)
    {
        List<QuestJson> questlist = new List<QuestJson>();
        foreach (int questid in trackinglist)
        {
            QuestJson questJson = QuestRepo.GetQuestByID(questid);
            if (questJson != null)
            {
                if (questJson.type == type)
                {
                    CurrentQuestData questData = GameInfo.gLocalPlayer.QuestController.GetQuestData(questJson.type, questid);
                    if (questData == null)
                    {
                        questlist.Add(questJson);
                    }
                }
            }
        }
        return questlist;
    }

    private List<CurrentQuestData> GetQuestDataByType(QuestType type, List<int> trackinglist)
    {
        List<CurrentQuestData> questlist = new List<CurrentQuestData>();
        foreach (int questid in trackinglist)
        {
            QuestJson questJson = QuestRepo.GetQuestByID(questid);
            if (questJson != null)
            {
                if (questJson.type == type)
                {
                    CurrentQuestData questData = GameInfo.gLocalPlayer.QuestController.GetQuestData(questJson.type, questid);
                    if (questData != null)
                    {
                        questlist.Add(questData);
                    }
                }
            }
        }
        return questlist;
    }
}
