using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using Zealot.Common;
using Zealot.Client.Entities;
using Zealot.Repository;
using Kopio.JsonContracts;

public class UI_EquipmentReform : BaseWindowBehaviour
{
    [Header("Helper")]
    public UI_EquipmentModding uiEquipModding;

    [Header("Prefabs")]
    public GameObject equipIconPrefab;
    public GameObject matIconPrefab;
    public Transform selectEquipIconParent;

    [Header("Toggle Tabs")]
    public Toggle reformTab;
    public Toggle revertTab;

    [Header("Text")]
    public Text         equipmentNameText;
    public GameObject   equipmentNameTextObj;
    public Text         equipmentLvlText;
    public GameObject   equipmentLvlTextObj;
    public Text         equipmentReformStatsText;

    [Header("Button")]
    public Button confirmSelectForReformBtn;
    public Button confirmReformBtn;

    [Header("General UI")]
    public Animator rightSideAnimator;

    [Header("RightSide Prefabs")]
    public Transform reformBagInvParent;
    public Transform recycleBagInvParent;
    public Transform itemStatsParent;
    public GameObject upgStatsValueLnPrefab;
    public GameObject upgStatsMultiLnPrefab;
    public GameObject upgStatsLinePrefab;

    [Header("RightSide Safe UI")]
    public ToggleGroup reformInvToggleGrp;

    [Header("RightSide Scrollviews")]
    public EquipmentModdingScrollView invScrollView;
    public EquipmentModdingScrollView reformScrollView;

    [Header("Results")]
    public GameObject successFxObj;

    // Private variables
    private int                 _slotID;
    private bool                _isEquipped;
    private Equipment           _selectedEquipment;
    private string              _defaultRightSideState      = "RFM_RightSlideIn_DefaultOffScreen";
    private string              _rightSideSlideOut          = "RFM_RightSlideOut";
    private string              _rightSideSlideIn           = "RFM_RightSlideIn_Equipment";
    private string              _rightSideSlideInEnd        = "UPG_RightSlideIn_EquipmentEnd";
    private string              _rightSideSlideInReform     = "RFM_RightSlideIn_Reform";
    private string              _rightSideSlideInReformEnd  = "UPG_RightSlideIn_ReformEnd";
    private GameObject          _selectedEquipmentIcon;
    private List<GameObject>    _inventoryEquipmentList;
    private List<GameObject>    _selectEquipStatsList;

    public override void OnOpenWindow()
    {
        invScrollView.InitScrollView();
        reformScrollView.InitScrollView();
    }

    public void InitEquipmentReform()
    {
        ClearSelectedEquipment();

        // Set default look for the UI
        equipmentNameTextObj.SetActive(true);
        equipmentNameText.text = GUILocalizationRepo.GetLocalizedString("rfm_please_select_reform_equip");
        equipmentLvlTextObj.SetActive(false);

        equipmentReformStatsText.text = "";

        rightSideAnimator.Play(_defaultRightSideState);

        confirmReformBtn.interactable = false;
    }

    public void InitEquipmentReformWithEquipment(int slotId, Equipment equipment)
    {
        ClearSelectedEquipment();

        // Set default look for the UI
        equipmentNameTextObj.SetActive(false);
        equipmentLvlTextObj.SetActive(false);
    }

    public void LoadEquippedBagInventory()
    {
        PlayerGhost player = GameInfo.gLocalPlayer;
        if (player == null)
        {
            return;
        }

        ClearInvEquipmentIcons();

        List<Equipment> equippedItemList = player.mEquipmentInvData.Slots;
        List<IInventoryItem> bagItemList = player.clientItemInvCtrl.itemInvData.Slots;
        FillInventory(equippedItemList, bagItemList);
    }

    public void OnClickOpenSelectEquipment()
    {
        if(rightSideAnimator.GetCurrentAnimatorStateInfo(0).IsName(_rightSideSlideInEnd))
        {
            return;
        }

        rightSideAnimator.Play(_rightSideSlideIn);
        //confirmSelectEquipBtn.interactable = false;
        //confirmSelectEquipBtn.onClick.RemoveAllListeners();
        //confirmSelectEquipBtn.onClick.AddListener(OnClickConfirmSelectUpgradeEquipment);

        LoadEquippedBagInventory();
    }

    public void OnClickOpenSelectReform()
    {

    }

    public void OnClickSelectReformEquipment(bool isToggleOn, bool isEquipped, ModdingEquipment equipToUpgrade)
    {
        PlayerGhost player = GameInfo.gLocalPlayer;

        if (player == null)
        {
            return;
        }

        if (isToggleOn)
        {
            _selectedEquipment = equipToUpgrade.mEquip;
            _isEquipped = isEquipped;
            _slotID = equipToUpgrade.mSlotID;

            // Equipment Stats
            //selectedEquipText.text = _selectedEquipment.GetEquipmentName();

            EquipmentType equipType = _selectedEquipment.EquipmentJson.equiptype;
            ItemRarity rarity = _selectedEquipment.EquipmentJson.rarity;
            int nextLevel = _selectedEquipment.UpgradeLevel + 1;

            EquipmentUpgradeJson upgradeData = EquipmentModdingRepo.GetEquipmentUpgradeData(equipType, rarity, nextLevel);

            if (upgradeData == null)
            {
                return;
            }

            ClearSelectEquipStatsList();

            List<int> buffSEIdList = EquipmentModdingRepo.GetEquipmentUpgradeBuff(equipType, rarity, nextLevel);

            if (buffSEIdList == null)
            {
                return;
            }

            GenerateEquipUpgBuffStats(buffSEIdList, nextLevel);

            //confirmSelectEquipBtn.interactable = true;
        }
        else
        {
            ClearSelectEquipStatsList();

            //confirmSelectEquipBtn.interactable = false;
        }
    }

    private void GenerateSelectedEquipIcon(Equipment equipment)
    {
        int currentStep = equipment.ReformStep;
        int currentLevel = equipment.UpgradeLevel;

        GameObject newEquipObj = Instantiate(equipIconPrefab);
        newEquipObj.transform.SetParent(selectEquipIconParent, false);

        GameIcon_Equip equipIcon = newEquipObj.GetComponent<GameIcon_Equip>();
        //equipIcon.Init(equipment.ItemID, 0, currentStep, currentLevel, false, false, false, OnClickOpenSelectReformEquipment);

        _selectedEquipmentIcon = newEquipObj;
    }

    private void FillInventory(List<Equipment> equippedEquipList, List<IInventoryItem> invEquipList)
    {
        int rowSize = 3;
        int equippedCount = equippedEquipList.Count;
        int equippedRemainder = equippedCount % rowSize;
        int equippedToFill = rowSize - equippedRemainder;

        int bagCount = invEquipList.Count;
        int actualBagCount = bagCount - equippedToFill;
        int bagRealStart = equippedToFill;

        List<ModdingEquipment> fullEquipmentList = uiEquipModding.GetModdingEquipmentList(equippedEquipList, invEquipList);
        int fullCount = equippedCount + bagCount;
        int maxRow = Mathf.CeilToInt(fullCount * 1.0f / rowSize);

        invScrollView.Populate(this, fullEquipmentList, equippedCount, reformInvToggleGrp);
    }

    private void GenerateEquipUpgBuffStats(List<int> buffSEIdList, int nextLevel)
    {
        if (buffSEIdList.Count == 0)
        {
            //GameObject emptyStatsEndLine = Instantiate(upgStatsLinePrefab);
            //emptyStatsEndLine.transform.SetParent(itemStatsParent, false);
            //_selectEquipStatsList.Add(emptyStatsEndLine);

            return;
        }

        for (int i = 0; i < buffSEIdList.Count; ++i)
        {
            int seId = buffSEIdList[i];
            SideEffectJson sideeffect = SideEffectRepo.GetSideEffect(seId);

            GameObject newStatsLine = Instantiate(upgStatsValueLnPrefab);
            newStatsLine.transform.SetParent(itemStatsParent, false);

            EquipmentModdingStats reformStatsObj = newStatsLine.GetComponent<EquipmentModdingStats>();
            if (reformStatsObj != null)
            {
                reformStatsObj.Init(EquipmentModdingType.Reform, EquipmentModdingStatsType.ToGet, nextLevel, sideeffect.description);
            }

            _selectEquipStatsList.Add(newStatsLine);
        }

        GameObject newStatsEndLine = Instantiate(upgStatsLinePrefab);
        newStatsEndLine.transform.SetParent(itemStatsParent, false);
        _selectEquipStatsList.Add(newStatsEndLine);
    }

    private void ClearSelectedEquipment()
    {
        Destroy(_selectedEquipmentIcon);
        _selectedEquipmentIcon = null;
    }

    private void ClearInvEquipmentIcons()
    {
        if (_inventoryEquipmentList == null)
        {
            _inventoryEquipmentList = new List<GameObject>();
            return;
        }

        for (int i = 0; i < _inventoryEquipmentList.Count; ++i)
        {
            Destroy(_inventoryEquipmentList[i]);
            _inventoryEquipmentList[i] = null;
        }
        _inventoryEquipmentList.Clear();
    }

    public void ClearSelectEquipStatsList()
    {
        if (_selectEquipStatsList == null)
        {
            _selectEquipStatsList = new List<GameObject>();
            return;
        }

        for (int i = 0; i < _selectEquipStatsList.Count; ++i)
        {
            Destroy(_selectEquipStatsList[i]);
            _selectEquipStatsList[i] = null;
        }
        _selectEquipStatsList.Clear();
    }
}
