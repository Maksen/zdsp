using System;
using System.Collections.Generic;
using System.Reflection;
using Zealot.Common;

namespace Photon.LoadBalancing.GameServer.Extensions
{
    
    public class UnRegisteredSysMsgAttribute : Attribute
    {
        public UnRegisteredSysMsgAttribute()
        {

        }
    }


    public static partial class GameClientPeerExtensions
    {
        /// <summary>
        /// UNREGISTERED_SYSTEM_MSG
        /// </summary>
        static Dictionary<string, string> CreateUnRegisteredSysMsg()
        {
            var dic = new Dictionary<string, string>();
            object[] p = new object[] { dic };

            MethodInfo[] methods = typeof(GameClientPeerExtensions).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            foreach (var method in methods)
            {
                if (method.GetCustomAttribute<UnRegisteredSysMsgAttribute>() != null)
                {
                    var param = method.GetParameters();
                    if (param.Length == 1 && param[0].ParameterType == typeof(Dictionary<string, string>))
                        method.Invoke(null, p);
                }
            }
            return dic;
        }
        static Dictionary<string, string> m_UnRegisteredSysMsg = null;

        /// <summary>
        /// 送系統訊息，如果要暫時註冊請參考，放上去時會自動變成以註冊的為主，請移至本函式定義看範例
        /// </summary>
        public static void SendSystemMessage(this GameClientPeer peer, string msgName, bool addToChatLog, string args = "")
        {
            if (m_UnRegisteredSysMsg == null)
                m_UnRegisteredSysMsg = CreateUnRegisteredSysMsg();

            int id = Zealot.Repository.GUILocalizationRepo.GetSysMsgIdByName(msgName);

            if (id <= 0)
            {
                string msg;
                if (m_UnRegisteredSysMsg.TryGetValue(msgName, out msg))
                    peer.ZRPC.CombatRPC.Ret_SendSystemMessage(GameUtils.FormatArgs(msg, args), string.Empty, false, peer);
                else
                    peer.ZRPC.CombatRPC.Ret_SendSystemMessage(string.Format("[unknown]{0}", msgName), string.Empty, false, peer);
            }
            else
            {
                peer.ZRPC.CombatRPC.Ret_SendSystemMessageId(id, args, false, peer);
            }
        }

        /// <example>
        /// add {Catalog}/{Catalog}Extensions.cs to your {Catalog} folder
        /// if messgae is registered, remove this static method.
        /// 
        /// public static partial class GameClientPeerExtensions
        /// {
        ///     [UnRegisteredSysMsg]
        ///     static void YourCatalogUnRegisteredSysMsg(Dictionary&lt;string, string&rt; dic)
        ///     {
        ///         dic["ret_YourCatalogMessage1"] = "{player} says: bla bla bla.";
        ///         dic["ret_YourCatalogMessage2"] = "{player} level upgraded.";
        ///         dic["ret_YourCatalogMessage3"] = "system msg 3";
        ///         dic["ret_YourCatalogMessageMore"] = "system msg more....";
        ///     }
        /// }
        /// 
        /// then call GameClientPeer.SendSystemMessage(msg_system_name,addToChatLog,param_dic)
        /// </example>
    }


}
