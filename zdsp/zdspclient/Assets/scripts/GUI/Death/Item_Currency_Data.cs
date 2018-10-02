using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;

public class Item_Currency_Data : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject gameIconPrefab;
    public Transform gameIconParent;

    [Header("Texts")]
    public Text requireAmtTxt;

    // Private Variables
    private GameObject _dataIcon;

    public void Init(ItemInfo item)
    {
        ClearIcon();

        GameObject _dataIcon = Instantiate(gameIconPrefab);
        _dataIcon.transform.SetParent(gameIconParent, false);

        GameIcon_MaterialConsumable gameIcon = _dataIcon.GetComponent<GameIcon_MaterialConsumable>();
        gameIcon.InitWithoutCallback(item.itemId, 0); // Don't show amount on icon
        requireAmtTxt.text = item.stackCount.ToString();
    }

    public void Init(CurrencyInfo currency)
    {
        ClearIcon();

        GameObject _dataIcon = Instantiate(gameIconPrefab);
        _dataIcon.transform.SetParent(gameIconParent, false);

        GameIcon_MaterialConsumable gameIcon = _dataIcon.GetComponent<GameIcon_MaterialConsumable>();
        gameIcon.InitWithoutCallback(currency.currencyType, 0); // Don't show amount on icon
        requireAmtTxt.text = currency.amount.ToString();
    }

    private void ClearIcon()
    {
        if(_dataIcon != null)
        {
            Destroy(_dataIcon);
            _dataIcon = null;
        }
    }
}
