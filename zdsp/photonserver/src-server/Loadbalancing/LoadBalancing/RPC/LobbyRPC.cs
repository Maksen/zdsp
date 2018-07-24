using Photon.LoadBalancing.Operations;

using Photon.SocketServer;
using Photon.LoadBalancing.GameServer;
using Zealot.Common.RPC;
using Zealot.Common;

namespace Zealot.RPC
{
    public class LobbyRPC : ServerRPCBase
    {
        public LobbyRPC(object zrpc) :
            base(typeof(LobbyRPC), (byte)OperationCode.Lobby, true, zrpc)
        {
            SetMainContext(typeof(GameLogic), RPCCategory.Lobby);
        }

        //public void GetCharactersResult(int charcount, int jobsect1, int jobsect2, int jobsect3, string char1, string char2, string char3, object target) { }

        [RPCMethod(RPCCategory.Lobby, (byte)ServerLobbyRPCMethods.GetCharactersResult)]
        public void GetCharactersResult(string charlist, int latestLoginIndex, object target)
        {
            ProxyMethod("GetCharactersResult", charlist, latestLoginIndex, target);
        }

        [RPCMethod(RPCCategory.Lobby, (byte)ServerLobbyRPCMethods.TransferRoom)]
        public void TransferRoom(string levelname, object target)
        {
            ProxyMethod("TransferRoom", levelname, target);
        }

        [RPCMethod(RPCCategory.Lobby, (byte)ServerLobbyRPCMethods.LoadLevel)]
        public void LoadLevel(string levelname, object target)
        {
            ProxyMethod("LoadLevel", levelname, target);
        }

        [RPCMethod(RPCCategory.Lobby, (byte)ServerLobbyRPCMethods.DeleteCharacterResult)]
        public void DeleteCharacterResult(bool result, string charname, object target)
        {
            ProxyMethod("DeleteCharacterResult", result, charname, target);
        }

        [RPCMethod(RPCCategory.Lobby, (byte)ServerLobbyRPCMethods.ShowSystemMessage)]
        public void ShowSystemMessage(string ret, object target)
        {
            ProxyMethod("ShowSystemMessage", ret, target);
        }

        [RPCMethod(RPCCategory.Lobby, (byte)ServerLobbyRPCMethods.CreateCharacterSuccess)]
        public void CreateCharacterSuccess(string charname, object target)
        {
            ProxyMethod("CreateCharacterSuccess", charname, target);
        }
    }
}
