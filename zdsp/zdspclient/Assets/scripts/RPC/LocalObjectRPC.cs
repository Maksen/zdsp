#define PROFILE_RPC

using System;
//#define clientrpc 0
using ExitGames.Client.Photon;
using UnityEngine;

using Zealot.Common;
using Zealot.Common.Entities;
using Zealot.Client;
using Zealot.Client.Entities;
using Zealot.Common.Datablock;

/// <summary>
/// CombatRPC .
/// </summary>
/// 

public class LocalObjectRPC : RPCBase
{
    public LocalObjectRPC() :
        base(typeof(LocalObjectRPC), OperationCode.LocalObject)
    {
    }

    public void OnLocalObject(object obj, EventData eventdata)
    {
#if PROFILE_RPC
        bytesReceivedThisFrame += ComputeDataSize(eventdata.Parameters);
#endif
        byte code = INIT_PCODE;
        ClientMain main = (ClientMain)obj;
        int cmdMode = (int)eventdata.Parameters[code++];
        //while (eventdata.Parameters.ContainsKey(code))
        while (eventdata.Parameters.ContainsKey(code) && code > 0)
        {
            // TODO: Think whether to use global shared localobject as entity (e.g Party, Guild, etc...) so that we can universally located by entitysystem
            // based on category, can determine the containerid (persistentid or sharedlocalobject id)
            byte locategory = (byte)eventdata.Parameters[code++];
            int containerid = (int)eventdata.Parameters[code++];
            LOTYPE objtype = (LOTYPE)eventdata.Parameters[code++];
            bool createnew = (bool)eventdata.Parameters[code++];
            if ((locategory == (byte)LOCATEGORY.EntitySyncStats) || (locategory == (byte)LOCATEGORY.LocalPlayerStats) || (locategory == (byte)LOCATEGORY.SharedStats))
                main.UpdateLocalObject(containerid, objtype, createnew, ref code, eventdata.Parameters);
        }        
    }
}
