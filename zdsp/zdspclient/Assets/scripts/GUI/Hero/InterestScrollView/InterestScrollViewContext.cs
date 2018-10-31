using System;

public class InterestScrollViewContext
{
    int selectedIndex = -1;

    public int SelectedIndex
    {
        get { return selectedIndex; }
        set
        {
            if (value == selectedIndex)
            {
                return;
            }

            selectedIndex = value;

            if (OnSelectedIndexChanged != null)
            {
                OnSelectedIndexChanged(selectedIndex);
            }
        }
    }

    public Action<InterestScrollViewCell> OnPressedCell;
    public Action<int> OnSelectedIndexChanged;

    private bool highlightSelected = true;

    public bool HighlightSelected
    {
        get { return highlightSelected; }
        set { highlightSelected = value; }
    }
}