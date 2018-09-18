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
    [SerializeField]
    private GameObject requiredItemDataPrefab;
    [SerializeField]
    private Transform ItemRequirements_Parents; //Content_RequiredItems //requiredItemDataParent

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
    private Text TXT_NowLevel;  //部位等級/玩家等級
    [SerializeField]
    private Text TXT_NexeLevel; //下個等級

    [SerializeField]
    private Text TXT_Effect;    //屬性能力:串聯SideEffect表
    [SerializeField]
    private Text TXT_NowValue;  //目前屬性數值
    [SerializeField]
    private Text TXT_NextValue; //下個等級的屬性數值

    [Space(5)]
    [SerializeField]
    private Animator AT_NoEnough;  //材料不足

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
    private Toggle[] powerUpToggle;

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

    void RefreshPowerUpShow(int part)
    {
        PlayerGhost player = GameInfo.gLocalPlayer;
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
        PlayerGhost player = GameInfo.gLocalPlayer;
        powerupData = PowerUpRepo.GetPowerUpByPartsLevel(nowPartType, player.clientPowerUpCtrl.PowerUpInventory.powerUpSlots[part]);

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
                
                if (int.TryParse(EndSplit_List[0], out ItemId) && int.TryParse(EndSplit_List[1], out ItemCount))
                {

                    GameObject reqITemDataObj = Instantiate(requiredItemDataPrefab);
                    reqITemDataObj.transform.SetParent(ItemRequirements_Parents, false);

                    if (player == null) { return; } //如果玩家還沒生成，return

                    int invAmount = player.clientItemInvCtrl.itemInvData.GetTotalStackCountByItemId((ushort)ItemId); //玩家擁有的道具
                    int money = player.SecondaryStats.Money; //玩家擁有的金錢

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
        AT_NoEnough.Play((haveEnoughMaterial == true) ? "inv_notenough_DEFAULT" : "inv_notenough");
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
