using ExitGames.Diagnostics.Counter;
using Photon.SocketServer;
using Zealot.Common;
using Zealot.Server.Entities;

namespace Zealot.Server.EventMessage
{
    public class RoomBroadcastMessage
    {
        public static readonly CountsPerSecondCounter CounterEventReceive = new CountsPerSecondCounter("RoomBroadcastMessage.Receive");

        public static readonly CountsPerSecondCounter CounterEventSend = new CountsPerSecondCounter("RoomBroadcastMessage.Send");

        protected readonly RPCBroadcastData rpcBroadcastData;

        public RoomBroadcastMessage(RPCBroadcastData broadcastData)
        {
            rpcBroadcastData = broadcastData;
        }

        #region Properties
        public RPCBroadcastData RPCBroadcastData
        {
            get { return rpcBroadcastData; }
        }

        public EventData EventData
        {
            get { return rpcBroadcastData.EventData; }
        }

        public SendParameters SendParameters
        {
            get { return rpcBroadcastData.SendParameters; }
        }

        public virtual void SendEvent(Player player)
        {
            player.Slot.SendEvent(rpcBroadcastData.EventData, rpcBroadcastData.SendParameters);
        }
        #endregion
    }

    public class FactionBroadcastMessage : RoomBroadcastMessage
    {
        protected byte mFaction;

        public FactionBroadcastMessage(RPCBroadcastData broadcastData, byte faction) : base(broadcastData)
        {
            mFaction = faction;
        }

        public override void SendEvent(Player player)
        {
            if (player.PlayerSynStats.faction == mFaction)
                player.Slot.SendEvent(rpcBroadcastData.EventData, rpcBroadcastData.SendParameters);
        }
    }

    public class ChatBroadcastMessage : RoomBroadcastMessage
    {
        protected ChatMessage mChatMessage;
        public ChatBroadcastMessage(ChatMessage chatMessage) : base(null)
        {
            mChatMessage = chatMessage;
        }

        public override void SendEvent(Player player)
        {
            player.AddToChatMessageQueue(mChatMessage);
        }
    }

    public class FactionChatBroadcastMessage : ChatBroadcastMessage
    {
        public FactionChatBroadcastMessage(ChatMessage chatMessage) : base(chatMessage)
        {}

        public override void SendEvent(Player player)
        {
            if (player.PlayerSynStats.faction == mChatMessage.mFaction)
                player.AddToChatMessageQueue(mChatMessage);
        }
    }

    public class GuildChatBroadcastMessage : ChatBroadcastMessage
    {
        private int mGuildId;
        public GuildChatBroadcastMessage(ChatMessage chatMessage, int guildId) : base(chatMessage)
        {
            mGuildId = guildId;
        }

        public override void SendEvent(Player player)
        {
            if (player.SecondaryStats.guildId == mGuildId)
                player.AddToChatMessageQueue(mChatMessage);
        }
    }

    //public class PartyChatBroadcastMessage : ChatBroadcastMessage
    //{
    //    private int mPartyId;
    //    public PartyChatBroadcastMessage(ChatMessage chatMessage, int partyId) : base(chatMessage)
    //    {
    //        mPartyId = partyId;
    //    }

    //    public override void SendEvent(Player player)
    //    {
    //        if (player.PlayerSynStats.Party == mPartyId)
    //            player.AddToChatMessageQueue(mChatMessage);
    //    }
    //}

    public class NewDayBroadcastMessage : RoomBroadcastMessage
    {
        public NewDayBroadcastMessage(RPCBroadcastData broadcastData)
            : base(broadcastData)
        {}

        public override void SendEvent(Player player)
        {
            player.NewDay();
            base.SendEvent(player);
        }
    }
}
