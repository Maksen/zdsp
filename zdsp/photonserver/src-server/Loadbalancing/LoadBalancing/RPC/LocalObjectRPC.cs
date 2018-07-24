using Photon.LoadBalancing.Operations;
using Photon.LoadBalancing.GameServer;
using Zealot.Common.Datablock;
using Zealot.Common.RPC;

namespace Zealot.RPC
{
    public class LocalObjectRPC : ServerRPCBase
    {
        public LocalObjectRPC() :
            base(typeof(LocalObjectRPC), (byte)OperationCode.LocalObject, true)
        {
            SetMainContext(typeof(GameLogic), RPCCategory.LocalObject);
        }

        // special rpc
        public void UpdateLocalObject(byte locategory, int id, LocalObject cmd, object target)
        {            
            ProxyMethod("UpdateLocalObject", locategory, id, cmd, target);
        }

        public void AddLocalObject(byte locategory, int id, LocalObject cmd, object target)
        {            
            ProxyMethod("AddLocalObject", locategory, id, cmd, target);
        }
    }
}
