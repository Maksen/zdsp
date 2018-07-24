using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using Kopio.JsonContracts;
using Zealot.Common;
using System.Globalization;

namespace Zealot.Common
{
    public static class ItemMallConstants
    {
        public static string DateFormat = "yyyy_MMdd_HHmm";
    }

    public enum ItemMallReturnCode
    {
        PurchaseItem_Success,
        PurchaseItem_Success_SentToMail,
        PurchaseItem_Fail_BagFull,
        PurchaseItem_Fail_VIPLevel,
        PurchaseItem_Fail_PeerNotFound,
        PurchaseItem_Fail_InvalidDateTime,
        PurchaseItem_Fail_InvalidCategory,
        PurchaseItem_Fail_InsufficientCurrency,
        PurchaseItem_Fail_UnknownInventoryReturnCode,
        PurchaseItem_Fail_PurchaseLimitExceed,
        PurchaseItem_Fail_ShopNotOpen,
        PurchaseItem_Fail_ShopNotSync,
    }



    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class ItemMallLimitEntry
    {
        [JsonProperty(PropertyName = "id")]
        public int id { get; set; }

        [JsonProperty(PropertyName = "stk")]
        public int stackPurchased { get; set; }

        [JsonProperty(PropertyName = "isGM")]
        public bool isGM { get; set; }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class ItemMallInventoryData
    {
        [JsonProperty(PropertyName = "limit")]
        public List<ItemMallLimitEntry> lstItemMallData = new List<ItemMallLimitEntry>();

        [JsonProperty(PropertyName = "timing")]
        public ItemMall_Treasure itemMallTreasureTiming = new ItemMall_Treasure();
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class ItemMall_Treasure
    {
        [JsonProperty(PropertyName = "tortoise")]
        public DateTime endtime_tortoise;
        [JsonProperty(PropertyName = "phoenix")]
        public DateTime endtime_phoenix;
        [JsonProperty(PropertyName = "tiger")]
        public DateTime endtime_tiger;
        [JsonProperty(PropertyName = "dragon")]
        public DateTime endtime_dragon;

        public void SetNewEndTime(ItemMallShoppingType type, DateTime dt)
        {
            if (type == ItemMallShoppingType.Phoenix)
                endtime_phoenix = dt;
            else if (type == ItemMallShoppingType.Tortoise)
                endtime_tortoise = dt;
            else if (type == ItemMallShoppingType.Dragon)
                endtime_dragon = dt;
            else if (type == ItemMallShoppingType.Tiger)
                endtime_tiger = dt;
        }

        public ItemMall_Treasure()
        {
            endtime_tortoise = endtime_phoenix = endtime_tiger = endtime_dragon = DateTime.MinValue;
        }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class ItemMall_InitMall
    {
        [JsonProperty(PropertyName = "1")]
        public ItemMall_PurchaseLimit purchaseLimit_Kopio = new ItemMall_PurchaseLimit();

        [JsonProperty(PropertyName = "2")]
        public ItemMall_PurchaseLimit purchaseLimit_GM = new ItemMall_PurchaseLimit();

        [JsonProperty(PropertyName = "3")]
        public DateTime _endtime_tortoise = DateTime.MinValue;

        [JsonProperty(PropertyName = "4")]
        public DateTime _endtime_phoenix = DateTime.MinValue;

        [JsonProperty(PropertyName = "5")]
        public DateTime _endtime_tiger = DateTime.MinValue;

        [JsonProperty(PropertyName = "6")]
        public DateTime _endtime_dragon = DateTime.MinValue;

        [JsonProperty(PropertyName = "7")]
        public List<bool> isShopOpen = new List<bool>();
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class ItemMall_InitItemInfo
    {
        [JsonProperty(PropertyName = "gmI")]
        public List<ItemMall_Item> gmItems = new List<ItemMall_Item>();
    }


    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class ItemMall_PurchaseLimit
    {
        //ID, limit
        [JsonProperty(PropertyName = "lmtT")]
        public Dictionary<int, int> dicLimit = new Dictionary<int, int>();
        [JsonProperty(PropertyName = "isGM")]
        public bool isGMTools;
    }


    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class ItemMall_Item
    {
        [JsonProperty(PropertyName = "1")]
        public int shopItemID = 0;
        [JsonProperty(PropertyName = "2")]
        public string name = "";
        [JsonProperty(PropertyName = "3")]
        public int itemid = 0;
        [JsonProperty(PropertyName = "4")]
        public int price = 0;
        [JsonProperty(PropertyName = "5")]
        public int stackcount = 0;
        [JsonProperty(PropertyName = "6")]
        public int limitcount = 0;
        [JsonProperty(PropertyName = "7")]
        public ItemMallCategory category = 0;
        [JsonProperty(PropertyName = "8")]
        public ItemMallShoppingType shoppingtype = 0;
        [JsonProperty(PropertyName = "9")]
        public int viplevel = 0;
        [JsonProperty(PropertyName = "10")]
        public DateTime beginDate = DateTime.MaxValue;
        [JsonProperty(PropertyName = "11")]
        public DateTime endDate = DateTime.MaxValue;
        [JsonProperty(PropertyName = "12")]
        public int sortnumber = 0;
        [JsonProperty(PropertyName = "13")]
        public bool online = false;
        [JsonProperty(PropertyName = "14")]
        public bool showTime = false;
        [JsonProperty(PropertyName = "15")]
        public bool showLimited = false;
        [JsonProperty(PropertyName = "16")]
        public bool showVIP = false;
        [JsonProperty(PropertyName = "17")]
        public JobType shoppingjob = JobType.Newbie;
        [JsonProperty(PropertyName = "18")]
        public bool isGM = false;
        public ItemMall_Item() { }

        public ItemMall_Item(ItemMallItemJson json)
        {
            shopItemID = json.id;
            name = json.name;
            itemid = json.itemid;
            price = json.price;
            stackcount = json.purchasecount;
            limitcount = json.limitcount;
            category = json.category;
            shoppingtype = json.shoppingtype;
            viplevel = json.viplevel;

            DateTime result;
            bool success = DateTime.TryParseExact(json.uptime, ItemMallConstants.DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out result);
            beginDate = success ? result : DateTime.MaxValue;

            success = DateTime.TryParseExact(json.downtime, ItemMallConstants.DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out result);
            endDate = success ? result : DateTime.MaxValue;

            sortnumber = json.sortnumber;
            online = json.online;
            showLimited = json.showlimited;
            showTime = json.showtime;
            showVIP = json.showvip;
            shoppingjob = json.shoppingjob;
        }

        public ItemMall_Item(Dictionary<string, object> info)
        {
            try
            {
                shopItemID = (int)info["GUID"];
                name = (string)info["ItemName"];
                itemid = (int)info["ItemID"];
                price = (int)info["XenjoPoints"];
                stackcount = (int)info["StackCount"];
                limitcount = (int)info["ShoppingCount"];
                category = (ItemMallCategory)info["ItemCategory"];
                shoppingtype = (ItemMallShoppingType)info["ShoppingType"];
                viplevel = (int)info["VIPLv"];

                DateTime result;
                bool success = DateTime.TryParseExact((string)info["ItemUpTime"], ItemMallConstants.DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out result);
                beginDate = success ? result : DateTime.MaxValue;

                success = DateTime.TryParseExact((string)info["ItemDownTime"], ItemMallConstants.DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out result);
                endDate = success ? result : DateTime.MaxValue;

                sortnumber = (int)info["SortNumber"];
                online = (bool)info["ItemOnline"];
                showLimited = (bool)info["showLimited"];
                showTime = (bool)info["showTime"];
                showVIP = (bool)info["showVIP"];
                shoppingjob = (JobType)info["shoppingJob"];

                isGM = true;
            }
            catch (Exception ex)
            {
                online = false;
            }
        }

        public bool GetIsGM()
        {
            return isGM;
        }
    }
}
