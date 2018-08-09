using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GameIcon_Equip : GameIcon_Base
{
    [SerializeField]
    GameObject iconStatusCannotuse = null;

    [SerializeField]
    GameObject iconStatusBreak = null;

    [SerializeField]
    Text txtPowerUp = null;

    [SerializeField]
    Text txtEvolve = null;

    [SerializeField]
    GameIconCmpt_Upgrade itemUpgrade = null;

    [SerializeField]
    GameIconCmpt_Socket itemSocket = null;

    [SerializeField]
    GameIconCmpt_SelectCheckmark toggleSelect = null;

    public void Init(int itemId, int powerUp, int evolve, int upgrade, bool statusCannotUse, bool statusBreak, 
        bool isToggleSelectOn = false, UnityAction onClickCallback = null)
    {
        Init(itemId);
        StatusCannotUse = statusCannotUse;
        StatusBreak = statusBreak;
        PowerUp = powerUp;
        Evolve = evolve;
        itemUpgrade.SetUpgradeCount(upgrade);
        if (toggleSelect != null)
            SetToggleSelectOn(isToggleSelectOn);
        if (onClickCallback != null)
            SetClickCallback(onClickCallback);
    }

    public void InitWithTooltipViewOnly(int itemId)
    {
        Init(itemId, 0, 0, 0, false, false, false, OnClickShowTooltipViewOnly);
    }

    public bool StatusCannotUse
    {
        set { iconStatusCannotuse.SetActive(value); }
    }

    public bool StatusBreak
    {
        set { iconStatusBreak.SetActive(value); }
    }

    public int PowerUp
    {
        set { txtPowerUp.text = (value > 0) ? value.ToString() : ""; }
    }

    public int Evolve
    {
        set { txtEvolve.text = (value > 0) ? value.ToString() : ""; }
    }

    public GameIconCmpt_SelectCheckmark GetToggleSelect()
    {
        return toggleSelect;
    }

    public void SetToggleSelectOn(bool isOn)
    {
        toggleSelect.SetCheckmarkVisible(isOn);
    }

    public void SetEquipIconClickCallback(UnityAction callback)
    {
        SetClickCallback(callback);
    }
}
