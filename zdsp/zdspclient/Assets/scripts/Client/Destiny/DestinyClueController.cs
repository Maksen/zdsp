using System;
using System.Collections.Generic;
using System.Linq;
using Zealot.Common;
using Zealot.Common.Entities;

public class DestinyClueStatsClient : DestinyClueSynStats
{

}

public class DestinyClueClientController
{
    private List<ActivatedClueData> mDestinyClues;
    private List<int> mUnlockMemory;
    private List<int> mUnlockClues;
    private List<int> mUnlockTimeClues;

    private bool bInit = false;
    private int mNewClueCount = 0;

    public void Init()
    {
        bInit = true;
    }

    public void DeserializeData(string field, string data, string olddata)
    {
        switch (field)
        {
            case "destinyClues":
                mDestinyClues = DeserializeClueData(data);
                CheckForNewDestinyClue();
                UpdateUI();
                break;
            case "unlockMemory":
                mUnlockMemory = DeserializeListData(data);
                break;
            case "unlockClues":
                mUnlockClues = DeserializeListData(data);
                break;
            case "unlockTimeClues":
                mUnlockTimeClues = DeserializeListData(data);
                break;
        }
    }

    private void CheckForNewDestinyClue()
    {
        List<ActivatedClueData> newclues = GetNewClue();

        if (newclues.Count > 0 && newclues.Count >= mNewClueCount)
        {
            if (!UIManager.IsWidgetActived(HUDWidgetType.DestinyNews))
            {
                UIManager.SetWidgetActive(HUDWidgetType.DestinyNews, true);
            }
            mNewClueCount = newclues.Count;
        }
    }

    private List<ActivatedClueData> GetNewClue()
    {
        return mDestinyClues.Where(c => c.Status == (byte)ClueStatus.New).ToList(); ;
    }
    
    public List<ActivatedClueData> GetClues()
    {
        return mDestinyClues.OrderBy(c => c.ActivatedDateTime).ToList();
    }

    private void UpdateUI()
    {
        if (UIManager.IsWindowOpen(WindowType.Destiny))
        {
            UIManager.GetWindowGameObject(WindowType.Destiny).GetComponent<UI_Destiny>().RefreshMessage();
        }
    }

    private List<ActivatedClueData> DeserializeClueData(string data)
    {
        if (!string.IsNullOrEmpty(data) && data != "{}")
        {
            return JsonConvertDefaultSetting.DeserializeObject<List<ActivatedClueData>>(data);
        }
        return new List<ActivatedClueData>();
    }
    
    private List<int> DeserializeListData(string data)
    {
        if (!string.IsNullOrEmpty(data) && data != "{}")
        {
            return JsonConvertDefaultSetting.DeserializeObject<List<int>>(data);
        }
        return new List<int>();
    }

    public bool IsClueAlreadyUnlock(int clueid)
    {
        return mUnlockClues.Contains(clueid);
    }

    public bool IsTimeClueAlreadyUnlock(int clueid)
    {
        return mUnlockTimeClues.Contains(clueid);
    }

    public ActivatedClueData GetClueData(int clueid, ClueType type)
    {
        foreach (ActivatedClueData cluedata in mDestinyClues)
        {
            if (cluedata.ClueId == clueid && cluedata.ClueType == (byte)ClueType.Normal)
            {
                return cluedata;
            }
        }
        return null;
    }
}
