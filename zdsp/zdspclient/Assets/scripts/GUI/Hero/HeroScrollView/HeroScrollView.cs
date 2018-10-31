using FancyScrollView;
using System;
using System.Collections.Generic;
using UnityEngine;

public class HeroScrollView : FancyScrollView<HeroCellDto, HeroScrollViewContext>
{
    [SerializeField]
    ScrollPositionController scrollPositionController;
    [SerializeField]
    float scrollToDuration = 0.4f;

    Action<int> onSelectedIndexChanged;

    private void Awake()
    {
        scrollPositionController.OnUpdatePosition(p => UpdatePosition(p));
        scrollPositionController.OnItemSelected(HandleItemSelected);

        SetContext(new HeroScrollViewContext
        {
            OnPressedCell = OnPressedCell,
            OnSelectedIndexChanged = index =>
            {
                if (onSelectedIndexChanged != null)
                {
                    onSelectedIndexChanged(index);
                }
            }
        });
    }

    public void UpdateSrollCellContents()
    {
        UpdateContents();
    }

    public void UpdateData(List<HeroCellDto> data)
    {
        //context.OnPressedCell = OnPressedCell;
        //SetContext(context);

        cellData = data;
        scrollPositionController.SetDataCount(cellData.Count);
        UpdateContents();
    }

    public void UpdateSelection(int index, float duration)
    {
        if (index < 0 || index >= cellData.Count)
            return;

        scrollPositionController.ScrollTo(index, duration);
        //Context.SelectedIndex = index;
        //UpdateContents();
    }

    public void JumpToSelection(int index)
    {
        if (index < 0 || index >= cellData.Count)
            return;

        scrollPositionController.JumpTo(index);
        Context.SelectedIndex = index;
        UpdateContents();
    }

    public void OnSelectedIndexChanged(Action<int> onSelectedIndexChanged)
    {
        this.onSelectedIndexChanged = onSelectedIndexChanged;
    }

    private void HandleItemSelected(int selectedItemIndex)
    {
        Context.SelectedIndex = selectedItemIndex;
        UpdateContents();
    }

    private void OnPressedCell(HeroScrollViewCell cell)
    {
        UpdateSelection(cell.DataIndex, scrollToDuration);
    }

    public void ResetSelectedIndex()
    {
        Context.SelectedIndex = -1;
    }
}
