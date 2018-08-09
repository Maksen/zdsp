using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zealot.Common;
using Zealot.Repository;
using Kopio.JsonContracts;
using System.Linq;
using Zealot.Client.Entities;
using UnityEngine.UI;

public class UI_CharacterPartToggle : MonoBehaviour {

    public UI_CharacterPowerup_Manager cs_UI_CPM;
    public PowerUpJson powerupData;

    public enum CharacterPart //強化部位
    {
        Helmet, //頭盔、頭飾
        Chest,  //胸膛
        Wing,   //背飾
        Amulet, //護身符
        Weapon, //武器
        Boosts, //鞋子
        Accs1,  //裝飾部位1
        Accs2,  //裝飾部位2
        Ring1,  //戒指1
        Ring2   //戒指2
    }
    [Header("部位")]
    public CharacterPart _CharacterPart;


    public void Start()
    {

    }

    ///For Toggle
    ///依據部位判斷，選擇的部位被選取/取消選取時，執行函式
    public void ToggleSelected(bool IsSelected)
    {
        if (IsSelected)
        {
            cs_UI_CPM.BTN_PowerUp.interactable = true;
            cs_UI_CPM.CleanItemRequirements();      //清除所有所需物件
            GetThisPartRequireInfo(_CharacterPart); //讀取資料
            UpdateUI(); //更新UI
        }
    }

    public void CS_MG_ToggleSelected(int partStats)
    {
        cs_UI_CPM.BTN_PowerUp.interactable = true;
        cs_UI_CPM.CleanItemRequirements();      //清除所有所需物件
        CharacterPart nowPart = CharacterPart.Helmet;
        switch (partStats)
        {
            case 0:
                nowPart = CharacterPart.Helmet;
                break;
            case 1:
                nowPart = CharacterPart.Chest;
                break;
            case 2:
                nowPart = CharacterPart.Wing;
                break;
            case 3:
                nowPart = CharacterPart.Amulet;
                break;
            case 4:
                nowPart = CharacterPart.Weapon;
                break;
            case 5:
                nowPart = CharacterPart.Boosts;
                break;
            case 6:
                nowPart = CharacterPart.Accs1;
                break;
            case 7:
                nowPart = CharacterPart.Accs2;
                break;
            case 8:
                nowPart = CharacterPart.Ring1;
                break;
            case 9:
                nowPart = CharacterPart.Ring2;
                break;
        }
        GetThisPartRequireInfo(nowPart); //暫定thisToggle._CharacterPart
        UpdateUI(); //更新UI
    }

    ///Get The PowerUp Need & Player Data取得升級此部位所需的資料
    void GetThisPartRequireInfo(CharacterPart ThisPart)
    {
        PlayerGhost player = GameInfo.gLocalPlayer;
        switch (ThisPart)
        {
            case CharacterPart.Helmet:
                powerupData = PowerUpRepo.GetPowerUpByPartsLevel(PowerUpPartsType.Helmet, player.clientPowerUpCtrl.PowerUpInventory.powerUpSlots[0] + 1);
                UI_CharacterPowerup_Manager.CP_State = 0;
                break;
            case CharacterPart.Chest:
                powerupData = PowerUpRepo.GetPowerUpByPartsLevel(PowerUpPartsType.Chest, player.clientPowerUpCtrl.PowerUpInventory.powerUpSlots[1] + 1);
                UI_CharacterPowerup_Manager.CP_State = 1;
                break;
            case CharacterPart.Wing:
                powerupData = PowerUpRepo.GetPowerUpByPartsLevel(PowerUpPartsType.Wing, player.clientPowerUpCtrl.PowerUpInventory.powerUpSlots[2] + 1);
                UI_CharacterPowerup_Manager.CP_State = 2;
                break;
            case CharacterPart.Amulet:
                powerupData = PowerUpRepo.GetPowerUpByPartsLevel(PowerUpPartsType.Amulet, player.clientPowerUpCtrl.PowerUpInventory.powerUpSlots[3] + 1);
                UI_CharacterPowerup_Manager.CP_State = 3;
                break;
            case CharacterPart.Weapon:
                powerupData = PowerUpRepo.GetPowerUpByPartsLevel(PowerUpPartsType.Weapon, player.clientPowerUpCtrl.PowerUpInventory.powerUpSlots[4] + 1);
                UI_CharacterPowerup_Manager.CP_State = 4;
                break;
            case CharacterPart.Boosts:
                powerupData = PowerUpRepo.GetPowerUpByPartsLevel(PowerUpPartsType.Boots, player.clientPowerUpCtrl.PowerUpInventory.powerUpSlots[5] + 1);
                UI_CharacterPowerup_Manager.CP_State = 5;
                break;
            case CharacterPart.Accs1:
                powerupData = PowerUpRepo.GetPowerUpByPartsLevel(PowerUpPartsType.Accessory, player.clientPowerUpCtrl.PowerUpInventory.powerUpSlots[6] + 1);
                UI_CharacterPowerup_Manager.CP_State = 6;
                break;
            case CharacterPart.Accs2:
                powerupData = PowerUpRepo.GetPowerUpByPartsLevel(PowerUpPartsType.Accessory, player.clientPowerUpCtrl.PowerUpInventory.powerUpSlots[7] + 1);
                UI_CharacterPowerup_Manager.CP_State = 7;
                break;
            case CharacterPart.Ring1:
                powerupData = PowerUpRepo.GetPowerUpByPartsLevel(PowerUpPartsType.Ring, player.clientPowerUpCtrl.PowerUpInventory.powerUpSlots[8] + 1);
                UI_CharacterPowerup_Manager.CP_State = 8;
                break;
            case CharacterPart.Ring2:
                powerupData = PowerUpRepo.GetPowerUpByPartsLevel(PowerUpPartsType.Ring, player.clientPowerUpCtrl.PowerUpInventory.powerUpSlots[9] + 1);
                UI_CharacterPowerup_Manager.CP_State = 9;
                break;
        }

        if (powerupData != null)
        {
            int power = powerupData.power;
            SideEffectJson sideeffect = SideEffectRepo.GetSideEffect(powerupData.effect);
            int value = powerupData.value; 
            CurrencyType currType = powerupData.currency; 
            int cost = powerupData.cost;
            UI_CharacterPowerup_Manager.haveEnoughMaterial = true;

            //Debug.Log("power: " + power +" sideeffect: " + sideeffect +" value: " + value +" currType: " + currType + " cost:" + cost  );

            string RawMatDataString = powerupData.material; //消耗材料 ItemID|Count;ItemID|Count;ItemID|Count;ItemID|Count;ItemID|Count
            

            List<string> Split_List = RawMatDataString.Split(';').ToList();
            for (int i = 0; i < Split_List.Count; i++)
            {
                List<string> EndSplit_List = Split_List[i].Split('|').ToList();

                int ItemId = 0;
                int ItemCount = 0;

                //若兩個String皆可轉換成Int
                if (int.TryParse(EndSplit_List[0], out ItemId) && int.TryParse(EndSplit_List[1], out ItemCount))
                {
                    /*Debug.Log("有ItemId" + EndSplit_List[0]);
                    Debug.Log("有ItemCount" + EndSplit_List[1]);*/

                    GameObject reqITemDataObj = Instantiate(cs_UI_CPM.requiredItemDataPrefab);
                    reqITemDataObj.transform.SetParent(cs_UI_CPM.ItemRequirements_Parents, false);

                    if (player == null) { return; } //如果玩家還沒生成，return

                    int invAmount = player.clientItemInvCtrl.itemInvData.GetTotalStackCountByItemId((ushort)ItemId); //玩家擁有的道具
                    int money = player.SecondaryStats.money; //玩家擁有的金錢

                    RequiredItemData reqItemData = reqITemDataObj.GetComponent<RequiredItemData>(); // 
                    reqItemData.InitMaterial(ItemId, invAmount, ItemCount); //初始化Material
                }
                else 
                { /*Debug.LogError("Don't have ItemId or ItemCount");*/ }
            } //end for
        }
        else
        {
            Debug.LogError("無法抓取資料");
        }
    }

    //依照所有的材料設定UI顯示，Also use for toggle
    public void UpdateUI()
    {
        UI_CharacterPowerup_Manager.CharacterToggle = this;
        cs_UI_CPM.ShowIconAndPartName(_CharacterPart.ToString()); //顯示ICON &部位名稱
        cs_UI_CPM.Show_PowerUpInfo(_CharacterPart.ToString());    //顯示部位等級/玩家等級 + 下個等級、Effect、Value
        cs_UI_CPM.NotEnoughAnimator(); //顯示材料是否充足
    }

}
