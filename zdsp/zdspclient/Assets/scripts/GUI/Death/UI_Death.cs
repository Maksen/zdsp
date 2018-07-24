using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using Zealot.Common;
using Zealot.Client.Entities;
using Zealot.Repository;
using Kopio.JsonContracts;

public class DeathItemData
{
    private int _itemId;
    private int _amount;

    public DeathItemData(int itemid, int amount)
    {
        _itemId = itemid;
        _amount = amount;
    }

    public int ItemID()
    {
        return _itemId;
    }

    public int Amount()
    {
        return _amount;
    }
}

public class DeathCurrencyData
{
    private CurrencyType    _type;
    private int             _amount;

    public DeathCurrencyData(CurrencyType type, int amount)
    {
        _type   = type;
        _amount = amount;
    }

    public CurrencyType Type()
    {
        return _type;
    }

    public int Amount()
    {
        return _amount;
    }
}

public class UI_Death : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject   respawnItemCurrPrefab;
    public Transform    respawnItemCurrParent;

    [Header("ItemCurrency")]
    public GameObject itmCurrScrollView;

    [Header("Text")]
    public Text killerTxt;
    public Text countdownTxtTtl;
    public Text countdownTxt;

    // Private variables
    private string                  _countDown;
    private int                     _timeLeft               = 30;
    private float                   _countdown_start        = 0.0f;
    private int                     _respawnId;
    private bool                    _autoRespawn            = false;
    private float                   _autoRespawnStartTime   = 0;
    private RespawnJson             _respawnData;
    private List<DeathItemData>     _itemList;
    private List<DeathCurrencyData> _currencyList;
    private List<GameObject>        _itemCurrencyList;

    public void Init(string killer, int respawnid)
    {
        ClearItemCurrencyList();
        ClearItemList();
        ClearCurrencyList();
        itmCurrScrollView.SetActive(true);

        killerTxt.text = killer;
        _respawnId = respawnid;

        _respawnData = RespawnRepo.GetRespawnDataByID(respawnid);
        if(_respawnData == null)
        {
            UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("death_RespawnDataNotFound"));

            return;
        }

        _itemList = GetRequiredItemsFromString(_respawnData.deductitem);
        _currencyList = GetRequiredCurrencyFromString(_respawnData.deductcurrency);
        if(_itemList == null && _currencyList == null)
        {
            itmCurrScrollView.SetActive(false);
        }
        else
        {
            if(_itemList != null)
            {
                for(int i = 0; i < _itemList.Count; ++i)
                {
                    DeathItemData data = _itemList[i];
                    GameObject newItemData = Instantiate(respawnItemCurrPrefab);
                    newItemData.transform.SetParent(respawnItemCurrParent, false);

                    Item_Currency_Data itemData = newItemData.GetComponent<Item_Currency_Data>();
                    itemData.Init(data);

                    _itemCurrencyList.Add(newItemData);
                }
            }

            if(_currencyList != null)
            {
                for(int i = 0; i < _currencyList.Count; ++i)
                {
                    DeathCurrencyData data = _currencyList[i];
                    GameObject newCurrencyData = Instantiate(respawnItemCurrPrefab);
                    newCurrencyData.transform.SetParent(respawnItemCurrParent, false);

                    Item_Currency_Data currencyData = newCurrencyData.GetComponent<Item_Currency_Data>();
                    currencyData.Init(data);

                    _itemCurrencyList.Add(newCurrencyData);
                }
            }
        }

        _timeLeft = 0;
        if(_respawnData.countdown != -1 && _respawnData.countdown > 0)
        {
            _timeLeft = _respawnData.countdown;
            SetCountDownTextActive(true);
        }
        else
        {
            SetCountDownTextActive(false);
        }
        _countdown_start = Time.time;
    }

    public void OnClickRespawn()
    {
        if(_respawnData == null)
        {
            UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("death_RespawnDataNotFound"));

            return;
        }

        if (_respawnData.countdown > -1 && _timeLeft <= 0)
        {
            UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("death_RespawnNotNeeded"));

            return;
        }

        if(_respawnData.siturespawn == true)
        {
            if(_itemList.Count == 0 && _currencyList.Count == 0)
            {
                UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("death_SituRespawnNoItemData"));

                return;
            }
            else
            {
                // Open dialog for deducting item and currency
            }
        }

        switch (_respawnData.returntype)
        {
            case ReturnType.ReturnCity:
                if(GameInfo.mRealmInfo.maptype == MapType.City || GameInfo.mRealmInfo.maptype == MapType.Wilderness)
                {
                    RPCFactory.CombatRPC.RespawnAtSafeZone();
                }
                else
                {
                    RPCFactory.CombatRPC.RespawnAtCity();
                }
                break;
            case ReturnType.ReturnSafeZone:
                RPCFactory.CombatRPC.RespawnAtSafeZone();
                break;
        }
    }

    public void OnClickChat()
    {
        UIManager.SetWidgetActive(HUDWidgetType.Chatroom, true);
    }

    private void SetCountDownTextActive(bool active)
    {
        countdownTxtTtl.gameObject.SetActive(active);
        countdownTxt.gameObject.SetActive(active);
    }

    public void Close()
    {
        UIManager.SetWidgetActive(HUDWidgetType.Death, false);
        gameObject.SetActive(false);
        _timeLeft = 0;
    }

    void GuiTimerUpdate()
    {
        if (_timeLeft == 0)
        {
            return;
        }
        int left = _timeLeft - (int)(Time.time - _countdown_start);
        if (left >= 0)
        {
            //RespawnTimerInTown.text = string.Format("<color=red>{0}</color>{1}", left, _countDown);
            //RespawnTimerInSafeArea.text = string.Format("<color=red>{0}</color>{1}", left, _countDown);
            TimeSpan countdown = new TimeSpan(0, 0, left);
            countdownTxt.text = string.Format("{0}:{1}", countdown.Minutes, countdown.Seconds);
        }
        else
        {
            _timeLeft = 0;
        }
    }

    void Update()
    {
        GuiTimerUpdate();
        if (_autoRespawn)
        {
            if (Time.time - _autoRespawnStartTime > 2)
            {
                _autoRespawn = false;
                RPCFactory.CombatRPC.RespawnOnSpot();
            }
        }
    }

    private List<DeathItemData> GetRequiredItemsFromString(string itemListStr)
    {
        if(string.IsNullOrEmpty(itemListStr))
        {
            return null;
        }

        List<DeathItemData> itemList = new List<DeathItemData>();

        List<string> itemListDataList = itemListStr.Split('|').ToList();
        if(itemListDataList != null)
        {
            for(int i = 0; i < itemListDataList.Count; ++i)
            {
                int itemid = 0;
                int amount = 0;
                List<string> itemDataList = itemListDataList[i].Split(';').ToList();
                if (int.TryParse(itemDataList[0], out itemid) && int.TryParse(itemDataList[1], out amount))
                {
                    DeathItemData newItem = new DeathItemData(itemid, amount);
                    itemList.Add(newItem);
                }
            }
        }

        return itemList;
    }

    private List<DeathCurrencyData> GetRequiredCurrencyFromString(string currencyListStr)
    {
        if (string.IsNullOrEmpty(currencyListStr))
        {
            return null;
        }

        List<DeathCurrencyData> currencyList = new List<DeathCurrencyData>();

        List<string> currencyListDataList = currencyListStr.Split('|').ToList();
        if (currencyListDataList != null)
        {
            for (int i = 0; i < currencyListDataList.Count; ++i)
            {
                int currencyid = 0;
                int amount = 0;
                List<string> currencyDataList = currencyListDataList[i].Split(';').ToList();
                if (int.TryParse(currencyDataList[0], out currencyid) && int.TryParse(currencyDataList[1], out amount))
                {
                    DeathCurrencyData newCurrency = new DeathCurrencyData((CurrencyType)currencyid, amount);
                    currencyList.Add(newCurrency);
                }
            }
        }

        return currencyList;
    }

    private void ClearItemCurrencyList()
    {
        if(_itemCurrencyList == null)
        {
            _itemCurrencyList = new List<GameObject>();

            return;
        }

        for(int i = 0; i < _itemCurrencyList.Count; ++i)
        {
            Destroy(_itemCurrencyList[i]);
            _itemCurrencyList[i] = null;
        }
        _itemCurrencyList.Clear();
    }

    private void ClearItemList()
    {
        if(_itemList == null)
        {
            _itemList = new List<DeathItemData>();

            return;
        }

        _itemList.Clear();
    }

    private void ClearCurrencyList()
    {
        if(_currencyList == null)
        {
            _currencyList = new List<DeathCurrencyData>();

            return;
        }
        
        _currencyList.Clear();
    }
}
