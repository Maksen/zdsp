using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using Zealot.Common;
using Kopio.JsonContracts;
using System.Linq;

/// <summary>
/// Add this component to GameObject with ScrollRect
/// </summary>
public class EquipmentModdingScrollView : MonoBehaviour
{
    #region Serialized
    public bool isUpgrade = true;

    [Tooltip("Additional rows for scroll padding. Default 2")]
    [Range(2, 4)]
    public int paddingRows;

    public GameObject rowPrefab;
    #endregion

    ScrollRect scrollRect;
    Transform contentTransform;
    VerticalLayoutGroup verticalLayout;

    bool bInitialized = false;
    List<GameObject> contentRowList;
    List<GameObject> emptyRowList;

    //scroll rect properties
    int maxRows;
    float visibleHeight;
    int numRowsAvailable;
    int numRowsVisible;

    Vector2 lastScrollPos;

    //vertical layout properties
    float topPadding;
    float iconHeight;
    float cellHeight;
    float maxHeight;

    int cellsPerRow;
    int currentTopIndex;
    int currentFirstRow;

    //void OnEnable()
    //{
    //    var uiHeroes = gameObject.GetComponent<UI_Heroes>();
    //    //InitHeroScrollView();
    //    //InitRows(8);
    //}

    void OnDisable()
    {
        Clear();
        //selectedHeroes = null;
    }

    public void InitScrollView()
    {
        currentTopIndex = 0;
        currentFirstRow = 1;

        scrollRect = this.gameObject.GetComponent<ScrollRect>();
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 1;

            emptyRowList = new List<GameObject>();
            contentRowList = new List<GameObject>();

            contentTransform = scrollRect.content.transform;
            verticalLayout = scrollRect.content.GetComponent<VerticalLayoutGroup>();

            visibleHeight = gameObject.GetComponent<RectTransform>().rect.height;
        }

        if (rowPrefab != null)
        {
            var rowData = rowPrefab.GetComponent<EquipmentModdingRow>();
            cellsPerRow = rowData.rowSize;
        }
    }

    private UI_EquipmentUpgrade uiEquipUpgrade = null;
    private UI_EquipmentReform uiEquipReform = null;
    private List<ModdingEquipment> equipmentList = null;
    private bool isSafeUpgrade = false;
    private ToggleGroup toggleGroup = null;
    public void Populate(UI_EquipmentUpgrade uiEquipUpgrade, List<ModdingEquipment> equipmentList, int bagStart, bool isSafeUpgrade, ToggleGroup toggleGroup)
    {
        Clear();

        int maxrows = Mathf.CeilToInt(equipmentList.Count * 1.0f / cellsPerRow);
        InitRows(maxrows);
        this.uiEquipUpgrade = uiEquipUpgrade;
        this.equipmentList = equipmentList;
        this.isSafeUpgrade = isSafeUpgrade;
        this.toggleGroup = toggleGroup;

        for (int i = 0, j = currentTopIndex; i < numRowsAvailable; ++i, ++j)
        {
            int realStart = j * cellsPerRow;
            int realEnd = (j + numRowsAvailable) * cellsPerRow;

            var currentRow = contentRowList[i];
            for (int c = 0; c < cellsPerRow; ++c)
            {
                EquipmentModdingRow equipmentRowData = currentRow.GetComponent<EquipmentModdingRow>();
                int displayIdx = realStart + c;

                if (displayIdx >= this.equipmentList.Count)
                {
                    break;
                }

                ModdingEquipment upgEquip = equipmentList[displayIdx];
                if(upgEquip == null)
                {
                    continue;
                }
                
                equipmentRowData.AddData(uiEquipUpgrade, upgEquip.mSlotID, upgEquip.mEquip, isSafeUpgrade, upgEquip.mIsEquipped, toggleGroup);
            }
        }
    }

    public void Populate(UI_EquipmentReform uiEquipReform, List<ModdingEquipment> equipmentList, int bagStart, ToggleGroup toggleGroup)
    {
        Clear();

        int maxrows = Mathf.CeilToInt(equipmentList.Count * 1.0f / cellsPerRow);
        InitRows(maxrows);
        this.uiEquipReform = uiEquipReform;
        this.equipmentList = equipmentList;
        this.toggleGroup = toggleGroup;

        for (int i = 0, j = currentTopIndex; i < numRowsAvailable; ++i, ++j)
        {
            int realStart = j * cellsPerRow;
            int realEnd = (j + numRowsAvailable) * cellsPerRow;

            var currentRow = contentRowList[i];
            for (int c = 0; c < cellsPerRow; ++c)
            {
                EquipmentModdingRow equipmentRowData = currentRow.GetComponent<EquipmentModdingRow>();
                int displayIdx = realStart + c;

                if (displayIdx >= this.equipmentList.Count)
                {
                    break;
                }

                ModdingEquipment upgEquip = equipmentList[displayIdx];
                if (upgEquip == null)
                {
                    continue;
                }

                equipmentRowData.AddData(uiEquipReform, uiEquipReform.reformTab.isOn, upgEquip.mSlotID, upgEquip.mEquip, upgEquip.mIsEquipped, toggleGroup);
            }
        }
    }

    public void Refresh(List<ModdingEquipment> equipmentList)
    {
        this.equipmentList = equipmentList;

        for (int i = 0, j = currentTopIndex; i < numRowsAvailable; ++i, ++j)
        {
            int realStart = j * cellsPerRow;
            int realEnd = (j + numRowsAvailable) * cellsPerRow;

            var currentRow = contentRowList[i];
            for (int c = 0; c < cellsPerRow; ++c)
            {
                EquipmentModdingRow equipmentRowData = currentRow.GetComponent<EquipmentModdingRow>();
                int displayIdx = realStart + c;

                if (displayIdx >= this.equipmentList.Count)
                {
                    break;
                }

                ModdingEquipment modEquip = equipmentList[displayIdx];
                if (modEquip == null)
                {
                    continue;
                }

                if (isUpgrade == true)
                {
                    equipmentRowData.UpdateData(c, uiEquipUpgrade, modEquip.mSlotID, modEquip.mEquip, isSafeUpgrade, modEquip.mIsEquipped, toggleGroup);
                }
                else
                {
                    equipmentRowData.UpdateData(c, uiEquipReform, uiEquipReform.reformTab.isOn, modEquip.mSlotID, modEquip.mEquip, modEquip.mIsEquipped, toggleGroup);
                }
            }
        }
    }

    public void InitRows(int maxrows)
    {
        maxRows = maxrows;

        if (scrollRect != null)
        {
            topPadding = verticalLayout.padding.top;
            iconHeight = rowPrefab.GetComponent<LayoutElement>().preferredHeight;
            cellHeight = verticalLayout.spacing + iconHeight;
            maxHeight = maxRows * cellHeight + iconHeight;

            numRowsVisible = (int)Math.Ceiling(visibleHeight / cellHeight);
            int paddedRows = numRowsVisible + paddingRows;
            int actualRows = maxRows >= paddedRows ? paddedRows : maxRows;
            numRowsAvailable = actualRows;

            var scrollContent = verticalLayout.gameObject;
            //create empty rows
            //for(int i = 1; i <= maxRows; i++)
            //{
            //    var emptyRow = new GameObject("row " + i.ToString());
            //    emptyRow.AddComponent<RectTransform>();
            //    emptyRow.AddComponent<LayoutElement>();
            //    LayoutElement emptyRowLE = emptyRow.GetComponent<LayoutElement>();
            //    emptyRowLE.preferredWidth = rowWidth;
            //    emptyRowLE.preferredHeight = iconHeight;
            //    emptyRow.transform.SetParent(scrollContent.transform, false);
            //    emptyRowList.Add(emptyRow);
            //}

            //create content row
            for(int i = currentTopIndex; i < currentTopIndex + numRowsAvailable; i++)
            {
                var contentRow = Instantiate(rowPrefab);
                contentRow.GetComponent<EquipmentModdingRow>().Init();

                //add to first numRowsAvailable rows
                //var emptyRow = emptyRowList[i];
                contentRow.transform.SetParent(scrollContent.transform, false);
                contentRow.SetActive(true);

                //int newRowName = i + 1;
                //var heroData = contentRow.GetComponentInChildren<HeroData>();
                //heroData.cpText.text = newRowName.ToString() + newRowName.ToString();

                contentRowList.Add(contentRow);
            }

            //int emptyRowCount = emptyRowList.Count;
            if(currentTopIndex + numRowsAvailable >= maxRows)
            {
                if(maxRows < numRowsVisible)
                {
                    currentTopIndex = 0;
                    currentFirstRow = 1;
                }
                else
                {
                    currentTopIndex = maxRows - numRowsAvailable;
                    if(currentTopIndex < 0)
                    {
                        currentTopIndex = 0;
                    }
                }
            }

            //create content row
            //for(int i = currentTopIndex; i < currentTopIndex + numRowsAvailable; i++)
            //{
            //    var contentRow = Instantiate(rowPrefab);
            //    contentRow.GetComponent<EquipmentModdingRow>().Init();

            //    //add to first numRowsAvailable rows
            //    var emptyRow = emptyRowList[i];
            //    contentRow.transform.SetParent(emptyRow.transform, false);
            //    contentRow.SetActive(true);

            //    //int newRowName = i + 1;
            //    //var heroData = contentRow.GetComponentInChildren<HeroData>();
            //    //heroData.cpText.text = newRowName.ToString() + newRowName.ToString();

            //    contentRowList.Add(contentRow);
            //}

            //Destroy(rowPrefab);

            //currentTopIndex = 0;
            //currentFirstRow = 1;

            scrollRect.onValueChanged.AddListener(OnUpdateScroll);
            bInitialized = true;
        }
    }

    void OnUpdateScroll(Vector2 scrollpos)
    {
        float posY = contentTransform.localPosition.y;
        //Debug.LogFormat("Curr Y {0}", posY);        

        //if (posY >= topPadding/* && posY <= maxHeight*/)
        {
            posY -= topPadding;
            bool scrollUp = lastScrollPos.y > scrollpos.y;

            int newFirst = Mathf.Clamp(TopRowSeen(posY), 1, maxRows - numRowsAvailable + 1);

            int newLast = newFirst + numRowsVisible - 1;

            //Debug.Log("posY " + posY.ToString());
            //Debug.Log("newFirst " + newFirst.ToString());
            //Debug.Log("newLast " + newLast.ToString());

            if (currentFirstRow != newFirst)
            {
                //if(scrollUp)
                //    Debug.LogFormat("up {0} {1}", currentFirstRow, newFirst);

                if (scrollUp && newFirst > 1 && newLast < maxRows && (currentTopIndex + numRowsAvailable) < emptyRowList.Count)
                {
                    int diff = newFirst - currentFirstRow;

                    for (int i = 0; i < diff; i++)
                    {
                        int newIndex = currentTopIndex + numRowsAvailable;
                        //Debug.Log("newIndex UP " + newIndex.ToString());

                        var contentRow = contentRowList[0];
                        contentRowList.RemoveAt(0);
                        contentRowList.Add(contentRow);

                        //test
                        //int newRowName = newIndex + 1;
                        //var heroData = contentRow.GetComponentInChildren<HeroData>();
                        //heroData.cpText.text = newRowName.ToString() + newRowName.ToString();

                        var emptyRow = emptyRowList[newIndex];
                        contentRow.transform.SetParent(emptyRow.transform, false);

                        // Update HeroData
                        RefreshNewRow(contentRow, newIndex);

                        currentTopIndex++;

                        //Debug.Log("scrollup CurrIndex " + currentTopIndex.ToString());
                    }
                }
                else if (!scrollUp && newLast < maxRows - 1 && currentTopIndex > 0)
                {
                    int diff = currentFirstRow - newFirst;
                    //Debug.LogFormat("down {0} {1}", currentFirstRow, newFirst);
                    for (int i = 0; i < diff; i++)
                    {
                        var contentRow = contentRowList[numRowsAvailable - 1];
                        contentRowList.RemoveAt(numRowsAvailable - 1);
                        contentRowList.Insert(0, contentRow);

                        int newIndex = currentTopIndex - 1;
                        //Debug.Log("newIndex DOWN " + newIndex.ToString());

                        //test
                        //int newRowName = newIndex + 1;
                        //var heroData = contentRow.GetComponentInChildren<HeroData>();
                        //heroData.cpText.text = newRowName.ToString() + newRowName.ToString();

                        // Update HeroData
                        RefreshNewRow(contentRow, newIndex);

                        var emptyRow = emptyRowList[newIndex];
                        contentRow.transform.SetParent(emptyRow.transform, false);

                        currentTopIndex--;
                        //Debug.Log("scrollDown CurrIndex " + currentTopIndex.ToString());
                    }
                }

                currentFirstRow = newFirst;
            }
        }

        lastScrollPos = scrollpos;
    }

    int TopRowSeen(float posY)
    {
        var hiddenGrids = Math.Floor(posY / cellHeight);
        return (int)hiddenGrids + 1;
    }

    private void RefreshNewRow(GameObject newRow, int newIndex)
    {
        if (newIndex < 0)
        {
            return;
        }

        EquipmentModdingRow equipmentRowData = newRow.GetComponent<EquipmentModdingRow>();
        equipmentRowData.ClearRefresh();

        for (int j = 0; j < cellsPerRow; ++j)
        {
            int realStart = newIndex * cellsPerRow;
            int displayIdx = realStart + j;
            //Debug.Log("realStart " + realStart.ToString());
            //Debug.Log("displayIdx " + displayIdx.ToString());
            //Debug.Log("this.heroes.Count " + heroes.Count.ToString());

            if (displayIdx >= equipmentList.Count)
            {
                break;
            }

            ModdingEquipment modEquip = equipmentList[displayIdx];
            if (modEquip == null)
            {
                continue;
            }

            if(isUpgrade == true)
            {
                equipmentRowData.AddData(uiEquipUpgrade, modEquip.mSlotID, modEquip.mEquip, isSafeUpgrade, modEquip.mIsEquipped, toggleGroup);
            }
            else
            {
                equipmentRowData.AddData(uiEquipReform, uiEquipReform.reformTab.isOn, modEquip.mSlotID, modEquip.mEquip, modEquip.mIsEquipped, toggleGroup);
            }
        }
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
                    EquipmentModdingRow equipmentRowData = rowData.GetComponent<EquipmentModdingRow>();
                    if (equipmentRowData != null)
                    {
                        equipmentRowData.Clear();
                    }
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
                    EquipmentModdingRow equipmentRowData = rowData.GetComponent<EquipmentModdingRow>();
                    if (equipmentRowData != null)
                    {
                        equipmentRowData.Clear();
                    }
                    Destroy(rowData);
                    rowData = null;
                }
            }
            emptyRowList.Clear();
        }
    }
}
