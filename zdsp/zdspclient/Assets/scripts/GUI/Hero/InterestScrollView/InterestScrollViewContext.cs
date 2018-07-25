﻿using System;

public class InterestScrollViewContext
{
    int selectedIndex = -1;

    public int SelectedIndex
    {
        get { return selectedIndex; }
        set
        {
            var prevSelectedIndex = selectedIndex;
            selectedIndex = value;
            if (prevSelectedIndex != selectedIndex)
            {
                if (OnSelectedIndexChanged != null)
                {
                    OnSelectedIndexChanged(selectedIndex);
                }
            }
        }
    }

    public Action<InterestScrollViewCell> OnPressedCell;
    public Action<int> OnSelectedIndexChanged;
}

