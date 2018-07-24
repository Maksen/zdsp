using UnityEngine;
using UnityEngine.UI;
using Zealot.Client.Entities;
using Zealot.Common;

public class UI_Currency : MonoBehaviour
{
    [SerializeField]
    CurrencyType currencyType = CurrencyType.None;

    [SerializeField]
    public Text txtValue = null;

    void OnEnable()
    {
        UpdateCurrencyAmount();
    }

    public void UpdateCurrencyAmount()
    {
        PlayerGhost player = GameInfo.gLocalPlayer;
        if (player != null)
            SetValue(player.GetCurrencyAmount(currencyType));
    }

    public void SetValue(long value)
    {
        txtValue.text = (value > 0) ? value.ToString() : "0";
    }

	public void OnClickPlusButton()
    {
    }
}
