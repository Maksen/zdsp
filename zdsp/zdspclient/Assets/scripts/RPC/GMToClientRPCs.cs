using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Zealot.Common.RPC;

public partial class ClientMain
{
    [RPCMethod(RPCCategory.NonCombat, (byte)ServerNonCombatRPCMethods.KickWithReason)]
    public void KickWithReason(string reason)
    {
        //UIManager.OpenOkDialog(reason,
                             //  PhotonNetwork.Disconnect);
        //Login.GMMessages.Add(reason);
    }    

    [RPCMethod(RPCCategory.NonCombat, (byte)ServerNonCombatRPCMethods.GMMessage)]
    public void GMMessage(string message, int mode)
    {
        //UIManager.OpenOkDialog(message, null);
        if(mode == 1)
            Login.GMMessages.Add(message);
        //else
            //UIManager.OpenOkDialog(message, null);
    }
}
