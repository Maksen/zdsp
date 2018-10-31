using UnityEngine;
using Zealot.Common;
using System.Collections.Generic;
using Zealot.Repository;
using Kopio.JsonContracts;
using System;
using System.Linq;

public class UI_Destiny : BaseWindowBehaviour
{
    [SerializeField]
    Transform MessageContent;

    [SerializeField]
    GameObject ClueMessageData;

    [SerializeField]
    GameObject ClueDateData;

    private DestinyClueClientController mDestinyClueController;
    private List<ActivatedClueData> mClues;
    private Dictionary<string, List<ActivatedClueData>> mCluesByDate;
    private List<GameObject> mDateObjects;
    private List<GameObject> mMessageObjects;

    private void OnEnable()
    {
        if (GameInfo.gLocalPlayer == null)
        {
            return;
        }

        mDestinyClueController = GameInfo.gLocalPlayer.DestinyClueController;
        mClues = mDestinyClueController.GetClues();
        OrderByDate();
        UpdateClueData();
    }

    private void OrderByDate()
    {
        mCluesByDate = new Dictionary<string, List<ActivatedClueData>>();
        List<string> stringdate = new List<string>();
        foreach(ActivatedClueData clue in mClues)
        {
            if (!mCluesByDate.ContainsKey(clue.ActivatedDate))
            {
                mCluesByDate.Add(clue.ActivatedDate, new List<ActivatedClueData>());
                stringdate.Add(clue.ActivatedDate);
            }
            mCluesByDate[clue.ActivatedDate].Add(clue);
        }

        foreach(string date in stringdate)
        {
            mCluesByDate[date] = mCluesByDate[date].OrderBy(c => c.ActivatedDT).ToList();
        }
    }

    private void UpdateClueData()
    {
        Clean();
        mDateObjects = new List<GameObject>();
        mMessageObjects = new List<GameObject>();
        foreach (KeyValuePair<string, List<ActivatedClueData>> entry in mCluesByDate)
        {
            GameObject dateobj = Instantiate(ClueDateData);
            dateobj.GetComponent<UI_ClueDateData>().Init(entry.Key);
            dateobj.transform.SetParent(MessageContent, false);
            mDateObjects.Add(dateobj);

            foreach(ActivatedClueData clue in entry.Value)
            {
                if (IsEndClue(clue))
                {
                    GameObject msgobj = Instantiate(ClueMessageData);
                    msgobj.GetComponent<UI_ClueMessageData>().Init(clue);
                    msgobj.transform.SetParent(MessageContent, false);
                    mMessageObjects.Add(msgobj);
                }
            }
        }
    }

    private bool IsEndClue(ActivatedClueData clue)
    {
        if (clue.ClueType == (byte)ClueType.Time)
        {
            TimeClueJson timeClueJson = DestinyClueRepo.GetTimeClueById(clue.ClueId);
            if (timeClueJson != null)
            {
                DateTime endtime = clue.ActivatedDT.AddMinutes(timeClueJson.time);
                if (endtime < clue.ActivatedDT)
                {
                    return false;
                }
            }
        }
        return true;
    }

    private void Clean()
    {
        if (mDateObjects != null)
        {
            foreach(GameObject obj in mDateObjects)
            {
                Destroy(obj);
            }
            mDateObjects = new List<GameObject>();
        }

        if (mMessageObjects != null)
        {
            foreach (GameObject obj in mMessageObjects)
            {
                Destroy(obj);
            }
            mMessageObjects = new List<GameObject>();
        }
    }

    private void OnDisable()
    {
        Clean();
    }
    
    public void RefreshMessage()
    {
        mClues = mDestinyClueController.GetClues();
        OrderByDate();
        UpdateClueData();
    }

    public void OnClickHero()
    {
        UIManager.CloseWindow(WindowType.Destiny);
        UIManager.OpenWindow(WindowType.Quest);
    }

    public void OnClickHistory()
    {
        if (!UIManager.IsWindowOpen(WindowType.DestinyHistory))
        {
            UIManager.CloseWindow(WindowType.Destiny);
            UIManager.OpenWindow(WindowType.DestinyHistory, (window) => { window.GetComponent<UI_DestinyHistory>().Init(mDestinyClueController); });
        }
    }

    public List<GameObject> GetMessages()
    {
        return mMessageObjects;
    }
}
