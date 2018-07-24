using Photon.LoadBalancing.Operations;
using Zealot.Common.RPC;
using Photon.LoadBalancing.MasterServer;

namespace Zealot.RPC
{
    public class GameToMasterRPC : ServerRPCBase
    {
        public GameToMasterRPC() :
            base(typeof(GameToMasterRPC), (byte)OperationCode.GameToMaster, true)
        {
            SetMainContext(typeof(MasterGame), RPCCategory.GameToMaster);
        }

        [RPCMethod(RPCCategory.GameToMaster, (byte)GameToMasterRPCMethods.RegularServerStatusUpdate)]
        public void RegularServerStatusUpdate(int roomupdatedur, byte cpuusage, object target)
        {
            ProxyMethod("RegularServerStatusUpdate", roomupdatedur, cpuusage, target);
        }

        [RPCMethod(RPCCategory.GameToMaster, (byte)GameToMasterRPCMethods.RegUser)]
        public void RegUser(string userid, object target)
        {
            ProxyMethod("RegUser", userid, target);
        }

        [RPCMethod(RPCCategory.GameToMaster, (byte)GameToMasterRPCMethods.UnRegUser)]
        public void UnRegUser(string userid, object target)
        {
            ProxyMethod("UnRegUser", userid, target);
        }

        [RPCMethod(RPCCategory.GameToMaster, (byte)GameToMasterRPCMethods.TransferServer)]
        public void TransferServer(string userid, string cookie, int serverid, object target)
        {
            ProxyMethod("TransferServer", userid, cookie, serverid, target);
        }

        [RPCMethod(RPCCategory.GameToMaster, (byte)GameToMasterRPCMethods.GMResultBool)]
        public void GMResultBool(string sessionid, bool result, object target)
        {
            ProxyMethod("GMResultBool", sessionid, result, target);
        }

        [RPCMethod(RPCCategory.GameToMaster, (byte)GameToMasterRPCMethods.GMResultString)]
        public void GMResultString(string sessionid, string result, object target)
        {
            ProxyMethod("GMResultString", sessionid, result, target);
        }
    }
}