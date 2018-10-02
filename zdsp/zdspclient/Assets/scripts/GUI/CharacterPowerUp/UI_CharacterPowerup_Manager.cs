using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Client.Entities;
using Kopio.JsonContracts;
using Zealot.Repository;
using Zealot.Common;

public class UI_CharacterPowerup_Manager : MonoBehaviour
{
    [SerializeField]
    private GameObject requiredItemDataPrefab;
    [SerializeField]
    private Transform ItemRequirements_Parents;

    PowerUpJson powerupData = new PowerUpJson();
    PowerUpJson NextpowerupData = new PowerUpJson();

    const int TopMaxPlayerLevel = 150;
    int NowPartLevel, NextPartLevel;

    [Space(10)]
    [Header("UI Element")]

    [SerializeField]
    private Image IMG_PartIcon;
    [SerializeField]
    private Text TXT_PartName;

    [SerializeField]
    private Text TXT_NowLevel;
    [SerializeField]
    private Text TXT_NexeLevel;

    [SerializeField]
    private Text TXT_Effect;
    [SerializeField]
    private Text TXT_NowValue;
    [SerializeField]
    private Text TXT_NextValue;

    [Space(5)]
    [SerializeField]
    private Animator AT_NoEnough;

    [Space(10)]
    [Header("Data")]
    [SerializeField]
    private Sprite[] SP_PartIcon;

    public static bool haveEnoughMaterial;
    public static bool LevelCanPowerUp;

    public static UI_CharacterPartToggle CharacterToggle;
    public static UI_CharacterPowerup_Manager mManager;

    [Space(10)]
    [SerializeField]
    private Button BTN_PowerUp;

    [Space(10)]
    [SerializeField]
    private Transform GameIcon;

    [SerializeField]
    private UI_Inventory CS_Inventory;
    PowerUpPartsType nowPartType;
    public static int nowPartTypeCount;

    [SerializeField]
    private Transform[] selectPartButtonParents;
    [SerializeField]
    private Toggle[] powerUpToggle;
    
    private List<GameIcon_Equip> equipIconData = new List<GameIcon_Equip>();

    PlayerGhost player;
    string[] partLocalName = new string[] { "頭部", "身體", "背部", "脖子", "慣用手", "腳部", "裝飾部位", "裝飾部位", "手指", "手指" };

    #region BasicSetting
    void Awake()
    {
        for (int i = 0; i < selectPartButtonParents.Length; ++i)
        {
            equipIconData.Add(selectPartButtonParents[i].Find("GameIcon_Equip").GetComponent<GameIcon_Equip>());
        }
    }

    void OnEnable()
    {
        mManager = this;
        player = GameInfo.gLocalPlayer;
        for (int i = 0; i < equipIconData.Count; ++i)
        {
            int index = i;
            equipIconData[index].SetEquipIconClickCallback(() => Init(index));
        }
        Init(nowPartTypeCount);
    }
    #endregion

    #region Refresh
    public void Init(int part)
    {
        powerUpToggle[nowPartTypeCount].isOn = false;
        nowPartTypeCount = part;
        powerUpToggle[nowPartTypeCount].isOn = true;
        ClientUtils.DestroyChildren(ItemRequirements_Parents);
        LevelCanPowerUp = false;
        haveEnoughMaterial = false;

        RefreshPowerUpShow(part);
        InstantiatePartRequir(part);
    }

    public static void StaticInit()
    {
        mManager.Init(nowPartTypeCount);
    }
    #endregion

    #region ShowInUI

    void RefreshPowerUpShow(int part)
    {
        TXT_PartName.text = partLocalName[part];
        IMG_PartIcon.sprite = SP_PartIcon[part];
        int partLevel = player.clientPowerUpCtrl.PowerUpInventory.powerUpSlots[part];
        NowPartLevel = partLevel;
        NextPartLevel = NowPartLevel + 1;
        
        nowPartType = (PowerUpPartsType)part;
        powerupData = PowerUpRepo.GetPowerUpByPartsLevel(nowPartType, partLevel);
        TXT_Effect.text = SideEffectRepo.GetSideEffect(powerupData.effect).localizedname ?? string.Empty;
        TXT_NowValue.text = PowerUpRepo.GetPowerUpByPartsLevel(nowPartType, partLevel).value.ToString() ?? string.Empty;
        TXT_NextValue.text = PowerUpRepo.GetPowerUpByPartsLevel(nowPartType, NextPartLevel).value.ToString() ?? string.Empty;

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
        powerupData = PowerUpRepo.GetPowerUpByPartsLevel(nowPartType, player.clientPowerUpCtrl.PowerUpInventory.powerUpSlots[part]);

        if (powerupData != null)
        {
            int power = powerupData.power;
            SideEffectJson sideeffect = SideEffectRepo.GetSideEffect(powerupData.effect);
            haveEnoughMaterial = true;
            string RawMatDataString = powerupData.material;
            List<ItemInfo> Split_List = PowerUpUtilities.ConvertMaterialFormat(RawMatDataString);

            for (int i = 0; i < Split_List.Count; ++i)
            {
                GameObject reqItemDataObj = Instantiate(requiredItemDataPrefab);
                reqItemDataObj.transform.SetParent(ItemRequirements_Parents, false);

                if (player == null) { return; }

                int invAmount = player.clientItemInvCtrl.itemInvData.GetTotalStackCountByItemId(Split_List[i].itemId);

                RequiredItemData reqItemData = reqItemDataObj.GetComponent<RequiredItemData>();
                reqItemData.InitMaterial(Split_List[i].itemId, invAmount, Split_List[i].stackCount);
            }
            
            int playerCurrency = player.SecondaryStats.Money;
            int requireCurrency = powerupData.cost;

            GameObject reqCurrencyObj = ClientUtils.CreateChild(ItemRequirements_Parents, requiredItemDataPrefab);
            RequiredItemData reqCurrency = reqCurrencyObj.GetComponent<RequiredItemData>();

            reqCurrency.InitCurrency(CurrencyType.Money, playerCurrency, requireCurrency);
        }
    }
    #endregion

    #region CompareConsume
    public void NotEnoughAnimator()
    {
        AT_NoEnough.Play((haveEnoughMaterial == true) ? "inv_notenough_DEFAULT" : "inv_notenough");
    }
    #endregion

    #region ClickEvent
    public void GameIconSwitch(bool Open)
    {
        int iconCount = GameIcon.childCount;
        if (Open)
        {
            for (int i = 0; i < equipIconData.Count; ++i)
            {
                int index = i;
                equipIconData[index].SetEquipIconClickCallback(() => CS_Inventory.OnEquipmentSlotClickedCB(index));
            }
            CS_Inventory.RefreshLeft(player);
        }
        else
        {
            List<int> mySlot = player.clientPowerUpCtrl.PowerUpInventory.powerUpSlots;
            for (int i = 0; i < iconCount; ++i)
            {
                int index = i;
                equipIconData[index].gameObject.SetActive(true);
                equipIconData[index].InitWithoutCallback(3, mySlot[index], 0, 0);
            }
        }
    }

    public void CloseSwitch ()
    {
        if (player != null)
        {
            powerUpToggle[nowPartTypeCount].isOn = false;
            powerUpToggle[0].isOn = true;
            nowPartTypeCount = 0;
            for (int i = 0; i < equipIconData.Count; ++i)
            {
                int index = i;
                equipIconData[index].SetEquipIconClickCallback(() => CS_Inventory.OnEquipmentSlotClickedCB(index));
            }
            CS_Inventory.RefreshLeft(player);
        }
    }
    #endregion
}