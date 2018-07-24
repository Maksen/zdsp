using System.Collections.Generic;
using System.Linq;
using Zealot.Common.Datablock;

namespace Zealot.Common.Entities
{
    public class PartyStats : LocalObject
    {
        private int _partyId;
        private string _leader;
        private string _partySetting;

        public Dictionary<string, PartyMember> GetPartyMemberList() { return mPartyMembers; }
        protected Dictionary<string, PartyMember> mPartyMembers;
        public Dictionary<string, PartyRequest> GetPartyRequestList() { return mPartyRequests; }
        protected Dictionary<string, PartyRequest> mPartyRequests;

        public PartySetting mPartySetting;

        public PartyStats() : base(LOTYPE.PartyStats)
        {
            members = new CollectionHandler<object>(PartyData.MAX_MEMBERS);
            members.SetParent(this, "members");
            mPartyMembers = new Dictionary<string, PartyMember>();
            requests = new CollectionHandler<object>(PartyData.MAX_REQUESTS);
            requests.SetParent(this, "requests");
            mPartyRequests = new Dictionary<string, PartyRequest>();
            mPartySetting = new PartySetting();
        }

        public CollectionHandler<object> members { get; set; } // Store member info in string
        public CollectionHandler<object> requests { get; set; } // Store member request info in string

        public int partyId
        {
            get { return _partyId; }
            set { this.OnSetAttribute("partyId", value); _partyId = value; }
        }

        public string leader
        {
            get { return _leader; }
            set { this.OnSetAttribute("leader", value); _leader = value; }
        }

        public string partySetting
        {
            get { return _partySetting; }
            set { this.OnSetAttribute("partySetting", value); _partySetting = value; }
        }

        /// <summary>
        /// Default includes hero
        /// </summary>
        public int MemberCount(bool includeHero = true)
        {
            return includeHero ? mPartyMembers.Count : mPartyMembers.Values.Count(x => !x.IsHero());
        }

        public bool IsMember(string name)
        {
            return mPartyMembers.ContainsKey(name);
        }

        public bool IsLeader(string name)
        {
            return leader == name;
        }

        public PartyMember GetPartyMember(string name)
        {
            PartyMember member;
            mPartyMembers.TryGetValue(name, out member);
            return member;
        }

        public PartyMember GetLeader()
        {
            return GetPartyMember(leader);
        }

        public PartyRequest GetPartyRequest(string name)
        {
            PartyRequest request;
            mPartyRequests.TryGetValue(name, out request);
            return request;
        }

        public bool IsPartyFull()
        {
            return mPartyMembers.Count == PartyData.MAX_MEMBERS;
        }

        public bool IsRequestFull()
        {
            return mPartyRequests.Count == PartyData.MAX_REQUESTS;
        }

        public bool HasApplied(string name)
        {
            return mPartyRequests.ContainsKey(name);
        }

        public bool IsMemberOnline(string name)
        {
            PartyMember member = GetPartyMember(name);
            if (member != null)
                return member.online;
            return false;
        }
    }

}

