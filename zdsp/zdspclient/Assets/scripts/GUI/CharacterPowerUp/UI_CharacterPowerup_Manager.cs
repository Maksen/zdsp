using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Client.Entities;

using Kopio.JsonContracts;
using Zealot.Repository;
using Zealot.Common;
using System.Linq;

public class UI_CharacterPowerup_Manager : MonoBehaviour
{
    public GameObject requiredItemDataPrefab;
    public Transform ItemRequirements_Parents; //Content_RequiredItems //requiredItemDataParent

    public PowerUpJson powerupData = new PowerUpJson();
    public PowerUpJson NextpowerupData = new PowerUpJson();

    public const int TopMaxPlayerLevel = 150;
    int NowPartLevel, NextPartLevel;

    [Space(10)]
    [Header("UI Element")]
    public Image IMG_PartIcon;
    public Text TXT_PartName;

    public Text TXT_NowLevel;  //部位等級/玩家等級
    public Text TXT_NexeLevel; //下個等級

    public Text TXT_Effect;    //屬性能力:串聯SideEffect表
    public Text TXT_NowValue;  //目前屬性數值
    public Text TXT_NextValue; //下個等級的屬性數值

    [Space(5)]
    public Animator AT_NoEnough;  //材料不足

    [Space(10)]
    [Header("Data")]
    public Sprite[] SP_PartIcon;

    public static bool haveEnoughMaterial;
    public static bool LevelCanPowerUp;

    public static UI_CharacterPartToggle CharacterToggle;
    public static UI_CharacterPowerup_Manager mManager;

    [Space(10)]
    public Button BTN_PowerUp;

    [Space(10)]
    public Transform GameIcon;

    public UI_Inventory CS_Inventory;
    PowerUpPartsType nowPartType;
    public static int nowPartTypeCount;

    public Toggle[] powerUpToggle;

    /*///淨空
    public void CleanItemRequirements()
    {
        int childCount = ItemRequirements_Parents.childCount;
        for (int x = 0; x < childCount; x++)
        {
            Destroy(ItemRequirements_Parents.GetChild(x).gameObject);
        }

    }

    ///設定Icon和部位名字
    public void ShowIconAndPartName(string PartName)
    {
        switch (PartName)
        {
            case "Helmet":
                IMG_PartIcon.sprite = SP_PartIcon[0];
                TXT_PartName.text = "頭部";
                break;
            case "Chest":
                IMG_PartIcon.sprite = SP_PartIcon[1];
                TXT_PartName.text = "身體";
                break;
            case "Wing":
                IMG_PartIcon.sprite = SP_PartIcon[2];
                TXT_PartName.text = "背部";
                break;
            case "Amulet":
                IMG_PartIcon.sprite = SP_PartIcon[3];
                TXT_PartName.text = "脖子";
                break;
            case "Weapon":
                IMG_PartIcon.sprite = SP_PartIcon[4];
                TXT_PartName.text = "慣用手";
                break;
            case "Boosts":
                IMG_PartIcon.sprite = SP_PartIcon[5];
                TXT_PartName.text = "腳部";
                break;
            case "Accs1":
                IMG_PartIcon.sprite = SP_PartIcon[6];
                TXT_PartName.text = "裝飾部位";
                break;
            case "Accs2":
                IMG_PartIcon.sprite = SP_PartIcon[7];
                TXT_PartName.text = "裝飾部位";
                break;
            case "Ring1":
                IMG_PartIcon.sprite = SP_PartIcon[8];
                TXT_PartName.text = "手指";
                break;
            case "Ring2":
                IMG_PartIcon.sprite = SP_PartIcon[9];
                TXT_PartName.text = "手指";
                break;
        }
    }

    ///設定部位等級/玩家等級
    public void Show_PowerUpInfo(string PartName)
    {
        PlayerGhost player = GameInfo.gLocalPlayer;
        PowerUpController powerupCtrler = player.clientPowerUpCtrl;

        switch (PartName)
        {
            case "Helmet":
                ViewPowerUp(0, PowerUpPartsType.Helmet, powerupCtrler);
                break;
            case "Chest":
                ViewPowerUp(1, PowerUpPartsType.Chest, powerupCtrler);
                break;
            case "Wing":
                ViewPowerUp(2, PowerUpPartsType.Wing, powerupCtrler);
                break;
            case "Amulet":
                ViewPowerUp(3, PowerUpPartsType.Amulet, powerupCtrler);
                break;
            case "Weapon":
                ViewPowerUp(4, PowerUpPartsType.Weapon, powerupCtrler);
                break;
            case "Boosts":
                ViewPowerUp(5, PowerUpPartsType.Boots, powerupCtrler);
                break;
            case "Accs1":
                ViewPowerUp(6, PowerUpPartsType.Accessory, powerupCtrler);
                break;
            case "Accs2":
                ViewPowerUp(7, PowerUpPartsType.Accessory, powerupCtrler);
                break;
            case "Ring1":
                ViewPowerUp(8, PowerUpPartsType.Ring, powerupCtrler);
                break;
            case "Ring2":
                ViewPowerUp(9, PowerUpPartsType.Ring, powerupCtrler);
                break;
        }

        //Set NextPartLevel And Judge if should be PowerUp

        if (NowPartLevel >= TopMaxPlayerLevel)
        {
            NextPartLevel = TopMaxPlayerLevel;
            LevelCanPowerUp = false;
        }
        else if (NowPartLevel > player.PlayerSynStats.Level && NowPartLevel < TopMaxPlayerLevel)
        {
            NextPartLevel = NowPartLevel;
            LevelCanPowerUp = false;
        }
        else
        {
            NextPartLevel = NowPartLevel;
            LevelCanPowerUp = true;
        }

        StringBuilder partsLevelStr = new StringBuilder();
        //partsLevelStr.Append(powerupCtrler.PowerUpInventory.powerUpSlots[CP_State]);
        //partsLevelStr.Append("/");
        //partsLevelStr.Append(player.PlayerSynStats.Level);
        partsLevelStr.AppendFormat("{0}/{1}", powerupCtrler.PowerUpInventory.powerUpSlots[CP_State], player.PlayerSynStats.Level);
        TXT_NowLevel.text = partsLevelStr.ToString();
        TXT_NexeLevel.text = NextPartLevel.ToString();
        //TXT_NowLevel.text = string.Format("{0}/{1}", powerupCtrler.PowerUpInventory.powerUpSlots[CP_State], player.PlayerSynStats.Level);

    }

    void ViewPowerUp (int partsId, PowerUpPartsType powerupTyoe, PowerUpController powerCtrl)
    {
        int level = powerCtrl.PowerUpInventory.powerUpSlots[partsId];
        level += 1;

        powerupData = PowerUpRepo.GetPowerUpByPartsLevel(powerupTyoe, level);
        if (powerupData != null)
        {
            TXT_NowValue.text = powerupData.value.ToString();
        }
        
        TXT_Effect.text = SideEffectRepo.GetSideEffect(powerupData.effect).localizedname.ToString();

        NextpowerupData = PowerUpRepo.GetPowerUpByPartsLevel(powerupTyoe, level + 1);
        if (powerupData != null && NextpowerupData != null)
        {
            TXT_NextValue.text = NextpowerupData.value.ToString();
        }
        NowPartLevel = level;
    }

    public static void CompareMaterial (Text colorText, int invAmount, int reqAmount)
    {
        if(invAmount >= reqAmount)
        {
            colorText.color = Color.white;
        } else
        {
            colorText.color = Color.red;
            haveEnoughMaterial = false;
        }
    }

    public void NotEnoughAnimator ()
    {
        if (haveEnoughMaterial == false)
        {
            AT_NoEnough.Play("inv_notenough");
        }
        else
        {
            AT_NoEnough.Play("inv_notenough_DEFAULT");
        }
    }

    void OnEnable()
    {
        CS_CharacterToggle.CS_MG_ToggleSelected(CP_State);
    }

    #region BTN Event
    public void GameIconSwitch(bool Open)
    {
        int iconCount = GameIcon.childCount;
        if (Open)
        {
            CS_Inventory.RefreshLeft(GameInfo.gLocalPlayer);
        }
        else
        {
            for (int i = 0; i < iconCount; ++i)
            {
                GameIcon.GetChild(i).transform.GetChild(2).gameObject.SetActive(false);
                GameIcon.GetChild(i).transform.GetChild(1).gameObject.SetActive(true);
            }
        }
    }

    #endregion*/
    string[] partLocalName = new string[] { "頭部", "身體", "背部", "脖子", "慣用手", "腳部", "裝飾部位", "裝飾部位", "手指", "手指" };

    void OnEnable()
    {
        mManager = this;
        for (int i = 0; i < powerUpToggle.Length; ++i)
        {
            int x = i;
            powerUpToggle[i].onValueChanged.AddListener(delegate {
                Init(x);
            });
        }
        Init(nowPartTypeCount);
    }

    public void Init(int part)
    {
        nowPartTypeCount = part;
        int childCount = ItemRequirements_Parents.childCount;
        for (int x = 0; x < childCount; x++)
        {
            Destroy(ItemRequirements_Parents.GetChild(x).gameObject);
        }

        LevelCanPowerUp = false;
        haveEnoughMaterial = false;

        RefreshPowerUpShow(part);
        InstantiatePartRequir(part);
    }

    public static void StaticInit()
    {
        mManager.Init(nowPartTypeCount);
    }

    void RefreshPowerUpShow(int part)
    {
        TXT_PartName.text = partLocalName[part];
        IMG_PartIcon.sprite = SP_PartIcon[part];
        PlayerGhost player = GameInfo.gLocalPlayer;
        int partLevel = player.clientPowerUpCtrl.PowerUpInventory.powerUpSlots[part];
        partLevel += 1;
        NowPartLevel = partLevel;
        NextPartLevel = NowPartLevel + 1;

        nowPartType = (PowerUpPartsType)part;
        powerupData = PowerUpRepo.GetPowerUpByPartsLevel(nowPartType, partLevel);
        if (powerupData != null)
        {
            TXT_NowValue.text = powerupData.value.ToString();
        }

        string effectName = SideEffectRepo.GetSideEffect(powerupData.effect).localizedname.ToString();
        if (effectName != null)
        {
            TXT_Effect.text = effectName;
        }

        powerupData = PowerUpRepo.GetPowerUpByPartsLevel(nowPartType, partLevel + 1);
        if (powerupData != null)
        {
            TXT_NextValue.text = powerupData.value.ToString();
        }

        if (NowPartLevel >= TopMaxPlayerLevel)
        {
            NextPartLevel = TopMaxPlayerLevel;
            LevelCanPowerUp = false;
        }
        else if (NowPartLevel + 1 > player.PlayerSynStats.Level && NextPartLevel < TopMaxPlayerLevel)
        {
            NextPartLevel = NowPartLevel + 1;
            LevelCanPowerUp = false;
        }
        else
        {
            NextPartLevel = NowPartLevel + 1;
            LevelCanPowerUp = true;
        }

        StringBuilder partsLevelStr = new StringBuilder();
        partsLevelStr.Append(NowPartLevel);
        partsLevelStr.Append("/");
        partsLevelStr.Append(player.PlayerSynStats.Level);
        TXT_NowLevel.text = partsLevelStr.ToString();
        TXT_NexeLevel.text = NextPartLevel.ToString();
    }

    void InstantiatePartRequir(int part)
    {
        PlayerGhost player = GameInfo.gLocalPlayer;
        powerupData = PowerUpRepo.GetPowerUpByPartsLevel(nowPartType, player.clientPowerUpCtrl.PowerUpInventory.powerUpSlots[part] + 1);

        if (powerupData != null)
        {
            int power = powerupData.power;
            SideEffectJson sideeffect = SideEffectRepo.GetSideEffect(powerupData.effect);
            haveEnoughMaterial = true;
            string RawMatDataString = powerupData.material;

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

                    GameObject reqITemDataObj = Instantiate(requiredItemDataPrefab);
                    reqITemDataObj.transform.SetParent(ItemRequirements_Parents, false);

                    if (player == null) { return; } //如果玩家還沒生成，return

                    int invAmount = player.clientItemInvCtrl.itemInvData.GetTotalStackCountByItemId((ushort)ItemId); //玩家擁有的道具
                    int money = player.SecondaryStats.money; //玩家擁有的金錢

                    RequiredItemData reqItemData = reqITemDataObj.GetComponent<RequiredItemData>(); // 
                    reqItemData.InitMaterial(ItemId, invAmount, ItemCount); //初始化Material
                }
                else
                { /*Debug.LogError("Don't have ItemId or ItemCount");*/ }
            }
        }
    }

    public static void CompareMaterial(Text colorText, int invAmount, int reqAmount)
    {
        if (invAmount >= reqAmount)
        {
            colorText.color = Color.white;
        }
        else
        {
            colorText.color = Color.red;
            haveEnoughMaterial = false;
        }
    }

    public void NotEnoughAnimator()
    {
        if (haveEnoughMaterial == false)
        {
            AT_NoEnough.Play("inv_notenough");
        }
        else
        {
            AT_NoEnough.Play("inv_notenough_DEFAULT");
        }
    }

    #region BTN Event
    public void GameIconSwitch(bool Open)
    {
        int iconCount = GameIcon.childCount;
        if (Open)
        {
            CS_Inventory.RefreshLeft(GameInfo.gLocalPlayer);
        }
        else
        {
            for (int i = 0; i < iconCount; ++i)
            {
                GameIcon.GetChild(i).transform.GetChild(2).gameObject.SetActive(false);
                GameIcon.GetChild(i).transform.GetChild(1).gameObject.SetActive(true);
            }
        }
    }
    public void CloseSwitch ()
    {
        powerUpToggle[nowPartTypeCount].isOn = false;
        powerUpToggle[0].isOn = true;
        nowPartTypeCount = 0;
        CS_Inventory.RefreshLeft(GameInfo.gLocalPlayer);
    }
    #endregion
}
