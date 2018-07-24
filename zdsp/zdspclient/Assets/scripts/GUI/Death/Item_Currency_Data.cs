using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using Zealot.Common;
using Zealot.Client.Entities;
using Zealot.Repository;

public class Item_Currency_Data : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject   gameIconPrefab;
    public Transform    gameIconParent;

    [Header("Texts")]
    public Text requireAmtTxt;

    // Private Variables
    private GameObject _dataIcon;

    public void Init(DeathItemData item)
    {
        ClearIcon();

        GameObject _dataIcon = Instantiate(gameIconPrefab);
        _dataIcon.transform.SetParent(gameIconParent, false);

        GameIcon_MaterialConsumable gameIcon = _dataIcon.GetComponent<GameIcon_MaterialConsumable>();
        gameIcon.Init(item.ItemID(), item.Amount(), false);
        requireAmtTxt.text = item.Amount().ToString();
    }

    public void Init(DeathCurrencyData currency)
    {
        ClearIcon();

        GameObject _dataIcon = Instantiate(gameIconPrefab);
        _dataIcon.transform.SetParent(gameIconParent, false);

        //GameIcon_MaterialConsumable gameIcon = _dataIcon.GetComponent<GameIcon_MaterialConsumable>();
        //gameIcon.Init(currency.ItemID(), currency.Amount(), false);
        requireAmtTxt.text = currency.Amount().ToString();
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
