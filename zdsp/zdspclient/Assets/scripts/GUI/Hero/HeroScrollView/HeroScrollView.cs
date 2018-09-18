using FancyScrollView;
using System.Collections.Generic;
using UnityEngine;

public class HeroScrollView : FancyScrollView<HeroCellDto, HeroScrollViewContext>
{
    [SerializeField]
    ScrollPositionController scrollPositionController;
    [SerializeField]
    float scrollToDuration = 0.4f;

    private void Awake()
    {
        scrollPositionController.OnUpdatePosition(UpdatePosition);
        scrollPositionController.OnItemSelected(HandleItemSelected);
    }

    public void UpdateData(List<HeroCellDto> data, HeroScrollViewContext context)
    {
        context.OnPressedCell = OnPressedCell;
        SetContext(context);

        cellData = data;
        scrollPositionController.SetDataCount(cellData.Count);
        UpdateContents();
    }

    public void UpdateSelection(int selectedCellIndex, float duration)
    {
        scrollPositionController.ScrollTo(selectedCellIndex, duration);
    }

    private void HandleItemSelected(int selectedItemIndex)
    {
        context.SelectedIndex = selectedItemIndex;
        UpdateContents();
    }

    private void OnPressedCell(HeroScrollViewCell cell)
    {
        UpdateSelection(cell.DataIndex, scrollToDuration);
    }

    public void ResetSelectedIndex()
    {
        context.SelectedIndex = -1;
    }
}
