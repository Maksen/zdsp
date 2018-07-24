using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Zealot.Common
{
    public enum DonateReturnCode
    {
        //Send Data ReturnCode
        UpdateData,
        RequestSuccess,
        ResponseSuccess,
        GetDonateSuccess,
        ResponseUpdateData,

        //Not Send Data ReturnCode
        DataNotDirty,
        NotEnoughSpace,
    }

    public class DonateToClientData
    {
        public Dictionary<string, DonateInventory> DonateRequestDic;
        public DonateRemainingTimesInv DonateRemainingTimes;
        public DateTime LastUpdateTime;

        [DefaultValue(true)]
        public bool CanRequest;

        public DonateToClientData()
        {
            DonateRequestDic = new Dictionary<string, DonateInventory>();
            DonateRemainingTimes = new DonateRemainingTimesInv();
            CanRequest = true;
        }
    }

    public class DonateInventory
    {
        public int HeroId;
        public int DonateStatusMin;
        public int DonateStatusMax;
        public int DonateCanGetCount;
        public bool RequestEnd;
        public int PortraitId;
        public int VipLvl;
    }

    public class DonateRemainingTimesInv
    {
        public int RequestCount;
        public int ResponseCount;
    }
}

