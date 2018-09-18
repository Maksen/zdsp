using FancyScrollView;
using System.Collections.Generic;
using UnityEngine;

public class InterestScrollView : FancyScrollView<InterestCellDto, InterestScrollViewContext>
{
    [SerializeField]
    ScrollPositionController scrollPositionController;
    [SerializeField]
    float scrollToDuration = 0.4f;

    private bool isActive = true;

    private void Awake()
    {
        scrollPositionController.OnUpdatePosition(UpdatePosition);
        scrollPositionController.OnItemSelected(HandleItemSelected);
    }

    public void UpdateData(List<InterestCellDto> data, InterestScrollViewContext context)
    {
        context.OnPressedCell = OnPressedCell;
        SetContext(context);

        cellData = data;
        scrollPositionController.SetDataCount(cellData.Count);
        UpdateContents();
    }

    public void UpdateSelection(int selectedCellIndex, float duration = 0)
    {
        scrollPositionController.ScrollTo(selectedCellIndex, duration);
    }

    private void HandleItemSelected(int selectedItemIndex)
    {
        context.SelectedIndex = selectedItemIndex;
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
