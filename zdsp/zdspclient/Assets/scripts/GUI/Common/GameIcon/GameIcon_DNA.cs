using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class GameIcon_DNA : GameIcon_Base
{
    [SerializeField]
    Text txtLevel = null;

    [SerializeField]
    Text txtEvolve = null;

    public void Init(int itemId, int level, int evolve, UnityAction onClickCallback = null)
    {
        Init(itemId);
        Level = level;
        Evolve = evolve;
        if (onClickCallback != null)
            SetClickCallback(onClickCallback);
    }

    public void InitWithTooltipViewOnly(int itemId, int level, int evolve)
    {
        Init(itemId, level, evolve, OnClickShowTooltipViewOnly);
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
