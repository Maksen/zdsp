namespace Zealot.Common
{
    public enum MessageType : byte
    {
        System,
        World,
        //Faction,
        Guild,
        Party,
        Nearby,
        Whisper,
        BroadcastMessage,
    }

    public enum BroadcastMessageType : byte 
    {
        BossSpawn,
        BossKilled,
        BossKilledMyDmg,
        BossKilledMyScore,
        MonsterSpawn,
        MonsterKilled,
        GMActivityConfigChanged,
        NotifyActivityStart,
        StatusActivityStart,
        StatusActivityEnd,
        NewServerEventConfigChanged,
        ArenaRankUp,
        TickerTapeMessage,
        AuctionBegin,
        AuctionEnd,
        AuctionChanged,
        RandomBoxRewardActive,
        RandomBoxRewardExprie,
        RareItemNotification,
        NewDay,
        SystemSwitchChange,
        GainExperience,
        MessageBroadcaster,
        DonationRefresh,
        InteractiveTrigger,
    }

    public class ChatMessage
    {
        public byte mMsgType;
        public string mMessage;
        public string mSender;
        public string mWhisperTo;
        public int mPortraitId;
        public byte mJobsect;
        public byte mVipLvl;
        public byte mFaction;
        public bool mIsVoiceChat;
        public byte mBroadcastMsgType;
        public bool mHasShadow;
        
        public ChatMessage(MessageType msgType, string msg, string sender="", string whisperTo="", 
                           int portraitId=0, byte jobsect=1, byte vipLvl=0, byte faction=1, 
                           bool isVoiceChat=false, byte broadcastMsgType=0, bool hasShadow=false)
        {
            mMsgType = (byte)msgType;
            mMessage = msg;
            mSender = sender;
            mWhisperTo = whisperTo;
            mPortraitId = portraitId;
            mJobsect = jobsect;
            mVipLvl = vipLvl;
            mFaction = faction;
            mIsVoiceChat = isVoiceChat;
            mBroadcastMsgType = broadcastMsgType;
            mHasShadow = hasShadow;
        }
    }
}