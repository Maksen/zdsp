//#define clientrpc 0
using ExitGames.Client.Photon;
using System.Collections.Generic;
using System.Text;
using Zealot.Common.RPC;

/// <summary>
/// CombatRPC .
/// </summary>
/// 
public class CombatRPC : RPCBase
{
    public CombatRPC() :
        base(typeof(CombatRPC), OperationCode.Combat)
    {
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.OnClientLevelLoaded)]
    public void OnClientLevelLoaded()
    {
        ProxyMethod("OnClientLevelLoaded");
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.OnEnterPortal)]
    public void OnEnterPortal(string entryName)
    {
        ProxyMethod("OnEnterPortal", entryName);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.OnEnterTrigger)]
    public void OnEnterTrigger(string entryName)
    {
        ProxyMethod("OnEnterTrigger", entryName);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.OnEnterSafeZone)]
    public void OnSafeZone(bool enter, int id, int type)
    {
        ProxyMethod("OnSafeZone", enter, id, type);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.OnClickWorldMap)]
    public void OnClickWorldMap(string entryName)
    {
        ProxyMethod("OnClickWorldMap", entryName);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.OnTeleportToLevel)]
    public void OnTeleportToLevel(string entryName)
    {
        ProxyMethod("OnTeleportToLevel", entryName);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.OnTeleportToLevelAndPos)]
    public void OnTeleportToLevelAndPos(string level, RPCPosition pos, int questid = -1)
    {
        ProxyMethod("OnTeleportToLevelAndPos", level, pos, questid);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.OnTeleportToPosInLevel)]
    public void OnTeleportToPosInLevel(RPCPosition pos)
    {
        ProxyMethod("OnTeleportToPosInLevel", pos);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.ExitGame)]
    public void ExitGame()
    {
        ProxyMethod("ExitGame");
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.RespawnOnSpot)]
    public void RespawnOnSpot(bool force = false)
    {
        ProxyMethod("RespawnOnSpot", force);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.RespawnAtCity)]
    public void RespawnAtCity()
    {
        ProxyMethod("RespawnAtCity");
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.RespawnAtSafeZone)]
    public void RespawnAtSafeZone()
    {
        ProxyMethod("RespawnAtSafeZone");
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.RespawnAtSafeZoneWithCost)]
    public void RespawnAtSafeZoneWithCost()
    {
        ProxyMethod("RespawnAtSafeZoneWithCost");
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.ClientSendChatMessage)]
    public void ClientSendChatMessage(byte msgType, string message, string whisperTo, bool isVoiceChat)
    {
        ProxyMethod("ClientSendChatMessage", msgType, message, whisperTo, isVoiceChat);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.BroadcastSysMsgToServer)]
    public void BroadcastSysMsgToServer(int msgid, string message)
    {
        ProxyMethod("BroadcastSysMsgToServer", msgid, message);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GetInspectPlayerInfo)]
    public void GetInspectPlayerInfo(string playerName)
    {
        ProxyMethod("GetInspectPlayerInfo", playerName);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.SaveGameSetting)]
    public void SaveGameSetting(string settings)
    {
        ProxyMethod("SaveGameSetting", settings);
    }

    #region Realm
    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.CreateRealmByID)]
    public void CreateRealmByID(int realmId, bool logAI, bool checkAllReq, int questid = -1)
    {
        ProxyMethod("CreateRealmByID", realmId, logAI, checkAllReq, questid);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.EnterRealmByID)]
    public void EnterRealmByID(int realmId)
    {
        ProxyMethod("EnterRealmByID", realmId);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.LeaveRealm)]
    public void LeaveRealm()
    {
        ProxyMethod("LeaveRealm");
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.InspectMode)]
    public void InspectMode()
    {
        ProxyMethod("InspectMode");
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.DungeonEnterRequest)]
    public void DungeonEnterRequest(int realmId)
    {
        ProxyMethod("DungeonEnterRequest", realmId);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.EnterRealmWithPartyResponse)]
    public void EnterRealmWithPartyResponse(int realmId, byte status)
    {
        ProxyMethod("EnterRealmWithPartyResponse", realmId, status);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.DungeonAutoClear)]
    public void DungeonAutoClear(int realmId, bool clearAll)
    {
        ProxyMethod("DungeonAutoClear", realmId, clearAll);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.OnPickResource)]
    public void OnPickResource(int pid)
    {
        ProxyMethod("OnPickResource", pid);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.RealmCollectReward)]
    public void RealmCollectReward()
    {
        ProxyMethod("RealmCollectReward");
    }
    #endregion

    #region Item
    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.AddItem)]
    public void AddItem(int itemId, int amount)
    {
        ProxyMethod("AddItem", itemId, amount);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.UseItem)]
    public void UseItem(int slotId, int amount)
    {
        ProxyMethod("UseItem", slotId, amount);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.SellItem)]
    public void SellItem(int slotId, int amount)
    {
        ProxyMethod("SellItem", slotId, amount);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.MassSellItems)]
    public void MassSellItems(Dictionary<int, int> sellAmtToSlotIdDict)
    {
        if (sellAmtToSlotIdDict.Count > 0)
        {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<int, int> kvp in sellAmtToSlotIdDict)
                sb.AppendFormat("{0}`{1};", kvp.Key, kvp.Value);

            sb.Remove(sb.Length-1, 1);
            ProxyMethod("MassSellItems", sb.ToString());
        }
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.MassUseItems)]
    public void MassUseItems(List<int> slotIds)
    {
        string str_slotids = "";
        foreach (var id in slotIds)
        {
            str_slotids += id.ToString() + ",";
        }
        ProxyMethod("MassUseItems", str_slotids);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.SortItem)]
    public void SortItem()
    {
        ProxyMethod("SortItem");
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.OpenNewSlot)]
    public void OpenNewSlot(int numOfSlots)
    {
        ProxyMethod("OpenNewSlot", numOfSlots);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.OpenNewSlotAuto)]
    public void OpenNewSlotAuto()
    {
        ProxyMethod("OpenNewSlotAuto");
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.BuyPotion)]
    public void BuyPotion(bool auto)
    {
        ProxyMethod("BuyPotion", auto);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.UnequipItem)]
    public void UnequipItem(int slotId, bool fashionslot)
    {
        ProxyMethod("UnequipItem", slotId, fashionslot);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.SetItemHotbar)]
    public void SetItemHotbar(byte index, int slotId)
    {
        ProxyMethod("SetItemHotbar", index, slotId);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.UseItemHotbar)]
    public void UseItemHotbar(byte index)
    {
        ProxyMethod("UseItemHotbar", index);
    }
    #endregion

    #region IAP
    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GetProductsWithLockGold)]
    public void GetProductsWithLockGold(byte merchantType)
    {
        ProxyMethod("GetProductsWithLockGold", merchantType);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.VerifyPurchase)]
    public void VerifyPurchase(string productId, string transactionId, string receipt, byte merchantType)
    {
        ProxyMethod("VerifyPurchase", productId, transactionId, receipt, merchantType);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GetMyCardAuthCode)]
    public void GetMyCardAuthCode(string productId)
    {
        ProxyMethod("GetMyCardAuthCode", productId);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.DelMyCardAuthCode)]
    public void DelMyCardAuthCode(string authCode)
    {
        ProxyMethod("DelMyCardAuthCode", authCode);
    }
    #endregion IAP

    #region Old Code:Social
    //[RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.SocialAcceptRequest)]
    //public void SocialAcceptRequest(string playerList)
    //{
    //    ProxyMethod("SocialAcceptRequest", playerList);
    //}

    //[RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.SocialRemoveRequest)]
    //public void SocialRemoveRequest(string playerList)
    //{
    //    ProxyMethod("SocialRemoveRequest", playerList);
    //}

    //[RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.SocialSendRequest)]
    //public void SocialSendRequest(string playerList)
    //{
    //    ProxyMethod("SocialSendRequest", playerList);
    //}

    //[RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.SocialRemoveFriend)]
    //public void SocialRemoveFriend(string playerName)
    //{
    //    ProxyMethod("SocialRemoveFriend", playerName);
    //}

    //[RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.SocialGetRecommendedFriends)]
    //public void SocialGetRecommendedFriends()
    //{
    //    ProxyMethod("SocialGetRecommendedFriends");
    //}

    //[RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.SocialUpdateFriendsInfo)]
    //public void SocialUpdateFriendsInfo()
    //{
    //    ProxyMethod("SocialUpdateFriendsInfo");
    //}
    #endregion

    #region Arena
    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GetArenaChallengers)]
    public void GetArenaChallengers()
    {
        ProxyMethod("GetArenaChallengers");
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.ChallengeArena)]
    public void ChallengeArena(int rank, bool free)
    {
        ProxyMethod("ChallengeArena", rank, free);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.ArenaClaimReward)]
    public void ArenaClaimReward(int rewardid)
    {
        ProxyMethod("ArenaClaimReward", rewardid);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GetArenaReport)]
    public void GetArenaReport()
    {
        ProxyMethod("GetArenaReport");
    }
    #endregion    

    #region Mail
    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.HasNewMail)]
    public void HasNewMail()
    {
        ProxyMethod("HasNewMail");
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.RetrieveMail)]
    public void RetrieveMail()
    {
        ProxyMethod("RetrieveMail");
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.OpenMail)]
    public void OpenMail(int mailIndex)
    {
        ProxyMethod("OpenMail", mailIndex);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.TakeAttachment)]
    public void TakeAttachment(int mailIndex)
    {
        ProxyMethod("TakeAttachment", mailIndex);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.TakeAllAttachment)]
    public void TakeAllAttachment()
    {
        ProxyMethod("TakeAllAttachment");
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.DeleteMail)]
    public void DeleteMail(int mailIndex)
    {
        ProxyMethod("DeleteMail", mailIndex);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.DeleteAllMail)]
    public void DeleteAllMail()
    {
        ProxyMethod("DeleteAllMail");
    }
    #endregion Mail

    #region OfflineExp

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.OfflineExpGetData)]
    public void OfflineExpGetData()
    {
        ProxyMethod("OfflineExpGetData");
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.OfflineExpStartReward)]
    public void OfflineExpStartReward(int cardNo)
    {
        ProxyMethod("OfflineExpStartReward", cardNo);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.OfflineExpClaimReward)]
    public void OfflineExpClaimReward(int claimCode)
    {
        ProxyMethod("OfflineExpClaimReward", claimCode);
    }

    #endregion OfflineExp

    #region Auction
    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.AuctionGetAuctionItem)]
    public void AuctionGetAuctionItem()
    {
        ProxyMethod("AuctionGetAuctionItem");
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.AuctionGetRecords)]
    public void AuctionGetRecords()
    {
        ProxyMethod("AuctionGetRecords");
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.AuctionGetBidItems)]
    public void AuctionGetBidItems()
    {
        ProxyMethod("AuctionGetBidItems");
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.AuctionCollectItem)]
    public void AuctionCollectItem(int bidId)
    {
        ProxyMethod("AuctionCollectItem", bidId);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.AuctionPlaceBid)]
    public void AuctionPlaceBid(int auctionId, int bidPrice)
    {
        ProxyMethod("AuctionPlaceBid", auctionId, bidPrice);
    }

    #endregion

    #region Guild
    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GuildGetGuildInfo)]
    public void GuildGetGuildInfo(byte searchFilter, string searchString)
    {
        ProxyMethod("GuildGetGuildInfo", searchFilter, searchString);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GuildCheckName)]
    public void GuildCheckName(string guildName)
    {
        ProxyMethod("GuildCheckName", guildName);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GuildAdd)]
    public void GuildAdd(string guildName, int guildIcon)
    {
        ProxyMethod("GuildAdd", guildName, guildIcon);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GuildJoin)]
    public void GuildJoin(int guildId)
    {
        ProxyMethod("GuildJoin", guildId);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GuildLeave)]
    public void GuildLeave(string playerName)
    {
        ProxyMethod("GuildLeave", playerName);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GuildMemberRequest)]
    public void GuildMemberRequest(string playerName, bool isAccept)
    {
        ProxyMethod("GuildMemberRequest", playerName, isAccept);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GuildRequestAll)]
    public void GuildRequestAll(bool isAccept)
    {
        ProxyMethod("GuildRequestAll", isAccept);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GuildRequestSetting)]
    public void GuildRequestSetting(int combatscore, int level, byte viplevel, bool autoAccept)
    {
        ProxyMethod("GuildRequestSetting", combatscore, level, viplevel, autoAccept);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GuildSetIcon)]
    public void GuildSetIcon(int icon, bool free)
    {
        ProxyMethod("GuildSetIcon", icon, free);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GuildSetNotice)]
    public void GuildSetNotice(string noticeStr)
    {
        ProxyMethod("GuildSetNotice", noticeStr);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GuildLvlUpTech)]
    public void GuildLvlUpTech(byte techType)
    {
        ProxyMethod("GuildLvlUpTech", techType);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GuildGetHistory)]
    public void GuildGetHistory()
    {
        ProxyMethod("GuildGetHistory");
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GuildShopExchange)]
    public void GuildShopExchange(byte shopId, int itemId)
    {
        ProxyMethod("GuildShopExchange", shopId, itemId);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GuildOpenSecretShop)]
    public void GuildOpenSecretShop()
    {
        ProxyMethod("GuildOpenSecretShop");
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GuildAppointRank)]
    public void GuildAppointRank(string appointName, byte appointRank)
    {
        ProxyMethod("GuildAppointRank", appointName, appointRank);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GuildInviteWorld)]
    public void GuildInviteWorld()
    {
        ProxyMethod("GuildInviteWorld");
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GuildDreamHouseAdd)]
    public void GuildDreamHouseAdd(byte type)
    {
        ProxyMethod("GuildDreamHouseAdd", type);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GuildDreamHouseCollect)]
    public void GuildDreamHouseCollect(int milestone)
    {
        ProxyMethod("GuildDreamHouseCollect", milestone);
    }
    #endregion

    #region Party
    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GetPartyList)]
    public void GetPartyList(int locationId, int minLevel, bool autoAcceptOnly, bool isRefresh)
    {
        ProxyMethod("GetPartyList", locationId, minLevel, autoAcceptOnly, isRefresh);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.CreateParty)]
    public void CreateParty()
    {
        ProxyMethod("CreateParty");
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.JoinParty)]
    public void JoinParty(int partyId)
    {
        ProxyMethod("JoinParty", partyId);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.ChangePartySetting)]
    public void ChangePartySetting(string settingStr)
    {
        ProxyMethod("ChangePartySetting", settingStr);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.KickPartyMember)]
    public void KickPartyMember(string memberName)
    {
        ProxyMethod("KickPartyMember", memberName);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.ChangePartyLeader)]
    public void ChangePartyLeader(string newLeaderName)
    {
        ProxyMethod("ChangePartyLeader", newLeaderName);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.ProcessPartyRequest)]
    public void ProcessPartyRequest(string playerName, bool isAccept)
    {
        ProxyMethod("ProcessPartyRequest", playerName, isAccept);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.InviteToParty)]
    public void InviteToParty(string playerName, int heroId = 0)
    {
        ProxyMethod("InviteToParty", playerName, heroId);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.AcceptPartyInvitation)]
    public void AcceptPartyInvitation(string senderName, bool isAccept)
    {
        ProxyMethod("AcceptPartyInvitation", senderName, isAccept);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.LeaveParty)]
    public void LeaveParty()
    {
        ProxyMethod("LeaveParty");
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.InviteToFollow)]
    public void InviteToFollow()
    {
        ProxyMethod("InviteToFollow");
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.AcceptFollowInvitation)]
    public void AcceptFollowInvitation(string senderName, bool isAccept)
    {
        ProxyMethod("AcceptFollowInvitation", senderName, isAccept);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.SendPartyRecruitment)]
    public void SendPartyRecruitment()
    {
        ProxyMethod("SendPartyRecruitment");
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.SaveAutoFollowSetting)]
    public void SaveAutoFollowSetting(bool isRejectFollow)
    {
        ProxyMethod("SaveAutoFollowSetting", isRejectFollow);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.FollowPartyMember)]
    public void FollowPartyMember(string memberName)
    {
        ProxyMethod("FollowPartyMember", memberName);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GetPartyMemberPosition)]
    public void GetPartyMemberPosition(string memberName)
    {
        ProxyMethod("GetPartyMemberPosition", memberName);
    }
    #endregion

    #region Hero
    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.UnlockHero)]
    public void UnlockHero(int heroId)
    {
        ProxyMethod("UnlockHero", heroId);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.LevelUpHero)]
    public void LevelUpHero(int heroId)
    {
        ProxyMethod("LevelUpHero", heroId);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.AddHeroSkillPoint)]
    public void AddHeroSkillPoint(int heroId)
    {
        ProxyMethod("AddHeroSkillPoint", heroId);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.ResetHeroSkillPoints)]
    public void ResetHeroSkillPoints(int heroId)
    {
        ProxyMethod("ResetHeroSkillPoints", heroId);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.LevelUpHeroSkill)]
    public void LevelUpHeroSkill(int heroId, int skillNo)
    {
        ProxyMethod("LevelUpHeroSkill", heroId, skillNo);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.ChangeHeroInterest)]
    public void ChangeHeroInterest(int heroId, byte assignedInterest, bool acceptResult = false)
    {
        ProxyMethod("ChangeHeroInterest", heroId, assignedInterest, acceptResult);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.AddHeroTrust)]
    public void AddHeroTrust(int heroId, int itemId)
    {
        ProxyMethod("AddHeroTrust", heroId, itemId);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.SummonHero)]
    public void SummonHero(int heroId, int tier = 0)
    {
        ProxyMethod("SummonHero", heroId, tier);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.ExploreMap)]
    public void ExploreMap(int mapId, int targetId, string heroes)
    {
        ProxyMethod("ExploreMap", mapId, targetId, heroes);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.ClaimExplorationReward)]
    public void ClaimExplorationReward(int mapId)
    {
        ProxyMethod("ClaimExplorationReward", mapId);
    }
    #endregion

    #region Achievement
    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.ClaimAchievementReward)]
    public void ClaimAchievementReward(byte type, int id)
    {
        ProxyMethod("ClaimAchievementReward", type, id);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.ClaimAllAchievementRewards)]
    public void ClaimAllAchievementRewards()
    {
        ProxyMethod("ClaimAllAchievementRewards");
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.AchievementNPCInteract)]
    public void AchievementNPCInteract(int npcId)
    {
        ProxyMethod("AchievementNPCInteract", npcId);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.StoreCollectionItem)]
    public void StoreCollectionItem(int id, bool isStore)
    {
        ProxyMethod("StoreCollectionItem", id, isStore);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.UnlockNextLISATier)]
    public void UnlockNextLISATier()
    {
        ProxyMethod("UnlockNextLISATier");
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.ChangeLISATier)]
    public void ChangeLISATier(int tier)
    {
        ProxyMethod("ChangeLISATier", tier);
    }
    #endregion

    #region Donate
    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GetGuildDonateData)]
    public void GetGuildDonateData(string lastUpdateTime)
    {
        ProxyMethod("GetGuildDonateData", lastUpdateTime);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.RequestGuildDonate)]
    public void RequestGuildDonate(int heroId)
    {
        ProxyMethod("RequestGuildDonate", heroId);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.ResponseGuildDonate)]
    public void ResponseGuildDonate(string requestPlayerName, int itemSlotId, int count)
    {
        ProxyMethod("ResponseGuildDonate", requestPlayerName, itemSlotId, count);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GetGuildDonate)]
    public void GetGuildDonate()
    {
        ProxyMethod("GetGuildDonate");
    }
    #endregion

    #region Bot
    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GetClosestValidMonSpawnPos)]
    public void GetClosestValidMonSpawnPos()
    {
        ProxyMethod("GetClosestValidMonSpawnPos");
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.BotSetting)]
    public void SendBotSettingToServer(string settings)
    {
        ProxyMethod("SendBotSettingToServer", settings);
    }
    #endregion

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.RemoveBuff)]
    public void RemoveSideBuff(int sideID)
    {
        ProxyMethod("RemoveSideBuff", sideID);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.AddCurrency)]
    public void AddCurrency(int type, int currency)
    {
        ProxyMethod("AddCurrency", type, currency);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.CurrencyExchange)]
    public void CurrencyExchange()
    {
        ProxyMethod("CurrencyExchange");
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.RedeemSerialCode)]
    public void RedeemSerialCode(string serial)
    {
        ProxyMethod("RedeemSerialCode", serial);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GetCompensate)]
    public void GetCompensateData(string playername)
    {
        ProxyMethod("GetCompensateData", playername);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.CharInfoSetPortrait)]
    public void CharInfoSetPortrait(int portraitID)
    {
        ProxyMethod("CharInfoSetPortrait", portraitID);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GetRandomBoxReward)]
    public void GetRandomBoxReward(int reward_id)
    {
        ProxyMethod("GetRandomBoxReward", reward_id);
    }

    #region Wardrobe
    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.EquipFashion)]
    public void EquipFashion(int item_id)
    {
        ProxyMethod("EquipFashion", item_id);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.UnequipFashion)]
    public void UnequipFashion(int item_id)
    {
        ProxyMethod("UnequipFashion", item_id);
    }
    #endregion

    #region Mount
    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.Mount)]
    public void Mount(int mount_id)
    {
        ProxyMethod("Mount", mount_id);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.UnMount)]
    public void UnMount()
    {
        ProxyMethod("UnMount");
    }

    #endregion

    #region Combat
    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.SetPlayerTeam)]
    public void SetPlayerTeam(int teamid)
    {
        ProxyMethod("SetPlayerTeam", teamid);
    }
    #endregion

    #region InvitePVP
    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.InvitePvpAsk)]
    public void InvitePvpAsk(string targetname)
    {
        ProxyMethod("InvitePvpAsk", targetname);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.InvitePvpReply)]
    public void InvitePvpReply(string askername, int reply)
    {
        ProxyMethod("InvitePvpReply", askername, reply);
    }

    #endregion

    #region Tutorial
    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.TutorialStep)]
    public void TutorialStep(int step)
    {
        ProxyMethod("TutorialStep", step);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.UpdateTutorialList)]
    public void UpdateTutorialList(int bitpos)
    {
        ProxyMethod("UpdateTutorialList", bitpos);
    }
    #endregion

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.SyncAttackResult)]
    public void SyncAttackResult(int targetpid, int dmg, bool heal, bool interrupte)
    {
        ProxyMethod("SyncAttackResult", targetpid, dmg,heal, interrupte);
    }

    #region Triggers
    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.OnColliderTrigger)]
    public void OnColliderTrigger(int objectId, bool enter)
    {
        ProxyMethod("OnColliderTrigger", objectId, enter);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.OnCutsceneFinished)]
    public void OnCutsceneFinished(int objectId)
    {
        ProxyMethod("OnCutsceneFinished", objectId);
    }
    #endregion

    #region InteractiveTrigger
    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.OnInteractiveInit)]
    public void OnInteractiveInit()
    {
        ProxyMethod("OnInteractiveInit");
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.OnInteractiveUse)]
    public void OnInteractiveUse(int objectId, bool enter)
    {
        ProxyMethod("OnInteractiveUse", objectId, enter);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.OnInteractiveTrigger)]
    public void OnInteractiveTrigger(int objectId)
    {
        ProxyMethod("OnInteractiveTrigger", objectId);
    }
    #endregion

    #region World Boss
    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GetWorldBossList)]
    public void GetWorldBossList()
    {
        ProxyMethod("GetWorldBossList");
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GetWorldBossDmgList)]
    public void GetWorldBossDmgList(int bossid)
    {
        ProxyMethod("GetWorldBossDmgList", bossid);
    }
    #endregion

    #region Side Effect
    [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.AddSideEffect)]
    public void AddSideEffect(int targetpid, int sideeffect_id)
    {
        ProxyMethod("AddSideEffect", targetpid, sideeffect_id);
    }

    [RPCMethod(RPCCategory.Combat,(byte)ClientCombatRPCMethods.RemoveSideEffect)]
    public void RemoveSideEffect(int targetpid, int sideeffect_id)
    {
        ProxyMethod("RemoveSideEffect", targetpid, sideeffect_id);
    }
    #endregion
}
