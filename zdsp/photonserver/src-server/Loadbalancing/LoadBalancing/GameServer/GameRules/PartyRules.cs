using Photon.LoadBalancing.GameServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Zealot.Common;
using Zealot.Common.Entities;
using Zealot.Common.RPC;
using Zealot.Repository;
using Zealot.Server.Entities;

namespace Zealot.Server.Rules
{
    public static class PartyIDPool
    {
        private static IDPool idpool;

        public static int AllocID()
        {
            if (idpool == null)
                idpool = new IDPool();

            return idpool.AllocID(0, false);
        }

        public static void FreeID(int id)
        {
            idpool.FreeID(id, 0);
        }
    }

    public class PartySizeComparer : IComparer<PartyStatsServer>
    {
        public int Compare(PartyStatsServer x, PartyStatsServer y)
        {
            if (x.MemberCount() >= PartyData.MAX_MEMBERS && y.MemberCount() < PartyData.MAX_MEMBERS)
                return 1;
            else if (x.MemberCount() < PartyData.MAX_MEMBERS && y.MemberCount() >= PartyData.MAX_MEMBERS)
                return -1;
            else
                return y.MemberCount().CompareTo(x.MemberCount());
        }
    }

    public static class PartyRules
    {
        public static Dictionary<int, PartyStatsServer> PartyList = new Dictionary<int, PartyStatsServer>();  // partyId -> PartyStats
        public static Dictionary<string, int> PlayerNameToPartyId = new Dictionary<string, int>();  // name -> partyId
        public static int UpdateInterval = 10000; //10 seconds

        public static void Init()
        {
            PartyIDPool.AllocID();  // reserve invalid id 0

            // for testing, create fake parties
            if (useFake)
            {
                for (int i = 0; i < 30; i++)
                    CreateFakeParty();
            }
        }

        // for testing use
        static bool useFake = false;
        private static void CreateFakeParty()
        {
            PartyStatsServer party = new PartyStatsServer();
            int partyId = PartyIDPool.AllocID();
            party.partyId = partyId;
            string fakeLeaderName = "FakeLeader" + partyId;
            EquipmentInventoryData invData = new EquipmentInventoryData();
            invData.InitDefault();
            AvatarInfo avatar = new AvatarInfo(invData, JobType.Newbie, Gender.Male);
            PartyMember leader = new PartyMember(fakeLeaderName, 1, 1, avatar, 1f, 1f, "");
            party.leader = fakeLeaderName;
            party.mPartySetting.autoAcceptType = (AutoAcceptType)GameUtils.RandomInt(0, 2);//AutoAcceptType.All;
            party.mPartySetting.minLevel = GameUtils.RandomInt(1, 50);
            party.mPartySetting.locationId = 0;//GameUtils.RandomInt(0, 2);
            party.partySetting = party.mPartySetting.ToString();
            party.AddPartyMember(leader);

            int memberCount = GameUtils.RandomInt(0, PartyData.MAX_MEMBERS - 1);  // random member count
            for (int i = 1; i <= memberCount; i++)
            {
                PartyMember member = new PartyMember("FakePlayer" + i, 1, 1, avatar, 1f, 1f, "");
                party.AddPartyMember(member);
            }
            PartyList.Add(partyId, party);
        }

        public static PartyStatsServer GetPartyById(int partyId)
        {
            PartyStatsServer partyStats;
            PartyList.TryGetValue(partyId, out partyStats);
            return partyStats;
        }

        public static PartyStatsServer GetMyParty(string name)
        {
            int id;
            if (PlayerNameToPartyId.TryGetValue(name, out id))
                return GetPartyById(id);
            return null;
        }

        public static int GetPartyIdByPlayerName(string name)
        {
            int id;
            PlayerNameToPartyId.TryGetValue(name, out id);
            return id;
        }

        public static void CreateParty(Player player)
        {
            PlayerSynStats playerStats = player.PlayerSynStats;
            AvatarInfo playerAvatar = new AvatarInfo(player.Slot.CharacterData.EquipmentInventory, (JobType)playerStats.jobsect, (Gender)playerStats.Gender);
            PartyMember leader = new PartyMember(player.Name, playerStats.Level, playerStats.PortraitID, playerAvatar,
                 playerStats.DisplayHp, 0f, playerStats.guildName);
            PartyStatsServer partyStats = new PartyStatsServer();
            int partyId = PartyIDPool.AllocID();
            partyStats.partyId = partyId;
            partyStats.leader = player.Name;
            partyStats.partySetting = partyStats.mPartySetting.ToString();

            partyStats.AddPartyMember(leader);
            PartyList.Add(partyId, partyStats);
            PlayerNameToPartyId.Add(player.Name, partyId);

            // add summoned hero
            if (player.HeroStats.SummonedHeroId > 0)
            {
                Hero hero = player.HeroStats.GetHero(player.HeroStats.SummonedHeroId);
                PartyMember newMember = new PartyMember(hero, player.Name);
                partyStats.AddPartyMember(newMember);
            }

            player.OnCreateParty(partyId);  // set player's partyId and add local object
        }

        public static void LeaveParty(int partyId, string leaverName, LeavePartyReason reason)
        {
            PartyStatsServer partyStats = GetPartyById(partyId);
            if (partyStats == null)
                return;

            if (partyStats.IsMember(leaverName))
            {
                PartyMember leavingMember = partyStats.GetPartyMember(leaverName);
                bool leaverIsPlayer = !leavingMember.IsHero();
                partyStats.RemovePartyMember(leaverName); // remove this member from members list

                if (leaverIsPlayer) // player is leaving party
                {
                    PlayerNameToPartyId.Remove(leaverName);

                    GameClientPeer peer = GameApplication.Instance.GetCharPeer(leaverName);
                    if (peer != null && peer.mPlayer != null) // player is online
                        peer.mPlayer.OnLeaveParty(reason);

                    // check for any owned hero in party and remove
                    PartyMember hero = partyStats.GetHeroOwnedByMember(leaverName);
                    if (hero != null)
                        partyStats.RemovePartyMember(hero.name);
                }
                else if (reason == LeavePartyReason.Kick) // is hero kicked from party so owner need to temporarily unsummon
                {
                    string ownerName = leavingMember.GetHeroOwner();
                    GameClientPeer ownerPeer = GameApplication.Instance.GetCharPeer(ownerName);
                    if (ownerPeer != null && ownerPeer.mPlayer != null) // owner is online
                        ownerPeer.mPlayer.HeroStats.UnSummonHeroEntity();
                }

                if (partyStats.MemberCount() == 0)  // last member leave party
                    RemoveParty(partyId);
                else
                {
                    partyStats.OnDirty();

                    // disband party if leader leave, or party only left 1 member
                    bool disbandParty = partyStats.IsLeader(leaverName) || reason == LeavePartyReason.Disband || (partyStats.MemberCount() == 1 && leaverIsPlayer);
                    if (disbandParty)
                    {
                        // call leave party on remaining member
                        string nextMemberName = partyStats.GetNextMemberName(false);
                        LeaveParty(partyId, nextMemberName, LeavePartyReason.Disband);
                    }
                }
            }
        }

        private static void RemoveParty(int partyId)
        {
            PartyList.Remove(partyId);
            PartyIDPool.FreeID(partyId);
        }

        public static void TryJoinParty(int partyId, Player player)
        {
            PartyStatsServer partyStats = GetPartyById(partyId);
            if (partyStats == null)  // party no longer exist
                return;

            if (partyStats.IsMember(player.Name)) // do nothing if is already is party's member
                return;

            PartySetting partySetting = partyStats.mPartySetting;
            if (player.PlayerSynStats.Level < partySetting.minLevel || player.PlayerSynStats.Level > partySetting.maxLevel)
            {
                player.Slot.ZRPC.CombatRPC.Ret_SendSystemMessage("sys_party_LevelReqNotMet", "", false, player.Slot);
                return;
            }

            if (partyStats.CanAutoAccept(player) && !partyStats.IsPartyFull())
            {
                JoinParty(partyId, player);
            }
            else  // cannot auto accept or party is full so add to request
            {
                if (partyStats.HasApplied(player.Name))  // check has already applied
                    return;
                if (partyStats.IsRequestFull()) // check request list full
                {
                    player.Slot.ZRPC.CombatRPC.Ret_SendSystemMessage("sys_party_RequestListFull", "", false, player.Slot);
                    return;
                }

                PartyRequest newRequest = new PartyRequest(player.Name, player.PlayerSynStats.Level,
                    (JobType)player.PlayerSynStats.jobsect, player.PlayerSynStats.PortraitID);
                partyStats.AddPartyRequest(newRequest);
                partyStats.OnDirty();
            }
        }

        /// <summary>
        /// This method do not check whether player is able to join party (eg. party not full/meet requirements).
        /// Need to do necessary checking before calling this method.
        /// </summary>
        private static void JoinParty(int partyId, Player player)
        {
            PartyStatsServer partyStats = GetPartyById(partyId);
            if (partyStats == null)
                return;

            PartyRequest request = partyStats.GetPartyRequest(player.Name);  // get any request from this player
            if (request != null)
                partyStats.RemovePartyRequest(player.Name);  // remove request from this player

            PlayerSynStats playerStats = player.PlayerSynStats;
            AvatarInfo playerAvatar = new AvatarInfo(player.Slot.CharacterData.EquipmentInventory, (JobType)playerStats.jobsect, (Gender)playerStats.Gender);
            PartyMember newMember = new PartyMember(player.Name, playerStats.Level, playerStats.PortraitID, playerAvatar,
                playerStats.DisplayHp, 0f, playerStats.guildName);
            partyStats.AddPartyMember(newMember);
            PlayerNameToPartyId.Add(player.Name, partyId);

            // check whether leader is online, if not, need to reassign leader (in case of player joining on invite but none online)
            if (!partyStats.IsLeaderOnline())
            {
                string newLeader = partyStats.GetNextOnlineMemberName();
                if (!string.IsNullOrEmpty(newLeader))
                    partyStats.leader = newLeader;
            }

            // check whether this player has any summoned hero
            HeroEntity heroEntity = player.HeroStats.GetSummonedHeroEntity();
            if (heroEntity != null)
            {
                if (partyStats.IsPartyFull()) // party is full so need to temporarily unsummon this hero
                {
                    player.HeroStats.UnSummonHeroEntity();
                }
                else // party still have space so add summoned hero from this player to party
                {
                    PartyMember heroMember = new PartyMember(heroEntity.Hero, player.Name);
                    partyStats.AddPartyMember(heroMember);
                }
            }

            player.OnJoinParty(partyId);  // set partyid and add party localobj
            partyStats.OnDirty(player.Name);  // update other members, no need to update player that just joined
        }

        public static void ProcessMemberRequest(int partyId, string charName, bool isAccept, Player player)
        {
            PartyStatsServer partyStats = GetPartyById(partyId);
            if (partyStats != null && partyStats.GetPartyRequest(charName) != null)  // ensure valid request
            {
                if (partyStats.IsLeader(player.Name))  // ensure sender is leader
                {
                    if (isAccept)
                    {
                        // check party is not full
                        if (partyStats.IsPartyFull())
                        {
                            player.Slot.ZRPC.CombatRPC.Ret_SendSystemMessage("sys_party_PartyFull", "", false, player.Slot);
                            return;
                        }
                        // check applied player is not already in a party
                        if (GetPartyIdByPlayerName(charName) > 0) // already have party
                        {
                            player.Slot.ZRPC.CombatRPC.Ret_SendSystemMessage("sys_party_TargetAlreadyHasParty", "", false, player.Slot);
                            return;
                        }

                        GameClientPeer peer = GameApplication.Instance.GetCharPeer(charName);
                        if (peer != null)  // applied player is online
                            JoinParty(partyId, peer.mPlayer);
                        else
                            player.Slot.ZRPC.CombatRPC.Ret_SendSystemMessage("sys_party_TargetIsOffline", "", false, player.Slot);
                    }
                    else
                    {
                        partyStats.RemovePartyRequest(charName);
                        partyStats.OnDirty();
                    }
                }
            }
        }

        public static void OnCharacterOnline(string charName, bool isOnline)
        {
            int partyId = GetPartyIdByPlayerName(charName);
            if (partyId > 0)
            {
                PartyStatsServer partyStats = GetPartyById(partyId);
                if (partyStats != null)
                {
                    partyStats.SetMemberOnline(charName, isOnline);

                    // set online status of any hero owned by this player
                    PartyMember hero = partyStats.GetHeroOwnedByMember(charName);
                    if (hero != null)
                        partyStats.SetMemberOnline(hero.name, isOnline);

                    // if leader is offline, change leader
                    if (!partyStats.IsLeaderOnline())
                    {
                        string newLeader = partyStats.GetNextOnlineMemberName();
                        if (!string.IsNullOrEmpty(newLeader))
                            partyStats.leader = newLeader;
                    }

                    partyStats.OnDirty(charName);
                }
            }
        }

        public static void ChangePartyLeader(int partyId, string newLeaderName, Player player)
        {
            PartyStatsServer partyStats = GetPartyById(partyId);
            if (partyStats == null)
                return;

            if (partyStats.IsLeader(player.Name))  // ensure sender is current party leader
            {
                if (partyStats.IsMember(newLeaderName))  // ensure new leader is a valid party member
                {
                    if (partyStats.IsMemberOnline(newLeaderName))  // ensure new leader is online
                    {
                        partyStats.leader = newLeaderName;
                        partyStats.lastFollowInvitationDT = DateTime.Now.AddHours(-1);  // reset
                        partyStats.OnDirty();
                    }
                }
            }
        }

        public static void SendPartyListToClient(int locationId, int minLevel, bool autoAcceptOnly, bool isRefresh, Player player)
        {
            List<PartyInfo> listToSend = new List<PartyInfo>();
            List<PartyStatsServer> filteredList = new List<PartyStatsServer>();
            if (locationId == 0)  // nearby
            {
                string currentRoom = player.mInstance.mRoom.Guid;
                if (autoAcceptOnly)
                {
                    filteredList = PartyList.Values.Where((x) =>
                    {
                        GameClientPeer leaderPeer = GameApplication.Instance.GetCharPeer(x.leader);
                        if (leaderPeer == null || leaderPeer.mPlayer == null || leaderPeer.mPlayer.mInstance == null)
                        {
                            if (useFake)  // to be deleted
                                return x.mPartySetting.minLevel >= minLevel && x.mPartySetting.autoAcceptType == AutoAcceptType.All && !x.IsPartyFull();
                            else
                                return false;
                        }
                        else
                        {   // is in same room
                            return leaderPeer.mPlayer.mInstance.mRoom.Guid == currentRoom && x.mPartySetting.minLevel >= minLevel
                            && x.mPartySetting.autoAcceptType == AutoAcceptType.All && !x.IsPartyFull();
                        }
                    }).ToList();
                }
                else
                {
                    filteredList = PartyList.Values.Where((x) =>
                    {
                        GameClientPeer leaderPeer = GameApplication.Instance.GetCharPeer(x.leader);
                        if (leaderPeer == null || leaderPeer.mPlayer == null || leaderPeer.mPlayer.mInstance == null)
                        {
                            if (useFake)
                                return x.mPartySetting.minLevel >= minLevel;
                            else
                                return false;
                        }
                        else // is in same room
                            return leaderPeer.mPlayer.mInstance.mRoom.Guid == currentRoom && x.mPartySetting.minLevel >= minLevel;
                    }).ToList();
                }
            }
            else  // location specific
            {
                if (autoAcceptOnly)
                {
                    filteredList = PartyList.Values.Where(x => x.IsLeaderOnline() && x.mPartySetting.locationId == locationId
                        && x.mPartySetting.minLevel >= minLevel && x.mPartySetting.autoAcceptType == AutoAcceptType.All && !x.IsPartyFull()).ToList();
                }
                else
                {
                    filteredList = PartyList.Values.Where(x => x.IsLeaderOnline() && x.mPartySetting.locationId == locationId
                        && x.mPartySetting.minLevel >= minLevel).ToList();
                }
            }

            if (filteredList.Count > 20)  // limit send 20 parties
            {
                List<PartyStatsServer> tempList = new List<PartyStatsServer>();
                while (tempList.Count < 20)
                {
                    int randIdx = GameUtils.RandomInt(0, filteredList.Count - 1);  // get random index in filtered list
                    tempList.Add(filteredList[randIdx]);  // add party at index to temp list
                    filteredList.RemoveAt(randIdx); // remove party at index from filtered list
                }
                filteredList.Clear();
                filteredList = tempList;
            }

            filteredList.Sort(new PartySizeComparer());  // sort by party size 4>3>2>1>5
            for (int i = 0; i < filteredList.Count; i++)
            {
                PartyInfo info = new PartyInfo(filteredList[i]);
                listToSend.Add(info);
            }

            string data = JsonConvertDefaultSetting.SerializeObject(listToSend);
            player.Slot.ZRPC.CombatRPC.Ret_GetPartyList(data, player.Slot);
        }

        public static void ChangePartySetting(int partyId, string setting, Player player)
        {
            PartyStatsServer partyStats = GetPartyById(partyId);
            if (partyStats == null)
                return;

            if (partyStats.IsLeader(player.Name))  // ensure sender is current party leader
            {
                PartySetting newSetting = PartySetting.ToObject(setting);
                if (partyStats.mPartySetting != newSetting)
                {
                    partyStats.mPartySetting = newSetting;  // update current settings
                    partyStats.partySetting = setting;
                    partyStats.OnDirty();
                }
            }
        }

        public static void InviteToParty(string charName, int heroId, Player player)
        {
            if (!string.IsNullOrEmpty(charName))  // is invite player
            {
                Dictionary<string, string> param = new Dictionary<string, string>();
                param.Add("name", charName);

                GameClientPeer invitedPeer = GameApplication.Instance.GetCharPeer(charName);
                if (invitedPeer != null) // invited player is online
                {
                    if (invitedPeer.mPlayer != null && !invitedPeer.mPlayer.IsInParty())  // check invited player already has party
                    {
                        invitedPeer.ZRPC.CombatRPC.SendPartyInvitation(player.Name, invitedPeer);
                        player.Slot.ZRPC.CombatRPC.Ret_SendSystemMessage("sys_party_PartyInvitationSent",
                            string.Format("name;{0}", charName), false, player.Slot);
                    }
                    else
                    {
                        player.Slot.ZRPC.CombatRPC.Ret_SendSystemMessage("sys_party_TargetAlreadyHasParty", "", false, player.Slot);
                    }
                }
            }
            else if (heroId > 0) // is invite hero
            {
                Hero hero = player.HeroStats.GetHero(heroId);
                if (hero != null)
                {
                    PartyStatsServer partyStats = GetPartyById(player.PlayerSynStats.Party);
                    if (partyStats == null)  // player has no party
                    {
                        player.HeroStats.SummonHero(heroId);  // summon out hero first
                        CreateParty(player); // then create party and hero will also join
                    }
                    else
                    {
                        PartyMember currHeroMember = partyStats.GetHeroOwnedByMember(player.Name);
                        // if player already have a hero in party, will replaced with this new hero
                        // else can only add this hero if party still have space
                        if (currHeroMember != null || !partyStats.IsPartyFull())  
                            player.HeroStats.SummonHero(heroId); // will replace or add hero to party                        
                        else
                            player.Slot.ZRPC.CombatRPC.Ret_SendSystemMessage("sys_party_PartyFull", "", false, player.Slot);
                    }
                }
            }
        }

        public static void AcceptPartyInvitation(string senderName, bool isAccept, Player player)
        {
            if (isAccept)
            {
                // check whether invite sender has party
                int partyId = GetPartyIdByPlayerName(senderName);
                if (partyId == 0)  // no party so need to create party
                {
                    GameClientPeer senderPeer = GameApplication.Instance.GetCharPeer(senderName);
                    if (senderPeer != null && senderPeer.mPlayer != null)  // sender is online
                    {
                        CreateParty(senderPeer.mPlayer);  // create party first with sender as leader
                        partyId = GetPartyIdByPlayerName(senderName);
                        JoinParty(partyId, player);  // then join the party
                    }
                    else
                        player.Slot.ZRPC.CombatRPC.Ret_SendSystemMessage("sys_party_TargetIsOffline", "", false, player.Slot);
                }
                else  // has party so check whether can join
                {
                    PartyStatsServer partyStats = GetPartyById(partyId);
                    if (!partyStats.IsPartyFull())  // check whether party is full
                        JoinParty(partyId, player);
                    else
                        player.Slot.ZRPC.CombatRPC.Ret_SendSystemMessage("sys_party_PartyFull", "", false, player.Slot);
                }
            }
            else  // rejected invitation
            {
                GameClientPeer senderPeer = GameApplication.Instance.GetCharPeer(senderName);
                if (senderPeer != null && senderPeer.mPlayer != null)  // sender is online
                {
                    senderPeer.ZRPC.CombatRPC.Ret_SendSystemMessage("sys_party_TargetRejectedInvitation",
                        string.Format("name;{0}", player.Name), false, senderPeer);
                }
            }
        }

        public static void InviteToFollow(int partyId, Player player)
        {
            PartyStatsServer partyStats = GetPartyById(partyId);
            if (partyStats == null)
                return;

            if (partyStats.CanSendFollowInvitation(DateTime.Now))  // check for cooldown
            {
                var memberNameList = partyStats.GetPartyMemberList().Keys;
                foreach (string memberName in memberNameList)
                {
                    if (memberName == player.Name)
                        continue;
                    GameClientPeer peer = GameApplication.Instance.GetCharPeer(memberName);
                    if (peer != null && peer.mPlayer != null)
                        peer.ZRPC.CombatRPC.SendFollowInvitation(player.Name, peer);
                }
                player.Slot.ZRPC.CombatRPC.Ret_SendSystemMessage("sys_party_FollowInvitationSent", "", false, player.Slot);
            }
            else
                player.Slot.ZRPC.CombatRPC.Ret_SendSystemMessage("sys_party_FollowInvitationCD", "", false, player.Slot);
        }

        public static void AcceptFollowInvitation(string senderName, bool isAccept, Player player)
        {
            int partyId = GetPartyIdByPlayerName(senderName);
            if (partyId > 0) // ensure party still exist
            {
                PartyStatsServer partyStats = GetPartyById(partyId);
                if (!partyStats.IsLeader(senderName) || !partyStats.IsMember(player.Name) || senderName == player.Name)
                    return;

                GameClientPeer leaderPeer = GameApplication.Instance.GetCharPeer(senderName);
                if (leaderPeer != null && leaderPeer.mPlayer != null)  // ensure leader is online
                {
                    if (isAccept)
                        FollowPartyMember(senderName, player);
                    else // rejected, need check member's follow request game setting
                    {
                        PartyMember member = partyStats.GetPartyMember(player.Name);
                        if (!member.rejectFollow)
                            FollowPartyMember(senderName, player);
                    }
                }
            }
        }

        public static void SendPartyRecruitment(int partyId, Player player)
        {
            PartyStatsServer partyStats = GetPartyById(partyId);
            if (partyStats == null)
                return;

            if (partyStats.IsPartyFull())  // ensure party is not full
                return;

            if (partyStats.CanSendRecruitment(DateTime.Now)) // check for cooldown
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("|party|");
                sb.AppendFormat("{0};", partyId);
                sb.AppendFormat("{0};", partyStats.MemberCount());
                var memberList = partyStats.GetPartyMemberList().Values;
                foreach (var member in memberList)
                    sb.AppendFormat("{0};{1};{2};{3};", member.name, member.level, member.portraitId, partyStats.IsLeader(member.name) ? 1 : 0);
                string link = sb.ToString().TrimEnd(';');
                sb.Clear();
                sb.Append(player.Name);
                sb.Append(GUILocalizationRepo.colon);
                sb.Append(PartyRepo.GetLocationName(partyStats.mPartySetting.locationId));
                sb.Append(string.Format(" ({0}/{1})", partyStats.MemberCount(), PartyData.MAX_MEMBERS));
                string message = GameUtils.GetHyperTextTag(link, sb.ToString());
                ChatMessage newMsg = new ChatMessage(MessageType.World, message);
                GameApplication.Instance.BroadcastChatMessage(newMsg);
                player.Slot.ZRPC.CombatRPC.Ret_SendSystemMessage("sys_party_RecruitmentSent", "", false, player.Slot);
            }
            else
                player.Slot.ZRPC.CombatRPC.Ret_SendSystemMessage("sys_party_RecruitmentCD", "", false, player.Slot);
        }

        public static void SaveAutoFollowSetting(int partyId, bool isRejectFollow, Player player)
        {
            PartyStatsServer partyStats = GetPartyById(partyId);
            if (partyStats == null)
                return;

            partyStats.SetMemberAutoFollowSetting(player.Name, isRejectFollow);
        }

        public static void FollowPartyMember(string followeeName, Player player)
        {
            if (string.IsNullOrEmpty(followeeName))  // means cancel follow
            {
                player.Slot.ZRPC.CombatRPC.OnFollowPartyMember(-1, "", "", Vector3.zero.ToRPCPosition(), player.Slot);
            }
            else
            {
                string currLevelName;
                Vector3 currPosition;
                int pid = GetPartyMemberPosition(followeeName, out currLevelName, out currPosition, player);
                if (pid != -1)
                    player.Slot.ZRPC.CombatRPC.OnFollowPartyMember(pid, followeeName, currLevelName, currPosition.ToRPCPosition(), player.Slot);
            }
        }

        // return member's persistentID, -1 if fail
        public static int GetPartyMemberPosition(string memberName, out string currLevelName, out Vector3 currPosition, Player player)
        {
            currLevelName = "";
            currPosition = Vector3.zero;
            PartyStatsServer partyStats = GetPartyById(player.PlayerSynStats.Party);
            if (partyStats == null)
                return -1;

            if (!partyStats.IsMember(memberName) || !partyStats.IsMember(player.Name))
                return -1;

            // todo: check member not in solo realm so cannot follow

            GameClientPeer peer = GameApplication.Instance.GetCharPeer(memberName);
            if (peer != null && peer.mPlayer != null && peer.mPlayer.mInstance != null)
            {
                currLevelName = peer.mPlayer.mInstance.currentlevelname;
                currPosition = peer.mPlayer.Position;
                return peer.mPlayer.GetPersistentID();
            }

            return -1;
        }

        public static void Update()
        {
            var _peers = GameApplication.Instance.GetAllCharPeer();
            foreach (var kvp in PartyList)
            {
                var _members = kvp.Value.GetPartyMemberList();
                var _membersGroupByInstance = new Dictionary<string, List<Player>>();  //key instance guid
                var _instanceInCombat = new Dictionary<string, bool>(); //key instance guid
                foreach (var _member in _members)
                {
                    if (!_member.Value.online || _member.Value.IsHero())
                        continue;
                    GameClientPeer _peer;
                    if (_peers.TryGetValue(_member.Key, out _peer))
                    {
                        Player _player = _peer.mPlayer;
                        if (_player != null && _player.mInstance.IsWorld())
                        {
                            string _guid = _player.mInstance.mRoom.Guid;
                            if (!_membersGroupByInstance.ContainsKey(_guid))
                                _membersGroupByInstance.Add(_guid, new List<Player>());
                            _membersGroupByInstance[_guid].Add(_player);
                            if (_player.LocalCombatStats.IsInCombat)
                                _instanceInCombat[_guid] = true;
                        }
                    }
                }
                foreach(var _playerListByInstance in _instanceInCombat) //Add battleTime for players in combat level
                {
                    
                    var _playerList = _membersGroupByInstance[_playerListByInstance.Key];

                     

                    for (int index = 0; index < _playerList.Count; index++)
                    {
                        _playerList[index].BattleTime = _playerList[index].SecondaryStats.BattleTime;
                        _playerList[index].BattleTime = _playerList[index].Slot.CharacterData.BattleTime;
                        //_playerList[index].Slot.CharacterData.BattleTime += 10;
                        _playerList[index].SecondaryStats.BattleTime = _playerList[index].BattleTime;
                        _playerList[index].Slot.CharacterData.BattleTime = _playerList[index].BattleTime;
                        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                        if ((_playerList[index].InCombatTime > 0)&&(_playerList[index+1].InCombatTime<=0))
                        {
                            sw.Reset();
                            sw.Start();
                        }
                        if (_playerList[index].InCombatTime <= 0)
                        {
                            sw.Stop();
                        }

                        string sw_result = sw.Elapsed.TotalSeconds.ToString();
                        float SWRESULT = (float)Convert.ToDouble(sw_result);
                        _playerList[index].BattleTime = (_playerList[index].SecondaryStats.BattleTime * 60 - Mathf.CeilToInt(SWRESULT / 10)) / 60;

                    }
                }
            }
        }
    }
}