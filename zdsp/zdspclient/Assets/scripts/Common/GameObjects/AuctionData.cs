using Kopio.JsonContracts;
using Zealot.Common;
using Zealot.Repository;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.ComponentModel;
using System;

namespace Zealot.Common
{
    public enum AuctionReturnCode
    {
        Failed,
        Success,
        InsufficientGold,
        InventoryFull,
        MinPrice,
        AlreadyPlacedBid,
        AuctionEnded,
    }

    public class BidItemInfo
    {
        public int bidId;
        public DateTime auctionEndDt;
        public int itemId;
        public int itemCount;
        public int winningPrice;
        public int refundLockGold;
        public int refundGold;
    }

    public class RecordDataInfo
    {
        public DateTime dt;
        public int auctionId;
        public int itemId;
        public int itemCount;
        public BidInfo bid1;
        public BidInfo bid2;
    }

    public class BidInfo
    {
        public DateTime bidDt;
        public int serverId;
        public string serverName;
        public string bidderName;
        public int bidPrice;
        public int lockGold;
        public int gold;

        public BidInfo() { }

        public BidInfo(int id, DateTime dt, string server, string bidder, int price, int bindgold, int gld)
        {
            serverId = id;
            bidDt = dt;
            serverName = server;
            bidderName = bidder;
            bidPrice = price;
            lockGold = bindgold;
            gold = gld;
        }
    }

    public class AuctionItem
    {
        public int auctionId;
        public string auctionName;
        public int itemId;
        public int itemCount;
        public DateTime beginDateTime;
        public DateTime endDateTime;
        public int minPrice;

        public AuctionItem() { }

        public AuctionItem(Dictionary<string, object> data)
        {
            auctionId = (int)data["Id"];
            auctionName = (string)data["Name"];
            itemId = (int)data["ItemId"];
            itemCount = (int)data["ItemCount"];
            beginDateTime = (DateTime)data["BeginDate"];
            endDateTime = (DateTime)data["EndDate"];
            minPrice = (int)data["MinPrice"];
        }
    }

}
