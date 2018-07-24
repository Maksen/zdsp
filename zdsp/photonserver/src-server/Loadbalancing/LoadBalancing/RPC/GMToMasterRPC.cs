using Photon.LoadBalancing.Operations;
using Zealot.Common.RPC;
using Photon.LoadBalancing.MasterServer.GameManager;

namespace Zealot.RPC
{
    public class MasterToGMRPC : ServerRPCBase
    {
        public MasterToGMRPC() :
            base(typeof(MasterToGMRPC), (byte)OperationCode.GMToMasterRPC, true)
        {
            SetMainContext(typeof(IncomingGMPeer), RPCCategory.MasterToGMRPC);
        }

        [RPCMethod(RPCCategory.MasterToGMRPC, (byte)MasterGMRPCMethods.GMResultBool)]
        public void GMResultBool(string session, bool results, object target)
        {
            ProxyMethod("GMResultBool", session, results, target);
        }

        [RPCMethod(RPCCategory.MasterToGMRPC, (byte)MasterGMRPCMethods.GMResultString)]
        public void GMResultString(string session, string results, object target)
        {
            ProxyMethod("GMResultString", session, results, target);
        }
    }
}
