using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InterestCircleScroll : MonoBehaviour
{
    [SerializeField] InterestScrollView scrollView;
    [SerializeField] int cellCount = 20;

    private List<InterestCellDto> cellData;
    private InterestScrollViewContext context;
    private Action<byte> OnSelectedIndexChanged;

    private void Start()  // to be removed
    {
        if (GameInfo.gCombat == null)
        {
            cellData = Enumerable.Range(0, cellCount)
                .Select(i => new InterestCellDto((byte)i, "Cell " + i))
                .ToList();

            context = new InterestScrollViewContext();
            context.OnSelectedIndexChanged = HandleSelectedIndexChanged;
            context.SelectedIndex = 0;

            scrollView.UpdateData(cellData, context);  // set contents and context
            scrollView.UpdateSelection(context.SelectedIndex);  // set selection
        }
    }

    public void SetUp(List<InterestCellDto> dataList, Action<byte> onSelectCallback)
    {
        context = new InterestScrollViewContext();
        context.OnSelectedIndexChanged = HandleSelectedIndexChanged;
        OnSelectedIndexChanged = onSelectCallback;

        cellData = dataList;
        scrollView.UpdateData(cellData, context);  // set contents and context
        scrollView.SetSelection(0);
    }

    private void SelectCell(int index)
    {
        if (index >= 0 && index < cellData.Count)
        {
            scrollView.UpdateSelection(index);
        }
    }

    private void HandleSelectedIndexChanged(int index)
    {
        if (OnSelectedIndexChanged != null)
            OnSelectedIndexChanged(cellData[index].Type);
    }
}