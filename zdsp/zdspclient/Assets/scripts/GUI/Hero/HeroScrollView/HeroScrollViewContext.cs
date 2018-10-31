using System;

public class HeroScrollViewContext
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

    public Action<HeroScrollViewCell> OnPressedCell;
    public Action<int> OnSelectedIndexChanged;
}