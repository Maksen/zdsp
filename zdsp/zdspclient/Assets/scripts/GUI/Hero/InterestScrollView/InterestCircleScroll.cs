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
            int totalCellCount = Enum.GetNames(typeof(HeroInterestType)).Length;
            cellData = new List<InterestCellDto>();
            for (int i = 0; i < totalCellCount; i++)
            {
                InterestCellDto cell = new InterestCellDto((byte)i, ((HeroInterestType)i).ToString());
                cellData.Add(cell);
            }

            //context = new InterestScrollViewContext();
            //context.OnSelectedIndexChanged = HandleSelectedIndexChanged;

            scrollView.UpdateData(cellData);  // set contents and context
            scrollView.UpdateSelection(0);
        }
    }

    public void SetUp(Action<byte> onSelectCallback)
    {
        int totalCellCount = Enum.GetNames(typeof(HeroInterestType)).Length;
        cellData = new List<InterestCellDto>();
        for (int i = 0; i < totalCellCount; i++)
        {
            InterestCellDto cell = new InterestCellDto((byte)i, "");
            cellData.Add(cell);
        }

        //context = new InterestScrollViewContext();
        scrollView.OnSelectedIndexChanged(HandleSelectedIndexChanged);
        OnSelectedIndexChanged = onSelectCallback;

        scrollView.UpdateData(cellData);  // set contents and context
    }

    public void SetCellsApplicable(int interestGroup)
    {
        for (int i = 0; i < cellData.Count; i++)
        {
            InterestCellDto cell = cellData[i];
            cell.Applicable = cell.Type == 0 || HeroRepo.IsInterestInGroup(interestGroup, (HeroInterestType)cell.Type);
        }
        scrollView.UpdateSrollCellContents();
    }

    public void SelectCell(int index, float scrollDuration)
    {
        if (index >= 0 && index < cellData.Count)
        {
            scrollView.UpdateSelection(index, scrollDuration);
        }
    }

    private void HandleSelectedIndexChanged(int index)
    {
        //print("selectedindex: " + index);
        if (index >= 0 && index < cellData.Count)
        {
            if (OnSelectedIndexChanged != null)
            {
                OnSelectedIndexChanged(cellData[index].Type);
            }
        }
    }

    public void EnableScrollView(bool value)
    {
        scrollView.EnableScrollPositionController(value);
    }

    public void SetAutoSpin(bool value)
    {
        //context.HighlightSelected = !value;
        scrollView.SetAutoSpin(value);
    }
}