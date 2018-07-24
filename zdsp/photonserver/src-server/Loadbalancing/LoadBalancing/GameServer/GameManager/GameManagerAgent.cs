using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Photon.SocketServer;
using Photon.SocketServer.Rpc;
using Photon.LoadBalancing.MasterServer.GameManager;
using Photon.LoadBalancing.ServerToServer.Operations;
using Zealot.Common;
using Zealot.Repository;

namespace Photon.LoadBalancing.GameServer.GameManager
{
    //class GameManagerAgent
    //{
    //    private static volatile GameManagerAgent instance;
    //    private static object syncRoot = new Object();

    //    private GameApplication application;

    //    private GameManagerAgent(GameApplication application)
    //    {
    //        this.application = application;
    //    }

    //    private Dictionary<string, object> MarshalCharStats(CharacterData charData)
    //    {
    //        Dictionary<string, object> dctCharStats = new Dictionary<string, object>();
    //        string jobSect = JobSectRepo.GetJobById(charData.JobSect).name;
    //        int progressLevel = charData.ProgressLevel;
    //        int equipScore = charData.EquipScore;
    //        int health = charData.Health;          
    //        long experience = charData.Experience;         
    //        dctCharStats.Add("Job Sect", jobSect);
    //        dctCharStats.Add("Progress Level", progressLevel);
    //        dctCharStats.Add("Equip Score", equipScore);
    //        dctCharStats.Add("Health", health);           
    //        dctCharStats.Add("Experience", experience);
    //        return dctCharStats;
    //    }

    //    private Dictionary<string, object> MarshalItemInventory(ItemInventoryData itemInvData)
    //    {
    //        Dictionary<string, object> dctItemInv = new Dictionary<string, object>();

    //        string strHeader = "Name";

    //        List<Dictionary<string, object>> lstDctItem = new List<Dictionary<string, object>>();

    //        List<IInventoryItem> lstItemInv = itemInvData.Slots;

    //        foreach (IInventoryItem item in lstItemInv)
    //        {
    //            if (item != null)
    //            {
    //                Dictionary<string, object> dctItem = new Dictionary<string, object>();

    //                bool bound = item.Bound;
    //                string name = item.Name;
    //                int id = (int)item.ItemID;
    //                int stackCount = (int)item.StackCount;
    //                int maxStackCount = (int)item.MaxStackCount;
    //                string type = Enum.GetName(typeof(ItemKind), item.itemkind);
    //                string rarity = Enum.GetName(typeof(ItemRarity), item.ItemRarity);

    //                dctItem.Add("Name", name);
    //                dctItem.Add("ID", id);
    //                dctItem.Add("Stack Count", stackCount);
    //                dctItem.Add("Max Stack Count", maxStackCount);
    //                dctItem.Add("Bound", bound);
    //                dctItem.Add("Type", type);
    //                dctItem.Add("Rarity", rarity);

    //                lstDctItem.Add(dctItem);
    //            }
    //            else
    //            {
    //                lstDctItem.Add(null);
    //            }
    //        }

    //        dctItemInv.Add("ItemInvHeader", strHeader);
    //        dctItemInv.Add("ItemInvList", lstDctItem);

    //        return dctItemInv;
    //    }

    //    private Dictionary<string, object> MarshalEquippedInventory(EquippedInventoryData equippedInvData)
    //    {
    //        Dictionary<string, object> dctEquippedInv = new Dictionary<string, object>();

    //        List<IInventoryItem> lstEquippedInv = new List<IInventoryItem>();
    //        List<string> lstHeader = new List<string>();

    //        foreach (EquipmentSlot slotType in Enum.GetValues(typeof(EquipmentSlot)))
    //        {
    //            if (slotType != EquipmentSlot.MAXSLOTS)
    //            {
    //                //IInventoryItem equippedItem = equippedInvData.GetItemBySlotId((int)slotType);

    //                //if (equippedItem != null)
    //                //{
    //                //    Dictionary<string, object> dctItem = new Dictionary<string, object>();

    //                //    bool bound = equippedItem.Bound;
    //                //    string name = equippedItem.Name;
    //                //    int id = (int)equippedItem.ItemID;
    //                //    int stackCount = (int)equippedItem.StackCount;
    //                //    int maxStackCount = (int)equippedItem.MaxStackCount;
    //                //    string type = Enum.GetName(typeof(ItemKind), equippedItem.itemkind);
    //                //    string rarity = Enum.GetName(typeof(ItemRarity), equippedItem.ItemRarity);
    //                //    string sellCurrType = Enum.GetName(typeof(CurrencyType), equippedItem.SellCurrencyType);

    //                //    dctItem.Add("Name", name);
    //                //    dctItem.Add("ID", id);
    //                //    dctItem.Add("Stack Count", stackCount);
    //                //    dctItem.Add("Max Stack Count", maxStackCount);
    //                //    dctItem.Add("Bound", bound);
    //                //    dctItem.Add("Type", type);
    //                //    dctItem.Add("Rarity", rarity);
    //                //    dctItem.Add("Selling Currency Type", sellCurrType);

    //                //    lstEquippedInv.Add(equippedItem);
    //                //}
    //                //else
    //                //{
    //                //    lstEquippedInv.Add(null);
    //                //}

    //                //string slotName = Enum.GetName(typeof(EquipmentSlot), slotType);
    //                //lstHeader.Add(slotName);
    //            }
    //        }

    //        dctEquippedInv.Add("EquippedInvHeader", lstHeader);
    //        dctEquippedInv.Add("EquippedInvList", lstEquippedInv);

    //        return dctEquippedInv;
    //    }

    //    private Dictionary<string, object> MarshalCurrencyInventory(CurrencyInventoryData currencyInvData)
    //    {
    //        Dictionary<string, object> dctCurrencyInv = new Dictionary<string, object>();

    //        int gold = currencyInvData.Gold;
    //        int bindGold = currencyInvData.BindGold;
    //        int lotteryPoint = currencyInvData.LotteryPoints;
    //        int honor = currencyInvData.Honor;
    //        dctCurrencyInv.Add("Gold", gold);
    //        dctCurrencyInv.Add("Bind Gold", bindGold);
    //        dctCurrencyInv.Add("Lottery Point", lotteryPoint);
    //        dctCurrencyInv.Add("Honor", honor);

    //        return dctCurrencyInv;
    //    }

    //    public static GameManagerAgent Instance(GameApplication application)
    //    {
    //        if (instance == null)
    //        {
    //            lock (syncRoot)
    //            {
    //                if (instance == null)
    //                {
    //                    instance = new GameManagerAgent(application);
    //                }
    //            }
    //        }

    //        return instance;
    //    }

    //    public void HandleOperationRequest(OperationRequest opRequest, SendParameters sendParam)
    //    {
    //        lock (syncRoot)
    //        {
    //            OutgoingMasterServerPeer masterPeer = application.MasterPeer;

    //            Dictionary<byte, object> opParam = opRequest.Parameters;
    //            byte opCode = (byte)opParam[GameManagerOpCode.InterServerOpCode];

    //            switch (opCode)
    //            {
    //                case GameManagerOpCode.Char_Req:
    //                    OpCharReq opCharReq = new OpCharReq(masterPeer, opRequest);

    //                    opCharReq.ProcessOperationResponseAsync();

    //                    break;
    //                case GameManagerOpCode.CharAddItem_Req:
    //                    OpCharAddItemReq opCharAddItemReq = new OpCharAddItemReq(masterPeer, opRequest);

    //                    opCharAddItemReq.ProcessOperationResponseAsync();

    //                    break;
    //                default:
    //                    break;
    //            }
    //        }
    //    }

    //    #region OpCharReq
    //    private class OpCharReq : Operation
    //    {
    //        OutgoingMasterServerPeer masterPeer;

    //        public OpCharReq(OutgoingMasterServerPeer masterPeer, OperationRequest opRequest)
    //            : base(masterPeer.Protocol, opRequest)
    //        {
    //            this.masterPeer = masterPeer;
    //        }

    //        public async void ProcessOperationResponseAsync()
    //        {
    //            Dictionary<byte, object> opParam = this.OperationRequest.Parameters;
    //            string serverId = (string)opParam[GameManagerOpCode.ServerId];
    //            string charName = (string)opParam[GameManagerOpCode.Char_Param_Name];

    //            Task<Dictionary<byte, object>> taskCharResponse = GetCharResponseAsync(charName);

    //            Dictionary<byte, object> dctCharResponse = await taskCharResponse;

    //            Dictionary<byte, object> dctParam = new Dictionary<byte, object>();
    //            dctParam.Add(GameManagerOpCode.ServerId, serverId);
    //            dctParam.Add(GameManagerOpCode.Char_Param_Name, charName);
    //            dctParam.Add(GameManagerOpCode.Char_Param_Data, dctCharResponse[GameManagerOpCode.Char_Param_Data]);
    //            dctParam.Add(GameManagerOpCode.ResponseOpCode, dctCharResponse[GameManagerOpCode.ResponseOpCode]);
    //            dctParam.Add(GameManagerOpCode.InterServerOpCode, GameManagerOpCode.Char_Res);
                
    //            OperationResponse opResponse = new OperationResponse
    //            {
    //                OperationCode = OperationCode.ResponseToGameManager,
    //                ReturnCode = 0,
    //                DebugMessage = "Char_Res",
    //                Parameters = dctParam
    //            };

    //            SendParameters sendParam = new SendParameters();

    //            masterPeer.SendOperationResponse(opResponse, sendParam);
    //        }

    //        private async Task<Dictionary<byte, object>> GetCharResponseAsync(string charName)
    //        {
    //            Dictionary<byte, object> dctCharResponse = new Dictionary<byte, object>();

    //            Task<Dictionary<string, object>> taskDctRepoChar = GameApplication.dbRepository.Character.GetByNameAsync(charName);

    //            Dictionary<string, object> dctRepoChar = await taskDctRepoChar;

    //            if (dctRepoChar.Count == 0)
    //            {
    //                dctCharResponse.Add(GameManagerOpCode.ResponseOpCode, GameManagerOpCode.Char_Res_NotFound);
    //                dctCharResponse.Add(GameManagerOpCode.Char_Param_Data, null);

    //                return dctCharResponse;
    //            }

    //            CharacterData charData = CharacterData.DeserializeFromDB((string)dctRepoChar["characterdata"]);

    //            ItemInventoryData itemInvData = charData.ItemInventory;
    //            EquippedInventoryData equippedInvData = charData.EquippedInventory;
    //            CurrencyInventoryData currencyInvData = charData.CurrencyInventory;

    //            Dictionary<string, object> dctCharStats = GameManagerAgent.instance.MarshalCharStats(charData);
    //            Dictionary<string, object> lstDctItemInv = GameManagerAgent.instance.MarshalItemInventory(itemInvData);
    //            Dictionary<string, object> dctEquippedInv = GameManagerAgent.instance.MarshalEquippedInventory(equippedInvData);
    //            Dictionary<string, object> dctCurrencyInv = GameManagerAgent.instance.MarshalCurrencyInventory(currencyInvData);

    //            Dictionary<string, object> dctChar = new Dictionary<string, object>();

    //            dctChar.Add("CharStats", dctCharStats);
    //            dctChar.Add("ItemInventory", lstDctItemInv);
    //            dctChar.Add("EquippedInventory", dctEquippedInv);
    //            dctChar.Add("CurrencyInventory", dctCurrencyInv);

    //            dctCharResponse.Add(GameManagerOpCode.ResponseOpCode, GameManagerOpCode.Char_Res_OK);
    //            dctCharResponse.Add(GameManagerOpCode.Char_Param_Data, dctChar);

    //            return dctCharResponse;
    //        }
    //    }
    //    #endregion OpCharReq

    //    #region OpCharAddItemReq
    //    private class OpCharAddItemReq : Operation
    //    {
    //        OutgoingMasterServerPeer masterPeer;

    //        public OpCharAddItemReq(OutgoingMasterServerPeer masterPeer, OperationRequest opRequest)
    //            : base(masterPeer.Protocol, opRequest)
    //        {
    //            this.masterPeer = masterPeer;
    //        }

    //        public async void ProcessOperationResponseAsync()
    //        {
    //            Dictionary<byte, object> opParam = this.OperationRequest.Parameters;
    //            string serverId = (string)opParam[GameManagerOpCode.ServerId];
    //            string charName = (string)opParam[GameManagerOpCode.CharAddItem_Param_Name];
    //            bool bound = (bool)opParam[GameManagerOpCode.CharAddItem_Param_Bound];
    //            int itemId = (int)opParam[GameManagerOpCode.CharAddItem_Param_ItemId];
    //            int itemIndex = (int)opParam[GameManagerOpCode.CharAddItem_Param_ItemIndex];
    //            ushort stackCount = (ushort)(int)opParam[GameManagerOpCode.CharAddItem_Param_StackCount];

    //            Task<Dictionary<byte, object>> taskCharAddItemResponse = GetCharAddItemResponseAsync(charName, bound, itemId, itemIndex, stackCount);

    //            Dictionary<byte, object> dctCharAddItemResponse = await taskCharAddItemResponse;

    //            Dictionary<byte, object> dctParam = new Dictionary<byte, object>();
    //            dctParam.Add(GameManagerOpCode.ServerId, serverId);
    //            dctParam.Add(GameManagerOpCode.CharAddItem_Param_Name, charName);
    //            dctParam.Add(GameManagerOpCode.CharAddItem_Param_Bound, bound);
    //            dctParam.Add(GameManagerOpCode.CharAddItem_Param_ItemId, itemId);
    //            dctParam.Add(GameManagerOpCode.CharAddItem_Param_ItemIndex, itemIndex);
    //            dctParam.Add(GameManagerOpCode.CharAddItem_Param_StackCount, (int)stackCount);
    //            dctParam.Add(GameManagerOpCode.CharAddItem_Param_Data, dctCharAddItemResponse[GameManagerOpCode.CharAddItem_Param_Data]);
    //            dctParam.Add(GameManagerOpCode.ResponseOpCode, dctCharAddItemResponse[GameManagerOpCode.ResponseOpCode]);
    //            dctParam.Add(GameManagerOpCode.InterServerOpCode, GameManagerOpCode.CharAddItem_Res);

    //            OperationResponse opResponse = new OperationResponse
    //            {
    //                OperationCode = OperationCode.ResponseToGameManager,
    //                ReturnCode = 0,
    //                DebugMessage = "ResCharAddItem",
    //                Parameters = dctParam
    //            };

    //            SendParameters sendParam = new SendParameters();

    //            masterPeer.SendOperationResponse(opResponse, sendParam);
    //        }

    //        private async Task<Dictionary<byte, object>> GetCharAddItemResponseAsync(string charName, bool bound, int itemId, int itemIndex, ushort stackCount)
    //        {
    //            Dictionary<byte, object> dctCharAddItemResponse = new Dictionary<byte, object>();

    //            if (GameRepo.ItemFactory.GetItemById(itemId) == null)
    //            {
    //                dctCharAddItemResponse.Add(GameManagerOpCode.ResponseOpCode, GameManagerOpCode.CharAddItem_Res_ItemIdNotFound);
    //                dctCharAddItemResponse.Add(GameManagerOpCode.CharAddItem_Param_Data, null);

    //                return dctCharAddItemResponse;
    //            }

    //            Task<Dictionary<string, object>> taskDctRepoChar = GameApplication.dbRepository.Character.GetByNameAsync(charName);

    //            Dictionary<string, object> dctRepoChar = await taskDctRepoChar;

    //            if (dctRepoChar.Count == 0)
    //            {
    //                dctCharAddItemResponse.Add(GameManagerOpCode.ResponseOpCode, GameManagerOpCode.CharAddItem_Res_CharNotFound);
    //                dctCharAddItemResponse.Add(GameManagerOpCode.CharAddItem_Param_Data, null);

    //                return dctCharAddItemResponse;
    //            }

    //            CharacterData charData = CharacterData.DeserializeFromDB((string)dctRepoChar["characterdata"]);

    //            ItemInventoryData itemInvData = charData.ItemInventory;
    //            EquippedInventoryData equippedInvData = charData.EquippedInventory;
    //            CurrencyInventoryData currencyInvData = charData.CurrencyInventory;

    //            IInventoryItem item = GameRepo.ItemFactory.CreateItemInstance(itemId);
    //            item.Bound = bound;
    //            item.StackCount = stackCount;

    //            itemInvData.SetSlotItem(itemIndex, item);

    //            //string charId = (string)dctRepoChar["charid"];
    //            //string serializedData = charData.SerializeForDB();
    //            //Task<bool> taskSaveChar = GameApplication.dbRepository.Character.SaveCharacter(charId, serializedData);

    //            //bool saveChar = await taskSaveChar;
                
    //            Dictionary<string, object> dctCharStats = GameManagerAgent.instance.MarshalCharStats(charData);
    //            Dictionary<string, object> lstDctItemInv = GameManagerAgent.instance.MarshalItemInventory(itemInvData);
    //            Dictionary<string, object> dctEquippedInv = GameManagerAgent.instance.MarshalEquippedInventory(equippedInvData);
    //            Dictionary<string, object> dctCurrencyInv = GameManagerAgent.instance.MarshalCurrencyInventory(currencyInvData);

    //            Dictionary<string, object> dctChar = new Dictionary<string, object>();

    //            dctChar.Add("CharStats", dctCharStats);
    //            dctChar.Add("ItemInventory", lstDctItemInv);
    //            dctChar.Add("EquippedInventory", dctEquippedInv);
    //            dctChar.Add("CurrencyInventory", dctCurrencyInv);

    //            dctCharAddItemResponse.Add(GameManagerOpCode.ResponseOpCode, GameManagerOpCode.CharAddItem_Res_OK);
    //            dctCharAddItemResponse.Add(GameManagerOpCode.CharAddItem_Param_Data, dctChar);

    //            return dctCharAddItemResponse;
    //        }
    //    }
    //    #endregion OpCharAddItemReq
    //}
}
