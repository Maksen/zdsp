using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Photon.LoadBalancing.Operations;

using Photon.SocketServer;
using Photon.LoadBalancing.GameServer;
using UnityEngine;
using Zealot.Common.Actions;
using Zealot.Common.RPC;

namespace Zealot.RPC
{
    public class ActionRPC : ServerRPCBase
    {
        public ActionRPC(object zrpc) :
            base(typeof(ActionRPC), (byte)OperationCode.Action, false, zrpc)
        {
            SetMainContext(typeof(GameLogic), RPCCategory.Action);
        }                

        // special rpc
        public void SendAction(int persid, ActionCommand cmd, object target)
        {                    
            ProxyMethod("SendAction", persid, cmd, target);
        }        
    }   
}
