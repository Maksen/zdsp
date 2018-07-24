using UnityEngine;
using UnityEngine.Events;

public class GameIcon_MaterialConsumable : GameIcon_Base
{
    [SerializeField]
    GameObject iconStatusCannotuse = null;

    [SerializeField]
    GameIconCmpt_StackCount itemStackCount = null;

    [SerializeField]
    GameIconCmpt_SelectCheckmark toggleSelect = null;

    public void Init(int itemId, int stackCount, bool statusCannotUse = false, bool isToggleSelectOn = false, UnityAction onClickCallback = null)
    {
        Init(itemId);
        StatusCannotUse = statusCannotUse;
        itemStackCount.SetStackCount(stackCount);
        if (toggleSelect != null)
            SetToggleSelectOn(isToggleSelectOn);
        if (onClickCallback != null)
            SetClickCallback(onClickCallback);
    }

    public bool StatusCannotUse
    {
        set { iconStatusCannotuse.SetActive(value); }
    }

    public GameIconCmpt_SelectCheckmark GetToggleSelect()
    {
        return toggleSelect;
    }

    public void SetToggleSelectOn(bool isOn)
    {
        toggleSelect.SetCheckmarkVisible(isOn);
    }
}
