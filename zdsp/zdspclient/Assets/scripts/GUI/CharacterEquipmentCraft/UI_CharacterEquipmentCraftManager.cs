using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Client.Entities;
using Kopio.JsonContracts;
using Zealot.Repository;
using Zealot.Common;

public class UI_CharacterEquipmentCraftManager : MonoBehaviour {

    static UI_CharacterEquipmentCraftManager managerScript;

    [SerializeField] Text categoryText; //H_Text_Title     in common

    [SerializeField] Transform partParent; //H_Content_Button_ComboBoxData
    [SerializeField] GameObject partButton; //I_Button_ComboBoxAData

    private List<PartsType> partList = new List<PartsType>();

    private List<EquipmentJson> myEquipmentData = new List<EquipmentJson>();

    [SerializeField] Image EquipmentIcon; //H_Image_TypeIcon     in nonePrefab
    [SerializeField] Text EquipmentName; //H_Text_ItemName
    [SerializeField] Text equipmentDescription; //H_Text_Stats

    [SerializeField] GameObject equipmentsPrefab; //I_TypesData
    [SerializeField] GameObject materialsReqAmountPrefab; //I_RequirementsData
    [SerializeField] GameObject materialsPrefab; //I_GameIcon_Material
    [SerializeField] Transform equipmentParent; //H_Content_Types
    [SerializeField] Transform materialReqAmountParent; //H_Content_Requirements

    static List<GameIcon_MaterialConsumable> afterCraftMaterial = new List<GameIcon_MaterialConsumable>();

    [SerializeField] Button CraftButton; //H_ButtonA2_Craft

    static int nowItemId = 0;

    static bool haveEnoughMaterial = true;

    [SerializeField] GameObject cannotCraftObject; //H_GameObject_CannotCraft_DEFAULTOFF
    [SerializeField] Text cannotCraftReason; //H_Text_CannotCraftReason

    void Start ()
    {
        managerScript = this;

        ClientUtils.DestroyChildren(partParent);

        partList = EquipmentCraftRepo.EquipmentPartsType();
        categoryText.text = EquipmentCraftRepo.equipmentTypesName[partList[0]];

        for (int i = 0; i < partList.Count; i++)
        {
            GameObject partBtn = ClientUtils.CreateChild(partParent, partButton);
            partBtn.transform.localScale = Vector3.one;
            partBtn.transform.GetChild(2).GetComponent<Text>().text = EquipmentCraftRepo.equipmentTypesName[partList[i]];
            int partIn = i;
            partBtn.GetComponent<Button>().onClick.AddListener(delegate { ShowPartName(partList[partIn]); });
        }

        CraftButton.onClick.AddListener(delegate { CraftEquipment(); });
    }

    void OnEnable()
    {
        ShowPartName(EquipmentCraftRepo.EquipmentPartsType()[0]);
    }

    void Init()
    {
        nowItemId = 0;

        ClientUtils.DestroyChildren(equipmentParent);
        ClientUtils.DestroyChildren(materialReqAmountParent);
    }

    public void ShowPartName (PartsType part)
    {
        categoryText.text = EquipmentCraftRepo.equipmentTypesName[part];

        Init();
        myEquipmentData = EquipmentCraftRepo.EquipmentJsonPartList(part);

        int partsCount = myEquipmentData.Count;
        nowItemId = myEquipmentData[0].itemid;

        for (int i = 0; i < partsCount; i++)
        {
            GameObject equipmentData = ClientUtils.CreateChild(equipmentParent, equipmentsPrefab);
            equipmentData.transform.GetChild(2).GetComponent<Image>().sprite = ClientUtils.LoadItemIcon(myEquipmentData[i].itemid);
            equipmentData.transform.GetChild(3).GetComponent<Text>().text = myEquipmentData[i].localizedname;

            int partIn = i;
            equipmentData.GetComponent<Toggle>().onValueChanged.AddListener(delegate { ChangePartInformation(myEquipmentData[partIn].itemid, partIn); });
            equipmentData.GetComponent<Toggle>().group = equipmentParent.GetComponent<ToggleGroup>();

            if (i == 0)
            {
                equipmentData.GetComponent<Toggle>().isOn = true;
            }
        }
        
        ChangePartInformation(myEquipmentData[0].itemid, 0);
    }

    public void ChangePartInformation (int itemId, int count)
    {
        nowItemId = itemId;
        EquipmentIcon.sprite = ClientUtils.LoadItemIcon(itemId);
        EquipmentName.text = myEquipmentData[count].localizedname;
        equipmentDescription.text = myEquipmentData[count].description;

        InitMaterial(itemId);
    }

    private void InitMaterial(int itemId)
    {
        ClientUtils.DestroyChildren(materialReqAmountParent);

        PlayerGhost player = GameInfo.gLocalPlayer;
        List<ItemInfo> material = EquipmentCraftRepo.GetEquipmentMaterial(itemId);
        
        if (player == null)
        {
            return;
        }

        afterCraftMaterial.Clear();
        haveEnoughMaterial = true;

        for (int i = 0; i < material.Count; ++i)
        {
            GameObject reqItemDataObject = ClientUtils.CreateChild(materialReqAmountParent, materialsReqAmountPrefab);
            reqItemDataObject.transform.localScale = Vector3.one;
            reqItemDataObject.transform.GetChild(1).transform.GetChild(0).gameObject.SetActive(false);
            reqItemDataObject.transform.GetChild(3).GetComponent<Text>().text = material[i].stackCount.ToString();

            int invAmount = player.clientItemInvCtrl.itemInvData.GetTotalStackCountByItemId(material[i].itemId);
            GameObject gameIconObj = ClientUtils.CreateChild(reqItemDataObject.transform.GetChild(1), materialsPrefab);
            gameIconObj.transform.localScale = Vector3.one;

            GameIcon_MaterialConsumable gameIcon = gameIconObj.GetComponent<GameIcon_MaterialConsumable>();
            afterCraftMaterial.Add(gameIcon);
            gameIcon.Init(itemId, invAmount, false, false, false, OnClickMaterial);
            gameIcon.SetFullStackCount(invAmount);
            CompareMaterial(gameIconObj.transform.GetChild(3).GetComponent<Text>(), invAmount, material[i].stackCount);
        }

        cannotCraftObject.SetActive(false);

        MaterialEnoughObject();
    }

    public void OnClickMaterial()
    {
        PlayerGhost player = GameInfo.gLocalPlayer;
        var _item = player.clientItemInvCtrl.itemInvData.GetItemByItemId((ushort)nowItemId);
        UIManager.OpenDialog(WindowType.DialogItemDetail, (window) => {
            OnClicked_InitTooltip(window.GetComponent<UI_DialogItemDetailToolTip>(), _item);
        });
    }

    private void OnClicked_InitTooltip(UI_DialogItemDetailToolTip component, IInventoryItem item)
    {
        component.InitTooltip(item);
        List<ItemDetailsButton> _buttons = new List<ItemDetailsButton>();
        component.SetButtonCallback(_buttons);
    }

    static void CompareMaterial (Text viewStack, int invAmount, int reqAmount)
    {
        if(invAmount < reqAmount)
        {
            viewStack.color = Color.red;
            haveEnoughMaterial = false;
        }
    }

    public void MaterialEnoughObject()
    {
        if (!haveEnoughMaterial)
        {
            cannotCraftObject.SetActive(true);
            cannotCraftReason.text = "You have not enought material.";
        }
    }

    public void CraftEquipment ()
    {
        RPCFactory.NonCombatRPC.EquipmentCraft(nowItemId);
    }

    public static void AfterCraft ()
    {
        List<ItemInfo> material = EquipmentCraftRepo.GetEquipmentMaterial(nowItemId);

        haveEnoughMaterial = true;
        for (int i = 0; i < afterCraftMaterial.Count; ++i)
        {
            int invAmount = GameInfo.gLocalPlayer.clientItemInvCtrl.itemInvData.GetTotalStackCountByItemId(material[i].itemId);
            afterCraftMaterial[i].InitWithoutCallback(nowItemId, invAmount);
            afterCraftMaterial[i].SetFullStackCount(invAmount);
            CompareMaterial(afterCraftMaterial[i].transform.GetChild(3).GetComponent<Text>(), invAmount, material[i].stackCount);
        }

        managerScript.MaterialEnoughObject();
    }

    public void NotEnoughButton ()
    {

    }
}