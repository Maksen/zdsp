using System.Collections.Generic;
using System.Threading.Tasks;

namespace Photon.LoadBalancing.GameServer.MultiServer
{
    public enum MultiServerDataState
    {
        Offline,
        Online,
        DiffChannel
    }
    public abstract class MultiServerDataHandler<T>
        where T : MultiServerDataHandler<T>.MultiServerDataSubHandler
    {
        public enum Stage
        {
            Begin,
            BeforeLoad,
            AfterLoad,
            BeforeSave,
            AfterSave
        }

        public readonly bool RefreshAfterLoad;
        public readonly bool DoPreSave;
        public readonly bool RefreshBeforeSave;
        public readonly bool RefreshAfterSave;
        public readonly MultiServerAPIManager APIManager;

        public Dictionary<string, APIHub> API { get { return APIManager.API; } }

        public readonly bool IsMultiData;
        protected Dictionary<string, T> MultiTable;
        public Dictionary<string, T> Multi { get { return MultiTable; } }
        protected T SingleData;
        public T Single { get { return SingleData; } }

        public Stage stage { get; protected set; }

        public void SetCharName(string charname)
        {
            if (stage != Stage.Begin)
                OnFinish();
            if (!IsMultiData)
                SingleData = OnCreateHandler(charname);
        }
        public void SetCharNames(IEnumerable<string> names)
        {
            if (stage != Stage.Begin)
                OnFinish();
            if (IsMultiData)
            {
                foreach (var name in names)
                    MultiTable.Add(name, OnCreateHandler(name));
            }
        }

        protected MultiServerDataHandler(MultiServerAPIManager apiManager, bool multi, bool RefreshAfterLoad = false, bool DoPreSave = true, bool RefreshBeforeSave = true, bool RefreshAfterSave = false, int defaultCapacity = 50)
        {
            this.APIManager = apiManager;
            IsMultiData = multi;
            if (IsMultiData)
            {
                MultiTable = new Dictionary<string, T>();
            }

            stage = Stage.Begin;

            this.RefreshAfterLoad = RefreshAfterLoad;
            this.DoPreSave = DoPreSave;
            this.RefreshBeforeSave = RefreshBeforeSave;
            this.RefreshAfterSave = RefreshAfterSave;

            if (IsMultiData)
            {
                if (defaultCapacity < 10)
                    defaultCapacity = 10;
                pool_multi = new List<T>(defaultCapacity);
                pool_multi_index = 0;
                for (int i = 0; i < defaultCapacity; i++)
                    pool_multi.Add(OnNewHandler());
            }
            else
                pool_single = OnNewHandler();

            m_Items = new List<T>(defaultCapacity);
            offline = new List<string>(defaultCapacity);
            online = new List<string>(defaultCapacity);
            diff = new List<string>(defaultCapacity);
        }

        private T pool_single;
        private List<T> pool_multi;
        private int pool_multi_index;

        private List<T> m_Items;
        void getDicItems()
        {
            m_Items.Clear();
            foreach (var item in MultiTable)
                m_Items.Add(item.Value);
        }
        private List<string> offline, online, diff;

        void getStateNames(bool onsave)
        {
            offline.Clear();
            online.Clear();
            diff.Clear();
            foreach (var item in MultiTable)
            {
                var value = item.Value;
                if (!onsave || value.loadFail || value.Changed)
                {
                    switch (value.State)
                    {
                        case MultiServerDataState.Offline:
                            offline.Add(item.Key);
                            break;
                        case MultiServerDataState.DiffChannel:
                            diff.Add(item.Key);
                            break;
                        case MultiServerDataState.Online:
                            online.Add(item.Key);
                            break;
                    }
                }
            }
        }

        public async Task OnLoad()
        {
            if (stage != Stage.Begin)
                OnFinish();
            stage = Stage.BeforeLoad;
            if (IsMultiData)
            {
                getDicItems();
                foreach (var item in m_Items)
                {
                    item.OnRefreshState(this);
                    item.BeforeLoadState = item.State;
                    item.PreLoad(this);
                }

                getStateNames(false);

                if (offline.Count > 0)
                    await OnLoadMultiOffline(offline);
                if (online.Count > 0)
                    await OnLoadMultiOnline(online);
                if (diff.Count > 0)
                    await OnLoadMultiDiffChannel(diff);

                stage = Stage.AfterLoad;
                if (RefreshAfterLoad)
                    foreach (var item in MultiTable.Values)
                    {
                        item.OnRefreshState(this);
                        item.BeforeLoadState = item.State;
                        item.PreLoad(this);
                    }
            }
            else
            {
                SingleData.OnRefreshState(this);
                SingleData.PreLoad(this);
                switch (SingleData.State)
                {
                    case MultiServerDataState.Offline:
                        await OnLoadSingleOffline(SingleData);
                        break;
                    case MultiServerDataState.Online:
                        await OnLoadSingleOnline(SingleData);
                        break;
                    case MultiServerDataState.DiffChannel:
                        await OnLoadSingleDiffChannel(SingleData);
                        break;
                }
                stage = Stage.AfterLoad;
                if (RefreshAfterLoad)
                    SingleData.OnRefreshState(this);
            }
        }

        protected virtual async Task<object[]> Request(T handler, string api, object[] args)
        {
            APIHub hub;
            object[] rlt = null;
            if (API.TryGetValue(api, out hub))
            {
                handler.OnRefreshState(this);
                if (handler.State == MultiServerDataState.DiffChannel)
                    rlt = await APIManager.Call(hub, handler.ServerID, args);
                else
                    rlt = await hub.RPC(false, args);
            }
            handler.RequestResult = rlt;
            return rlt;
        }

        public async Task<object[]> Request(string api, params object[] args)
        {
            return await Request(api, args);
        }

        public async Task RequestMulti(string api, params object[] args)
        {
            foreach (var item in MultiTable)
                await Request(item.Value, api, args);
        }

        public async Task OnSave()
        {
            stage = Stage.BeforeSave;
            if (IsMultiData)
            {
                if (DoPreSave)
                {
                    getDicItems();
                    foreach (var v in m_Items)
                    {
                        if (RefreshBeforeSave)
                            v.OnRefreshState(this);
                        v.PreSave(this);
                    }
                }

                getStateNames(true);

                if (offline.Count > 0)
                    await OnSaveMultiOffline(offline);
                if (online.Count > 0)
                    await OnSaveMultiOnline(online);
                if (diff.Count > 0)
                    await OnSaveMultiDiffChannel(diff);

                stage = Stage.AfterSave;

                if (RefreshAfterSave)
                    foreach (var item in MultiTable.Values)
                        item.OnRefreshState(this);
            }
            else
            {
                if (DoPreSave)
                {
                    if (RefreshBeforeSave)
                        SingleData.OnRefreshState(this);
                    SingleData.PreSave(this);
                }
                switch (SingleData.State)
                {
                    case MultiServerDataState.Offline:
                        await OnSaveSingleOffline(SingleData);
                        break;
                    case MultiServerDataState.Online:
                        await OnSaveSingleOnline(SingleData);
                        break;
                    case MultiServerDataState.DiffChannel:
                        await OnSaveSingleDiffChannel(SingleData);
                        break;
                }
                stage = Stage.AfterSave;
                if (RefreshAfterSave)
                    SingleData.OnRefreshState(this);
            }

            OnFinish();
        }

        protected abstract T OnNewHandler();
        protected virtual T OnCreateHandler(string charname)
        {
            T h;
            if (IsMultiData)
            {
                if (pool_multi_index == pool_multi.Count)
                {
                    pool_multi_index++;
                    h = OnNewHandler();
                    pool_multi.Add(h);
                }
                else
                    h = pool_multi[pool_multi_index++];
            }
            else
                h = pool_single;
            h.Reset(charname);
            return h;
        }


        protected abstract Task OnLoadSingleOffline(T handler);
        protected abstract Task OnLoadSingleOnline(T handler);
        protected abstract Task OnLoadSingleDiffChannel(T handler);
        protected abstract Task OnSaveSingleOffline(T handler);
        protected abstract Task OnSaveSingleOnline(T handler);
        protected abstract Task OnSaveSingleDiffChannel(T handler);

        protected abstract Task OnLoadMultiOffline(List<string> names);
        protected virtual async Task OnLoadMultiOnline(List<string> names)
        {
            foreach (var name in names)
                await OnLoadSingleOnline(MultiTable[name]);
        }
        protected abstract Task OnLoadMultiDiffChannel(List<string> names);
        protected abstract Task OnSaveMultiOffline(List<string> names);
        protected virtual async Task OnSaveMultiOnline(List<string> names)
        {
            foreach (var name in names)
                await OnSaveSingleOnline(MultiTable[name]);
        }
        protected abstract Task OnSaveMultiDiffChannel(List<string> names);

        public virtual void OnFinish()
        {
            if (IsMultiData)
            {
                MultiTable.Clear();
                pool_multi_index = 0;
            }
            stage = Stage.Begin;
        }

        public abstract class MultiServerDataSubHandler
        {
            internal protected abstract void Reset(string charname);

            protected void SetCharName(string charname) { this.charname = charname; }
            public string CharName { get { return charname; } }
            private string charname;

            public abstract bool Changed { get; }
            public bool loadFail;
            public int ServerID { get; internal protected set; }
            public object[] RequestResult;
            public MultiServerDataState BeforeLoadState { get; internal protected set; }
            public MultiServerDataState AfterLoadState { get; internal protected set; }
            public MultiServerDataState BeforeSaveState { get; internal protected set; }
            public MultiServerDataState AfterSaveState { get; internal protected set; }
            public abstract MultiServerDataState State { get; }

            internal protected abstract void OnRefreshState(MultiServerDataHandler<T> manager);

            internal protected virtual void PreLoad(MultiServerDataHandler<T> manager) { }
            internal protected virtual void PreSave(MultiServerDataHandler<T> manager) { }
        }
    }
}
