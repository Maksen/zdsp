using FancyScrollView;
using System;
using System.Collections.Generic;
using UnityEngine;

public class InterestScrollView : FancyScrollView<InterestCellDto, InterestScrollViewContext>
{
    [SerializeField]
    ScrollPositionController scrollPositionController;
    [SerializeField]
    float scrollToDuration = 0.4f;

    private bool isActive = true;
    private Action<int> onSelectedIndexChanged;

    private void Awake()
    {
        scrollPositionController.OnUpdatePosition(p => UpdatePosition(p));
        scrollPositionController.OnItemSelected(HandleItemSelected);

        SetContext(new InterestScrollViewContext
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

    public void UpdateData(List<InterestCellDto> data)
    {
        //context.OnPressedCell = OnPressedCell;
        //SetContext(context);

        cellData = data;
        scrollPositionController.SetDataCount(cellData.Count);
        UpdateContents();
    }

    public void UpdateSelection(int index, float duration = 0)
    {
        if (index < 0 || index >= cellData.Count)
            return;

        scrollPositionController.ScrollTo(index, duration);
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

    private void OnPressedCell(InterestScrollViewCell cell)
    {
        if (isActive)
            UpdateSelection(cell.DataIndex, scrollToDuration);
    }

    public void EnableScrollPositionController(bool value)
    {
        scrollPositionController.CanDrag = value;
        isActive = value;
    }

    public void SetAutoSpin(bool value)
    {
        scrollPositionController.AutoSpin = value;
        scrollPositionController.inertia = !value;
    }
}
