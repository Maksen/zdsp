using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Photon.LoadBalancing.GameServer.MultiServer;
using Zealot.Common.Entities.Social;

namespace Photon.LoadBalancing.GameServer
{
    public class SocialDataHandler : MultiServerDataHandler<SocialDataHandler.SocialDataSubHandler>
    {
        #region Fields
        public readonly bool HasData;
        public readonly bool HasGuid;
        public readonly bool HasState;
        #endregion

        #region Constructors
        public SocialDataHandler(MultiServerAPIManager apiManager, bool hasData, bool hasGuid, bool hasState, bool multi) : base(apiManager, multi, false, false, false, false)
        {
            this.HasData = hasData;
            this.HasGuid = hasGuid;
            this.HasState = hasState;
            init();
        }
        #endregion

        #region Private
        private JsonSerializerSettings jsonSettingsLoad;
        private bool loadErrorHappened;

        //private JsonSerializerSettings jsonSettingsSave;

        void loadErrorHandler(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs args)
        {
            args.ErrorContext.Handled = true;
            loadErrorHappened = true;
        }

        void init()
        {
            jsonSettingsLoad = new JsonSerializerSettings();
            jsonSettingsLoad.Error = loadErrorHandler;


        }

        SocialData loadSocialData(string jsonStr, out bool error)
        {
            JObject json = null;
            loadErrorHappened = false;
            json = JsonConvert.DeserializeObject<JToken>(jsonStr, jsonSettingsLoad) as JObject;
            if (json == null || loadErrorHappened)
                json = new JObject();
            error = loadErrorHappened;
            loadErrorHappened = false;
            return new SocialData(json, false);
        }

        private void loadOffline(SocialDataSubHandler handler, Dictionary<string, object> dic)
        {
            object obj;

            if (HasData)
            {
                if (!dic.TryGetValue("friends", out obj))
                {
                    handler.loadFail = true;
                    return;
                }
                bool error;
                handler.data = loadSocialData((string)obj, out error);
                if (error)
                    handler.SetChanged(true);
            }

            if (HasGuid)
            {
                if (!dic.TryGetValue("charid", out obj))
                {
                    handler.loadFail = true;
                    return;
                }
                if (obj is Guid)
                    handler.id = ((Guid)obj).ToString();
                else if (obj is string)
                    handler.id = ((string)obj);
                else
                {
                    handler.loadFail = true;
                    return;
                }
            }

            if (HasState)
                handler.dic = dic;
        }
        #endregion

        #region overrides-MultiServerDataHandler

        protected override SocialDataSubHandler OnNewHandler()
        {
            return new SocialDataSubHandler();
        }

        protected override Task OnLoadMultiDiffChannel(List<string> names)
        {
            return Task.CompletedTask;
        }

        protected override async Task OnLoadMultiOffline(List<string> names)
        {
            List<Dictionary<string, object>> list;

            if (HasData)
                list = await GameApplication.dbRepository.Character.GetSocialByNames(names);
            else
                list = await GameApplication.dbRepository.Character.GetSocialStateByNames(names);

            foreach (var name in names)
                MultiTable[name].nameNotFound = true;
            foreach (var dic in list)
            {
                object obj;
                if (!dic.TryGetValue("charname", out obj))
                    continue;
                string name = (string)obj;
                var h = MultiTable[name];
                h.nameNotFound = false;
                loadOffline(h, dic);
            }

        }

        protected override Task OnLoadSingleDiffChannel(SocialDataSubHandler handler)
        {
            return Task.CompletedTask;
        }

        protected override async Task OnLoadSingleOffline(SocialDataSubHandler handler)
        {
            var dic = await GameApplication.dbRepository.Character.GetSocialByName(handler.CharName);
            if (!dic.ContainsKey("charname"))
            {
                handler.nameNotFound = true;
                return;
            }
            loadOffline(handler, dic);
        }

        protected override Task OnLoadSingleOnline(SocialDataSubHandler handler)
        {
            if (HasData)
                handler.data = SocialController.GetData(handler.peer);
            if (HasGuid)
                handler.id = handler.peer.GetCharId();
            if (HasState)
                handler.dic = null;
            return Task.CompletedTask;
        }

        protected override Task OnSaveMultiDiffChannel(List<string> names)
        {
            return Task.CompletedTask;
        }

        private List<string> tempNames = new List<string>();
        private List<string> multi_friends = new List<string>();

        protected override async Task OnSaveMultiOffline(List<string> names)
        {
            tempNames.Clear();
            multi_friends.Clear();
            foreach (var name in names)
            {
                SocialDataSubHandler handler = MultiTable[name];
                if (handler.data != null)
                {
                    multi_friends.Add(handler.data.BuildRecordsString());
                    tempNames.Add(name);
                }
            }
            await GameApplication.dbRepository.Character.UpdateMultipleSocialList(tempNames, multi_friends);
        }

        protected override Task OnSaveSingleDiffChannel(SocialDataSubHandler handler)
        {
            return Task.CompletedTask;
        }

        protected override async Task OnSaveSingleOffline(SocialDataSubHandler handler)
        {
            if (handler.data != null)
                await GameApplication.dbRepository.Character.UpdateSocialList(handler.CharName, handler.data.BuildRecordsString());
        }

        protected override Task OnSaveSingleOnline(SocialDataSubHandler handler)
        {
            return Task.CompletedTask;
        }
        #endregion

        #region class SocialDataSubHandler
        public class SocialDataSubHandler : MultiServerDataSubHandler
        {
            public SocialData data;
            public string id;
            public GameClientPeer peer;
            internal bool dataChanged;
            internal bool dataChangedSet;
            internal Dictionary<string, object> dic;

            public bool nameNotFound;
            public override bool Changed
            {
                get
                {
                    if (dataChangedSet)
                        return dataChanged;
                    else
                    {
                        if (data != null)
                            return data.Dirty;
                        else
                            return false;
                    }
                }
            }

            internal void SetChanged(bool changed)
            {
                dataChangedSet = true;
                dataChanged = changed;
            }

            public SocialFriendState getState()
            {
                if (dic == null)
                {
                    var od = GameApplication.Instance.GetCharDatablock(CharName);
                    return new SocialFriendState(
                          name: CharName,
                          offlineTime: null,
                          channel: od.server == null ? string.Empty : od.server,
                          progressLevel: od.level,
                          guildName: string.Empty,
                          guildIcon: string.Empty
                          );
                }
                else
                {
                    return new SocialFriendState(
                        name: CharName,
                        offlineTime: SocialController.GetOfflineTime((DateTime)dic["dtlogout"]),
                        channel: SocialController.GetChannel(CharName),
                        progressLevel: (int)dic["progresslevel"],
                        guildName: string.Empty,
                        guildIcon: string.Empty);
                }
            }

            public SocialDataSubHandler()
            {
            }

            internal protected override void Reset(string charname)
            {
                SetCharName(charname);
                dataChanged = false;
                dataChangedSet = false;
                nameNotFound = false;
                loadFail = false;
                data = null;
                id = null;
                peer = null;
                dic = null;
            }

            private MultiServerDataState state;


            public override MultiServerDataState State { get { return state; } }

            internal protected override void OnRefreshState(MultiServerDataHandler<SocialDataSubHandler> manager)
            {
                if (manager.stage != Stage.BeforeLoad && manager.stage != Stage.Begin)
                    return;
                var onlineInfo = GameApplication.Instance.GetCharDatablock(CharName);

                if (onlineInfo == null || string.IsNullOrEmpty(onlineInfo.server))
                    state = MultiServerDataState.Offline;
                else
                {
                    var cfg = GameApplication.Instance.MyServerConfig;
                    if (onlineInfo.server == cfg.servername)
                    {
                        peer = SocialController.GetPeer(CharName);
                        if (peer == null)
                            state = MultiServerDataState.Offline;
                        else
                            state = MultiServerDataState.Online;
                    }
                    else
                    {
                        ServerID = ClusterServer.ClusterApplication.Instance.mClusterServer.GetServerId(cfg.serverline, onlineInfo.server);
                        state = MultiServerDataState.DiffChannel;
                    }
                }

            }
        }
        #endregion

        #region Utils
        public string id { get { return SingleData.id; } }
        public bool changed { get { return SingleData.Changed; } set { SingleData.SetChanged(value); } }

        public bool nameNotFound { get { return SingleData.nameNotFound; } }

        public async Task<SocialData> LoadSocialData(GameClientPeer peer)
        {
            SetCharName(peer.CharacterData.Name);
            await OnLoad();
            return SingleData.data;
        }
        public async Task<SocialData> LoadSocialData(string charname)
        {
            SetCharName(charname);
            await OnLoad();
            return SingleData.data;
        }
        public SocialFriendState LoadState(string charname)
        {
            SetCharName(charname);
            SingleData.OnRefreshState(this);
            return SingleData.getState();
        }


        #endregion
    }
}
