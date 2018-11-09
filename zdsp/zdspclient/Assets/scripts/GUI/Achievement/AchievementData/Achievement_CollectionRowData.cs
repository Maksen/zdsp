using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Repository;

public class Achievement_CollectionRowData : MonoBehaviour
{
    [SerializeField] GameObject dataPrefab;

    private List<Achievement_CollectionData> dataList = new List<Achievement_CollectionData>();

    public void Init(int cellsPerRow, ToggleGroup toggleGroup)
    {
        for (int i = 0; i < cellsPerRow; ++i)
        {
            GameObject icon = Instantiate(dataPrefab);
            icon.transform.SetParent(transform, false);
            Achievement_CollectionData coldata = icon.GetComponent<Achievement_CollectionData>();
            coldata.Setup(toggleGroup);
            dataList.Add(coldata);
            icon.SetActive(false);
        }
    }

    public void AddChildData(Achievement_CollectionData child)
    {
        child.transform.SetParent(transform, false);
        dataList.Add(child);
    }

    public void AddData(CollectionInfo info, UnityAction<bool, CollectionObjective> callback, bool selected = false)
    {
        Achievement_CollectionData colData = GetNewCollectionData();
        colData.Init(info, callback, selected);
    }

    public void UpdateData(int index, CollectionInfo info)
    {
        Achievement_CollectionData colData = dataList[index];
        colData.UpdateData(info);
    }

    private Achievement_CollectionData GetNewCollectionData()
    {
        for (int i = 0; i < dataList.Count; ++i)
        {
            Achievement_CollectionData colData = dataList[i];
            CollectionInfo info = colData.GetCollectionInfo();
            if (info == null)
            {
                colData.gameObject.SetActive(true);
                return colData;
            }
        }
        return null;
    }

    public void ClearRow()
    {
        for (int i = 0; i < dataList.Count; ++i)
        {
            Achievement_CollectionData colData = dataList[i];
            colData.Clear();
            colData.gameObject.SetActive(false);
        }
    }

    public void TransferChildrenTo(GameObject parent)
    {
        Achievement_CollectionRowData parentRowData = parent.GetComponent<Achievement_CollectionRowData>();
        for (int i = 0; i < dataList.Count; ++i)
        {
            Achievement_CollectionData colData = dataList[i];
            colData.Clear();
            colData.gameObject.SetActive(false);
            parentRowData.AddChildData(dataList[i]);
        }
        dataList.Clear();
    }

    public void SelectChild(int index)
    {
        if (index >= 0 && index < dataList.Count)
            dataList[index].SetToggleOn(true);
    }
}