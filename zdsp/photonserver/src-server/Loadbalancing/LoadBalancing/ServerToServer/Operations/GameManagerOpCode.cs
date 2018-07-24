namespace Photon.LoadBalancing.ServerToServer.Operations
{
    public class GameManagerOpCode
    {
        public const byte ServerId = 0;
        public const byte SessionToken = 1;
        public const byte ResponseOpCode = 2;
        public const byte InterServerOpCode = 3;
        
        public const byte Servers_Req = 10;
        public const byte Servers_Res = 11;
        public const byte Servers_Res_OK = 12;
        public const byte Servers_Res_NotFound = 13;
        public const byte Servers_Param_Data = 14;        

        public const byte Char_Req = 20;
        public const byte Char_Res = 21;
        public const byte Char_Res_OK = 22;
        public const byte Char_Res_NotFound = 23;
        public const byte Char_Param_Name = 24;
        public const byte Char_Param_Data = 25;
        
        public const byte CharAddItem_Req = 40;
        public const byte CharAddItem_Res = 41;
        public const byte CharAddItem_Res_OK = 42;
        public const byte CharAddItem_Res_CharNotFound = 43;
        public const byte CharAddItem_Res_ItemIdNotFound = 44;
        public const byte CharAddItem_Param_Name = 45;
        public const byte CharAddItem_Param_Data = 46;
        public const byte CharAddItem_Param_Bound = 47;
        public const byte CharAddItem_Param_ItemId = 48;
        public const byte CharAddItem_Param_ItemIndex = 49;
        public const byte CharAddItem_Param_StackCount = 50;
    }
}