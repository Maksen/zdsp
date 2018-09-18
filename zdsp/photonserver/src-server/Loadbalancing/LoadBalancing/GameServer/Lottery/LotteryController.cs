using System.Collections.Generic;
using Zealot.Common;
using Zealot.Repository;

namespace Photon.LoadBalancing.GameServer.Lottery
{
    public class LotteryController
    {
        // Player Slot
        private GameClientPeer _peer;
        private LotteryInventoryData inventory;
        private Dictionary<int, int> idToStatIndex;

        public LotteryController(GameClientPeer peer)
        {
            _peer = peer;
            inventory = _peer.CharacterData.LotteryInventory;
            idToStatIndex = new Dictionary<int, int>();
        }

        private bool CheckHaveData(int lottery_id)
        {
            if (inventory.CheckHaveData(lottery_id) == false)
            {
                RestToNewData();
                // RestToNewData成功,傳入的舊id所屬資料已經變換成新的,所以要回傳失敗,等LO自動更新
                // RestToNewData不成功, 理所當然回傳失敗
                return false;
            }
            return true;
        }

        public bool RestToNewData()
        {
            idToStatIndex.Clear();
            if (LotteryMainRepo.GetPeriodLottery().Count != 0)
            {
                inventory.InitFromInventory(_peer.mPlayer.LotteryInfoStats);
                idToStatIndex = inventory.GetIdToIndex();
                return true;
            }

            return false;
        }

        public void ResetOnNewDay()
        {
            _peer.CharacterData.LotteryInventory.NewDayReset();
            inventory.InitFromInventory(_peer.mPlayer.LotteryInfoStats);
        }

        public void UpdateLotteryStat(int lottery_id)
        {
            int idx;
            if (idToStatIndex.TryGetValue(lottery_id, out idx))
            {
                _peer.CharacterData.LotteryInventory.UpdateInfoToStat(_peer.mPlayer.LotteryInfoStats, idx);
            }
        }

        public void AddFreeTicket(int lottery_id, int count)
        {
            if (CheckHaveData(lottery_id))
            {
                int result = _peer.CharacterData.LotteryInventory.AddFreeTicket(lottery_id, count);
                //_peer.mPlayer.LotteryInfoStats.FreeTicket = result;
                ////_peer.mPlayer.LotteryInfoStats.LastUpdateTime = inventory.GetLastUpdateDateTimeTicks(lottery_id);
            }  
        }

        public int GetFreeTicketCount(int lottery_id)
        {
            if (CheckHaveData(lottery_id))
                return _peer.CharacterData.LotteryInventory.GetFreeTicketCount(lottery_id);

            return 0;
        }

        public int DeductFreeTicket(int lottery_id, int used_free_tickets)
        {
            if (CheckHaveData(lottery_id))
            {
                var result = _peer.CharacterData.LotteryInventory.DeductFreeTicket(lottery_id, used_free_tickets);
                //_peer.mPlayer.LotteryInfoStats.FreeTicket = result;
                //_peer.mPlayer.LotteryInfoStats.LastUpdateTime = inventory.GetLastUpdateDateTimeTicks(lottery_id);
                return result;
            }

            return -1;
        }

        public int GetPoint(int lottery_id)
        {
            if (CheckHaveData(lottery_id))
                return _peer.CharacterData.LotteryInventory.GetPoint(lottery_id);

            return 0;
        }

        public int AddPoint(int lottery_id, int point)
        {
            if (CheckHaveData(lottery_id))
            {
                var result = _peer.CharacterData.LotteryInventory.AddPoint(lottery_id, point);
                //_peer.mPlayer.LotteryInfoStats.LotteryPoint = result;
                //_peer.mPlayer.LotteryInfoStats.LastUpdateTime = inventory.GetLastUpdateDateTimeTicks(lottery_id);

                return result;
            }

            return -1;
        }

        public int GetLotteryCount(int lottery_id)
        {
            if (CheckHaveData(lottery_id))
                return _peer.CharacterData.LotteryInventory.GetLotteryCount(lottery_id);

            return 0;
        }

        public bool AddCountAndPoint(int lottery_id, int count, int point)
        {
            if (CheckHaveData(lottery_id))
            {
                if (_peer.CharacterData.LotteryInventory.AddCountAndPoint(lottery_id, count, point))
                {
                    //_peer.mPlayer.LotteryInfoStats.TotalLotteryCount = _peer.CharacterData.LotteryInventory.GetLotteryCount(lottery_id);
                    //_peer.mPlayer.LotteryInfoStats.LotteryPoint = _peer.CharacterData.LotteryInventory.GetPoint(lottery_id);
                    //_peer.mPlayer.LotteryInfoStats.LastUpdateTime = inventory.GetLastUpdateDateTimeTicks(lottery_id);
                    return true;
                }
            }

            return false;
        }

        public List<int> GetRewardedPoints(int lottery_id)
        {
            if (CheckHaveData(lottery_id))
                return _peer.CharacterData.LotteryInventory.GetRewardedPoints(lottery_id);

            return new List<int>();
        }

        public bool AddRewarderPoint(int lottery_id, int point)
        {
            if (CheckHaveData(lottery_id))
            {
                int idx = _peer.CharacterData.LotteryInventory.AddRewarderPoint(lottery_id, point);
                if (idx != -1)
                {
                    //_peer.mPlayer.LotteryInfoStats.pointReward[idx] = point;
                    //_peer.mPlayer.LotteryInfoStats.LastUpdateTime = inventory.GetLastUpdateDateTimeTicks(lottery_id);
                    return true;
                }
            }
            return false;
        }
    }
}
