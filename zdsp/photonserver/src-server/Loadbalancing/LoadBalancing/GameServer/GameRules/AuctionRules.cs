using ExitGames.Concurrency.Fibers;
using Photon.LoadBalancing.GameServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Zealot.Common;

namespace Zealot.Server.Rules
{
    public class BidData
    {
        public int bidId { get; set; }
        public int auctionId { get; set; }
        public int itemId { get; set; }
        public int itemCount { get; set; }
        public DateTime bidDt { get; set; }
        public int serverID { get; set; }
        public string serverName { get; set; }
        public string bidderName { get; set; }
        public int bidPrice { get; set; }
        public int lockGold { get; set; }
        public int gold { get; set; }
        public bool isWinner { get; set; }
        public int winningPrice { get; set; }
        public DateTime auctionEndDt { get; set; }

        public BidData() { }

        public BidData(Dictionary<string, object> data)
        {
            bidId = (int)data["bidid"];
            auctionId = (int)data["auctionid"];
            itemId = (int)data["ItemId"];
            itemCount = (int)data["ItemCount"];
            bidDt = (DateTime)data["dtbid"];
            serverID = (int)data["serverid"];
            serverName = (string)data["servername"];
            bidderName = (string)data["bidder"];
            bidPrice = (int)data["bidprice"];
            lockGold = (int)data["lockgold"];
            gold = (int)data["gold"];
            isWinner = (data["Winner"] != DBNull.Value) ? (string)data["Winner"] == bidderName : false;
            winningPrice = (data["WinningPrice"] != DBNull.Value) ? (int)data["WinningPrice"] : -1;
            auctionEndDt = (DateTime)data["EndDate"];
        }

        public BidData(int auctionid, int itemid, int itemcount, DateTime dtBid, int serverid, string servername,
                        string name, int price, int lockgold, int goldused, DateTime dtEnd)
        {
            auctionId = auctionid;
            itemId = itemid;
            itemCount = itemcount;
            bidDt = dtBid;
            serverID = serverid;
            serverName = servername;
            bidderName = name;
            bidPrice = price;
            lockGold = lockgold;
            gold = goldused;
            isWinner = false;
            winningPrice = -1;
            auctionEndDt = dtEnd;
        }

        public void UpdateBidWinner(bool winner, int price)
        {
            isWinner = winner;
            winningPrice = price;
        }

        public void CalculateRefund(out int refundLockGold, out int refundGold, out int spentLockGold, out int spentGold)
        {
            if (lockGold > winningPrice)
            {
                refundLockGold = lockGold - winningPrice;
                refundGold = gold;
                spentLockGold = winningPrice;
                spentGold = 0;
            }
            else
            {
                refundLockGold = 0;
                int leftover = winningPrice - lockGold;
                refundGold = gold - leftover;
                spentLockGold = lockGold;
                spentGold = leftover;
            }
        }
    }

    public static class AuctionRules
    {
        public static List<AuctionItem> AuctionItemList = new List<AuctionItem>();
        public static Dictionary<string, List<BidData>> BidsByPlayer = new Dictionary<string, List<BidData>>();

        private static readonly PoolFiber executionFiber;
        private static int serverId;
        private static AuctionItem currentAuctionItem;
        private static long endTimeDelay = 500;  // wait a little for all bids to come in
        private static IDisposable auctionStart;
        private static IDisposable auctionEnd;

        static AuctionRules()
        {
            executionFiber = GameApplication.Instance.executionFiber;
        }

        public static void Dispose()
        {
        }

        public static void StopTimer()
        {
            if (auctionStart != null)
            {
                auctionStart.Dispose();
                auctionStart = null;
            }

            if (auctionEnd != null)
            {
                auctionEnd.Dispose();
                auctionEnd = null;
            }
        }

        public static async Task Init()
        {
            serverId = GameApplication.Instance.GetMyServerline();
            await InitAuctionBidsFromDB();
            await InitAuctionItemsFromDB();
        }

        private static async Task InitAuctionBidsFromDB()
        {
            List<Dictionary<string, object>> results = await GameApplication.dbRepository.AuctionData.GetAllAuctionBids(serverId);
            executionFiber.Enqueue(() =>
            {
                BidsByPlayer.Clear();
                foreach (Dictionary<string, object> data in results)
                {
                    BidData bid = new BidData(data);
                    if (BidsByPlayer.ContainsKey(bid.bidderName))
                        BidsByPlayer[bid.bidderName].Add(bid);
                    else
                        BidsByPlayer.Add(bid.bidderName, new List<BidData>() { bid });
                }
            });
        }

        public static async Task InitAuctionItemsFromDB()
        {
            List<AuctionItem> expiredAuctionItems = new List<AuctionItem>();
            List<Dictionary<string, object>> results = await GameApplication.dbRepository.AuctionData.GetAuctionItems();
            executionFiber.Enqueue(() =>
            {
                AuctionItemList.Clear();
                foreach (Dictionary<string, object> item in results)
                {
                    AuctionItem newItem = new AuctionItem(item);
                    if (newItem.endDateTime <= DateTime.Now)  // auction supposed to be over but result not determined
                    {
                        expiredAuctionItems.Add(newItem);
                        continue;
                    }
                    if (currentAuctionItem != null && newItem.auctionId == currentAuctionItem.auctionId)  // skip if auction is already ongoing 
                        continue;
                    AuctionItemList.Add(newItem);
                }

                // Process all expired auctions
                ProcessExpiredAuctions(expiredAuctionItems);

                // Sort and start new auctions
                if (AuctionItemList.Count > 0)
                {
                    AuctionItemList.Sort((x, y) => x.beginDateTime.CompareTo(y.beginDateTime));
                    if (currentAuctionItem == null)
                        SetAuctionTimer();
                    else
                    {                        
                        if (currentAuctionItem.endDateTime < AuctionItemList[0].beginDateTime)
                        {
                            string nextAuctionTime = AuctionItemList[0].beginDateTime.ToString("yyyy/MM/dd HH:mm");
                            GameApplication.Instance.BroadcastMessage(BroadcastMessageType.AuctionChanged, nextAuctionTime);
                        }                      
                    }
                }
            });
        }

        private static void ProcessExpiredAuctions(List<AuctionItem> expiredItems)
        {
            for (int i = 0; i < expiredItems.Count; i++)
            {
                AuctionItem item = expiredItems[i];
                executionFiber.Enqueue(async () => await DetermineAuctionWinner(item, true).ConfigureAwait(false));
            }
        }

        private static void SetAuctionTimer()
        {
            StopTimer();
            if (AuctionItemList.Count == 0)
                return;

            double timeToStart = AuctionItemList[0].beginDateTime.Subtract(DateTime.Now).TotalMilliseconds;
            if (timeToStart > 2100000000)
                timeToStart = 2100000000;
            if (timeToStart < 0)
                timeToStart = 0;
            auctionStart = executionFiber.Schedule(OnBeginAuction, (long)timeToStart);

            double timeToEnd = AuctionItemList[0].endDateTime.Subtract(DateTime.Now).TotalMilliseconds;
            if (timeToEnd > 2100000000)
                timeToEnd = 2100000000;
            if (timeToEnd < 0)
                timeToEnd = 0;
            else
                timeToEnd += endTimeDelay;
            auctionEnd = executionFiber.Schedule(OnEndAuction, (long)timeToEnd);

            if (timeToStart > 0)  // only push notification if not immediate start
            {
                string nextAuctionTime = AuctionItemList[0].beginDateTime.ToString("yyyy/MM/dd HH:mm");
                GameApplication.Instance.BroadcastMessage(BroadcastMessageType.AuctionChanged, nextAuctionTime);
            }
            //GameUtils.DebugWriteLine("set timer start: " + AuctionItemList[0].beginDateTime);
        }

        private static void OnBeginAuction()
        {
            if (AuctionItemList.Count == 0 || currentAuctionItem != null)
                return;

            currentAuctionItem = AuctionItemList[0];
            AuctionItemList.RemoveAt(0);
            BroadcastAuctionBegin();
        }

        private static void OnEndAuction()
        {
            if (DateTime.Now < currentAuctionItem.endDateTime)
            {
                double timeToEnd = currentAuctionItem.endDateTime.Subtract(DateTime.Now).TotalMilliseconds;
                auctionEnd = executionFiber.Schedule(OnEndAuction, (long)timeToEnd + endTimeDelay);
            }
            else
            {
                AuctionItem endedAuctionItem = currentAuctionItem;
                currentAuctionItem = null;

                // determine winner
                executionFiber.Enqueue(async () => await DetermineAuctionWinner(endedAuctionItem).ConfigureAwait(false));
            }
        }

        private static async Task DetermineAuctionWinner(AuctionItem auctionItem, bool isBackDated = false)
        {
            List<Dictionary<string, object>> results = await GameApplication.dbRepository.AuctionData.GetAllAuctionBidsByAuctionId(auctionItem.auctionId);

            executionFiber.Enqueue(() =>
            {
                BidInfo bid1 = null, bid2 = null;
                string winner = "";
                int winningPrice = 0;
                if (results.Count > 0)
                {
                    List<BidData> bidList = new List<BidData>();
                    List<string> biddersList = new List<string>();
                    foreach (Dictionary<string, object> data in results)
                    {
                        BidData bid = new BidData();
                        bid.bidId = (int)data["bidid"];
                        bid.bidDt = (DateTime)data["dtbid"];
                        bid.serverID = (int)data["serverid"];
                        bid.serverName = (string)data["servername"];
                        bid.bidderName = (string)data["bidder"];
                        bid.bidPrice = (int)data["bidprice"];
                        bid.lockGold = (int)data["lockgold"];
                        bid.gold = (int)data["gold"];
                        bidList.Add(bid);
                        biddersList.Add(bid.bidderName);
                    }

                    // sort by highest price first, then by earlier bid time first
                    var sortedList = bidList.OrderByDescending(x => x.bidPrice).ThenBy(x => x.bidDt).ToList();
                    if (sortedList[0].bidPrice > auctionItem.minPrice)
                    {
                        BidData highestBid = sortedList[0];  // highest bid
                        winner = highestBid.bidderName;
                        BidData secondHighestBid = null;  // second bid
                        //2nd highest bidder
                        if (sortedList.Count > 1)
                            secondHighestBid = sortedList[1];

                        // Add auction record
                        bid1 = new BidInfo(highestBid.serverID, highestBid.bidDt, highestBid.serverName, highestBid.bidderName, highestBid.bidPrice,
                            highestBid.lockGold, highestBid.gold);

                        if (secondHighestBid != null)
                        {
                            bid2 = new BidInfo(secondHighestBid.serverID, secondHighestBid.bidDt, secondHighestBid.serverName, secondHighestBid.bidderName, secondHighestBid.bidPrice,
                                secondHighestBid.lockGold, secondHighestBid.gold);
                            winningPrice = secondHighestBid.bidPrice;
                        }
                        else  // only 1 bidder, 2nd highest is system
                        {
                            bid2 = new BidInfo(0, auctionItem.beginDateTime, "", "", auctionItem.minPrice, 0, 0);
                            winningPrice = auctionItem.minPrice;
                        }
                        // update bids with winning bid info
                        UpdateAuctionBidsWinner(bidList, auctionItem.auctionId, highestBid.bidId, winningPrice);

                        // Broadcast winner
                        BroadcastAuctionEnd(1, auctionItem.itemId, highestBid.serverID, highestBid.serverName, highestBid.bidderName, winningPrice, biddersList);

                        // System log
                        LogAuctionItem(GameApplication.Instance.GetCharPeer(highestBid.bidderName), "WinAuction", highestBid.bidderName, auctionItem.itemId, auctionItem.itemCount);
                    }
                }
                else  // no bids
                {
                    BroadcastAuctionEnd(0, auctionItem.itemId, 0, "", "", 0, null);
                }

                // Add record and update winner and winning price on auction item
                AddAuctionRecord(auctionItem, bid1, bid2, winningPrice, winner);

                if (!isBackDated)
                    SetAuctionTimer();  // check for next auction item
            });
        }

        private static void UpdateAuctionBidsWinner(List<BidData> bidList, int auctionId, int winnerBidId, int winningPrice)
        {
            for (int i = 0; i < bidList.Count; i++)
            {
                if (bidList[i].serverID == serverId) // only update my server
                {
                    if (BidsByPlayer.ContainsKey(bidList[i].bidderName))
                    {
                        List<BidData> bids = BidsByPlayer[bidList[i].bidderName];  // list of bids by this player
                        BidData currentBid = bids.FirstOrDefault(x => x.auctionId == auctionId);  // find the bid for this auction
                        if (currentBid != null)  // shouldn't be null but check anyway
                        {
                            bool isWinner = currentBid.bidId == winnerBidId;
                            currentBid.UpdateBidWinner(isWinner, winningPrice);
                        }
                    }
                }
            }
        }

        private static void AddAuctionRecord(AuctionItem item, BidInfo bid1, BidInfo bid2, int winPrice, string winner)
        {
            string bid1Str = null;
            string bid2Str = null;
            if (bid1 != null)
                bid1Str = JsonConvert.SerializeObject(bid1);
            if (bid2 != null)
                bid2Str = JsonConvert.SerializeObject(bid2);

            executionFiber.Enqueue(async () => await AddNewRecord(item, bid1Str, bid2Str, winPrice, winner).ConfigureAwait(false));
        }

        private static async Task AddNewRecord(AuctionItem item, string bid1, string bid2, int winPrice, string winner)
        {
            bool res = await GameApplication.dbRepository.AuctionData.AddAuctionRecord(item.auctionId, item.auctionName, item.beginDateTime, item.endDateTime,
                item.itemId, item.itemCount, bid1, bid2, winPrice, winner);
        }

        private static void BroadcastAuctionBegin()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(currentAuctionItem.itemId);
            sb.Append(";");
            sb.Append(currentAuctionItem.endDateTime.ToString("yyyy/MM/dd HH:mm"));
            GameApplication.Instance.BroadcastMessage(BroadcastMessageType.AuctionBegin, sb.ToString());
        }

        private static void BroadcastAuctionEnd(byte result, int itemId, int serverId, string serverName, string bidder, int price, List<string> biddersList)
        {
            string bidderListStr = JsonConvert.SerializeObject(biddersList);
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0};{1};{2};{3};{4};{5}", result, itemId, serverName, bidder, price, bidderListStr);
            GameApplication.Instance.BroadcastMessage(BroadcastMessageType.AuctionEnd, sb.ToString());
        }

        private static bool HasSubmittedBid(int auctionId, string playerName, out int bidPrice)
        {
            bidPrice = 0;
            if (BidsByPlayer.ContainsKey(playerName))
            {
                BidData placedBid = BidsByPlayer[playerName].FirstOrDefault(x => x.auctionId == auctionId);
                if (placedBid != null)
                {
                    bidPrice = placedBid.bidPrice;
                    return true;
                }
                else
                    return false;
            }
            return false;
        }

        public static void PlaceBid(int auctionId, int bidPrice, GameClientPeer peer)
        {
            DateTime now = DateTime.Now;

            if (currentAuctionItem != null)
            {
                if (currentAuctionItem.auctionId != auctionId)  // check is placing bid for current auction item
                {
                    peer.ZRPC.CombatRPC.Ret_AuctionPlaceBid((byte)AuctionReturnCode.AuctionEnded, 0, peer);
                    return;
                }
                if (currentAuctionItem.endDateTime <= now) // check whether auction ended
                {
                    peer.ZRPC.CombatRPC.Ret_AuctionPlaceBid((byte)AuctionReturnCode.AuctionEnded, 0, peer);
                    return;
                }
            }
            else  // no current auction means already ended
            {
                peer.ZRPC.CombatRPC.Ret_AuctionPlaceBid((byte)AuctionReturnCode.AuctionEnded, 0, peer);
                return;
            }
            // check whether already submitted bid
            int myBidPrice;
            if (HasSubmittedBid(auctionId, peer.mChar, out myBidPrice))
            {
                peer.ZRPC.CombatRPC.Ret_AuctionPlaceBid((byte)AuctionReturnCode.AlreadyPlacedBid, myBidPrice, peer);
                return;
            }
            // check whether bid is more than minimum price
            if (bidPrice <= currentAuctionItem.minPrice)
            {
                peer.ZRPC.CombatRPC.Ret_AuctionPlaceBid((byte)AuctionReturnCode.MinPrice, 0, peer);
                return;
            }
            // check player has sufficient gold to bid
            if (!peer.mPlayer.IsCurrencySufficient(CurrencyType.Gold, bidPrice, true))
            {
                peer.ZRPC.CombatRPC.Ret_AuctionPlaceBid((byte)AuctionReturnCode.InsufficientGold, 0, peer);
                return;
            }

            executionFiber.Enqueue(async () => await AddNewBid(peer, auctionId, now, bidPrice).ConfigureAwait(false));
        }


        private static async Task AddNewBid(GameClientPeer peer, int auctionId, DateTime dt, int bidPrice)
        {
            int lockGoldToUse = 0;
            int goldToUse = 0;
            if (peer.mPlayer.SecondaryStats.bindgold >= bidPrice)
                lockGoldToUse = bidPrice;
            else
            {
                lockGoldToUse = peer.mPlayer.SecondaryStats.bindgold;
                goldToUse = bidPrice - lockGoldToUse;
            }

            if (currentAuctionItem != null)
            {
                string serverName = GameApplication.Instance.MyServerConfig.servername;
                BidData newBid = new BidData(auctionId, currentAuctionItem.itemId, currentAuctionItem.itemCount, dt, serverId, serverName,
                                                peer.mChar, bidPrice, lockGoldToUse, goldToUse, currentAuctionItem.endDateTime);

                Tuple<int, bool> res = await GameApplication.dbRepository.AuctionData.InsertNewBid(auctionId, newBid.bidDt, newBid.serverID, newBid.serverName,
                                            newBid.bidderName, newBid.bidPrice, newBid.lockGold, newBid.gold).ConfigureAwait(false);
                executionFiber.Enqueue(() =>
                {
                    if (res.Item2) // insert success
                    {
                        int lockGoldOld = peer.mPlayer.SecondaryStats.bindgold;
                        int goldOld = peer.mPlayer.SecondaryStats.Gold;

                        peer.mPlayer.DeductGold(bidPrice, true, false, "Auction_Bid"); // deduct gold here but do not spend
                        newBid.bidId = res.Item1;
                        if (BidsByPlayer.ContainsKey(peer.mChar))
                            BidsByPlayer[peer.mChar].Add(newBid);
                        else
                            BidsByPlayer.Add(peer.mChar, new List<BidData>() { newBid });

                        peer.ZRPC.CombatRPC.Ret_AuctionPlaceBid((byte)AuctionReturnCode.Success, bidPrice, peer);

                        int lockGoldNew = peer.mPlayer.SecondaryStats.bindgold;
                        int goldNew = peer.mPlayer.SecondaryStats.Gold;
                        // System log place bid
                        LogAuctionCurrency(peer, "PlaceBid", currentAuctionItem.itemId, lockGoldToUse, goldToUse, lockGoldOld, lockGoldNew, goldOld, goldNew);
                    }
                    else
                    {
                        peer.ZRPC.CombatRPC.Ret_AuctionPlaceBid((byte)AuctionReturnCode.Failed, 0, peer);
                    }
                });
            }
            else
                peer.ZRPC.CombatRPC.Ret_AuctionPlaceBid((byte)AuctionReturnCode.Failed, 0, peer);
        }

        public static void GetAuctionItem(GameClientPeer peer)
        {
            if (currentAuctionItem == null)
            {
                peer.ZRPC.CombatRPC.Ret_AuctionGetAuctionItem(null, 0, peer);
            }
            else
            {
                string stringData = JsonConvert.SerializeObject(currentAuctionItem);
                int bidPrice = 0;
                // get bid price if have submitted bid 
                HasSubmittedBid(currentAuctionItem.auctionId, peer.mChar, out bidPrice);

                peer.ZRPC.CombatRPC.Ret_AuctionGetAuctionItem(stringData, bidPrice, peer);
            }
        }

        public static async Task GetAuctionRecords(GameClientPeer peer)
        {
            List<Dictionary<string, object>> results = await GameApplication.dbRepository.AuctionData.GetAuctionRecords().ConfigureAwait(false);

            executionFiber.Enqueue(() =>
            {
                List<RecordDataInfo> list = new List<RecordDataInfo>();
                foreach (Dictionary<string, object> data in results)  // results already in latest date first
                {
                    RecordDataInfo record = new RecordDataInfo();
                    record.dt = (DateTime)data["EndDate"];
                    record.auctionId = (int)data["Id"];
                    record.itemId = (int)data["ItemId"];
                    record.itemCount = (int)data["ItemCount"];
                    record.bid1 = JsonConvert.DeserializeObject<BidInfo>((string)data["Bid1"]);
                    record.bid2 = JsonConvert.DeserializeObject<BidInfo>((string)data["Bid2"]);
                    list.Add(record);
                }
                SendAuctionRecordsToClient(list, peer);
            });
        }

        private static void SendAuctionRecordsToClient(List<RecordDataInfo> list, GameClientPeer peer)
        {
            string stringData = JsonConvert.SerializeObject(list);
            peer.ZRPC.CombatRPC.Ret_AuctionGetRecords(stringData, peer);
        }

        public static void GetBidItems(GameClientPeer peer)
        {
            List<BidItemInfo> list = new List<BidItemInfo>();

            if (BidsByPlayer.ContainsKey(peer.mChar))
            {
                List<BidData> bidsList = BidsByPlayer[peer.mChar];
                for (int i = 0; i < bidsList.Count; i++)
                {
                    BidData data = bidsList[i];
                    if (data.winningPrice >= 0)  // winning price will be at least 0 if auction has ended
                    {
                        BidItemInfo bidItem = new BidItemInfo();
                        bidItem.bidId = data.bidId;
                        bidItem.auctionEndDt = data.auctionEndDt;
                        bidItem.itemId = data.itemId;
                        bidItem.itemCount = data.itemCount;
                        if (data.isWinner)
                        {
                            bidItem.winningPrice = data.winningPrice;
                            int refundLockGold, refundGold, spentLockGold, spentGold;
                            data.CalculateRefund(out refundLockGold, out refundGold, out spentLockGold, out spentGold);
                            bidItem.refundLockGold = refundLockGold;
                            bidItem.refundGold = refundGold;
                        }
                        else
                        {
                            bidItem.winningPrice = 0;  // for client to determine is not winner
                            bidItem.refundLockGold = data.lockGold;
                            bidItem.refundGold = data.gold;
                        }
                        list.Add(bidItem);
                    }
                }
            }

            list = list.OrderByDescending(x => x.auctionEndDt).ToList();  // latest first

            string stringData = JsonConvert.SerializeObject(list);
            peer.ZRPC.CombatRPC.Ret_AuctionGetBidItems(stringData, peer);
        }


        public static void CollectAuctionItem(int bidId, GameClientPeer peer)
        {
            if (BidsByPlayer.ContainsKey(peer.mChar))
            {
                List<BidData> bidsList = BidsByPlayer[peer.mChar];
                BidData itemToCollect = bidsList.FirstOrDefault(x => x.bidId == bidId);
                if (itemToCollect != null && itemToCollect.winningPrice >= 0)  // make sure auction item has ended
                {
                    if (itemToCollect.isWinner)
                    {
                        // try give item to player                       
                        InvRetval retval = peer.mInventory.AddItemsIntoInventory((ushort)itemToCollect.itemId, itemToCollect.itemCount, true, "Auction");
                        if (retval.retCode != InvReturnCode.AddSuccess)  // inventory full
                        {
                            peer.ZRPC.CombatRPC.Ret_AuctionCollectItem((byte)AuctionReturnCode.InventoryFull, peer);
                            return;
                        }
                        // remove bid and give gold
                        executionFiber.Enqueue(async () => await RemoveAuctionBid(peer, itemToCollect, retval).ConfigureAwait(false));
                    }
                    else // not winner so immediately refund gold only
                    {
                        // remove bid and give gold
                        executionFiber.Enqueue(async () => await RemoveAuctionBid(peer, itemToCollect, null).ConfigureAwait(false));
                    }
                }
                else
                    peer.ZRPC.CombatRPC.Ret_AuctionCollectItem((byte)AuctionReturnCode.Failed, peer);
            }
            else
                peer.ZRPC.CombatRPC.Ret_AuctionCollectItem((byte)AuctionReturnCode.Failed, peer);
        }

        private static async Task RemoveAuctionBid(GameClientPeer peer, BidData itemToCollect, InvRetval retval)
        {
            bool res = await GameApplication.dbRepository.AuctionData.CollectAuctionBid(itemToCollect.bidId);

            executionFiber.Enqueue(() =>
            {
                if (res)
                {
                    int lockGoldOld = peer.mPlayer.SecondaryStats.bindgold;
                    int goldOld = peer.mPlayer.SecondaryStats.Gold;
                    int refundLockGold, refundGold, spentLockGold, spentGold;

                    // give gold                    
                    if (itemToCollect.isWinner)
                    {
                        itemToCollect.CalculateRefund(out refundLockGold, out refundGold, out spentLockGold, out spentGold);
                        peer.mPlayer.AddBindGold(refundLockGold, "Auction_WinRefund");
                        peer.mPlayer.AddGold(refundGold, "Auction_WinRefund");
                        peer.mPlayer.SpendGold(spentLockGold, spentGold);  // spend the win price

                        // System log collect item
                        LogAuctionItem(peer, "CollectItem", peer.mChar, itemToCollect.itemId, itemToCollect.itemCount);
                    }
                    else // not winner
                    {
                        refundLockGold = itemToCollect.lockGold;
                        refundGold = itemToCollect.gold;
                        peer.mPlayer.AddBindGold(refundLockGold, "Auction_LoseRefund");
                        peer.mPlayer.AddGold(refundGold, "Auction_LoseRefund");
                    }

                    int lockGoldNew = peer.mPlayer.SecondaryStats.bindgold;
                    int goldNew = peer.mPlayer.SecondaryStats.Gold;

                    List<BidData> bidsList = BidsByPlayer[peer.mChar];
                    bidsList.Remove(itemToCollect);
                    peer.ZRPC.CombatRPC.Ret_AuctionCollectItem((byte)AuctionReturnCode.Success, peer);

                    // System log refund gold
                    LogAuctionCurrency(peer, "Refund", itemToCollect.itemId, refundLockGold, refundGold, lockGoldOld, lockGoldNew, goldOld, goldNew);
                }
                else
                {
                    if (itemToCollect.isWinner)  // Take back added item
                        peer.mInventory.RevertAdd(retval.invSlot);

                    peer.ZRPC.CombatRPC.Ret_AuctionCollectItem((byte)AuctionReturnCode.Failed, peer);
                }
            });
        }

        public static string GetAuctionStatus(GameClientPeer peer)
        {
            int bidPrice;
            int status = 0;
            StringBuilder sb = new StringBuilder();

            if (currentAuctionItem != null && !HasSubmittedBid(currentAuctionItem.auctionId, peer.mChar, out bidPrice))
            {
                status = GameUtils.SetBit(status, (int)AuctionStatusBit.AuctionOpen);
            }

            if (BidsByPlayer.ContainsKey(peer.mChar))
            {
                List<BidData> bidsList = BidsByPlayer[peer.mChar];
                if (bidsList.Any(x => x.winningPrice >= 0))
                    status = GameUtils.SetBit(status, (int)AuctionStatusBit.CollectionAvailable);
            }
            sb.AppendFormat("{0};", status);

            string nextAuctionTime = "-1";
            if (AuctionItemList.Count > 0)
                nextAuctionTime = AuctionItemList[0].beginDateTime.ToString("yyyy/MM/dd HH:mm");

            sb.Append(nextAuctionTime);

            return sb.ToString();
        }

        #region Logging
        private static void LogAuctionItem(GameClientPeer peer, string action, string playerName, int itemId, int itemCount)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("actionType: {0} | ", action);
            sb.AppendFormat("playerName: {0} | ", playerName);
            sb.AppendFormat("itemId: {0} |  ", itemId);
            sb.AppendFormat("itemCount: {0}", itemCount);

            Zealot.Logging.Client.LogClasses.AuctionItem sysLog = new Zealot.Logging.Client.LogClasses.AuctionItem();
            sysLog.userId = peer == null ? Guid.Empty.ToString() : peer.mUserId;
            sysLog.charId = peer == null ? Guid.Empty.ToString() : peer.GetCharId();
            sysLog.message = sb.ToString();
            sysLog.actionType = action;
            sysLog.itemId = itemId;
            sysLog.itemCount = itemCount;
            sysLog.playerName = playerName;
            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(sysLog);
        }

        private static void LogAuctionCurrency(GameClientPeer peer, string action, int itemId, int lockGold, int gold,
                                                int lockGoldOld, int lockGoldNew, int goldOld, int goldNew)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("actionType: {0} | ", action);
            sb.AppendFormat("itemId: {0} | ", itemId);
            sb.AppendFormat("lockGold: {0} |  ", lockGold);
            sb.AppendFormat("gold: {0} | ", gold);
            sb.AppendFormat("lockGoldBefore: {0} | ", lockGoldOld);
            sb.AppendFormat("lockGoldAfter: {0} |  ", lockGoldNew);
            sb.AppendFormat("goldBefore: {0} | ", goldOld);
            sb.AppendFormat("goldAfter: {0}", goldNew);

            Zealot.Logging.Client.LogClasses.AuctionCurrency sysLog = new Zealot.Logging.Client.LogClasses.AuctionCurrency();
            sysLog.userId = peer.mUserId;
            sysLog.charId = peer.GetCharId();
            sysLog.message = sb.ToString();
            sysLog.actionType = action;
            sysLog.itemId = itemId;
            sysLog.lockGold = lockGold;
            sysLog.gold = gold;
            sysLog.lockGoldBefore = lockGoldOld;
            sysLog.lockGoldAfter = lockGoldNew;
            sysLog.goldBefore = goldOld;
            sysLog.goldAfter = goldNew;
            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(sysLog);
        }
        #endregion
    }
}
