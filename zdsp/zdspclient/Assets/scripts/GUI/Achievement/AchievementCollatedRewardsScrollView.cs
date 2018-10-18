﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Zealot.Common;
using System;
using System.Collections;

public class AchievementCollatedRewardsScrollView : MonoBehaviour
{
    [SerializeField] int paddingRows = 2;
    [SerializeField] GameObject dataPrefab;
    [SerializeField] ScrollRect scrollRect;
    [SerializeField] Transform contentTransform;
    [SerializeField] VerticalLayoutGroup verticalLayout;

    private List<GameObject> contentRowList;
    private List<GameObject> emptyRowList;

    //scroll rect properties
    private int maxRows;
    private float visibleHeight;
    private int numRowsAvailable;
    private int numRowsVisible;

    private Vector2 lastScrollPos;

    private float topPadding;
    private float cellHeight;

    private int currentTopIndex;
    private int currentFirstRow;
    private bool initialized = false;

    private List<AchievementReward> dataList;

    public void Clear()
    {
        if (contentRowList != null)
        {
            for (int i = 0; i < contentRowList.Count; ++i)
            {
                GameObject rowData = contentRowList[i];
                if (rowData != null)
                {
                    Destroy(rowData);
                    rowData = null;
                }
            }
            contentRowList.Clear();
        }

        if (emptyRowList != null)
        {
            for (int i = 0; i < emptyRowList.Count; ++i)
            {
                GameObject rowData = emptyRowList[i];
                if (rowData != null)
                {
                    Destroy(rowData);
                    rowData = null;
                }
            }
            emptyRowList.Clear();
        }
    }

    void OnDisable()
    {
        initialized = false;
    }

    void OnDestroy()
    {
        dataPrefab = null;
        scrollRect = null;
        contentTransform = null;
        verticalLayout = null;

        contentRowList = null;
        emptyRowList = null;
    }

    private void Awake()
    {
        InitScrollView();
    }

    public void InitScrollView()
    {
        currentTopIndex = 0;
        currentFirstRow = 1;

        scrollRect.verticalNormalizedPosition = 1;

        contentRowList = new List<GameObject>();
        emptyRowList = new List<GameObject>();

        visibleHeight = GetComponent<RectTransform>().rect.height;
    }

    public void Populate(List<AchievementReward> rewardList)
    {
        Clear();

        currentTopIndex = 0;
        currentFirstRow = 1;
        scrollRect.verticalNormalizedPosition = 1;

        int maxrows = rewardList.Count;
        InitRows(maxrows);
        dataList = rewardList;

        for (int i = 0, j = currentTopIndex; i < numRowsAvailable; ++i, ++j)
        {
            var currentRow = contentRowList[i];
            Achievement_CollatedRewardData achData = currentRow.GetComponent<Achievement_CollatedRewardData>();
            AchievementReward info = dataList[j];
            achData.Init(info);
        }
        if (!initialized)
            StartCoroutine(WaitForFrame());
    }

    IEnumerator WaitForFrame()
    {
        yield return null;
        scrollRect.verticalNormalizedPosition = 1;
        initialized = true;
    }

    public void Refresh(List<AchievementReward> rewardList)
    {
        dataList = rewardList;

        for (int i = 0, j = currentTopIndex; i < numRowsAvailable; ++i, ++j)
        {
            var currentRow = contentRowList[i];
            Achievement_CollatedRewardData achData = currentRow.GetComponent<Achievement_CollatedRewardData>();
            AchievementReward info = dataList[j];
            achData.Init(info);
        }
    }

    public void InitRows(int maxrows)
    {
        maxRows = maxrows;

        topPadding = verticalLayout.padding.top;
        LayoutElement dataLayoutElement = dataPrefab.GetComponent<LayoutElement>();
        float iconHeight = dataLayoutElement.preferredHeight;
        float iconWidth = dataLayoutElement.preferredWidth;
        cellHeight = verticalLayout.spacing + iconHeight;

        numRowsVisible = (int)Math.Ceiling(visibleHeight / cellHeight);
        int paddedRows = numRowsVisible + paddingRows;
        numRowsAvailable = maxRows >= paddedRows ? paddedRows : maxRows;

        //create empty rows
        for (int i = 1; i <= maxRows; i++)
        {
            var emptyRow = new GameObject("row " + i);
            emptyRow.AddComponent<RectTransform>();
            LayoutElement layoutElement = emptyRow.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = iconHeight;
            layoutElement.preferredWidth = iconWidth;
            emptyRow.transform.SetParent(contentTransform, false);
            emptyRowList.Add(emptyRow);
        }

        int emptyRowCount = emptyRowList.Count;
        if (currentTopIndex + numRowsAvailable >= emptyRowCount)
        {
            if (emptyRowCount < numRowsVisible)
            {
                currentTopIndex = 0;
                currentFirstRow = 1;
            }
            else
            {
                currentTopIndex = emptyRowCount - numRowsAvailable;
                if (currentTopIndex < 0)
                {
                    currentTopIndex = 0;
                }
            }
        }

        //create content row
        for (int i = currentTopIndex; i < currentTopIndex + numRowsAvailable; i++)
        {
            var contentRow = Instantiate(dataPrefab);

            //add to first numRowsAvailable rows
            var emptyRow = emptyRowList[i];
            contentRow.transform.SetParent(emptyRow.transform, false);
            contentRow.SetActive(true);

            contentRowList.Add(contentRow);
        }

        scrollRect.onValueChanged.RemoveAllListeners();
        scrollRect.onValueChanged.AddListener(OnUpdateScroll);
    }

    void OnUpdateScroll(Vector2 scrollpos)
    {
        float posY = contentTransform.localPosition.y;

        posY -= topPadding;
        bool scrollUp = lastScrollPos.y > scrollpos.y;

        int newFirst = Mathf.Clamp(TopRowSeen(posY), 1, maxRows - numRowsAvailable + 1);
        int newLast = newFirst + numRowsVisible - 1;

        if (currentFirstRow != newFirst)
        {
            if (scrollUp && newFirst > 1 && newLast < maxRows && (currentTopIndex + numRowsAvailable) < emptyRowList.Count)
            {
                int diff = newFirst - currentFirstRow;

                for (int i = 0; i < diff; i++)
                {
                    int newIndex = currentTopIndex + numRowsAvailable;

                    var contentRow = contentRowList[0];
                    contentRowList.RemoveAt(0);
                    contentRowList.Add(contentRow);

                    var emptyRow = emptyRowList[newIndex];
                    contentRow.transform.SetParent(emptyRow.transform, false);

                    // Update Data
                    RefreshNewRow(contentRow, newIndex);

                    currentTopIndex++;
                }
            }
            else if (!scrollUp && newLast < maxRows - 1 && currentTopIndex > 0)
            {
                int diff = currentFirstRow - newFirst;
                for (int i = 0; i < diff; i++)
                {
                    var contentRow = contentRowList[numRowsAvailable - 1];
                    contentRowList.RemoveAt(numRowsAvailable - 1);
                    contentRowList.Insert(0, contentRow);

                    int newIndex = currentTopIndex - 1;

                    // Update Data
                    RefreshNewRow(contentRow, newIndex);

                    var emptyRow = emptyRowList[newIndex];
                    contentRow.transform.SetParent(emptyRow.transform, false);

                    currentTopIndex--;
                }
            }

            currentFirstRow = newFirst;
        }

        lastScrollPos = scrollpos;
    }

    private int TopRowSeen(float posY)
    {
        var hiddenGrids = Math.Floor(posY / cellHeight);
        return (int)hiddenGrids + 1;
    }

    private void RefreshNewRow(GameObject newRow, int newIndex)
    {
        if (newIndex < 0)
            return;

        Achievement_CollatedRewardData achData = newRow.GetComponent<Achievement_CollatedRewardData>();
        AchievementReward info = dataList[newIndex];
        achData.Init(info);
    }
}
