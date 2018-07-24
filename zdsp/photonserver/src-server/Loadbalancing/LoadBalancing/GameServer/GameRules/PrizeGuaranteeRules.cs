using System;
using System.Collections.Generic;
using Zealot.Common;
using Zealot.Common.Entities;
using Zealot.Server.Entities;
using Zealot.Repository;
using Kopio.JsonContracts;
using ExitGames.Logging;
using Photon.LoadBalancing.GameServer;
using Zealot.Logging.Client.LogClasses;

namespace Zealot.Server.Rules
{
    public static class PrizeGuaranteeRules
    {
        private static readonly ILogger debug = LogManager.GetCurrentClassLogger();
        private const string BeforeKey = "before";
        private const string AfterKey = "after";
        private const int PrizeGuaranteeCount=10;

        #region GetPrizeGuarantee
        public static void GetPrizeGuarantee(GameClientPeer peer, PrizeGuaranteeType type,int itemID,ref int prizeID,ref int prizeCount)
        {
            IInventoryItem item = GameRules.GenerateItem(prizeID, null, prizeCount);
            if (item == null)
                return;

            ItemInfo iteminfo = new ItemInfo() { itemId = (ushort)prizeID, stackCount = (ushort)prizeCount };
            List<ItemInfo> list = new List<ItemInfo>() { iteminfo };
            GetPrizeGuarantee(peer,type,itemID,list);
            prizeID = item.ItemID;
            prizeCount = item.StackCount;
        }
        #endregion

        #region GetPrizeGuarantee
        public static void GetPrizeGuarantee(GameClientPeer peer, PrizeGuaranteeType type,int itemID, List<ItemInfo> prizes)
        {
            PrizeGuaranteeTable table=PrizeGuaranteeRepository.GetPrizeGuaranteeByType((int)type, itemID);
            if(table==null)
            {
                debug.InfoFormat("Not Found PrizeGuarantee Setting in Table!! Type:{0} ItemID:{1}",type,itemID);
                return;
            }

            if (prizes == null || prizes.Count == 0)
            {
                debug.InfoFormat("Prizes is empty!!");
                return;
            }

            PrizeGuaranteeInfo info=GetPrizeGuaranteeInfo(peer, table);
            if (info == null)
            {
                debug.InfoFormat("PrizeGuarantee data not found.");
                return;
            }

            if(info.Count == -1)
            {
                //debug.InfoFormat("PrizeGuarantee already get.Count:-1 Can't not reset.");
                return;
            }

            if (CheckExistPrizeID(table, prizes))
            {
                return;
            }

            //debug.InfoFormat("CountID:{0},Type:{1},ItemID:{2},BeforeCount:{3},BeforePrize:{4},BeforePrizeCount:{5}", table.countId, type, itemID, info.Count, prizes[0].itemid, prizes[0].stackcount);


            //Log紀錄
            PrizeGuaranteeItemUse itemUseLog= CreateLog<PrizeGuaranteeItemUse>(peer);
            itemUseLog.countID = table.countId;
            itemUseLog.type = table.type;
            itemUseLog.itemID = table.itemId;
            itemUseLog.beforeCount = info.Count;

            PrizeGuaranteeGiveAward giveAwardLog = CreateLog<PrizeGuaranteeGiveAward>(peer);
            giveAwardLog.countID = table.countId;
            giveAwardLog.beforeCount = info.Count;

            //保底次數+1
            info.Count++;

            itemUseLog.afterCount = info.Count;
            //Log紀錄:動作：保底道具使用
            SendLog(itemUseLog);

            giveAwardLog.afterCount = info.Count;

            PrizeGuaranteeCount countLog = CreateLog<PrizeGuaranteeCount>(peer);
            countLog.countID = table.countId;
            countLog.count= info.Count;

            //Log紀錄:動作：保底累計
            SendLog(countLog);
            if (CheckThreshold(info,table, prizes))
            {
                //debug.InfoFormat("CountID:{0},Type:{1},ItemID:{2},AfterCount:{3},TargetPrize:{4},TargetPrizeCount:{5}", table.countId, type, itemID, info.Count,prizes[0].itemid, prizes[0].stackcount);
            }
            else
            {

                //Log紀錄:動作：保底給獎
                SendLog(giveAwardLog);
                //debug.InfoFormat("CountID:{0},Type:{1},ItemID:{2},AfterCount:{3}", table.countId, type, itemID,info.Count);
            }
        }
        #endregion

        #region CheckExistPrizeID
        private static bool CheckExistPrizeID(PrizeGuaranteeTable table,List<ItemInfo> prize)
        {
            for (int index = 0; index < prize.Count; index++)
            {
                for (int i = 0; i < table.container.Length; i++)
                {
                    if (prize[index].itemId == table.container[i].id)
                    {
                        debug.InfoFormat("PrizeGuarantee have the same item in talbe.PrizeID:{0}", prize[index].itemId);
                        return true;
                    }
                }
            }
            return false;
        }
        #endregion

        #region CheckThreshold
        private static bool CheckThreshold(PrizeGuaranteeInfo info,PrizeGuaranteeTable table,List<ItemInfo> prize)
        {
            for (int i = 0; i < table.container.Length; i++)
            {
                if (info.Count == table.container[i].threshold)
                {
                    if (i == table.container.Length - 1 || table.container[i+1].threshold==0)
                    {
                        if (table.reset)
                        {
                            info.Count = 0;
                        }
                        else
                        {
                            info.Count = -1;
                        }
                    }
                    prize[0].itemId = (ushort)table.container[i].id;
                    prize[0].stackCount = (ushort)table.container[i].count;
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region CreateLog
        private static T CreateLog<T>(GameClientPeer peer)where T:LogClass,new()
        {
            T log = new T();
            log.userId = peer.mUserId;
            log.charId = peer.GetCharId();

            return log;
        }
        #endregion

        #region SendLog
        private static void SendLog(LogClass log)
        {
            var ignoreAwait = Logging.Client.LoggingAgent.Instance.LogAsync(log);
        }
        #endregion

        #region GetPrizeGuaranteeInfo
        private static PrizeGuaranteeInfo GetPrizeGuaranteeInfo(GameClientPeer peer, PrizeGuaranteeTable table)
        {
            PrizeGuaranteeData data = peer.CharacterData.PrizeGuaranteeData;
            PrizeGuaranteeInfo info = null;
            if (data.PrizeGuaranteeInfos == null)
                data.PrizeGuaranteeInfos = new List<PrizeGuaranteeInfo>();
            for (int i = 0; i < data.PrizeGuaranteeInfos.Count; i++)
            {
                if (data.PrizeGuaranteeInfos[i].ID == table.countId)
                {
                    info = data.PrizeGuaranteeInfos[i];
                    //Reset On Type or ItemIDs Change
                    if (info.Type != table.type || info.ItemID != table.itemId)
                    {
                        info.Type = table.type;
                        info.ItemID = table.itemId;
                        info.Count = 0;
                    }
                    break;
                }
            }

            if (info == null)
            {
                if(data.PrizeGuaranteeInfos.Count>= PrizeGuaranteeCount)
                {
                    debug.InfoFormat("Set PrizeGuaranteeJson no more than {0} groups", PrizeGuaranteeCount);
                    return null;
                }
                info = new PrizeGuaranteeInfo() { ID = table.countId, Type = table.type, ItemID = table.itemId };
                data.PrizeGuaranteeInfos.Add(info);
            }
            else
            {
                if(info.Count==-1 && table.reset)
                {
                    info.Count = 0;
                }
            }
            return info;
        }
        #endregion

        #region ResetPrizeGuarantee
        public static void ResetPrizeGuarantee(GameClientPeer peer)
        {
            peer.CharacterData.PrizeGuaranteeData = new PrizeGuaranteeData();
        }
        #endregion
    }
}
