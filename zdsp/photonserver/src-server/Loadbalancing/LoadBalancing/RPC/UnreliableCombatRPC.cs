using Photon.LoadBalancing.Operations;
using Photon.SocketServer;
using Photon.LoadBalancing.GameServer;
using Zealot.Common.RPC;

namespace Zealot.RPC
{
    using Zealot.Common.RPC;

    public class UnreliableCombatRPC : ServerRPCBase
    {
        public UnreliableCombatRPC(object zrpc) :
            base(typeof(UnreliableCombatRPC), (byte)OperationCode.UnreliableCombat, false, zrpc)
        {
            SetMainContext(typeof(GameLogic), RPCCategory.UnreliableCombat);
        }

        //Place here because hit is only for display
        [RPCMethod(RPCCategory.UnreliableCombat, (byte)ServerUnreliableCombatRPCMethods.SideEffectHit)]
        public void SideEffectHit(int targetPID, int sideeffectID, object target)
        {
            ProxyMethod("SideEffectHit", targetPID, sideeffectID, target);
        }

        [RPCMethod(RPCCategory.UnreliableCombat, (byte)ServerUnreliableCombatRPCMethods.EntityOnDamage)]
        public void EntityOnDamage(int targetpid, int attackerpid, int info, int attackdamage, int labels, int skillid, object target)
        {
            ProxyMethod("EntityOnDamage", targetpid, attackerpid, info, attackdamage, labels, skillid, target);
        }
    }
}