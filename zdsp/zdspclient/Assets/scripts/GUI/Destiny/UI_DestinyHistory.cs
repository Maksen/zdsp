using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Zealot.Common;
using System;
using Kopio.JsonContracts;
using Zealot.Repository;
using System.Linq;

public class UI_DestinyHistory : BaseWindowBehaviour
{
    [SerializeField]
    Transform FolderContent;

    [SerializeField]
    GameObject FolderData;

    [SerializeField]
    ToggleGroup ToggleGroup;

    [SerializeField]
    Toggle OverviewTab;

    [SerializeField]
    Toggle EventTab;

    [SerializeField]
    Toggle SpecialTab;

    private DestinyClueClientController mDestinyClueController;
    private Dictionary<int, DestinyClueJson> mDestinyClues;
    private Dictionary<ClueMemoryType, List<int>> mClueIdByType;
    private Dictionary<int, GameObject> mClueObjectById;
    private ClueMemoryType mSelectedMemoryType;
    private int mSelectedHero;

    public void Init(DestinyClueClientController controller)
    {
        mDestinyClueController = controller;
        mSelectedMemoryType = ClueMemoryType.Non;
        mDestinyClues = new Dictionary<int, DestinyClueJson>();
        mClueIdByType = new Dictionary<ClueMemoryType, List<int>>();
        mClueObjectById = new Dictionary<int, GameObject>();
        mSelectedHero = 0;

        foreach (ClueMemoryType type in Enum.GetValues(typeof(ClueMemoryType)))
        {
            mClueIdByType.Add(type, new List<int>());
        }

        UpdateFolderData();
    }

    public void OnClickFilter()
    {
        if (!UIManager.IsWindowOpen(WindowType.DialogHistoryFilter))
        {
            UIManager.OpenDialog(WindowType.DialogHistoryFilter, (window) => {
                window.GetComponent<UI_HistoryFilter>().InitFromDestinyUI(mDestinyClueController, this);
            });
        }
    }

    public void UpdateSelectedHero(int heroid)
    {
        mSelectedHero = heroid;
    }

    private void UpdateFolderData()
    {
        mDestinyClues = DestinyClueRepo.GetDestinyClues();
        mDestinyClues = mDestinyClues.OrderBy(o => o.Value.orderno).ToDictionary(o => o.Key, o => o.Value);

        foreach(KeyValuePair<int, DestinyClueJson> entry in mDestinyClues)
        {
            ActivatedClueData clueData = mDestinyClueController.GetClueData(entry.Key, ClueType.Normal);
            GameObject newfolderdata = Instantiate(FolderData);
            newfolderdata.GetComponent<UI_FolderData>().Init(entry.Value, clueData, ToggleGroup, this);
            mClueIdByType[entry.Value.memorytype].Add(entry.Key);
            mClueObjectById.Add(entry.Key, newfolderdata);
            newfolderdata.transform.SetParent(FolderContent, false);
        }
        OnMemoryTypeChanged();
    }

    private void OnMemoryTypeChanged()
    {
        if (mSelectedMemoryType == ClueMemoryType.Non)
        {
            foreach(KeyValuePair<int, GameObject> entry in mClueObjectById)
            {
                if (mSelectedHero != 0 && mDestinyClues[entry.Key].heroid != mSelectedHero)
                {
                    entry.Value.SetActive(false);
                }
                else
                {
                    entry.Value.SetActive(true);
                }
            }
        }
        else
        {
            List<int> idlist = mClueIdByType[mSelectedMemoryType];
            foreach (KeyValuePair<int, GameObject> entry in mClueObjectById)
            {
                if (idlist.Contains(entry.Key))
                {
                    if (mSelectedHero != 0 && mDestinyClues[entry.Key].heroid != mSelectedHero)
                    {
                        entry.Value.SetActive(false);
                    }
                    else
                    {
                        entry.Value.SetActive(true);
                    }
                }
               else
                {
                    entry.Value.SetActive(false);
                }
            }
        }
    }

    public void OnSelectAllMemory()
    {
        if (OverviewTab.isOn)
        {
            mSelectedMemoryType = ClueMemoryType.Non;
            OnMemoryTypeChanged();
        }
    }

    public void OnSelectEventMemory()
    {
        if (EventTab.isOn)
        {
            mSelectedMemoryType = ClueMemoryType.Event;
            OnMemoryTypeChanged();
        }
    }

    public void OnSelectSpecialMemory()
    {
        if (SpecialTab.isOn)
        {
            mSelectedMemoryType = ClueMemoryType.Special;
            OnMemoryTypeChanged();
        }
    }

    private void Clean()
    {
        if (mClueObjectById != null)
        {
            foreach(KeyValuePair<int, GameObject> entry in mClueObjectById)
            {
                Destroy(entry.Value);
            }
        }

        mClueObjectById = new Dictionary<int, GameObject>();
        mClueIdByType = new Dictionary<ClueMemoryType, List<int>>();
    }

    private void OnDisable()
    {
        Clean();
    }
}
