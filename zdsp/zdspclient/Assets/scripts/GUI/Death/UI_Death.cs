using Kopio.JsonContracts;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using Zealot.Common;
using Zealot.Client.Entities;
using Zealot.Repository;

public class UI_Death : BaseWindowBehaviour
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
    private List<ItemInfo>          _itemList;
    private List<CurrencyInfo>      _currencyList;
    private List<GameObject>        _itemCurrencyList;

    public void Init(string killer, int respawnid)
    {
        SetHideWhenDeath(false);

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

        _itemList = RespawnRepo.GetItemListByID(respawnid);
        _currencyList = RespawnRepo.GetCurrencyListByID(respawnid);
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
                    ItemInfo data = _itemList[i];
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
                    CurrencyInfo data = _currencyList[i];
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

        if(_respawnData.countdown > -1 && _timeLeft <= 0)
        {
            UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("death_RespawnNotNeeded"));

            return;
        }

        if(_respawnData.siturespawn == true)
        {
            if(_itemList.Count > 0 || _currencyList.Count > 0)
            {
                if(IsSuffcient() == false)
                {
                    return;
                }

                RPCFactory.CombatRPC.RespawnOnSpot();
            }
            else
            {
                RPCFactory.CombatRPC.RespawnOnSpot();
            }

            return;
        }
        
        switch(_respawnData.respawntype)
        {
            case RespawnType.City:
                if(GameInfo.mRealmInfo.maptype != MapType.City)
                {
                    RPCFactory.CombatRPC.RespawnAtSafeZone();
                }
                else
                {
                    RPCFactory.CombatRPC.RespawnAtCity();
                }
                break;
            case RespawnType.SafeZone:
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

    private void OnDisable()
    {
        SetHideWhenDeath(true);
    }

    private void SetHideWhenDeath(bool bShow)
    {
        UIManager.SetWidgetActive(HUDWidgetType.HideWhenDeath, bShow);
    }

    private bool IsSuffcient()
    {
        PlayerGhost player = GameInfo.gLocalPlayer;
        if(player == null)
        {
            return false;
        }

        bool res = true;
        List<ItemInfo> itemList = RespawnRepo.GetItemListByID(_respawnId);
        List<CurrencyInfo> currencyList = RespawnRepo.GetCurrencyListByID(_respawnId);
        if(itemList.Count == 0 && currencyList.Count == 0)
        {
            return false;
        }

        if(itemList.Count > 0)
        {
            for(int i = 0; i < itemList.Count; ++i)
            {
                ItemInfo item = itemList[i];
                int invItemCount = player.clientItemInvCtrl.itemInvData.GetTotalStackCountByItemId(item.itemId);

                if(invItemCount < item.stackCount)
                {
                    UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("ret_Death_InsufficientReviveItem"));

                    res = false;
                }
            }
        }

        if(currencyList.Count > 0)
        {
            for(int i = 0; i < currencyList.Count; ++i)
            {
                CurrencyInfo currency = currencyList[i];
                long invCurrencyCount = player.GetCurrencyAmount(currency.currencyType);

                if(invCurrencyCount < currency.amount)
                {
                    UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("ret_Death_InsufficientReviveCurrency"));

                    res = false;
                }
            }
        }

        return res;
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
            _itemList = new List<ItemInfo>();

            return;
        }

        _itemList.Clear();
    }

    private void ClearCurrencyList()
    {
        if(_currencyList == null)
        {
            _currencyList = new List<CurrencyInfo>();

            return;
        }
        
        _currencyList.Clear();
    }
}
