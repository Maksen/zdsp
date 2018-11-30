using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zealot.Common;

public class AchievementRewardsScrollView : MonoBehaviour, IBeginDragHandler
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
    private float iconHeight;
    private float iconWidth;
    private float cellHeight;

    private int currentTopIndex;
    private int currentFirstRow;
    private bool initialized = false;

    private List<AchievementRewardInfo> dataList;
    private UI_Achievement_RewardsDialog parent;

    private int playEfxCount = 0;
    private bool CanDrag
    {
        get { return playEfxCount == 0; }
    }

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
        playEfxCount = 0;
    }

    public void InitScrollView(UI_Achievement_RewardsDialog myParent)
    {
        parent = myParent;

        currentTopIndex = 0;
        currentFirstRow = 1;

        scrollRect.verticalNormalizedPosition = 1;

        contentRowList = new List<GameObject>();
        emptyRowList = new List<GameObject>();

        visibleHeight = GetComponent<RectTransform>().rect.height;

        scrollRect.onValueChanged.RemoveAllListeners();
        scrollRect.onValueChanged.AddListener(OnUpdateScroll);
    }

    public void GoToTop()
    {
        scrollRect.verticalNormalizedPosition = 1;
    }

    public void InitRows(int maxrows)
    {
        maxRows = maxrows;

        topPadding = verticalLayout.padding.top;
        LayoutElement dataLayoutElement = dataPrefab.GetComponent<LayoutElement>();
        iconHeight = dataLayoutElement.preferredHeight;
        iconWidth = dataLayoutElement.preferredWidth;
        cellHeight = verticalLayout.spacing + iconHeight;

        numRowsVisible = (int)Math.Ceiling(visibleHeight / cellHeight);
        int paddedRows = numRowsVisible + paddingRows;
        numRowsAvailable = maxRows >= paddedRows ? paddedRows : maxRows;

        //create empty rows
        for (int i = 0; i < maxRows; ++i)
        {
            var emptyRow = new GameObject("row " + i);
            RectTransform rectTransform = emptyRow.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(iconWidth, rectTransform.sizeDelta.y);
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
        for (int i = currentTopIndex; i < currentTopIndex + numRowsAvailable; ++i)
        {
            var contentRow = Instantiate(dataPrefab);

            //add to first numRowsAvailable rows
            var emptyRow = emptyRowList[i];
            contentRow.transform.SetParent(emptyRow.transform, false);
            contentRow.SetActive(true);

            contentRowList.Add(contentRow);
        }
    }

    public void Populate(List<AchievementRewardInfo> rewardList)
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
            Achievement_RewardData achData = currentRow.GetComponent<Achievement_RewardData>();
            AchievementRewardInfo info = dataList[j];
            achData.Init(info, parent, this);
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

    public void Refresh(int dataIndex)
    {
        for (int i = 0, j = currentTopIndex; i < numRowsAvailable; ++i, ++j)
        {
            if (j == dataIndex)
            {
                var currentRow = contentRowList[i];
                Achievement_RewardData achData = currentRow.GetComponent<Achievement_RewardData>();
                AchievementRewardInfo info = dataList[j];
                achData.UpdateDataAndPlayEfx(info);
                ++playEfxCount;
                break;
            }
        }
    }

    public void RefreshAll()
    {
        for (int i = 0, j = currentTopIndex; i < numRowsAvailable; ++i, ++j)
        {
            var currentRow = contentRowList[i];
            Achievement_RewardData achData = currentRow.GetComponent<Achievement_RewardData>();
            AchievementRewardInfo info = dataList[j];
            achData.UpdateData(info);
        }
    }

    public void PlayEfx(int dataIndex)
    {
        for (int i = 0, j = currentTopIndex; i < numRowsAvailable; ++i, ++j)
        {
            if (j == dataIndex)
            {
                var currentRow = contentRowList[i];
                Achievement_RewardData achData = currentRow.GetComponent<Achievement_RewardData>();
                AchievementRewardInfo info = dataList[j];
                achData.PlayClaimEfx();
                ++playEfxCount;
                break;
            }
        }
    }

    private void OnUpdateScroll(Vector2 scrollpos)
    {
        float posY = contentTransform.localPosition.y;

        posY -= topPadding;
        bool scrollUp = lastScrollPos.y > scrollpos.y;

        int newFirst = Mathf.Clamp(TopRowSeen(posY), 1, maxRows - numRowsAvailable + 1);
        int newLast = newFirst + numRowsVisible - 1;

        if (currentFirstRow != newFirst)
        {
            //print("newFirst: " + newFirst);
            //print("newLast: " + newLast);

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
                    //print("currentTopIndex: " + currentTopIndex);
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
                    //print("currentTopIndex: " + currentTopIndex);
                }
            }

            currentFirstRow = newFirst;
        }

        lastScrollPos = scrollpos;
    }

    private int TopRowSeen(float posY)
    {
        int hiddenGrids = Mathf.FloorToInt(posY / cellHeight);
        return hiddenGrids + 1;
    }

    private void RefreshNewRow(GameObject newRow, int newIndex)
    {
        if (newIndex < 0)
            return;

        Achievement_RewardData achData = newRow.GetComponent<Achievement_RewardData>();
        AchievementRewardInfo info = dataList[newIndex];
        achData.Init(info, parent, this);
    }

    public void RemoveData(AchievementRewardInfo achReward)
    {
        for (int i = 0; i < numRowsAvailable; ++i)
        {
            var currentRow = contentRowList[i];
            Achievement_RewardData achData = currentRow.GetComponent<Achievement_RewardData>();
            AchievementRewardInfo info = achData.GetRewardInfo();
            if (achReward.rewardClaim.ClaimType == info.rewardClaim.ClaimType && achReward.rewardClaim.Id == info.rewardClaim.Id)
            {
                RemoveContentRow(i);
                --playEfxCount;
                break;
            }
        }
    }

    private void RemoveContentRow(int rowIndex)
    {
        //print("startIndex: " + startIndex);
        if (numRowsAvailable == maxRows)  // no empty rows
        {
            var contentRow = contentRowList[rowIndex];
            contentRowList.RemoveAt(rowIndex);
            Destroy(contentRow);

            var emptyRow = emptyRowList[rowIndex];
            emptyRowList.RemoveAt(rowIndex);
            Destroy(emptyRow);

            numRowsAvailable--;
            maxRows--;

            dataList.RemoveAt(rowIndex);
        }
        else
        {
            int lastRowIndex = currentTopIndex + numRowsAvailable;
            //print("lastRowIndex: " + lastRowIndex);
            if (lastRowIndex < emptyRowList.Count)  // still have empty row at the bottom
            {
                // move content row at startindex to bottom empty row and refresh the row
                GameObject contentRowToMove = contentRowList[rowIndex];
                contentRowList.RemoveAt(rowIndex);
                contentRowList.Add(contentRowToMove);

                GameObject moveToEmptyRow = emptyRowList[lastRowIndex];
                contentRowToMove.transform.SetParent(moveToEmptyRow.transform, false);

                RefreshNewRow(contentRowToMove, lastRowIndex);

                // destroy emptied row
                int emptyRowIndex = currentTopIndex + rowIndex;
                GameObject emptiedOutRow = emptyRowList[emptyRowIndex];
                emptyRowList.RemoveAt(emptyRowIndex);
                Destroy(emptiedOutRow);

                maxRows--;

                dataList.RemoveAt(emptyRowIndex);
            }
            else if (currentTopIndex > 0)// have empty row at the top
            {
                // move content row at startindex to top empty row and refresh the row
                GameObject contentRowToMove = contentRowList[rowIndex];
                contentRowList.RemoveAt(rowIndex);
                contentRowList.Insert(0, contentRowToMove);

                int newTopIndex = currentTopIndex - 1;
                GameObject moveToEmptyRow = emptyRowList[newTopIndex];
                contentRowToMove.transform.SetParent(moveToEmptyRow.transform, false);

                RefreshNewRow(contentRowToMove, newTopIndex);

                // destroy emptied row
                int emptyRowIndex = currentTopIndex + rowIndex;
                GameObject emptiedOutRow = emptyRowList[emptyRowIndex];
                emptyRowList.RemoveAt(emptyRowIndex);
                Destroy(emptiedOutRow);

                maxRows--;
                dataList.RemoveAt(emptyRowIndex);
                currentTopIndex--;
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!CanDrag)
        {
            eventData.pointerDrag = null;
        }
    }
}