using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Common.Entities;
using Kopio.JsonContracts;
using Zealot.Repository;

public class UI_CharacterPowerUpButton : MonoBehaviour
{
    public void BTN_PowerUp()
    {
        PowerUp(UI_CharacterPowerup_Manager.CP_State);
        UI_CharacterPowerup_Manager.CharacterToggle.UpdateUI();
    }
    
    //消耗玩家材料，並升級 或 無法升級
    void PowerUp(int part)
    {
        if (UI_CharacterPowerup_Manager.haveEnoughMaterial && UI_CharacterPowerup_Manager.LevelCanPowerUp)
        {
            //升級回傳
            RPCFactory.NonCombatRPC.PowerUp(part);
        }
        else
        {
            UIManager.SystemMsgManager.ShowSystemMessage("材料或貨幣不夠強化，請玩家努力收集！", true);
            //OpenPowerUpItemStoreDialog();
        }
    }

    private void OpenPowerUpItemStoreDialog()
    {
        Debug.LogError("Not enough material, opening Item Store Dialog.");
        //UIManager.OpenDialog(WindowType.DialogItemStore);
    }




}
