using System.Collections.Generic;

namespace Zealot.Common
{
    public enum InvitePvpReturnCode : byte
    {
        Ret_NotOnline = 0,
        Ret_AskToTarget = 1,
        Ret_IngToAsker = 2,
        Ret_YesToAsker = 3,
        Ret_NoToAsker = 4,
        Ret_InRealm = 5,
        Ret_NotInCity = 6
    }
}
