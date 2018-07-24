using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using Kopio.JsonContracts;
using Zealot.Common;
using System.Globalization;
using Zealot.Repository;

namespace Zealot.Common
{
    //public static class StoreConstants
    //{
    //    //public static string DateFormat = "yyyy_MMdd_HHmm";
    //}

    public enum StoreReturnCode
    {
        PurchaseItem_Success,
        //PurchaseItem_Success_SentToMail,
        PurchaseItem_Fail_NoBagSpace,
        PurchaseItem_Fail_Level,
        PurchaseItem_Fail_PeerNotFound,
        PurchaseItem_Fail_InvalidItem,
        PurchaseItem_Fail_NotInList,
        PurchaseItem_Fail_InsufficientCurrency,
        PurchaseItem_Fail_UnknownInventoryReturnCode,
    }

    
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class Store_Item
    {
        [JsonProperty(PropertyName = "id")]
        public int storeID = 0;
        [JsonProperty(PropertyName = "s")]
        public byte isSold = 0;

        public Store_Item(int id, byte s)
        {
            storeID = id;
            isSold = s;
        }
    }

    public class StoreCategory
    {
        [JsonProperty(PropertyName = "c")]
        public int category = 0;
        [JsonProperty(PropertyName = "l")]
        public List<Store_Item> list_storeitem = new List<Store_Item>();
        [JsonProperty(PropertyName = "r")]
        public int refreshCount = 0;
        [JsonProperty(PropertyName = "n")]
        public DateTime nextRefresh = DateTime.MinValue;
    }

    public class StoreRPCData
    {
        public StoreCategory mCate = null;
        public List<int> mOpenedCateList = new List<int>();

        public bool Init(StoreData sd, int selectedCateId)
        {
            //error-check
            if (sd == null || selectedCateId <= 0 || selectedCateId >= sd.list_store.Count)
                return true;

            //Selected category
            mCate = sd.list_store[selectedCateId];

            //Show all non-hero stores, and hero stores if available
            for (int i = 0; i < sd.list_store.Count; ++i)
            {
                if (sd.list_store[i] != null || StoreRepo.GetStoreSetting(i) != null && StoreRepo.GetStoreSetting(i).heroID == 0)
                    mOpenedCateList.Add(i);
            }

            return false;
        }
    }


    public class StoreData
    {
        [JsonProperty(PropertyName = "lst")]
        public List<StoreCategory> list_store = null;

        public void InitDefault()
        {
            list_store = new List<StoreCategory>(50);
            for (int i = 0; i < 50; ++i)
            {
                list_store.Add(null);
            }
        }
    }

}
