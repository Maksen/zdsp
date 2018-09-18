using Newtonsoft.Json;
using System;
using Zealot.Common;
using Zealot.Common.Actions;
using Zealot.Server.Entities;
using Zealot.Repository;

namespace Photon.LoadBalancing.GameServer.OfflineExp
{
    public class OfflineExpManager2
    {
        private static volatile OfflineExpManager2 instance;
        private static object syncRoot = new object();

        private static int MONEY_COST = 20000;
        private static int GOLD_COST = 9;

        public OfflineExpManager2()
        {
            MONEY_COST = GameConstantRepo.GetConstantInt("OfflineExp_MoneyCost");
            GOLD_COST  = GameConstantRepo.GetConstantInt("OfflineExp_GoldCost");
        }

        public static OfflineExpManager2 Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new OfflineExpManager2();
                        }
                    }
                }

                return instance;
            }
        }

        public bool GetRedDot(GameClientPeer peer, bool sendPkt=true)
        {
            OfflineExpInventory2 offlineExpInv = peer.CharacterData.OfflineExpInv2;
            int minLvl = GameConstantRepo.GetConstantInt("OfflineExp_UnlockLvl", 1);

            //If not unlocked yet
            if (peer.CharacterData.ProgressLevel < minLvl)
                return false;

            //If no reward chosen
            if (offlineExpInv.rewardLv < 0)
            {
                return true;
            }
            else
            {
                TimeSpan ts = TimeSpan.FromTicks(offlineExpInv.rewardStartTime);
                long elapsedTime = DateTime.Now.Subtract(ts).Ticks;

                int hours = -1;
                switch (offlineExpInv.rewardIndex - 1)
                {
                    case 0:
                        hours = 1;
                        break;
                    case 1:
                        hours = 2;
                        break;
                    case 2:
                        hours = 4;
                        break;
                    case 3:
                        hours = 6;
                        break;
                    case 4:
                        hours = 8;
                        break;
                    case 5:
                        hours = 12;
                        break;
                }

#if DEBUG
                //long timeDura = TimeSpan.FromMinutes(hours).Ticks;              //<============================= HACK for playtest
                long timeDura = TimeSpan.FromHours(hours).Ticks;                  //<============================= HACK for publisher
#else
                long timeDura = TimeSpan.FromHours(hours).Ticks;
#endif

                //If reward time is not up
                if (elapsedTime < timeDura)
                {
                    return false;
                }

                //If reward is ready
                if (sendPkt)
                    peer.ZRPC.CombatRPC.OfflineExpRedDot(true, peer);
                return true;
            }
        }

        public string GetOfflineExpData(GameClientPeer playerPeer)
        {
            string serializedData;
            OfflineExpInventory2 offlineExpInv = playerPeer.CharacterData.OfflineExpInv2;
            OfflineExpClientData2 clientData = new OfflineExpClientData2();

            //HACK: to allow select reward everytime
            //offlineExpInv.rewardLv = -1;
            //offlineExpInv.rewardIndex = -1;
            //offlineExpInv.rewardStartTime = -1;

            clientData.rewardLv = offlineExpInv.rewardLv;
            clientData.rewardIndex = offlineExpInv.rewardIndex - 1; //Bypass characterdata being a sparse array
            clientData.rewardTimeLeft = 0;

            //If no reward chosen
            if (offlineExpInv.rewardLv < 0)
            {
                clientData.retcode = (int)OfflineExpRetCode2.OE_NoRewardChosen;
            }
            else
            {
                TimeSpan ts = TimeSpan.FromTicks(offlineExpInv.rewardStartTime);
                long elapsedTime = DateTime.Now.Subtract(ts).Ticks;

                int hours = -1;
                switch (clientData.rewardIndex)
                {
                    case 0:
                        hours = 1;
                        break;
                    case 1:
                        hours = 2;
                        break;
                    case 2:
                        hours = 4;
                        break;
                    case 3:
                        hours = 6;
                        break;
                    case 4:
                        hours = 8;
                        break;
                    case 5:
                        hours = 12;
                        break;
                }

#if DEBUG
                //long timeDura = TimeSpan.FromMinutes(hours).Ticks;              //<============================= HACK for playtest
                long timeDura = TimeSpan.FromHours(hours).Ticks;                  //<============================= HACK for publisher
#else
                long timeDura = TimeSpan.FromHours(hours).Ticks;
#endif

                //If reward time is not up
                if (elapsedTime < timeDura)
                {
                    clientData.retcode = (int)OfflineExpRetCode2.OE_RewardChosen;
                    clientData.rewardTimeLeft = timeDura - elapsedTime;
                }
                //If reward is ready
                else
                {
                    clientData.retcode = (int)OfflineExpRetCode2.OE_RewardReady;
                    clientData.rewardTimeLeft = 0;
                }
            }

            serializedData = JsonConvert.SerializeObject(clientData);
            return serializedData;
        }

        public string ChosenReward(GameClientPeer playerPeer, int cardNo)
        {
            OfflineExpInventory2 offlineExpInv = playerPeer.CharacterData.OfflineExpInv2;
            OfflineExpClientData2 clientData = new OfflineExpClientData2();
            string serializedData;

            if (cardNo < 0 || cardNo >= OfflineExpRepo.GetNumberExpReward())
            {
                //Invalid card
                clientData.retcode = (int)OfflineExpRetCode2.OE_ChooseRewardFailed_InvalidReward;
                serializedData = JsonConvert.SerializeObject(clientData);
                return serializedData;
            }
            //if (playerPeer.mPlayer.PlayerSynStats.vipLvl)
            //{
            //}

            offlineExpInv.rewardLv = playerPeer.mPlayer.PlayerSynStats.Level;
            offlineExpInv.rewardIndex = cardNo + 1; //Bypass characterdata sparse array
            offlineExpInv.rewardStartTime = DateTime.Now.Ticks;

            int hours = -1;
            switch (cardNo)
            {
                case 0:
                    hours = 1;
                    break;
                case 1:
                    hours = 2;
                    break;
                case 2:
                    hours = 4;
                    break;
                case 3:
                    hours = 6;
                    break;
                case 4:
                    hours = 8;
                    break;
                case 5:
                    hours = 12;
                    break;
            }

#if DEBUG
            //long timeLeft = TimeSpan.FromMinutes(hours).Ticks;          //<============================= HACK for playtest
            long timeLeft = TimeSpan.FromHours(hours).Ticks;              //<============================= HACK for publisher
#else
            long timeLeft = TimeSpan.FromHours(hours).Ticks;
#endif

            clientData.rewardLv = offlineExpInv.rewardLv;
            clientData.rewardIndex = cardNo;
            clientData.rewardTimeLeft = timeLeft;
            clientData.retcode = (int)OfflineExpRetCode2.OE_ChooseRewardSuccess;

            Log_ChooseReward(offlineExpInv.rewardLv,
                             hours,
                             OfflineExpRepo.GetExpReward(offlineExpInv.rewardLv, cardNo),
                             playerPeer);

            //Send confirmation to client
            serializedData = JsonConvert.SerializeObject(clientData);
            return serializedData;
        }

        public int ClaimReward(GameClientPeer playerPeer, int claimcode, out int newlvl)
        {
            OfflineExpInventory2 offlineExpInv = playerPeer.CharacterData.OfflineExpInv2;
            int rewardIndex = offlineExpInv.rewardIndex - 1; //Bypass characterdata sparse array
            int experience = OfflineExpRepo.GetExpReward(offlineExpInv.rewardLv, rewardIndex);
            newlvl = -1;

            //Check if reward time is ripe
            int hours = -1;
            switch (rewardIndex)
            {
                case 0:
                    hours = 1;
                    break;
                case 1:
                    hours = 2;
                    break;
                case 2:
                    hours = 4;
                    break;
                case 3:
                    hours = 6;
                    break;
                case 4:
                    hours = 8;
                    break;
                case 5:
                    hours = 12;
                    break;
            }
            TimeSpan ts = TimeSpan.FromTicks(offlineExpInv.rewardStartTime);
            long elapsedTime = DateTime.Now.Subtract(ts).Ticks;


#if DEBUG
            //long timeDura = TimeSpan.FromMinutes(hours).Ticks;    //<============================= HACK for playtest
            long timeDura = TimeSpan.FromHours(hours).Ticks;        //<============================= HACK for publisher
#else
            long timeDura = TimeSpan.FromHours(hours).Ticks;
#endif

            //If this is true, reward cannot be claimed
            if (elapsedTime < timeDura)
            {
                //Send failure to client
                return (int)OfflineExpRetCode2.OE_ClaimRewardFailed_NotReady;
            }

            float bonus_multiplier = 1f;
            //if money not enough [Wait for CR]
            if (claimcode == (int)OfflineExpClaimBonusCode.OEClaimBonus_Money)
            {
                if (!playerPeer.mPlayer.DeductMoney(MONEY_COST, "OfflineExp"))  // deduct money
                     return (int)OfflineExpRetCode2.OE_ClaimRewardFailed_NoMoney;

                bonus_multiplier = 1.5f;
            }
            //if gold not enough [Wait for CR]
            else if (claimcode == (int)OfflineExpClaimBonusCode.OEClaimBonus_Gold)
            {
                if (!playerPeer.mPlayer.DeductGold(GOLD_COST, true, true, "OfflineExp"))  //Deduct gold
                    return (int)OfflineExpRetCode2.OE_ClaimRewardFailed_NoGold;

                bonus_multiplier = 2f;
            }

            //Add experience
            int oldlvl = playerPeer.mPlayer.PlayerSynStats.Level;
            int finalexperience = (int)(experience * bonus_multiplier);
            playerPeer.mPlayer.AddExperience(finalexperience);
            newlvl = playerPeer.mPlayer.PlayerSynStats.Level;

            playerPeer.mQuestExtraRewardsCtrler.UpdateTask(Zealot.Common.QuestExtraType.OfflineExpGet);

            //Reset chosen reward
            offlineExpInv.rewardLv = -1;
            offlineExpInv.rewardIndex = -1;
            offlineExpInv.rewardStartTime = -1;

            Log_ClaimReward(oldlvl, newlvl,
                            (OfflineExpClaimBonusCode)claimcode,
                            experience, finalexperience,
                            playerPeer.mPlayer.SecondaryStats.experience - finalexperience, playerPeer.mPlayer.SecondaryStats.experience,
                            playerPeer);

            //Send confirmation to client
            return (int)OfflineExpRetCode2.OE_ClaimRewardSuccess;
        }

        void Log_ChooseReward(int charLevel, int chosenTime, int baseExp, GameClientPeer peer)
        {
            Zealot.Logging.Client.LogClasses.OfflineExp_ChooseReward log = new Zealot.Logging.Client.LogClasses.OfflineExp_ChooseReward();

            //if peer disconnect or invalid?
            if (peer == null || peer.mPlayer == null)
                return;

            log.userId = peer.mUserId;
            log.charId = peer.GetCharId();
            log.message = "";

            log.recvDate = DateTime.Now;
            log.charLevel = charLevel;
            log.chosenTime = chosenTime;
            log.baseExpReward = baseExp;

            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(log);
        }

        void Log_ClaimReward(int bcLevel, int acLevel, OfflineExpClaimBonusCode chosenBonus, int baseExp, int finalExp, int beforeCharExp, int afterCharExp, GameClientPeer peer)
        {
            Zealot.Logging.Client.LogClasses.OfflineExp_ClaimReward log = new Zealot.Logging.Client.LogClasses.OfflineExp_ClaimReward();

            //if peer disconnect or invalid?
            if (peer == null || peer.mPlayer == null)
                return;

            log.userId = peer.mUserId;
            log.charId = peer.GetCharId();
            log.message = "";

            log.recvDate = DateTime.Now;
            log.beforeCharLevel = bcLevel;
            log.afterCharLevel = acLevel;
            switch (chosenBonus)
            {
                case OfflineExpClaimBonusCode.OEClaimBonus_None:
                    log.chosenBonus = "None";
                    break;
                case OfflineExpClaimBonusCode.OEClaimBonus_Money:
                    log.chosenBonus = "Money";
                    break;
                case OfflineExpClaimBonusCode.OEClaimBonus_Gold:
                    log.chosenBonus = "Gold";
                    break;
            }
            log.baseExpReward = baseExp;
            log.finalExpReward = finalExp;
            log.beforeCharExp = beforeCharExp;
            log.afterCharExp = afterCharExp;

            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(log);
        }
    }
}
