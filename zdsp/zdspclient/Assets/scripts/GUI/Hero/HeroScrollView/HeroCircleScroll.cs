using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HeroCircleScroll : MonoBehaviour
{
    [SerializeField] HeroScrollView scrollView;
    [SerializeField] int cellCount = 20;

    private List<HeroCellDto> cellData;
    private HeroScrollViewContext context;
    private Action<int> OnSelectedIndexChanged;

    private void Start()  // to be removed
    {
        if (GameInfo.gCombat == null)
        {
            cellData = Enumerable.Range(0, cellCount)
                .Select(i => new HeroCellDto(i, "Cell " + i, true))
                .ToList();

            context = new HeroScrollViewContext();
            //context.OnSelectedIndexChanged = HandleSelectedIndexChanged;
            context.SelectedIndex = 0;

            scrollView.UpdateData(cellData, context);  // set contents and context
            scrollView.UpdateSelection(context.SelectedIndex, 0);  // set selection
        }
    }

    public void SetUp(List<HeroCellDto> dataList, Action<int> onSelectCallback)
    {
        context = new HeroScrollViewContext();
        context.OnSelectedIndexChanged = HandleSelectedIndexChanged;
        OnSelectedIndexChanged = onSelectCallback;

        cellData = dataList;
        scrollView.UpdateData(cellData, context);  // set contents and context
    }

    public void UpdateCell(int id, bool unlocked)
    {
        HeroCellDto cell = cellData.Find(x => x.HeroId == id);
        if (cell != null)
            cell.Unlocked = unlocked;
        scrollView.UpdateData(cellData, context);
    }

    public void SelectHero(int heroId)
    {
        int index = cellData.FindIndex(x => x.HeroId == heroId);
        if (index != -1)
            scrollView.UpdateSelection(index, 0f);
        else
            scrollView.UpdateSelection(0, 0f);
    }

    public void SelectCell(int index)
    {
        if (index >= 0 && index < cellData.Count)
        {
            scrollView.UpdateSelection(index, 0.4f);
        }
    }

    private void HandleSelectedIndexChanged(int index)
    {
        if (index >= 0 && index < cellData.Count)
        {
            if (OnSelectedIndexChanged != null)
                OnSelectedIndexChanged(cellData[index].HeroId);
        }
    }

    public void ResetSelectedIndex()
    {
        context.SelectedIndex = -1;
    }
}