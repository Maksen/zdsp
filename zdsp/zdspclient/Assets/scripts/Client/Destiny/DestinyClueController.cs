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

    private bool bInit = false;

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
                break;
            case "unlockMemory":
                mUnlockMemory = DeserializeMemoryData(data);
                break;
        }
    }

    private void CheckForNewDestinyClue()
    {
        List<ActivatedClueData> newclues = GetNewClue();
        newclues = newclues.Where(c => c.ClueType == (byte)ClueType.Normal).ToList();

        if (newclues.Count > 0)
        {
            ActivatedClueData clueData = newclues[newclues.Count - 1];
            if (!UIManager.IsWidgetActived(HUDWidgetType.DestinyNews))
            {
                UIManager.SetWidgetActive(HUDWidgetType.DestinyNews, true);
            }
            UIManager.GetWidget(HUDWidgetType.DestinyNews).GetComponent<Hud_DestinyNews>().UpdateClue(clueData);
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

    private List<ActivatedClueData> DeserializeClueData(string data)
    {
        if (!string.IsNullOrEmpty(data) && data != "{}")
        {
            return JsonConvertDefaultSetting.DeserializeObject<List<ActivatedClueData>>(data);
        }
        return new List<ActivatedClueData>();
    }
    
    private List<int> DeserializeMemoryData(string data)
    {
        if (!string.IsNullOrEmpty(data) && data != "{}")
        {
            return JsonConvertDefaultSetting.DeserializeObject<List<int>>(data);
        }
        return new List<int>();
    }
}
