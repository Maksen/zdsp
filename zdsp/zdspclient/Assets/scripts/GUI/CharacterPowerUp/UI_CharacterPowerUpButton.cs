using UnityEngine;

public class UI_CharacterPowerUpButton : MonoBehaviour
{
    public void BTN_PowerUp()
    {
        PowerUp(UI_CharacterPowerup_Manager.nowPartTypeCount);
        UI_CharacterPowerup_Manager.StaticInit();
    }
    
    void PowerUp(int part)
    {
        if (UI_CharacterPowerup_Manager.haveEnoughMaterial && UI_CharacterPowerup_Manager.LevelCanPowerUp)
        {
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
