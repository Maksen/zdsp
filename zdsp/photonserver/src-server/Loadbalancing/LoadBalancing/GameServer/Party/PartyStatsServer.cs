using System;
using System.Collections.Generic;
using System.Linq;
using Zealot.Common;
using Zealot.Common.Datablock;
using Zealot.Common.Entities;
using Zealot.Server.Entities;

namespace Photon.LoadBalancing.GameServer
{
    public class PartyStatsServer : PartyStats
    {
        public DateTime lastRecruitmentDT;
        public DateTime lastFollowInvitationDT;

        public PartyStatsServer() : base()
        {
            lastRecruitmentDT = DateTime.Now.AddHours(-1);
            lastFollowInvitationDT = DateTime.Now.AddHours(-1);
        }

        public void OnDirty(string excludeName = "")
        {
            if (IsDirty())
            {
                var memberList = mPartyMembers.Values;
                foreach (PartyMember member in memberList)
                {
                    if (member.name == excludeName || member.IsHero())
                        continue;
                    GameClientPeer peer = GameApplication.Instance.GetCharPeer(member.name);
                    if (peer != null && peer.mPlayer != null)
                        peer.ZRPC.LocalObjectRPC.UpdateLocalObject((byte)LOCATEGORY.SharedStats, -1, this, peer);
                }
                Reset();
            }
        }


        public void SetDirty(string memberName)
        {
            PartyMember member = GetPartyMember(memberName);
            if (member != null)
            {
                members[member.slotIdx] = member.ToString();
                OnDirty();
            }
        }

        public int GetEmptyMemberSlot()
        {
            for (int i = 0; i < members.Count; i++)
            {
                if (members[i] == null)
                    return i;
            }
            return -1;
        }

        public int GetEmptyRequestSlot()
        {
            for (int i = 0; i < requests.Count; i++)
            {
                if (requests[i] == null)
                    return i;
            }
            return -1;
        }

        public void AddPartyMember(PartyMember member)
        {
            int slotIdx = GetEmptyMemberSlot();
            member.slotIdx = slotIdx;
            mPartyMembers[member.name] = member;
            members[slotIdx] = member.ToString();
        }

        public void RemovePartyMember(string name)
        {
            PartyMember member = GetPartyMember(name);
            if (member != null)
            {
                mPartyMembers.Remove(name);
                members[member.slotIdx] = null;
            }
        }

        public void AddPartyRequest(PartyRequest request)
        {
            int slotIdx = GetEmptyRequestSlot();
            request.slotIdx = slotIdx;
            mPartyRequests[request.name] = request;
            requests[slotIdx] = request.ToString();
        }

        public void RemovePartyRequest(string name)
        {
            PartyRequest request = GetPartyRequest(name);
            if (request != null)
            {
                mPartyRequests.Remove(name);
                requests[request.slotIdx] = null;
            }
        }

        public string GetNextMemberName(bool preferOnline)
        {
            foreach (var member in mPartyMembers.Values)
            {
                if ((!preferOnline || (preferOnline && member.online)) && !member.IsHero())
                    return member.name;
            }

            // want online members but all are offline, so just get an offline one
            if (preferOnline && mPartyMembers.Count > 0)
                return GetNextMemberName(false);
            else
                return "";
        }

        public string GetNextOnlineMemberName()
        {
            foreach (var member in mPartyMembers.Values)
            {
                if (member.online && !member.IsHero())
                    return member.name;
            }
            return "";
        }

        public bool CanAutoAccept(Player player)
        {
            if (mPartySetting.autoAcceptType == AutoAcceptType.Closed)
                return false;
            else if (mPartySetting.autoAcceptType == AutoAcceptType.All)
                return true;
            else  // guild member and friends only
            {
                GameClientPeer leaderPeer = GameApplication.Instance.GetCharPeer(leader);
                if (leaderPeer != null && leaderPeer.mPlayer != null)
                {
                    bool isGuildMate = false;
                    bool isFriend = false;
                    // check guild
                    if (leaderPeer.mPlayer.SecondaryStats.guildId <= 0)  // leader have no guild
                        isGuildMate = false;
                    else if (leaderPeer.mPlayer.SecondaryStats.guildId == player.SecondaryStats.guildId)
                        isGuildMate = true;

                    // check friend
                    #region Old Code:Social: modified future
                    //var friendList = leaderPeer.mPlayer.SocialStats.GetFriendListDict().Keys;
                    //if (friendList.Contains(player.Name))
                    //    isFriend = true;
                    #endregion

                    return (isGuildMate || isFriend);
                }
                else
                    return false;
            }
        }

        public void SetMemberOnline(string name, bool isOnline)
        {
            PartyMember member = GetPartyMember(name);
            if (member != null && member.online != isOnline)
            {
                member.online = isOnline;
                members[member.slotIdx] = member.ToString();
            }
        }

        public bool IsLeaderOnline()
        {
            return IsMemberOnline(leader);
        }

        public void SetMemberHP(string name, float hp)
        {
            PartyMember member = GetPartyMember(name);
            if (member != null && member.hp != hp)
            {
                member.hp = hp;
                members[member.slotIdx] = member.ToString();
                OnDirty(name);  // don't need to sync own's health in partystats
            }
        }

        public void SetMemberMP(string name, float mp)
        {
            PartyMember member = GetPartyMember(name);
            if (member != null && member.mp != mp)
            {
                member.mp = mp;
                members[member.slotIdx] = member.ToString();
                OnDirty(name);  // don't need to sync own's mp in partystats
            }
        }

        public void SetMemberLevel(string name, int level)
        {
            PartyMember member = GetPartyMember(name);
            if (member != null && member.level != level)
            {
                member.level = level;
                members[member.slotIdx] = member.ToString();
                OnDirty();
            }
        }

        public void SetMemberAvatar(string name, EquipmentInventoryData equipInvData, JobType job)
        {
            PartyMember member = GetPartyMember(name);
            if (member != null)
            {
                member.avatar.equipInvData = equipInvData;
                member.avatar.jobType = job;
                members[member.slotIdx] = member.ToString();
            }
        }

        public void SetMemberAutoFollowSetting(string name, bool isRejectFollow)
        {
            PartyMember member = GetPartyMember(name);
            if (member != null && member.rejectFollow != isRejectFollow)
            {
                member.rejectFollow = isRejectFollow;
                members[member.slotIdx] = member.ToString();
                // no need to sync
            }
        }

        public bool CanSendRecruitment(DateTime now)
        {
            if ((now - lastRecruitmentDT).TotalMinutes > 1)
            {
                lastRecruitmentDT = now;
                return true;
            }
            return false;
        }

        public bool CanSendFollowInvitation(DateTime now)
        {
            if ((now - lastFollowInvitationDT).TotalSeconds > 20)
            {
                lastFollowInvitationDT = now;
                return true;
            }
            return false;
        }

        public PartyMember GetHeroOwnedByMember(string name)
        {
            return mPartyMembers.Values.FirstOrDefault(x => x.GetHeroOwner() == name);
        }

        public void UpdateMemberHero(string oldHeroName, PartyMember newHero)
        {
            mPartyMembers.Remove(oldHeroName);
            mPartyMembers[newHero.name] = newHero;
            members[newHero.slotIdx] = newHero.ToString();
        }

        public bool CanAddMemberHeroToParty(string memberName)
        {
            return GetHeroOwnedByMember(memberName) != null || !IsPartyFull();
        }

        public List<Player> GetOnlinePartyMembers(string excludeName)
        {
            List<Player> ret = new List<Player>();
            var memberList = mPartyMembers.Values;
            foreach (PartyMember member in memberList)
            {
                if (member.name == excludeName || member.IsHero())
                    continue;
                GameClientPeer peer = GameApplication.Instance.GetCharPeer(member.name);
                if (peer != null && peer.mPlayer != null)
                    ret.Add(peer.mPlayer);
            }
            return ret;
        }

        public List<Player> GetSameInstancePartyMembers(string excludeName, GameLogic instance)
        {
            List<Player> ret = new List<Player>();
            var memberList = mPartyMembers.Values;
            foreach (PartyMember member in memberList)
            {
                if (member.name == excludeName || member.IsHero())
                    continue;
                GameClientPeer peer = GameApplication.Instance.GetCharPeer(member.name);
                if (peer != null && peer.mPlayer != null && peer.mPlayer.mInstance == instance)
                    ret.Add(peer.mPlayer);
            }
            return ret;
        }
    }
}