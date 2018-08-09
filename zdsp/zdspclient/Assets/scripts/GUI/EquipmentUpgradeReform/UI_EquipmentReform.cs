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
    public Transform recycleReturnMatParent;

    [Header("Toggle Tabs")]
    public Toggle reformTab;
    public Toggle recycleTab;

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
    public Button confirmRecycleBtn;

    [Header("General UI")]
    public Animator rightSideAnimator;

    [Header("RightSide Prefabs")]
    public GameObject   reformBagDataPrefab;
    public Transform    reformBagParent;
    public Transform    recycleBagParent;
    public Transform    itemStatsParent;
    public GameObject   rfmStatsValueLnPrefab;
    public GameObject   rfmStatsMultiLnPrefab;
    public GameObject   rfmStatsLinePrefab;

    [Header("RightSide Text")]
    public Text selectedEquipText;

    [Header("RightSide Inv UI")]
    public ToggleGroup reformInvToggleGrp;

    [Header("RightSide Reform Bag UI")]
    public ToggleGroup reformBagToggleGrp;

    [Header("RightSide Scrollviews")]
    public EquipmentModdingScrollView invScrollView;
    //public EquipmentModdingScrollView reformScrollView;

    [Header("Results")]
    public GameObject successFxObj;

    // Private variables
    private int                 _slotID;
    private bool                _isEquipped;
    private Equipment           _selectedEquipment;
    private int                 _reformSel;
    private string              _defaultRightSideState      = "RFM_RightSlideIn_DefaultOffScreen";
    private string              _rightSideSlideOut          = "RFM_RightSlideOut";
    private string              _rightSideSlideIn           = "RFM_RightSlideIn_Equipment";
    private string              _rightSideSlideInEnd        = "UPG_RightSlideIn_EquipmentEnd";
    private string              _rightSideSlideInReform     = "RFM_RightSlideIn_Reform";
    private string              _rightSideSlideInReformEnd  = "UPG_RightSlideIn_ReformEnd";
    private GameObject          _selectedEquipmentIcon;
    private List<GameObject>    _inventoryEquipmentList;
    private List<GameObject>    _selectEquipStatsList;
    private List<GameObject>    _reformBagDataList;
    private List<GameObject>    _recycleReturnsList;

    public override void OnOpenWindow()
    {
        invScrollView.InitScrollView();
        //reformScrollView.InitScrollView();
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
        selectedEquipText.text = "";

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

    public void InitEquipmentRecycle()
    {
        ClearSelectedEquipment();

        // Set default look for the UI
        equipmentNameTextObj.SetActive(true);
        equipmentNameText.text = GUILocalizationRepo.GetLocalizedString("rfm_please_select_reform_equip");
        equipmentLvlTextObj.SetActive(false);

        equipmentReformStatsText.text = "";

        rightSideAnimator.Play(_defaultRightSideState);
        selectedEquipText.text = "";

        confirmReformAttributesBtn.gameObject.SetActive(false);
        confirmReformAttributesBtn.interactable = false;
        confirmReformBtn.gameObject.SetActive(false);
        confirmReformBtn.interactable = false;
        confirmRecycleBtn.gameObject.SetActive(true);
        confirmRecycleBtn.interactable = false;
    }

    public void InitEquipmentRecycleWithEquipment(int slotId, Equipment equipment)
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
        confirmRecycleBtn.gameObject.SetActive(true);
        confirmRecycleBtn.interactable = false;

        LoadEquipmentData(equipment);
    }

    private void LoadEquipmentData(Equipment equipment)
    {
        string equipName = equipment.GetEquipmentName();
        equipmentNameTextObj.SetActive(true);
        equipmentNameText.text = equipName;

        int currentLevel = equipment.ReformStep;
        equipmentLvlTextObj.SetActive(true);
        equipmentLvlText.text = currentLevel.ToString();

        EquipmentType equipType = equipment.EquipmentJson.equiptype;
        ItemRarity rarity = equipment.EquipmentJson.rarity;
        int nextLevel = currentLevel + 1;

        //SetEquipmentUpgradeButtonState(equipment);
    }

    public void InitEquipmentReformRefresh(Equipment equipment)
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

    private void LoadNextReformData(Equipment equipment)
    {
        string reformGrp = equipment.EquipmentJson.evolvegrp;
        int reformStep = equipment.ReformStep;
        int nextStep = reformStep + 1;

        ClearReformBagDataList();

        List<EquipmentReformGroupJson> reformDataList = EquipmentModdingRepo.GetEquipmentReformDataByGroupStep(reformGrp, nextStep);

        if(reformDataList == null)
        {
            return;
        }

        for(int i = 0; i < reformDataList.Count; ++i)
        {
            EquipmentReformGroupJson reformData = reformDataList[i];
            if(reformData != null)
            {
                GameObject reformBagDataObj = Instantiate(reformBagDataPrefab);
                reformBagDataObj.transform.SetParent(reformBagParent, false);

                ReformBagData reformBagData = reformBagDataObj.GetComponent<ReformBagData>();
                reformBagData.Init(this, i, reformBagToggleGrp, nextStep, reformData);

                _reformBagDataList.Add(reformBagDataObj);
            }
        }
    }

    public void OnClickOpenSelectEquipment()
    {
        if(rightSideAnimator.GetCurrentAnimatorStateInfo(0).IsName(_rightSideSlideInEnd))
        {
            return;
        }

        rightSideAnimator.Play(_rightSideSlideIn);
        selectedEquipText.text = "";

        if(reformTab.isOn)
        {
            confirmReformAttributesBtn.gameObject.SetActive(true);
            confirmReformAttributesBtn.interactable = false;
            confirmReformBtn.gameObject.SetActive(false);
            confirmReformBtn.interactable = false;
        }
        else
        {
            confirmRecycleBtn.gameObject.SetActive(true);
            confirmRecycleBtn.interactable = false;
        }

        //confirmSelectEquipBtn.interactable = false;
        //confirmSelectEquipBtn.onClick.RemoveAllListeners();
        //confirmSelectEquipBtn.onClick.AddListener(OnClickConfirmSelectUpgradeEquipment);

        LoadEquippedBagInventory();
    }

    public void OnClickSelectReformEquipment(bool isToggleOn, bool isEquipped, ModdingEquipment equipToUpgrade)
    {
        PlayerGhost player = GameInfo.gLocalPlayer;

        if(player == null)
        {
            return;
        }

        if(isToggleOn)
        {
            _selectedEquipment = equipToUpgrade.mEquip;
            _isEquipped = isEquipped;
            _slotID = equipToUpgrade.mSlotID;

            // Equipment Stats
            selectedEquipText.text = _selectedEquipment.GetEquipmentName();
            
            string reformGrp = _selectedEquipment.EquipmentJson.evolvegrp;

            if(reformGrp != "-1" && reformGrp != "#unnamed#")
            {
                int currentStep = _selectedEquipment.ReformStep;
                int nextStep = currentStep + 1;

                if(currentStep > 0)
                {
                    List<EquipmentReformGroupJson> reformData = EquipmentModdingRepo.GetEquipmentReformDataByGroupStep(reformGrp, currentStep);

                    if(reformData == null)
                    {
                        return;
                    }

                    ClearSelectEquipStatsList();

                    List<EquipReformData> reformDataList = EquipmentModdingRepo.GetEquipmentReformData(_selectedEquipment);

                    GenerateEquipRfmStatsObj(reformDataList);
                }
            }

            confirmReformAttributesBtn.interactable = true;
        }
        else
        {
            ClearSelectEquipStatsList();

            confirmReformAttributesBtn.interactable = false;
        }
    }

    public void OnClickSelectRecycleEquipment(bool isToggleOn, bool isEquipped, ModdingEquipment equipToUpgrade)
    {
        PlayerGhost player = GameInfo.gLocalPlayer;

        if(player == null)
        {
            return;
        }

        if(isToggleOn)
        {
            _selectedEquipment = equipToUpgrade.mEquip;
            _isEquipped = isEquipped;
            _slotID = equipToUpgrade.mSlotID;

            // Left side
            // Equipment data
            LoadEquipmentData(_selectedEquipment);
            
            // Right side
            selectedEquipText.text = _selectedEquipment.GetEquipmentName();
            
            string reformGrp = _selectedEquipment.EquipmentJson.evolvegrp;

            if(reformGrp != "-1" && reformGrp != "#unnamed#")
            {
                int currentStep = _selectedEquipment.ReformStep;
                int nextStep = currentStep + 1;

                if(currentStep > 0)
                {
                    List<EquipmentReformGroupJson> reformData = EquipmentModdingRepo.GetEquipmentReformDataByGroupStep(reformGrp, currentStep);

                    if(reformData == null)
                    {
                        return;
                    }

                    // Equipment Stats
                    ClearSelectEquipStatsList();

                    List<EquipReformData> reformDataList = EquipmentModdingRepo.GetEquipmentReformData(_selectedEquipment);

                    if(reformDataList != null)
                    {
                        GenerateEquipRfmStatsObj(reformDataList);
                    }

                    // Recycle returns
                    ClearRecycleReturnsList();

                    int selection = _selectedEquipment.GetSelectionsList()[currentStep];
                    GenerateEquipRecycleReturns(reformGrp, currentStep, selection);

                    confirmRecycleBtn.interactable = true;
                }
                else
                {
                    UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("ret_Equip_RecycleAtBaseLevel"));

                    confirmRecycleBtn.interactable = false;
                }
            }
        }
        else
        {
            ClearSelectEquipStatsList();

            confirmRecycleBtn.interactable = false;
        }
    }

    public void OnClickConfirmSelectReformEquipment()
    {
        equipmentReformStatsText.text = GenerateEquipmentStatsString(_selectedEquipment);

        LoadEquipmentData(_selectedEquipment);
        GenerateSelectedEquipIcon(_selectedEquipment);

        LoadNextReformData(_selectedEquipment);

        confirmReformAttributesBtn.gameObject.SetActive(false);
        confirmReformBtn.gameObject.SetActive(true);
        confirmReformBtn.interactable = false;

        InitCostString();
    }

    public void OnClickSelectReformStep(int selection, bool isToggleOn)
    {
        if(isToggleOn)
        {
            _reformSel = selection;

            string reformGrp = _selectedEquipment.EquipmentJson.evolvegrp;
            int nextStep = _selectedEquipment.ReformStep + 1;
            int reformCost = EquipmentModdingRepo.GetEquipmentReformCost(reformGrp, nextStep, selection);
            reformCostText.text = reformCost.ToString();

            if (CheckSufficientMoney(_reformSel) == false)
            {
                confirmReformBtn.interactable = false;

                reformCostText.color = Color.red;
            }
            else
            {
                confirmReformBtn.interactable = true;

                reformCostText.color = Color.white;
            }
        }
        else
        {
            _reformSel = -1;

            confirmReformBtn.interactable = false;

            reformCostText.text = "0";
            reformCostText.color = Color.white;
        }
    }

    public void OnClickEquipmentReform()
    {
        if(!CheckMaxedLevel(true) || !CheckSufficientMoney(_reformSel, true))
        {
            return;
        }

        PlayerGhost player = GameInfo.gLocalPlayer;
        if(player == null)
        {
            return;
        }

        // Check if enough materials
        if(!CheckSufficientMaterials(_reformSel, false))
        {
            OpenReformItemStoreDialog();
            return;
        }

        RPCFactory.NonCombatRPC.EquipmentReformEquipment(_slotID, _isEquipped, _reformSel);
    }

    public void PlayEquipmentUpgradeSuccess()
    {
        if (successFxObj.activeSelf == false)
        {
            successFxObj.SetActive(true);
        }
    }

    public void Refresh()
    {
        PlayerGhost player = GameInfo.gLocalPlayer;
        if (player == null)
        {
            return;
        }

        Equipment equipment = _isEquipped ? player.mEquipmentInvData.Slots[_slotID] as Equipment : player.clientItemInvCtrl.itemInvData.Slots[_slotID] as Equipment;

        InitEquipmentReformRefresh(equipment);
    }

    void OnDisable()
    {
        rightSideAnimator.Play(_rightSideSlideOut);

        ClearSelectedEquipment();
        selectedEquipText.text = "";
        equipmentReformStatsText.text = "";
        ClearSelectEquipStatsList();
    }

    private void OpenReformItemStoreDialog()
    {
        Debug.LogError("Not enough material, opening Item Store Dialog.");
        //UIManager.OpenDialog(WindowType.DialogItemStore);
    }

    private bool CheckMaxedLevel(bool showMessage = false)
    {
        PlayerGhost player = GameInfo.gLocalPlayer;
        if(player == null)
        {
            return false;
        }

        // Check if already maxed out reform steps
        string reformGroup = _selectedEquipment.EquipmentJson.evolvegrp;
        int reformNextEquip = _selectedEquipment.EquipmentJson.evolvechange;
        int nextStep = _selectedEquipment.ReformStep + 1;
        int maxLevel = EquipmentModdingRepo.GetEquipmentReformGroupMaxLevel(reformGroup);

        if(nextStep >= maxLevel && reformNextEquip == -1)
        {
            // Exceeded max upgrade level
            if (showMessage)
                UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("ret_Equip_ReformLevelMaxed"));

            return false;
        }

        return true;
    }

    private bool CheckSufficientMoney(int selection, bool showMessage = false)
    {
        PlayerGhost player = GameInfo.gLocalPlayer;
        if(player == null)
        {
            return false;
        }

        int nextStep = _selectedEquipment.ReformStep + 1;
        string reformGrp = _selectedEquipment.EquipmentJson.evolvegrp;

        // Check enough gold
        int moneyCost = EquipmentModdingRepo.GetEquipmentReformCost(reformGrp, nextStep, selection);
        if(player.IsCurrencyEnough(CurrencyType.Money, moneyCost) == false)
        {
            if(showMessage)
                UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("sys_InsufficientMoney"));
            return false;
        }

        return true;
    }

    private bool CheckSufficientMaterials(int selection, bool showMessage = false)
    {
        PlayerGhost player = GameInfo.gLocalPlayer;
        if(player == null)
        {
            return false;
        }

        int nextStep = _selectedEquipment.ReformStep + 1;
        string reformGrp = _selectedEquipment.EquipmentJson.evolvegrp;

        // Check enough materials
        List<EquipModMaterial> materialList = EquipmentModdingRepo.GetEquipmentReformMaterials(reformGrp, nextStep, selection);
        for(int i = 0; i < materialList.Count; ++i)
        {
            EquipModMaterial mat = materialList[i];
            int currMatCount = player.clientItemInvCtrl.itemInvData.GetTotalStackCountByItemId((ushort)mat.mItemID);
            int reqMatCount = mat.mAmount;
            if(currMatCount < reqMatCount)
            {
                if (showMessage)
                    UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("ret_Equip_ReformInsufficientMaterials"));

                return false;
            }
        }

        return true;
    }

    private void GenerateSelectedEquipIcon(Equipment equipment)
    {
        int currentStep = equipment.ReformStep;
        int currentLevel = equipment.UpgradeLevel;

        GameObject newEquipObj = Instantiate(equipIconPrefab);
        newEquipObj.transform.SetParent(selectEquipIconParent, false);

        GameIcon_Equip equipIcon = newEquipObj.GetComponent<GameIcon_Equip>();
        equipIcon.Init(equipment.ItemID, 0, currentStep, currentLevel, false, false, false, OnClickOpenSelectEquipment);

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
            for (int j = 0; j < seIds.Count; ++j)
            {
                int seId = seIds[j];
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
        int reformStep = reformEquipment.ReformStep;
        List<EquipReformData> reformDataList = EquipmentModdingRepo.GetEquipmentReformData(reformEquipment);

        if(reformDataList != null)
        {
            StringBuilder statsStr = new StringBuilder();
            for (int i = 0; i < reformDataList.Count; ++i)
            {
                EquipReformData reformData = reformDataList[i];

                statsStr.Append(FormatSideEffectString(reformStep, reformData.GetSideEffects()));
            }

            return statsStr.ToString();
        }

        return "";
    }

    private void GenerateEquipRecycleReturns(string reformGrp, int currentStep, int selection)
    {
        int recycleMoney = EquipmentModdingRepo.GetEquipmentReformCost(reformGrp, currentStep, selection);

        GameObject retMoneyObj = Instantiate(matIconPrefab);
        retMoneyObj.transform.SetParent(recycleBagParent, false);

        GameIcon_MaterialConsumable retMoneyIcon = retMoneyObj.GetComponent<GameIcon_MaterialConsumable>();
        retMoneyIcon.SetFullStackCount(recycleMoney);

        _recycleReturnsList.Add(retMoneyObj);
        
        List<EquipModMaterial> recycleReturnsList = EquipmentModdingRepo.GetEquipmentReformMaterials(reformGrp, currentStep, selection);

        if(recycleReturnsList != null)
        {
            for(int i = 0; i < recycleReturnsList.Count; ++i)
            {
                EquipModMaterial equipModMat = recycleReturnsList[i];

                GameObject retMatObj = Instantiate(equipIconPrefab);
                retMatObj.transform.SetParent(recycleBagParent, false);

                GameIcon_Equip retMatIcon = retMatObj.GetComponent<GameIcon_Equip>();
                retMatIcon.Init(equipModMat.mItemID);

                _recycleReturnsList.Add(retMatObj);
            }
        }
    }

    private void InitCostString()
    {
        reformCostText.text = "0";
        reformCostText.color = Color.white;
    }

    public string FormatSideEffectString(int reformStep, List<int> seIds)
    {
        StringBuilder statsStr = new StringBuilder();

        if(seIds.Count > 0)
        {
            int lastPos = seIds.Count - 1;
            string reformKai = ClientUtils.GetLocalizedReformKai(reformStep);

            if(seIds.Count > 1)
            {
                for (int i = 0; i < lastPos; ++i)
                {
                    SideEffectJson se = SideEffectRepo.GetSideEffect(seIds[i]);
                    statsStr.AppendFormat("{0} {1}+{2}\n", reformKai, se.localizedname, se.max);
                }
            }

            SideEffectJson lastSE = SideEffectRepo.GetSideEffect(seIds[lastPos]);
            ClientUtils.GetLocalizedReformKai(reformStep);
            statsStr.AppendFormat("{0} {1}+{2}", reformKai, lastSE.localizedname, lastSE.max);
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

    private void ClearReformBagDataList()
    {
        if(_reformBagDataList == null)
        {
            _reformBagDataList = new List<GameObject>();
            return;
        }

        for(int i = 0; i < _reformBagDataList.Count; ++i)
        {
            Destroy(_reformBagDataList[i]);
            _reformBagDataList[i] = null;
        }
        _reformBagDataList.Clear();
    }

    private void ClearRecycleReturnsList()
    {
        if(_recycleReturnsList == null)
        {
            _recycleReturnsList = new List<GameObject>();
            return;
        }

        for(int i = 0; i < _recycleReturnsList.Count; ++i)
        {
            Destroy(_recycleReturnsList[i]);
            _recycleReturnsList[i] = null;
        }
        _recycleReturnsList.Clear();
    }
}
