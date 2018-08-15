using System;
using System.Collections.Generic;
using UnityEngine;
using Zealot.Common;
using Zealot.Repository;

public class InterestCircleScroll : MonoBehaviour
{
    [SerializeField] InterestScrollView scrollView;
    [SerializeField] int cellCount = 20;

    private List<InterestCellDto> cellData;
    private InterestScrollViewContext context;
    private Action<byte> OnSelectedIndexChanged;

    private void Awake()
    {
        if (GameInfo.gCombat == null)
        {
            cellData = new List<InterestCellDto>();
            for (HeroInterestType i = HeroInterestType.Random; i < HeroInterestType.TotalNum; i++)
            {
                InterestCellDto cell = new InterestCellDto((byte)i, i.ToString());
                cellData.Add(cell);
            }

            context = new InterestScrollViewContext();
            context.OnSelectedIndexChanged = HandleSelectedIndexChanged;

            scrollView.UpdateData(cellData, context);  // set contents and context
            scrollView.SetSelection(0);
        }
    }

    public void SetUp(Action<byte> onSelectCallback)
    {
        cellData = new List<InterestCellDto>();
        for (HeroInterestType i = HeroInterestType.Random; i < HeroInterestType.TotalNum; i++)
        {
            InterestCellDto cell = new InterestCellDto((byte)i, i.ToString());
            cellData.Add(cell);
        }

        context = new InterestScrollViewContext();
        context.OnSelectedIndexChanged = HandleSelectedIndexChanged;
        OnSelectedIndexChanged = onSelectCallback;

        scrollView.UpdateData(cellData, context);  // set contents and context
        scrollView.SetSelection(0);
    }

    public void SetCellsApplicable(int interestGroup)
    {
        for (int i = 0; i < cellData.Count; i++)
        {
            InterestCellDto cell = cellData[i];
            cell.Applicable = cell.Type == 0 || HeroRepo.IsInterestInGroup(interestGroup, (HeroInterestType)cell.Type);
        }
        scrollView.UpdateData(cellData, context);
    }

    public void SelectCell(int index)
    {
        if (index >= 0 && index < cellData.Count)
        {
            scrollView.UpdateSelection(index);
        }
    }

    public void ResetSelection()
    {
        scrollView.SetSelection(0);
    }

    private void HandleSelectedIndexChanged(int index)
    {
        if (OnSelectedIndexChanged != null)
        {
            OnSelectedIndexChanged(cellData[index].Type);
        }
    }

    public void EnableScrollView(bool value)
    {
        scrollView.EnableScrollPositionController(value);
    }
}