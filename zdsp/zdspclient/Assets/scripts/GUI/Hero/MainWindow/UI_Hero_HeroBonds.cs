using System;
using System.Collections.Generic;
using UIWidgets;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common.Entities;
using Zealot.Repository;

public class UI_Hero_HeroBonds : MonoBehaviour
{
    [SerializeField] Transform bondDataParent;
    [SerializeField] GameObject bondDataPrefab;
    [SerializeField] ScrollRect scrollRect;
    [SerializeField] Spinner pageSpinner;
    [SerializeField] UI_Hero_TotalBuffPanel totalBuffPanel;

    private const int ITEMS_PER_PAGE = 10;
    private int startingIndex;
    private List<Hero_BondSimpleData> bondList = new List<Hero_BondSimpleData>();
    private List<HeroBond> allBondsList;
    private bool initialized = false;

    private void Awake()
    {
        allBondsList = HeroRepo.heroBondsList;

        ToggleGroup toggleGroup = bondDataParent.GetComponent<ToggleGroup>();
        for (int i = 0; i < ITEMS_PER_PAGE; i++)
        {
            GameObject obj = ClientUtils.CreateChild(bondDataParent, bondDataPrefab);
            Hero_BondSimpleData data = obj.GetComponent<Hero_BondSimpleData>();
            data.SetUp(toggleGroup);
            obj.SetActive(false);
            bondList.Add(data);
        }

        pageSpinner.Min = 1;
        pageSpinner.Max = Math.Max(1, (int)Math.Ceiling((double)allBondsList.Count / ITEMS_PER_PAGE));
        pageSpinner.Value = 1;
        pageSpinner.onPlusClick.AddListener(GoToNextPage);
        pageSpinner.onMinusClick.AddListener(GoToPreviousPage);
    }

    private void OnEnable()
    {
        PopulateList();
        totalBuffPanel.Init();
    }

    private void PopulateList()
    {
        int endIndex = startingIndex + ITEMS_PER_PAGE;
        if (endIndex > allBondsList.Count)
            endIndex = allBondsList.Count;

        int idx = startingIndex;
        for (int i = 0; i < ITEMS_PER_PAGE; i++)
        {
            if (idx < endIndex)
            {
                bondList[i].Init(allBondsList[idx++]);
                bondList[i].gameObject.SetActive(true);
            }
            else
                bondList[i].gameObject.SetActive(false);
        }

        if (!initialized)
        {
            if (bondList[0].gameObject.activeSelf)
                bondList[0].SetToggleOn(true);
            initialized = true;
        }

        scrollRect.verticalNormalizedPosition = 1f;
    }

    public void RefreshList(int heroId)
    {
        for (int i = 0; i < bondList.Count; i++)
        {
            if (bondList[i].gameObject.activeInHierarchy)  // only need refresh if is active
                bondList[i].Refresh(heroId);
        }

        totalBuffPanel.Init();
    }

    private void GoToNextPage()
    {
        ToggleSelectionOff();
        startingIndex += ITEMS_PER_PAGE;
        PopulateList();
    }

    private void GoToPreviousPage()
    {
        ToggleSelectionOff();
        startingIndex -= ITEMS_PER_PAGE;
        PopulateList();
    }

    public void CleanUp()
    {
        ToggleSelectionOff();
        startingIndex = 0;
        pageSpinner.Value = 1;
        initialized = false;
    }

    private void ToggleSelectionOff()
    {
        for (int i = 0; i < bondList.Count; i++)
        {
            if (bondList[i].IsToggleOn())
            {
                bondList[i].SetToggleOn(false);
                break;
            }
        }
    }
}