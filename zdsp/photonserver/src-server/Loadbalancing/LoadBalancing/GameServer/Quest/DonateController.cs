using Kopio.JsonContracts;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Zealot.Common;
using Zealot.Common.Entities;
using Zealot.Repository;
using Zealot.Server.Entities;
using Zealot.Server.Rules;

namespace Photon.LoadBalancing.GameServer
{
    public class DonateController
    {
        private Player mPlayer;
        private List<DonateOrderData> mDonateOrder;
        private DateTime mLastUpdated;
        private int mMaxOrder;
        private bool bLock = false;
        private List<string> mListForUpdate = new List<string>();

        public DonateController(Player player)
        {
            mPlayer = player;
        }

        public void InitFromData(CharacterData characterData)
        {
            mDonateOrder = DeserializeData(characterData.DonateInventory.DonateData);
            mLastUpdated = DeserializeLastUpdated(characterData.DonateInventory.LastUpdated);

            if (mDonateOrder.Count == 0 && mLastUpdated == DateTime.MinValue)
            {
                InitDefault();
            }
            else
            {
                CheckMissedUpdate();
            }
        }

        public void RefreshData(bool normalrefresh)
        {
            bLock = true;
            List<DonateJson> donatelist = GetDonateData();
            if (donatelist != null)
            {
                mLastUpdated = mLastUpdated.AddHours(6);
                DonateOrderData donateOrderData = GetDonateDataByProbability(normalrefresh, donatelist, mLastUpdated);
                if (donateOrderData != null)
                {
                    mDonateOrder.Add(donateOrderData);
                    DeleteOldestDonateData();
                    SyncDonateData();
                }
            }
        }

        private void InitDefault()
        {
            List<DonateJson> donatelist = GetDonateData();
            if (donatelist != null)
            {
                List<DonateJson> rarelist = donatelist.Where(o => o.rarity == 1).ToList();
                List<DonateJson> normallist = donatelist.Where(o => o.rarity == 0).ToList();
                mLastUpdated = GetLastUpdateDateTime();
                mDonateOrder.Add(GetRandomDonateData(rarelist, mLastUpdated));
                int remain = mMaxOrder - mDonateOrder.Count;
                for (int i = 0; i < remain; i++)
                {
                    mDonateOrder.Add(GetRandomDonateData(normallist, mLastUpdated));
                }
            }
        }

        private void CheckMissedUpdate()
        {
            List<DonateJson> donatelist = GetDonateData();
            if (donatelist != null)
            {
                DateTime nextupdate = GetNextUpdateDateTime();
                double hours = (nextupdate - mLastUpdated).TotalHours;
                while (hours > 0)
                {
                    mLastUpdated = mLastUpdated.AddHours(6);
                    bool normalrefresh = (mLastUpdated.Hour == 6 || mLastUpdated.Hour == 18) ? true : false;
                    DonateOrderData donateOrderData = GetDonateDataByProbability(normalrefresh, donatelist, mLastUpdated);
                    if (donateOrderData != null)
                    {
                        mDonateOrder.Add(donateOrderData);
                        DeleteOldestDonateData();
                    }
                    hours = (nextupdate - mLastUpdated).TotalHours;
                }
            }
        }

        public void InitDonateStats(ref DonateSynStats donateSynStats)
        {
            donateSynStats.donateData = SerializeData(mDonateOrder);
        }

        private string SerializeData(List<DonateOrderData> data)
        {
            return JsonConvertDefaultSetting.SerializeObject(data);
        }

        private List<DonateOrderData> DeserializeData(string data)
        {
            return string.IsNullOrEmpty(data) ? new List<DonateOrderData>() : JsonConvertDefaultSetting.DeserializeObject<List<DonateOrderData>>(data);
        }

        private DateTime DeserializeLastUpdated(string data)
        {
            return string.IsNullOrEmpty(data) ? DateTime.MinValue : DateTime.ParseExact(data, "yyyy/MM/dd/HH", CultureInfo.InvariantCulture);
        }

        private List<DonateJson> GetDonateData()
        {
            int group;
            if (DonateRepo.GetMaxOrderAndGroup(mPlayer.PlayerStats.Level, out mMaxOrder, out group))
            {
                return DonateRepo.GetDonateListByGroupId(group);
            }
            return null;
        }

        private DonateOrderData GetRandomDonateData(List<DonateJson> donatelist, DateTime dateTime)
        {
            int result = GameUtils.RandomInt(0, donatelist.Count - 1);
            DonateJson donateJson = donatelist[result];
            if (donateJson != null)
            {
                DonateOrderData donateOrderData = new DonateOrderData();
                donateOrderData.Guid = Guid.NewGuid().ToString();
                donateOrderData.Id = donateJson.id;
                donateOrderData.Count = 0;
                donateOrderData.ActivatedDT = dateTime;
                return donateOrderData;
            }
            return null;
        }

        private DateTime GetLastUpdateDateTime()
        {
            int hour = DateTime.Now.Hour;
            if (hour >= 0 && hour < 6)
            {
                return new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
            }
            else if (hour >= 6 && hour < 12)
            {
                return new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 6, 0, 0);
            }
            else if (hour >= 12 && hour < 18)
            {
                return new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 12, 0, 0);
            }
            else
            {
                return new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 18, 0, 0);
            }
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

        private DonateOrderData GetDonateDataByProbability(bool normalrefresh, List<DonateJson> donatelist, DateTime dateTime)
        {
            int maxweight = 0;
            Dictionary<int, int> idbymax = new Dictionary<int, int>();
            foreach (DonateJson donatejson in donatelist)
            {
                maxweight += normalrefresh ? donatejson.normalprobability : donatejson.rareprobability;
                idbymax.Add(donatejson.id, maxweight);
            }

            int result = GameUtils.RandomInt(0, maxweight);
            foreach(KeyValuePair<int, int> entry in idbymax)
            {
                if (result <= entry.Value)
                {
                    DonateOrderData donateOrderData = new DonateOrderData();
                    donateOrderData.Guid = Guid.NewGuid().ToString();
                    donateOrderData.Id = donatelist[entry.Key - 1].id;
                    donateOrderData.Count = 0;
                    donateOrderData.ActivatedDT = dateTime;
                    return donateOrderData;
                }
            }
            return null;
        }

        private DonateOrderData GetDonateOrderData(string guid)
        {
            List< DonateOrderData> result =  mDonateOrder.Where(o => o.Guid == guid).ToList();
            if (result.Count > 0)
            {
                return result[0];
            }
            return null;
        }

        public int DonateItem(string guid)
        {
            DonateOrderData donateOrder = GetDonateOrderData(guid);
            if (donateOrder != null)
            {
                DonateJson donateJson = DonateRepo.GetDonateById(donateOrder.Id);
                if (donateJson != null)
                {
                    int amount = ((donateOrder.Count * donateJson.increase) + donateJson.amount);
                    InvRetval retval = mPlayer.Slot.mInventory.DeductItems((ushort)donateJson.donateitemid, amount, "Donate");
                    if (retval.retCode == InvReturnCode.UseSuccess)
                    {
                        bool isfull = false;
                        GameRules.GiveReward_CheckBagSlot(mPlayer, new List<int>() { donateJson.reward }, out isfull, true, true, "Donate Order");
                        if (!isfull)
                        {
                            donateOrder.Count += 1;
                            mListForUpdate.Add(donateOrder.Guid);
                            SyncDonateData();
                            return 2;
                        }
                        else
                        {
                            mPlayer.Slot.mInventory.RevertRemove(retval.invSlot);
                            return 1;
                        }
                    }
                    else
                    {
                        return 0;
                    }
                }
            }
            return 0;
        }

        private void DeleteCompletedOrder()
        {
            if (mListForUpdate.Count > 0)
            {
                foreach (string guid in mListForUpdate)
                {
                    DonateOrderData donateOrder = GetDonateOrderData(guid);
                    if (donateOrder != null)
                    {
                        DonateJson donateJson = DonateRepo.GetDonateById(donateOrder.Id);
                        if (donateJson != null)
                        {
                            if (donateOrder.Count >= donateJson.maxdonate)
                            {
                                mDonateOrder.Remove(donateOrder);
                            }
                        }
                    }
                }

                mListForUpdate = new List<string>();
                SyncDonateData();
            }
        }

        private void DeleteOldestDonateData()
        {
            int deletecount = mDonateOrder.Count - mMaxOrder;
            if (deletecount > 0)
            {
                for (int i = 0; i < deletecount; i++)
                {
                    DonateOrderData donateOrderData = mDonateOrder.OrderBy(o => o.ActivatedDT).First();
                    mDonateOrder.Remove(donateOrderData);
                }
            }
        }

        public void SaveDonateInventory(DonateInventoryData donateInventory)
        {
            donateInventory.SerializeData(mDonateOrder);
            donateInventory.SerializeLastUpdated(mLastUpdated);
        }

        private void SyncDonateData()
        {
            mPlayer.DonateStats.donateData = SerializeData(mDonateOrder);
            bLock = false;
            DeleteCompletedOrder();
        }
    }
}