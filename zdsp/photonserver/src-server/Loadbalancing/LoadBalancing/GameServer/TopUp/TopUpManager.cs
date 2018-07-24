namespace Photon.LoadBalancing.GameServer.TopUp
{
    using Kopio.JsonContracts;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Zealot.Billing.Client;
    using Zealot.Common;
    using Zealot.Repository;
    using Zealot.Server.Entities;

    public sealed class TopUpManager
    {
        private static TopUpManager _instance;

        private List<Tuple<string, DateTime>> currentActivities;

        private TopUpManager()
        {
          
        }

        public static async Task<TopUpManager> InstanceAsync()
        {
            if (_instance == null)
            {
                _instance = new TopUpManager();

                await _instance.UpdateTopUpActivityAsync().ConfigureAwait(false);
            }

            return _instance;
        }

        public async Task UpdateTopUpActivityAsync()
        {
            currentActivities = await GameApplication.dbGM.topUpActivityRepository.GetActivity().ConfigureAwait(false);
        }

        public async Task GetProductsWithLockGoldAsync(byte merchantType, GameClientPeer gameClientPeer)
        {
            List<string> productsWithDoubleBonus = new List<string>();

            bool isAndroid = BillingAgent.Instance.IsAndroid(merchantType);

            if (isAndroid)
            {
                List<Dictionary<string, string>> topUpItems = TopUpRepo.GetUIOrderedTopUpItemsAndroid();
                
                foreach (Dictionary<string, string> topUpItem in topUpItems)
                {
                    if (bool.Parse(topUpItem["doublebonus"]) == true)
                    {
                        productsWithDoubleBonus.Add(topUpItem["name"]);
                    }
                }
            }
            else
            {
                List<Dictionary<string, string>> topUpItems = TopUpRepo.GetUIOrderedTopUpItemsApple();

                foreach (Dictionary<string, string> topUpItem in topUpItems)
                {
                    if (bool.Parse(topUpItem["doublebonus"]) == true)
                    {
                        productsWithDoubleBonus.Add(topUpItem["name"]);
                    }
                }
            }            

            Guid charId = Guid.Parse(gameClientPeer.GetCharId());
            string serverId = GameApplication.Instance.GetMyServerId().ToString();

            Dictionary<string, DateTime> latestPurchases = await BillingAgent.Instance.GetLatestPurchasesAsync(charId, serverId).ConfigureAwait(false);

            DateTime earliestStartDateTime = GetEarliestActivityBonusDateTimeAsync();

            List<string> productsWithLockGold = new List<string>();

            if (earliestStartDateTime == DateTime.MaxValue)
            {
                List<string> latestPurchasedProductIds = latestPurchases.Keys.ToList();
                productsWithLockGold = productsWithDoubleBonus.Except(latestPurchasedProductIds).ToList();
            }
            else
            {
                foreach (KeyValuePair<string, DateTime> kvp in latestPurchases)
                {
                    string productId = kvp.Key;
                    DateTime purchasedDate = kvp.Value;

                    if (purchasedDate < earliestStartDateTime)
                    {
                        productsWithLockGold.Add(productId);
                    }
                }
            }

            gameClientPeer.ZRPC.CombatRPC.Ret_GetProductsWithLockGold(string.Join("|", productsWithLockGold), gameClientPeer);
        }

        public async Task VerifyPurchaseAsync(string productId, string transactionId, string receipt, byte merchantType, GameClientPeer gameClientPeer)
        {
            int gold = 0;
            int lockGold = 0;
            int vipPoints = 0;

            Guid charId = Guid.Parse(gameClientPeer.GetCharId());
            string serverId = GameApplication.Instance.GetMyServerId().ToString();

            Guid billingId = await BillingAgent.Instance.VerifyPurchaseAsync(serverId, charId, productId, transactionId, receipt, merchantType).ConfigureAwait(false);

            if (billingId.Equals(Guid.Empty) == false)
            {
                bool isAndroid = BillingAgent.Instance.IsAndroid(merchantType);

                Dictionary<string, int> claimedProducts = await ClaimPurchaseAsync(billingId, isAndroid, gameClientPeer).ConfigureAwait(false);
                gold = claimedProducts["gold"];
                lockGold = claimedProducts["lockGold"];
                vipPoints = claimedProducts["vipPoints"];
            }

            gameClientPeer.ZRPC.CombatRPC.Ret_VerifyPurchase(gold, lockGold, vipPoints, gameClientPeer);
        }

        private async Task<Dictionary<string, int>> ClaimPurchaseAsync(Guid billingId, bool isAndroid, GameClientPeer gameClientPeer)
        {
            Dictionary<string, int> claimedProducts = new Dictionary<string, int>();

            int gold = 0;
            int lockGold = 0;
            int vipPoints = 0;

            Player player = gameClientPeer.mPlayer;

            if (player != null)
            {
                Guid charId = Guid.Parse(gameClientPeer.GetCharId());
                string serverId = GameApplication.Instance.GetMyServerId().ToString();

                string productId = await BillingAgent.Instance.ClaimPurchaseAsync(serverId, charId, billingId).ConfigureAwait(false);

                if (string.IsNullOrEmpty(productId) == false)
                {
                    if (isAndroid)
                    {
                        TopUpItemAndroidJson topUpItemJson = TopUpRepo.GetTopUpItemAndroidByName(productId);

                        gold = topUpItemJson.gold;
                        player.AddCurrency(CurrencyType.Gold, gold, "TopUp");

                        vipPoints = topUpItemJson.vippoints;
                        player.AddCurrency(CurrencyType.VIP, vipPoints, "TopUp");

                        if (DateTime.Now >= GetEarliestActivityBonusDateTimeAsync())
                        {
                            lockGold = topUpItemJson.lockgold;
                            player.AddCurrency(CurrencyType.LockGold, lockGold, "TopUp");
                        }

                        // For Welfare tracking
                        gameClientPeer.mWelfareCtrlr.OnCredited(gold);
                    }
                    else
                    {
                        TopUpItemAppleJson topUpItemJson = TopUpRepo.GetTopUpItemAppleByName(productId);

                        gold = topUpItemJson.gold;
                        player.AddCurrency(CurrencyType.Gold, gold, "TopUp");

                        vipPoints = topUpItemJson.vippoints;
                        player.AddCurrency(CurrencyType.VIP, vipPoints, "TopUp");

                        if (DateTime.Now >= GetEarliestActivityBonusDateTimeAsync())
                        {
                            lockGold = topUpItemJson.lockgold;
                            player.AddCurrency(CurrencyType.LockGold, lockGold, "TopUp");
                        }

                        // For Welfare tracking
                        gameClientPeer.mWelfareCtrlr.OnCredited(gold);
                    }
                }
            }
            
            claimedProducts.Add("gold", gold);
            claimedProducts.Add("lockGold", lockGold);
            claimedProducts.Add("vipPoints", vipPoints);

            return claimedProducts;
        }

        private DateTime GetEarliestActivityBonusDateTimeAsync()
        {
            DateTime earliestStartDateTime = DateTime.MaxValue;

            if (currentActivities.Count > 0)
            {
                string currentServerId = GameApplication.Instance.GetMyServerId().ToString();

                foreach (Tuple<string, DateTime> activity in currentActivities)
                {
                    string included_servers = activity.Item1;
                    DateTime startDateTime = activity.Item2;

                    string[] servers = included_servers.Split('`');

                    if (servers.Any(currentServerId.Contains))
                    {
                        earliestStartDateTime = earliestStartDateTime < startDateTime ? startDateTime : earliestStartDateTime;
                    }
                }
            }

            return earliestStartDateTime;
        }
    }
}
