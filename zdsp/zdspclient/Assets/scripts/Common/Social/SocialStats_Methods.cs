using System;
using Zealot.Common.Datablock;
using Zealot.DebugTools;
using Zealot.Common.Entities.Social;


namespace Zealot.Common.Entities
{
    public partial class SocialStats : AdvancedLocalObject, IStats
    {
        [NotSynced]
        public bool StatsLoaded { get; private set; }

        #region Private Fields

        #endregion

        #region Constructors
        public SocialStats(bool isServer) : base(LOTYPE.SocialStats, isServer)
        {
            m_DebugMode = true;
            StatsLoaded = false;

            if (!isServer)
            {
                m_Tag = "SocialStats";
                data = new SocialData();
                this.Root = this.data.Root;
            }
        }
        #endregion

        #region Private Methods
        void InitFromRoot()
        {
            data.BuildNameMap();
        }
        #endregion

        #region Interface Methods

        #region AdvancedLocalObject
        protected override object OnGetRootData()
        {
            return data;
        }
        protected override Type OnGetRootDataType()
        {
            return typeof(SocialData);
        }

        bool onLoad =false;

        public override void BeforePacth(int cmdIndex, MessageType op, string path, object key)
        {
            if (!onLoad)
                data.BeforePacth(op, path, key);
        }
        public override void AfterPacth(int cmdIndex, MessageType op, string path, object key)
        {
            if (!onLoad)
                data.AfterPacth(op, path, key);
        }

        public override void OnEvent(int index, string path, string eventName, string param)
        {
            switch(eventName)
            {
                case "begin_load":
                    onLoad = true;
                    if (m_DebugDetailMode)
                        DebugTool.Print("[Social]: evnetName:begin_load"+" "+index);
                    break;
                case "end_load":
                    if (m_DebugDetailMode)
                        DebugTool.Print(Root);
                    data.UpdateRootValue(Root);
                    InitFromRoot();
                    if (m_DebugDetailMode)
                        DebugTool.Print("[Social]: evnetName:end_load" + " " + index);
                    onLoad = false;
                    break;
            }
        }
        #endregion

        #region IStats
        public void LoadFromInventoryData(IInventoryData data)
        {
            GameUtils.CatchException(SocialData.Debug, () =>
            {
                SocialInventoryData _data = data as SocialInventoryData;

                if (m_IsServer)
                {
                    this.data = new SocialData(_data.data, this);
                    this.Root = this.data.Root;

                    this.PatchEvent(string.Empty, "begin_load", string.Empty);
                    this.data.PatchToClient();
                    this.PatchEvent(string.Empty, "end_load", string.Empty);
                }
            });
            StatsLoaded = true;
        }

        public void SaveToInventoryData(IInventoryData data)
        {
            GameUtils.CatchException(SocialData.Debug, () =>
            {
                SocialInventoryData _data = data as SocialInventoryData;
            });
        }
        #endregion

        #endregion
    }

    #region removed future
    //public class SocialInfoBase
    //{
    //    public string charName = "";
    //    public int portraitId = 0;
    //    public byte jobSect = 0;
    //    public byte vipLvl = 0;
    //    public int charLvl = 0;
    //    public int combatScore = 0;
    //    public int localObjIdx = 0;

    //    public SocialInfoBase(string charname, int portrait, byte job, byte viplvl, int progresslvl, int combatscore, int mLocalObjIdx)
    //    {
    //        charName = charname;
    //        portraitId = portrait;
    //        jobSect = job;
    //        vipLvl = viplvl;
    //        charLvl = progresslvl;
    //        combatScore = combatscore;
    //        localObjIdx = mLocalObjIdx;
    //    }

    //    public SocialInfoBase(string str = "")
    //    {
    //        InitFromString(str);
    //    }

    //    public void InitFromString(string str)
    //    {
    //        string[] infos = str.Split('`');
    //        if (infos.Length == 6)
    //        {
    //            int idx = 0;
    //            charName = infos[idx++];
    //            portraitId = int.Parse(infos[idx++]);
    //            jobSect = byte.Parse(infos[idx++]);
    //            vipLvl = byte.Parse(infos[idx++]);
    //            charLvl = int.Parse(infos[idx++]);
    //            combatScore = int.Parse(infos[idx++]);
    //        }
    //    }

    //    public override string ToString()
    //    {
    //        StringBuilder sb = new StringBuilder();
    //        sb.Append(charName);
    //        sb.Append("`");
    //        sb.Append(portraitId);
    //        sb.Append("`");
    //        sb.Append(jobSect);
    //        sb.Append("`");
    //        sb.Append(vipLvl);
    //        sb.Append("`");
    //        sb.Append(charLvl);
    //        sb.Append("`");
    //        sb.Append(combatScore);
    //        return sb.ToString();
    //    }
    //}

    //public class SocialInfo : SocialInfoBase
    //{
    //    public byte faction = 0;
    //    public string guildName = "";
    //    public bool isOnline = false;

    //    public SocialInfo(string charname, int portrait, byte job, byte viplvl, int progresslvl, int combatscore,
    //                      byte factiontype, string guildname, bool online, int mLocalObjIdx)
    //                    : base(charname, portrait, job, viplvl, progresslvl, combatscore, mLocalObjIdx)
    //    {
    //        faction = factiontype;
    //        guildName = guildname;
    //        isOnline = online;
    //    }

    //    public SocialInfo(string str = "")
    //    {
    //        InitFromString(str);
    //    }

    //    public new void InitFromString(string str)
    //    {
    //        string[] infos = str.Split('`');
    //        if (infos.Length == 9)
    //        {
    //            int idx = 0;
    //            charName = infos[idx++];
    //            portraitId = int.Parse(infos[idx++]);
    //            jobSect = byte.Parse(infos[idx++]);
    //            vipLvl = byte.Parse(infos[idx++]);
    //            charLvl = int.Parse(infos[idx++]);
    //            combatScore = int.Parse(infos[idx++]);
    //            faction = byte.Parse(infos[idx++]);
    //            guildName = infos[idx++];
    //            isOnline = bool.Parse(infos[idx++]);
    //        }
    //    }

    //    public override string ToString()
    //    {
    //        StringBuilder sb = new StringBuilder();
    //        sb.Append(charName);
    //        sb.Append("`");
    //        sb.Append(portraitId);
    //        sb.Append("`");
    //        sb.Append(jobSect);
    //        sb.Append("`");
    //        sb.Append(vipLvl);
    //        sb.Append("`");
    //        sb.Append(charLvl);
    //        sb.Append("`");
    //        sb.Append(combatScore);
    //        sb.Append("`");
    //        sb.Append(faction);
    //        sb.Append("`");
    //        sb.Append(guildName);
    //        sb.Append("`");
    //        sb.Append(isOnline);
    //        return sb.ToString();
    //    }
    //}
    #endregion
}
