using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class GameIcon_DNA : GameIcon_Base
{
    [SerializeField]
    Text txtLevel = null;

    [SerializeField]
    Text txtEvolve = null;

    public void Init(int itemId, int level, int evolve, bool isNew, UnityAction onClickCallback = null)
    {
        Init(itemId, isNew);
        Level = level;
        Evolve = evolve;
        if (onClickCallback != null)
            SetClickCallback(onClickCallback);
    }

    public void InitWithoutCallback(int itemId, int level, int evolve)
    {
        Init(itemId, level, evolve, false);
    }

    public void InitWithToolTipView(int itemId, int level, int evolve)
    {
        Init(itemId, level, evolve, false, () => OnClickShowItemToolTip(1));
    }

    public int Level
    {
        set { txtLevel.text = (value > 0) ? value.ToString() : ""; }
    }

    public int Evolve
    {
        set { txtEvolve.text = (value > 0) ? value.ToString() : ""; }
    }
}
