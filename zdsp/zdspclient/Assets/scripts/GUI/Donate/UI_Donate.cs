using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using Zealot.Common;
using System;
using Zealot.Repository;
using Kopio.JsonContracts;

public class UI_Donate : MonoBehaviour
{
    [SerializeField]
    Text NextUpdate;

    [SerializeField]
    Text Progress;

    [SerializeField]
    Transform DonateContent;

    [SerializeField]
    GameObject DonateData;

    private DonateClientController mDonateController;
    private List<DonateOrderData> mDonateOrder;
    private Dictionary<string, GameObject> mDonateObj = new Dictionary<string, GameObject>();
    private int mRemainMinute;

    private void OnEnable()
    {
        if (GameInfo.gLocalPlayer == null)
        {
            return;
        }
        
        mDonateController = GameInfo.gLocalPlayer.DonateController;
        mDonateOrder = mDonateController.GetDonateOrders();
        UpdateUI();
    }

    public void UpdateDonateData(string guid,int result)
    {
        if (mDonateObj.ContainsKey(guid))
        {
            DonateOrderData donateOrder = mDonateController.GetDonateData(guid);
            if (donateOrder != null && result == 2)
            {
                DonateJson donateJson = DonateRepo.GetDonateById(donateOrder.Id);
                if (donateJson != null && donateOrder.Count + 1 >= donateJson.maxdonate)
                {
                    Destroy(mDonateObj[guid]);
                    mDonateObj.Remove(guid);
                }
            }
        }
    }

    public void RefreshDonate()
    {
        if (mDonateController == null)
        {
            return;
        }
        
        mDonateOrder = mDonateController.GetDonateOrders();
        UpdateUI();
    }

    private void UpdateUI()
    {
        UpdateRemainTime();
        int maxorder = DonateRepo.GetMaxOrder(GameInfo.gLocalPlayer.PlayerSynStats.Level);
        Progress.text = mDonateOrder.Count + "/" + maxorder;
        UpdateDonateData();
    }

    private void UpdateDonateData()
    {
        Clean();
        foreach (DonateOrderData donate in mDonateOrder)
        {
            GameObject newdonate = Instantiate(DonateData);
            newdonate.GetComponent<UI_DonateData>().Init(donate, GameInfo.gLocalPlayer);
            newdonate.transform.SetParent(DonateContent, false);
            mDonateObj.Add(donate.Guid, newdonate);
        }
    }

    private void UpdateRemainTime()
    {
        DateTime nextupdatedt = GetNextUpdateDateTime();
        mRemainMinute = (int)(nextupdatedt - DateTime.Now).TotalMinutes;
        NextUpdate.text = GameUtils.FormatTimeStringTillMinute(mRemainMinute);
        StartCoroutine(RemainTimeUpdate());
    }

    private IEnumerator RemainTimeUpdate()
    {
        yield return new WaitForSecondsRealtime(60);
        UpdateRemainTime();
    }

    private DateTime GetNextUpdateDateTime()
    {
        int hour = DateTime.Now.Hour;
        if (hour >= 0 && hour < 6)
        {
            return new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 6, 0, 0);
        }
        else if (hour >= 6 && hour < 12)
        {
            return new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 12, 0, 0);
        }
        else if (hour >= 12 && hour < 18)
        {
            return new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 18, 0, 0);
        }
        else
        {
            return new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day + 1, 0, 0, 0);
        }
    }

    private void Clean()
    {
        foreach(KeyValuePair<string, GameObject> entry in mDonateObj)
        {
            Destroy(entry.Value);
        }
        mDonateObj = new Dictionary<string, GameObject>();
    }

    private void OnDisable()
    {
        Clean();
    }
}
