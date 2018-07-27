using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public Text         reformCostText;

    [Header("Button")]
    public Button confirmReformAttributesBtn;
    public Button confirmReformBtn;

    [Header("General UI")]
    public Animator rightSideAnimator;

    [Header("RightSide Prefabs")]
    public Transform reformBagInvParent;
    public Transform recycleBagInvParent;
    public Transform itemStatsParent;
    public GameObject rfmStatsValueLnPrefab;
    public GameObject rfmStatsMultiLnPrefab;
    public GameObject rfmStatsLinePrefab;

    [Header("RightSide Text")]
    public Text selectedEquipText;

    [Header("RightSide Inv UI")]
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

        confirmReformAttributesBtn.gameObject.SetActive(true);
        confirmReformAttributesBtn.interactable = false;
        confirmReformBtn.gameObject.SetActive(false);
        confirmReformBtn.interactable = false;
    }

    public void InitEquipmentReformWithEquipment(int slotId, Equipment equipment)
    {
        ClearSelectedEquipment();

        // Set default look for the UI
        equipmentNameTextObj.SetActive(false);
        equipmentLvlTextObj.SetActive(false);

        _slotID = slotId;
        _selectedEquipment = equipment;

        rightSideAnimator.Play(_defaultRightSideState);

        ClearSelectEquipStatsList();

        confirmReformAttributesBtn.gameObject.SetActive(true);
        confirmReformAttributesBtn.interactable = false;
        confirmReformBtn.gameObject.SetActive(false);
        confirmReformBtn.interactable = false;

        LoadEquipmentData(equipment);
    }

    private void LoadEquipmentData(Equipment equipment)
    {
        string equipName = equipment.GetEquipmentName();
        equipmentNameTextObj.SetActive(true);
        equipmentNameText.text = equipName;

        int currentLevel = equipment.UpgradeLevel;
        equipmentLvlTextObj.SetActive(true);
        equipmentLvlText.text = currentLevel.ToString();

        EquipmentType equipType = equipment.EquipmentJson.equiptype;
        ItemRarity rarity = equipment.EquipmentJson.rarity;
        int nextLevel = currentLevel + 1;

        //SetEquipmentUpgradeButtonState(equipment);
    }

    public void InitEquipmentUpgradeRefresh(Equipment equipment)
    {
        rightSideAnimator.Play(_defaultRightSideState);

        _selectedEquipment = equipment;

        LoadEquipmentDataRefresh(equipment);
    }

    private void LoadEquipmentDataRefresh(Equipment equipment)
    {
        string equipName = equipment.GetEquipmentName();
        equipmentNameTextObj.SetActive(true);
        equipmentNameText.text = equipName;

        int currentLevel = equipment.UpgradeLevel;
        equipmentLvlTextObj.SetActive(true);
        equipmentLvlText.text = currentLevel.ToString();

        EquipmentType equipType = equipment.EquipmentJson.equiptype;
        ItemRarity rarity = equipment.EquipmentJson.rarity;
        int nextLevel = currentLevel + 1;
        float successProb = EquipmentModdingRepo.GetEquipmentUpgradeSuccessProb(equipType, rarity, nextLevel);

        //SetEquipmentUpgradeButtonState(equipment);
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
            selectedEquipText.text = _selectedEquipment.GetEquipmentName();

            EquipmentType equipType = _selectedEquipment.EquipmentJson.equiptype;
            ItemRarity rarity = _selectedEquipment.EquipmentJson.rarity;
            string reformGrp = _selectedEquipment.EquipmentJson.evolvegrp;

            if(reformGrp != "-1" && reformGrp != "#unnamed#")
            {
                int currentStep = _selectedEquipment.ReformStep;
                int nextStep = currentStep + 1;

                List<EquipmentReformGroupJson> reformData = EquipmentModdingRepo.GetEquipmentReformDataByGroupStep(reformGrp, currentStep);

                if (reformData == null)
                {
                    return;
                }

                ClearSelectEquipStatsList();

                List<EquipReformData> reformDataList = EquipmentModdingRepo.GetEquipmentReformData(_selectedEquipment);

                GenerateEquipRfmStatsObj(reformDataList);
            }

            confirmReformAttributesBtn.interactable = true;
        }
        else
        {
            ClearSelectEquipStatsList();

            confirmReformAttributesBtn.interactable = false;
        }
    }

    public void OnClickConfirmSelectEquipment()
    {
        equipmentReformStatsText.text = GenerateEquipmentStatsString(_selectedEquipment);
    }

    public void OnClickEquipmentReform()
    {
        //if(!CheckMaxedLevel(true) || !CheckSufficientMoney(true))
        //{
        //    return;
        //}

        //PlayerGhost player = GameInfo.gLocalPlayer;
        //if (player == null)
        //{
        //    return;
        //}

        //// Check if already maxed out upgrade level
        //int upgradeLevel = _selectedEquipment.UpgradeLevel + 1;

        //bool isSafeUpgrade = safeUpgradeTab.isOn;
        //EquipmentType equipType = _selectedEquipment.EquipmentJson.equiptype;
        //ItemRarity rarity = _selectedEquipment.EquipmentJson.rarity;
        //EquipmentUpgradeJson upgradeData = EquipmentModdingRepo.GetEquipmentUpgradeData(equipType, rarity, upgradeLevel);
        //if (upgradeData == null)
        //{
        //    // Unable to get upgrade data
        //    UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("ret_EquipUpgrade_UpgradeDataReadFailed"));
        //    return;
        //}

        //bool isEnoughGenMat = true;
        //bool isEnoughSafeMat = true;
        //// Check enough normal materials
        //List<EquipUpgMaterial> generalMatList = EquipmentModdingRepo.GetEquipmentUpgradeMaterials(equipType, rarity, upgradeLevel, false);
        //if (_genMatSel >= 0 && _genMatSel < generalMatList.Count)
        //{
        //    EquipUpgMaterial selectedGenMat = generalMatList[_genMatSel];
        //    int genMatCount = player.clientItemInvCtrl.itemInvData.GetTotalStackCountByItemId((ushort)selectedGenMat.mMat.mItemID);
        //    if (genMatCount < selectedGenMat.mMat.mAmount)
        //    {
        //        //UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("ret_EquipUpgrade_InsufficientGenMaterials"));
        //        isEnoughGenMat = false;
        //    }
        //}

        //// Check enough safe materials (if use Safe Upgrade)
        //if (isSafeUpgrade)
        //{
        //    if (_safeMatSel == -1 && _selectedSafeEquipment == null)
        //    {
        //        //UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("ret_Missing_Replace_Equip_Gem"));
        //        isEnoughSafeMat = false;
        //    }
        //    else if (_safeMatSel != -1 && _selectedSafeEquipment == null)
        //    {
        //        List<EquipUpgMaterial> safeMatList = EquipmentModdingRepo.GetEquipmentUpgradeMaterials(equipType, rarity, upgradeLevel, true);
        //        if (_safeMatSel >= 0 && _safeMatSel < safeMatList.Count)
        //        {
        //            EquipUpgMaterial selectedSafeMat = safeMatList[_safeMatSel];
        //            int safeMatCount = player.clientItemInvCtrl.itemInvData.GetTotalStackCountByItemId((ushort)selectedSafeMat.mMat.mItemID);
        //            if (safeMatCount < selectedSafeMat.mMat.mAmount)
        //            {
        //                //UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("ret_EquipUpgrade_InsufficientSafeMaterials"));
        //                isEnoughGenMat = false;
        //            }
        //        }
        //    }
        //}

        //if (!isEnoughGenMat || !isEnoughSafeMat)
        //{
        //    OpenUpgradeItemStoreDialog(isEnoughGenMat, isEnoughSafeMat);
        //    return;
        //}
        
        //bool isGenMat = _genMatSel == 0;
        //bool isSafeEquip = isSafeUpgrade && _selectedSafeEquipment != null && _safeMatSel == -1;
        //bool isSafeGenMat = _safeMatSel == 0;
        //if(isSafeEquip)
        //{
        //    RPCFactory.NonCombatRPC.EquipmentUpgradeEquipment(_slotID, _isEquipped, isGenMat, isSafeUpgrade, true, isSafeGenMat, _slotID);
        //}
        //else
        //{
        //    RPCFactory.NonCombatRPC.EquipmentUpgradeEquipment(_slotID, _isEquipped, isGenMat, isSafeUpgrade, false, isSafeGenMat);
        //}
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

    private void GenerateEquipRfmStatsObj(List<EquipReformData> reformDataList)
    {
        if(reformDataList.Count == 0)
        {
            return;
        }

        for(int i = 0; i < reformDataList.Count; ++i)
        {
            int currentStep = reformDataList[i].mReformStep;
            List<int> seIds = reformDataList[i].GetSideEffects();
            for (int j = 0; j < seIds.Count; ++i)
            {
                int seId = seIds[i];
                SideEffectJson sideeffect = SideEffectRepo.GetSideEffect(seId);

                GameObject newStatsLine = Instantiate(rfmStatsValueLnPrefab);
                newStatsLine.transform.SetParent(itemStatsParent, false);

                EquipmentReformStats reformStatsObj = newStatsLine.GetComponent<EquipmentReformStats>();
                if (reformStatsObj != null)
                {
                    reformStatsObj.Init(currentStep, sideeffect.localizedname);
                }

                _selectEquipStatsList.Add(newStatsLine);
            }
        }

        GameObject newStatsEndLine = Instantiate(rfmStatsLinePrefab);
        newStatsEndLine.transform.SetParent(itemStatsParent, false);
        _selectEquipStatsList.Add(newStatsEndLine);
    }

    private string GenerateEquipmentStatsString(Equipment reformEquipment)
    {
        List<EquipReformData> reformDataList = EquipmentModdingRepo.GetEquipmentReformData(reformEquipment);

        if(reformDataList != null)
        {
            StringBuilder statsStr = new StringBuilder();
            for (int i = 0; i < reformDataList.Count; ++i)
            {
                EquipReformData reformData = reformDataList[i];

                statsStr.Append(FormatSideEffectString(reformData.GetSideEffects()));
            }

            return statsStr.ToString();
        }

        return "";
    }

    private string FormatSideEffectString(List<int> seIds)
    {
        StringBuilder statsStr = new StringBuilder();

        if(seIds.Count > 0)
        {
            for(int i = 0; i < seIds.Count; ++i)
            {
                SideEffectJson se = SideEffectRepo.GetSideEffect(seIds[i]);
                statsStr.AppendFormat("{0} {1} {2}\n", se.localizedname, se.max);
            }
        }

        return statsStr.ToString();
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
