using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using Zealot.Common;
using Zealot.Common.RPC;
using Zealot.Repository;

public partial class ClientMain : MonoBehaviour
{
    [RPCMethod(RPCCategory.NonCombat, (byte)ServerNonCombatRPCMethods.Ret_TransferServer)]
    public void Ret_TransferServer(int serverid, string serverAddress)
    {
        UIManager.StopHourglass();
        if (!string.IsNullOrEmpty(serverAddress))
        {
            UIManager.ShowLoadingScreen(true);
            GameInfo.TransferingServer = true;
            PhotonNetwork.networkingPeer.TransferGameServer(serverAddress);
        }
        else
        {
            UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("sys_ServerNotFound"));
        }
    }

    #region EquipmentUpgrade
    [RPCMethod(RPCCategory.NonCombat, (byte)ServerNonCombatRPCMethods.EquipmentUpgradeEquipmentFailed)]
    public void EquipmentUpgradeEquipmentFailed()
    {
        GameObject uiEquipUpgradeObj = UIManager.GetWindowGameObject(WindowType.EquipUpgrade);
        if(uiEquipUpgradeObj != null)
        {
            UI_EquipmentUpgrade uiEquipUpgrade = uiEquipUpgradeObj.GetComponent<UI_EquipmentUpgrade>();
            if(uiEquipUpgrade != null)
            {
                uiEquipUpgrade.PlayEquipmentUpgradeFailure();
            }
        }
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ServerNonCombatRPCMethods.EquipmentUpgradeEquipmentSuccess)]
    public void EquipmentUpgradeEquipmentSuccess()
    {
        GameObject uiEquipUpgradeObj = UIManager.GetWindowGameObject(WindowType.EquipUpgrade);
        if(uiEquipUpgradeObj != null)
        {
            UI_EquipmentUpgrade uiEquipUpgrade = uiEquipUpgradeObj.GetComponent<UI_EquipmentUpgrade>();
            if(uiEquipUpgrade != null)
            {
                uiEquipUpgrade.PlayEquipmentUpgradeSuccess();
            }
        }
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ServerNonCombatRPCMethods.EquipmentReformEquipmentFailed)]
    public void EquipmentReformEquipmentFailed()
    {

    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ServerNonCombatRPCMethods.EquipmentReformEquipmentSuccess)]
    public void EquipmentReformEquipmentSuccess()
    {

    }
    #endregion

    #region ReviveItem
    [RPCMethod(RPCCategory.NonCombat, (byte)ServerNonCombatRPCMethods.RequestReviveItem)]
    public void RequestReviveItem(string requestor, int requestId)
    {
        Dictionary<string, string> param = new Dictionary<string, string>();
        param.Add("requestor", requestor);
        UIManager.OpenYesNoDialog(GUILocalizationRepo.GetLocalizedString("reviveItem_RequestRevive", param), 
            () => { RPCFactory.NonCombatRPC.AcceptReviveItem(requestId); }, () => { RPCFactory.NonCombatRPC.RejectReviveItem(requestId); });
    }
    #endregion

    #region Quest
    [RPCMethod(RPCCategory.NonCombat, (byte)ServerNonCombatRPCMethods.Ret_DeleteQuest)]
    public void Ret_DeleteQuest(bool result, int questid)
    {
        UI_Quest quest = UIManager.GetWindowGameObject(WindowType.Quest).GetComponent<UI_Quest>();
        if (quest != null)
        {
            quest.OnDeleteQuest(result, questid);
        }
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ServerNonCombatRPCMethods.Ret_ResetQuest)]
    public void Ret_ResetQuest(bool result, int questid)
    {
        UI_Quest quest = UIManager.GetWindowGameObject(WindowType.Quest).GetComponent<UI_Quest>();
        if (quest != null)
        {
            quest.OnResetQuest(result, questid);
        }
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ServerNonCombatRPCMethods.Ret_CompleteQuest)]
    public void Ret_CompleteQuest(bool result, int questid)
    {
        if (GameInfo.gLocalPlayer != null)
        {
            GameInfo.gLocalPlayer.QuestController.QuestCompleted(questid, result);
        }
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ServerNonCombatRPCMethods.Ret_InteractAction)]
    public void Ret_InteractAction()
    {
        if (UIManager.IsWidgetActived(HUDWidgetType.QuestAction))
        {
            Hud_QuestAction questAction = UIManager.GetWidget(HUDWidgetType.QuestAction).GetComponent<Hud_QuestAction>();
            questAction.SetButtonStatus(true);
        }
    }
    #endregion

    #region CharacterInfo
    [RPCMethod(RPCCategory.NonCombat, (byte)ServerNonCombatRPCMethods.Ret_CharacterInfoSpendStatsPoints)]
    public void Ret_CharacterInfoSpendStatsPoints(int retVal)
    {
        GameObject obj = UIManager.GetWindowGameObject(WindowType.CharacterInfo);
        UI_CharacterInfo ci = obj.GetComponent<UI_CharacterInfo>();

        ci.mTabTwo.OnConfirmStatsAllocation_ServerFeedback(retVal);
    }
    #endregion

    #region NPCStore
    [RPCMethod(RPCCategory.NonCombat, (byte)ServerNonCombatRPCMethods.Ret_NPCStoreInit)]
    public void Ret_NPCStoreInit(string scString)
    {
        JsonSerializerSettings jsonSetting = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto, DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore, ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
        var store = JsonConvert.DeserializeObject<NPCStoreInfo>(scString, jsonSetting);

        if (GameInfo.gUIShopSell != null)
        {
            GameInfo.gUIShopSell.init(store.inventory, store.Type);
        }
    }
    
    [RPCMethod(RPCCategory.NonCombat, (byte)ServerNonCombatRPCMethods.Ret_NPCStoreGetPlayerTransactions)]
    public void Ret_NPCStoreGetPlayerTransactions(string scString)
    {
        JsonSerializerSettings jsonSetting = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto, DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore, ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
        var trans = JsonConvert.DeserializeObject<Dictionary<string, NPCStoreInfo.Transaction>>(scString, jsonSetting);

        if (GameInfo.gUIShopSell != null)
        {
            GameInfo.gUIShopSell.UpdateTransactions(trans);
        }
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ServerNonCombatRPCMethods.Ret_NPCStoreBuy)]
    public void Ret_NPCStoreBuy(string scString)
    {
        if (GameInfo.gUIShopSell != null)
        {
            GameInfo.gUIShopSell.SignalBuySuccess();
        }
    }
    #endregion

    #region Skill
    [RPCMethod(RPCCategory.NonCombat, (byte)ServerNonCombatRPCMethods.Ret_AddToSkillInventory)]
    public void Ret_AddToSkillInventory(byte result, int skillid, int skillpoint, int money)
    {
        GameObject obj = UIManager.GetWindowGameObject(WindowType.Skill);
        UI_SkillTree ui = obj.GetComponent<UI_SkillTree>();
        ui.OnEventSkillLevelUp(result, skillid, skillpoint, money);
    }
    #endregion
}
