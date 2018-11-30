using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using ExitGames.Diagnostics.Counter;
using ExitGames.Concurrency.Fibers;
using Zealot.Server.Counters;

namespace Photon.LoadBalancing.GameServer.MultiServer
{
    #region MultiServerAPIManager
    public class APIHub
    {
        public readonly string api;
        public readonly Func<bool, object[], Task<object[]>> RPC;
        public readonly bool enqueue;
        public APIHub(string api, Func<bool, object[], Task<object[]>> RPC, bool enqueue)
        {
            this.api = api;
            this.RPC = RPC;
            this.enqueue = enqueue;
        }
    }
    public struct APIHubResult
    {
        public SemaphoreSlim Signal;
        public object[] args;
    }
    public class MultiServerAPIManager
    {
        public static MultiServerAPIManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MultiServerAPIManager();
                    instance.ExeQueueCounter = GameCounters.ExecutionFiberQueue;
                    instance.ExeQueue = GameApplication.Instance.executionFiber;
                }
                return instance;
            }
        }

        static MultiServerAPIManager instance;

        public readonly Dictionary<string, APIHub> API;
        public MultiServerAPIManager()
        {
            API = new Dictionary<string, APIHub>();
        }

        public void RegisterAPI(string api, Func<bool, object[], Task<object[]>> RPC, bool enqueue)
        {
            API.Add(api, new APIHub(api, RPC, enqueue));
        }

        public PoolFiber ExeQueue;
        public NumericCounter ExeQueueCounter;
        public bool ENABLE_QUEUE_COUNTER;

        long id_gen;
        Dictionary<long, APIHubResult> m_RequestSignals = new Dictionary<long, APIHubResult>();


        public void ReleaseRequest(long id, params object[] args)
        {
            if (m_RequestSignals.ContainsKey(id))
            {
                var val = m_RequestSignals[id];
                val.args = args;
                m_RequestSignals[id] = val;
                val.Signal.Release();
            }
        }

        public void CallRPC(string api, long id, object[] args, Photon.LoadBalancing.ClusterServer.GameServer.IncomingGameServerPeer peer)
        {
            APIHub hub;
            if (API.TryGetValue(api, out hub))
            {
                if (hub.enqueue)
                {
                    if (ENABLE_QUEUE_COUNTER) ExeQueueCounter.Increment();
                    ExeQueue.Enqueue(async () =>
                    {
                        if (ENABLE_QUEUE_COUNTER) ExeQueueCounter.Decrement();

                        var rlt = await hub.RPC(true, args);

                        GameApplication.Instance.ZRPC.ClusterToGameRPC.RET_APIManagerCallRPC(id, rlt, peer);
                    });
                }
                else
                {
                    var task = hub.RPC(true, args);
                }
            }
        }

        public async Task<object[]> Call(APIHub api, int serverid, object[] args)
        {
            long id = NewRequest();
            GameApplication.Instance.ZRPC.GameToClusterRPC.APIManagerCallRPC(api.api, serverid, id, args, GameApplication.Instance.clusterPeer);
            return await WaitResult(id);
        }

        long NewRequest()
        {
            id_gen++;
            APIHubResult rlt = new APIHubResult();
            rlt.Signal = new SemaphoreSlim(0);
            m_RequestSignals.Add(id_gen, rlt);
            return id_gen;
        }
        async Task<object[]> WaitResult(long id)
        {
            APIHubResult rlt;
            if (m_RequestSignals.TryGetValue(id, out rlt))
            {
                await rlt.Signal.WaitAsync();
                return rlt.args;
            }
            else
                return null;
        }
    }
    #endregion
}
