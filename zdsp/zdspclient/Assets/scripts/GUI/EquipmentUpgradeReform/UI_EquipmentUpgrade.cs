using Kopio.JsonContracts;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Zealot.Common;
using Zealot.Client.Entities;
using Zealot.Repository;

public class UI_EquipmentUpgrade : BaseWindowBehaviour
{
    [Header("Helper")]
    public UI_EquipmentModding uiEquipModding;

    [Header("Prefabs")]
    public GameObject   equipIconPrefab;
    public GameObject   matIconPrefab;
    public Transform    selectEquipIconParent;
    public Transform    selectSafeEquipIconParent;
    public Transform    selectMatIconParent;

    [Header("Toggle Tabs")]
    public Toggle normalUpgradeTab;
    public Toggle safeUpgradeTab;

    [Header("Text")]
    public Text         equipmentNameText;
    public GameObject   equipmentNameTextObj;
    public Text         equipmentLvlText;
    public GameObject   equipmentLvlTextObj;
    public Text         probSuccessText;
    public GameObject   probSuccessTextObj;
    public Text         upgradeCostText;

    [Header("Button")]
    public Button confirmSelectEquipBtn;
    public Button confirmUpgradeBtn;

    [Header("General UI")]
    public GameObject   itemForUpgrade;
    public ToggleGroup  itemForUpgradeToggleGrp;
    public Animator     rightSideAnimator;

    [Header("RightSide Prefabs")]
    public Transform    nmlBagInvParent;
    public Transform    safeGemInvParent;
    public Transform    safeBagInvParent;
    public Transform    itemStatsParent;
    public GameObject   upgStatsValueLnPrefab;
    public GameObject   upgStatsMultiLnPrefab;
    public GameObject   upgStatsLinePrefab;

    [Header("RightSide Text")]
    public Text         selectedEquipText;

    [Header("RightSide Safe UI")]
    public ToggleGroup safeInvToggleGrp;

    [Header("RightSide Scrollviews")]
    public EquipmentModdingScrollView normalBagScrollView;
    public EquipmentModdingScrollView safeBagScrollView;

    [Header("Results")]
    public GameObject upgradeSuccessFxObj;
    public GameObject upgradeFailureFxObj;

    // Private variables
    private int                 _slotID                     = -1;
    private int                 _safeEquipSlotID            = -1;
    private bool                _isEquipped                 = false;
    private Equipment           _selectedEquipment          = null;
    private int                 _genMatSel                  = -1;
    private int                 _safeMatSel                 = -1;
    private bool                _isEquippedSafe             = false;
    private Equipment           _selectedSafeEquipment      = null;
    private string              _defaultRightSideState      = "UPG_RightSlideIn_DefaultOffScreen";
    private string              _rightSideSlideOut          = "UPG_RightSlideOut";
    private string              _rightSideSlideIn           = "UPG_RightSlideIn_Normal";
    private string              _rightSideSlideInEnd        = "UPG_RightSlideIn_NormalEnd";
    private string              _rightSideSlideInSafe       = "UPG_RightSlideIn_Safe";
    private string              _rightSideSlideInSafeEnd    = "UPG_RightSlideIn_SafeEnd";
    private GameObject          _selectedEquipmentIcon;
    private List<GameObject>    _materialIconsList;
    private GameObject          _safeEquipMatIcon;
    private List<GameObject>    _safeMaterialIconList;
    private List<GameObject>    _selectEquipBaseStatsList;
    private List<GameObject>    _selectEquipExtraStatsList;
    private List<GameObject>    _selectEquipBuffStatsList;
    
    public override void OnOpenWindow()
    {
        normalBagScrollView.InitScrollView();
        safeBagScrollView.InitScrollView();
    }

    public void InitEquipmentUpgrade()
    {
        ClearSelectedEquipment();
        ClearSafeEquipMatIcon();
        ClearMaterialIcons();

        // Set default look for the UI
        equipmentNameTextObj.SetActive(true);
        equipmentNameText.text = GUILocalizationRepo.GetLocalizedString("upg_please_select_upg_equip");
        equipmentLvlTextObj.SetActive(false);
        probSuccessTextObj.SetActive(false);

        //itemForUpgrade.SetActive(false);
        rightSideAnimator.Play(_defaultRightSideState);

        confirmUpgradeBtn.interactable = false;
        upgradeCostText.text = "0";
        upgradeCostText.color = Color.white;

        ClearSelectEquipStatsLists();
    }

    public void InitEquipmentUpgradeWithEquipment(int slotId, Equipment equipment)
    {
        ClearSelectedEquipment();
        ClearSafeEquipMatIcon();
        ClearMaterialIcons();

        // Set default look for the UI
        equipmentNameTextObj.SetActive(false);
        equipmentLvlTextObj.SetActive(false);
        probSuccessTextObj.SetActive(false);

        _slotID = slotId;
        _selectedEquipment = equipment;
        _isEquipped = true;
        _isEquippedSafe = false;
        _selectedSafeEquipment = null;
        _genMatSel = -1;
        _safeMatSel = -1;

        //itemForUpgrade.SetActive(true);
        rightSideAnimator.Play(_defaultRightSideState);

        upgradeCostText.text = "0";
        upgradeCostText.color = Color.white;

        ClearSelectEquipStatsLists();

        GenerateSelectedEquipIcon(equipment);
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
        float successProb = EquipmentModdingRepo.GetEquipmentUpgradeSuccessProb(equipType, rarity, nextLevel);
        probSuccessTextObj.SetActive(true);
        probSuccessText.text = string.Format("{0}%", successProb);

        bool isSafeUpgrade = safeUpgradeTab.isOn;

        List<EquipUpgMaterial> requiredMats = EquipmentModdingRepo.GetEquipmentUpgradeMaterials(equipType, rarity, nextLevel, isSafeUpgrade);

        if(requiredMats != null)
        {
            GenerateNormalMaterialIcons(requiredMats, selectMatIconParent);
        }

        SetEquipmentUpgradeButtonState(equipment);
    }

    public void InitEquipmentUpgradeRefresh(Equipment equipment)
    {
        //itemForUpgrade.SetActive(true);
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

        if(CheckMaxedLevel() == true)
        {
            float successProb = EquipmentModdingRepo.GetEquipmentUpgradeSuccessProb(equipType, rarity, nextLevel);
            probSuccessTextObj.SetActive(true);
            probSuccessText.text = string.Format("{0}%", successProb);
        }

        bool isSafeUpgrade = safeUpgradeTab.isOn;

        ClearSelectedEquipment();
        GenerateSelectedEquipIcon(equipment);

        List<EquipUpgMaterial> requiredMats = EquipmentModdingRepo.GetEquipmentUpgradeMaterials(equipType, rarity, nextLevel, isSafeUpgrade);

        if(requiredMats != null)
        {
            GenerateNormalMaterialIcons(requiredMats, selectMatIconParent);

            if(_genMatSel >= 0 && _genMatSel < _materialIconsList.Count)
            {
                GameObject matIconObj = _materialIconsList[_genMatSel];
                GameIcon_MaterialConsumable matIcon = matIconObj.GetComponent<GameIcon_MaterialConsumable>();
                GameIconCmpt_SelectCheckmark selectCheckMark = matIcon.GetToggleSelect();

                if(selectCheckMark != null)
                {
                    Toggle checkmarkToggle = selectCheckMark.GetToggleSelect();
                    checkmarkToggle.isOn = true;
                }
            }
        }

        if(_safeEquipSlotID != -1)
        {
            GenerateSelectedSafeEquipGemIcon(_selectedSafeEquipment);
        }

        SetEquipmentUpgradeButtonState(equipment);
    }

    public void LoadEquippedBagInventory()
    {
        PlayerGhost player = GameInfo.gLocalPlayer;
        if (player == null)
        {
            return;
        }

        List<Equipment> equippedItemList = player.mEquipmentInvData.Slots;
        List<IInventoryItem> bagItemList = player.clientItemInvCtrl.itemInvData.Slots;
        FillInventory(equippedItemList, bagItemList, false);

        selectedEquipText.text = "";
    }

    public void LoadSafeEquippedBagInventory()
    {
        PlayerGhost player = GameInfo.gLocalPlayer;
        if (player == null)
        {
            return;
        }

        int safeMatGenId = GameConstantRepo.GetConstantInt("safe_gem_general");
        int safeMatAdvId = GameConstantRepo.GetConstantInt("safe_gem_advanced");

        int safeMatGenCount = player.clientItemInvCtrl.itemInvData.GetTotalStackCountByItemId((ushort)safeMatGenId);
        int safeMatAdvCount = player.clientItemInvCtrl.itemInvData.GetTotalStackCountByItemId((ushort)safeMatAdvId);

        ClearSafeMaterialIcons();
        GenerateSafeMaterialIcons(safeMatGenId, safeMatGenCount, safeMatAdvId, safeMatAdvCount, safeGemInvParent);

        List<Equipment> equippedItemList = player.mEquipmentInvData.Slots;
        List<IInventoryItem> bagItemList = player.clientItemInvCtrl.itemInvData.Slots;
        FillInventory(equippedItemList, bagItemList, true);

        selectedEquipText.text = "";
    }

    public void OnClickOpenSelectUpgradeEquipment()
    {
        if(rightSideAnimator.GetCurrentAnimatorStateInfo(0).IsName(_rightSideSlideInEnd))
        {
            return;
        }

        rightSideAnimator.Play(_rightSideSlideIn);
        confirmSelectEquipBtn.interactable = false;
        confirmSelectEquipBtn.onClick.RemoveAllListeners();
        confirmSelectEquipBtn.onClick.AddListener(OnClickConfirmSelectUpgradeEquipment);

        LoadEquippedBagInventory();
    }

    public void OnClickOpenSelectSafeUpgradeEquipment()
    {
        if(_selectedEquipmentIcon == null)
        {
            return;
        }

        if (rightSideAnimator.GetCurrentAnimatorStateInfo(0).IsName(_rightSideSlideInSafeEnd))
        {
            return;
        }

        rightSideAnimator.Play(_rightSideSlideInSafe);
        confirmSelectEquipBtn.interactable = false;
        confirmSelectEquipBtn.onClick.RemoveAllListeners();
        confirmSelectEquipBtn.onClick.AddListener(OnClickConfirmSelectSafeUpgradeEquipment);

        LoadSafeEquippedBagInventory();
    }

    public void OnClickSelectUpgradeEquipment(bool isToggleOn, bool isEquipped, ModdingEquipment equipToUpgrade)
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

            EquipmentType   equipType   = _selectedEquipment.EquipmentJson.equiptype;
            ItemRarity      rarity      = _selectedEquipment.EquipmentJson.rarity;
            int             nextLevel   = _selectedEquipment.UpgradeLevel + 1;

            EquipmentUpgradeJson upgradeData = EquipmentModdingRepo.GetEquipmentUpgradeData(equipType, rarity, nextLevel);

            if(upgradeData == null)
            {
                return;
            }

            ClearSelectEquipStatsLists();

            List<int> buffSEIdList = EquipmentModdingRepo.GetEquipmentUpgradeBuff(equipType, rarity, nextLevel);

            if(buffSEIdList == null)
            {
                return;
            }

            GenerateEquipBaseStats(equipToUpgrade.mEquip);
            GenerateEquipExtraStats(equipToUpgrade.mEquip);
            GenerateEquipUpgBuffStats(buffSEIdList, nextLevel);

            confirmSelectEquipBtn.interactable = true;
        }
        else
        {
            ClearSelectEquipStatsLists();

            confirmSelectEquipBtn.interactable = false;
        }
    }

    private void OnClickSelectSafeUpgradeMaterial(bool isToggleOn, int safeMatSel)
    {
        if(isToggleOn)
        {
            _isEquippedSafe = false;
            _safeMatSel = safeMatSel;

            confirmSelectEquipBtn.interactable = true;

            int matId = safeMatSel == 0 ? GameConstantRepo.GetConstantInt("safe_gem_general") : GameConstantRepo.GetConstantInt("safe_gem_advanced");
            IInventoryItem material = GameRepo.ItemFactory.GetInventoryItem(matId);
            if(material != null)
            {
                selectedEquipText.text = material.JsonObject.localizedname;
            }
            else
            {
                selectedEquipText.text = "";
            }
        }
        else
        {
            confirmSelectEquipBtn.interactable = false;
        }
    }

    public void OnClickSelectSafeUpgradeEquipment(bool isToggleOn, bool isEquipped, ModdingEquipment equipToUpgrade)
    {
        PlayerGhost player = GameInfo.gLocalPlayer;

        if(player == null)
        {
            return;
        }

        if(isToggleOn)
        {
            _selectedSafeEquipment = equipToUpgrade.mEquip;
            _isEquippedSafe = isEquipped;
            if(equipToUpgrade.mSlotID != -1)
            {
                _safeEquipSlotID = equipToUpgrade.mSlotID;
            }

            confirmSelectEquipBtn.interactable = true;

            // Equipment Stats
            selectedEquipText.text = _selectedEquipment.GetEquipmentName();

            EquipmentType equipType = _selectedEquipment.EquipmentJson.equiptype;
            ItemRarity rarity = _selectedEquipment.EquipmentJson.rarity;
            int nextLevel = _selectedEquipment.UpgradeLevel + 1;

            EquipmentUpgradeJson upgradeData = EquipmentModdingRepo.GetEquipmentUpgradeData(equipType, rarity, nextLevel);

            if (upgradeData == null)
            {
                return;
            }

            ClearSelectEquipStatsLists();

            List<int> buffSEIdList = EquipmentModdingRepo.GetEquipmentUpgradeBuff(equipType, rarity, nextLevel);

            if (buffSEIdList == null)
            {
                return;
            }

            GenerateEquipBaseStats(equipToUpgrade.mEquip);
            GenerateEquipExtraStats(equipToUpgrade.mEquip);
            GenerateEquipUpgBuffStats(buffSEIdList, nextLevel);
        }
        else
        {
            confirmSelectEquipBtn.interactable = false;
        }
    }

    public void OnClickConfirmSelectUpgradeEquipment()
    {
        //rightSideAnimator.Play(_rightSideSlideOut);

        ClearSelectedEquipment();

        EquipmentType equipType = _selectedEquipment.EquipmentJson.equiptype;
        ItemRarity rarity = _selectedEquipment.EquipmentJson.rarity;
        int currentStep = _selectedEquipment.ReformStep;
        int currentLevel = _selectedEquipment.UpgradeLevel;
        int nextLevel = currentLevel + 1;

        EquipmentUpgradeJson upgradeData = EquipmentModdingRepo.GetEquipmentUpgradeData(equipType, rarity, nextLevel);

        if (upgradeData == null)
        {
            return;
        }

        bool isSafeUpgrade = safeUpgradeTab.isOn;

        ClearSelectedEquipment();
        GenerateSelectedEquipIcon(_selectedEquipment);

        LoadEquipmentData(_selectedEquipment);
    }

    public void OnClickConfirmSelectSafeUpgradeEquipment()
    {
        //rightSideAnimator.Play(_rightSideSlideOut);

        ClearSafeEquipMatIcon();

        GenerateSelectedSafeEquipGemIcon(_selectedSafeEquipment);
    }

    private void OnClickSelectGeneralUpgradeMaterial(bool isToggleOn, int genMatSel)
    {
        if (isToggleOn)
        {
            _genMatSel = genMatSel;

            if(CheckMaxedLevel() && CheckSufficientMoney() && CheckValidMat(genMatSel))
            {
                confirmUpgradeBtn.interactable = true;
            }
            else
            {
                if(!CheckSufficientMoney())
                {
                    upgradeCostText.color = ClientUtils.ColorRed;
                }
                else
                {
                    upgradeCostText.color = Color.white;
                }

                confirmUpgradeBtn.interactable = false;
            }
        }
        else
        {
            confirmUpgradeBtn.interactable = false;
        }
    }

    public void OnClickEquipmentUpgrade()
    {
        if(!CheckMaxedLevel(true) || !CheckSufficientMoney(true))
        {
            return;
        }

        PlayerGhost player = GameInfo.gLocalPlayer;
        if (player == null)
        {
            return;
        }

        // Check if already maxed out upgrade level
        int upgradeLevel = _selectedEquipment.UpgradeLevel + 1;

        bool isSafeUpgrade = safeUpgradeTab.isOn;
        EquipmentType equipType = _selectedEquipment.EquipmentJson.equiptype;
        ItemRarity rarity = _selectedEquipment.EquipmentJson.rarity;
        EquipmentUpgradeJson upgradeData = EquipmentModdingRepo.GetEquipmentUpgradeData(equipType, rarity, upgradeLevel);
        if (upgradeData == null)
        {
            // Unable to get upgrade data
            UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("ret_EquipUpgrade_UpgradeDataReadFailed"));
            return;
        }

        bool isEnoughGenMat = true;
        bool isEnoughSafeMat = true;
        // Check enough normal materials
        List<EquipUpgMaterial> generalMatList = EquipmentModdingRepo.GetEquipmentUpgradeMaterials(equipType, rarity, upgradeLevel, false);
        if (_genMatSel >= 0 && _genMatSel < generalMatList.Count)
        {
            EquipUpgMaterial selectedGenMat = generalMatList[_genMatSel];
            int genMatCount = player.clientItemInvCtrl.itemInvData.GetTotalStackCountByItemId((ushort)selectedGenMat.mMat.itemId);
            if (genMatCount < selectedGenMat.mMat.stackCount)
            {
                //UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("ret_EquipUpgrade_InsufficientGenMaterials"));
                isEnoughGenMat = false;
            }
        }

        // Check enough safe materials (if use Safe Upgrade)
        if (isSafeUpgrade)
        {
            if (_safeMatSel == -1 && _selectedSafeEquipment == null)
            {
                //UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("ret_Missing_Replace_Equip_Gem"));
                isEnoughSafeMat = false;
            }
            else if (_safeMatSel != -1 && _selectedSafeEquipment == null)
            {
                List<EquipUpgMaterial> safeMatList = EquipmentModdingRepo.GetEquipmentUpgradeMaterials(equipType, rarity, upgradeLevel, true);
                if (_safeMatSel >= 0 && _safeMatSel < safeMatList.Count)
                {
                    EquipUpgMaterial selectedSafeMat = safeMatList[_safeMatSel];
                    int safeMatCount = player.clientItemInvCtrl.itemInvData.GetTotalStackCountByItemId((ushort)selectedSafeMat.mMat.itemId);
                    if (safeMatCount < selectedSafeMat.mMat.stackCount)
                    {
                        //UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("ret_EquipUpgrade_InsufficientSafeMaterials"));
                        isEnoughGenMat = false;
                    }
                }
            }
        }

        if (!isEnoughGenMat || !isEnoughSafeMat)
        {
            OpenUpgradeItemStoreDialog(isEnoughGenMat, isEnoughSafeMat);
            return;
        }
        
        bool isGenMat = _genMatSel == 0;
        bool isSafeEquip = isSafeUpgrade && _selectedSafeEquipment != null && _safeMatSel == -1;
        bool isSafeGenMat = _safeMatSel == 0;
        if(isSafeEquip)
        {
            RPCFactory.NonCombatRPC.EquipmentUpgradeEquipment(_slotID, _isEquipped, isGenMat, isSafeUpgrade, true, isSafeGenMat, _slotID);
        }
        else
        {
            RPCFactory.NonCombatRPC.EquipmentUpgradeEquipment(_slotID, _isEquipped, isGenMat, isSafeUpgrade, false, isSafeGenMat);
        }
    }

    public void PlayEquipmentUpgradeSuccess()
    {
        if(upgradeSuccessFxObj.activeSelf == false)
        {
            upgradeSuccessFxObj.SetActive(true);
        }
    }

    public void PlayEquipmentUpgradeFailure()
    {
        if(upgradeFailureFxObj.activeSelf == false)
        {
            upgradeFailureFxObj.SetActive(true);
        }
    }

    public void Refresh()
    {
        PlayerGhost player = GameInfo.gLocalPlayer;
        if (player == null)
        {
            return;
        }

        if(_slotID == -1)
        {
            InitEquipmentUpgrade();

            return;
        }

        Equipment equipment = _isEquipped ? player.mEquipmentInvData.Slots[_slotID] as Equipment : player.clientItemInvCtrl.itemInvData.Slots[_slotID] as Equipment;

        if(equipment != null)
        {
            InitEquipmentUpgradeRefresh(equipment);
        }
        else
        {
            InitEquipmentUpgrade();
        }
    }

    public override void OnCloseWindow()
    {
        ClearSelectedEquipment();
        ClearSafeEquipMatIcon();
        ClearMaterialIcons();
        ClearSelectEquipStatsLists();

        _slotID                 = -1;
        _safeEquipSlotID        = -1;
        _isEquipped             = false;
        _selectedEquipment      = null;
        _genMatSel              = -1;
        _safeMatSel             = -1;
        _isEquippedSafe         = false;
        _selectedSafeEquipment  = null;

        //rightSideAnimator.Play(_rightSideSlideOut);
    }

    private void OpenUpgradeItemStoreDialog(bool isEnoughGenMat, bool isEnoughSafeMat)
    {
        Debug.LogError("Not enough material, opening Item Store Dialog.");
        //UIManager.OpenDialog(WindowType.DialogItemStore);
    }

    private bool CheckMaxedLevel(bool showMessage = false)
    {
        PlayerGhost player = GameInfo.gLocalPlayer;
        if (player == null)
        {
            return false;
        }

        // Check if already maxed out upgrade level
        int upgradeLevel = _selectedEquipment.UpgradeLevel + 1;
        int maxLevel = _selectedEquipment.EquipmentJson.upgradelimit;

        if (upgradeLevel > maxLevel)
        {
            // Exceeded max upgrade level
            if (showMessage)
                UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("ret_Equip_UpgradeLevelMaxed"));
            return false;
        }

        return true;
    }

    private bool CheckSufficientMoney(bool showMessage = false)
    {
        PlayerGhost player = GameInfo.gLocalPlayer;
        if (player == null)
        {
            return false;
        }

        int upgradeLevel = _selectedEquipment.UpgradeLevel + 1;

        bool isSafeUpgrade = safeUpgradeTab.isOn;
        EquipmentType equipType = _selectedEquipment.EquipmentJson.equiptype;
        ItemRarity rarity = _selectedEquipment.EquipmentJson.rarity;
        EquipmentUpgradeJson upgradeData = EquipmentModdingRepo.GetEquipmentUpgradeData(equipType, rarity, upgradeLevel);
        if (upgradeData == null)
        {
            // Unable to get upgrade data
            if (showMessage)
                UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("ret_EquipUpgrade_UpgradeDataReadFailed"));
            return false;
        }

        // Check enough gold
        int moneyCost = isSafeUpgrade ? upgradeData.safecost : upgradeData.generalcost;
        if (player.IsCurrencyEnough(CurrencyType.Money, moneyCost) == false)
        {
            if (showMessage)
                UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("sys_InsufficientMoney"));
            return false;
        }

        return true;
    }

    private bool CheckValidMat(int genMatSel, int safeMatSel = -1)
    {
        PlayerGhost player = GameInfo.gLocalPlayer;
        if(player == null)
        {
            return false;
        }

        if(genMatSel < 0 || genMatSel >= _materialIconsList.Count)
        {
            return false;
        }

        if(safeMatSel != -1 && (safeMatSel < 0 || safeMatSel >= _safeMaterialIconList.Count))
        {
            return false;
        }

        return true;
    }

    private bool IsMatSelected()
    {
        for(int i = 0; i < _materialIconsList.Count; ++i)
        {
            GameIcon_MaterialConsumable matIcon = _materialIconsList[i].GetComponent<GameIcon_MaterialConsumable>();
            if(matIcon == null)
            {
                continue;
            }

            GameIconCmpt_SelectCheckmark checkMark = matIcon.GetToggleSelect();
            if(checkMark == null)
            {
                continue;
            }

            Toggle matIconToggle = checkMark.GetToggleSelect();
            if(matIconToggle == null)
            {
                continue;
            }

            if(matIconToggle.isOn)
            {
                return true;
            }
        }

        return false;
    }

    private void SetEquipmentUpgradeButtonState(Equipment equipment)
    {
        EquipmentType equipType = _selectedEquipment.EquipmentJson.equiptype;
        ItemRarity rarity = _selectedEquipment.EquipmentJson.rarity;
        int currentStep = _selectedEquipment.ReformStep;
        int currentLevel = _selectedEquipment.UpgradeLevel;
        int nextLevel = currentLevel + 1;

        bool isSafeUpgrade = safeUpgradeTab.isOn;

        int moneyCost = 0;
        if(CheckMaxedLevel() == true)
        {
            moneyCost = EquipmentModdingRepo.GetEquipmentUpgradeCost(equipType, rarity, nextLevel, isSafeUpgrade);
        }
        upgradeCostText.text = moneyCost.ToString();

        if(CheckSufficientMoney())
        {
            upgradeCostText.color = Color.white;
        }
        else
        {
            upgradeCostText.color = ClientUtils.ColorRed;
        }

        if(CheckSufficientMoney() && IsMatSelected() == true && CheckMaxedLevel() == true)
        {
            confirmUpgradeBtn.interactable = true;
        }
        else
        {
            confirmUpgradeBtn.interactable = false;
        }
    }

    private void GenerateSelectedEquipIcon(Equipment equipment)
    {
        int currentStep = equipment.ReformStep;
        int currentLevel = equipment.UpgradeLevel;

        GameObject newEquipObj = ClientUtils.CreateChild(selectEquipIconParent, equipIconPrefab);

        GameIcon_Equip equipIcon = newEquipObj.GetComponent<GameIcon_Equip>();
        equipIcon.Init(equipment.ItemID, 0, currentStep, currentLevel, false, false, false, OnClickOpenSelectUpgradeEquipment);

        _selectedEquipmentIcon = newEquipObj;
    }

    private void GenerateSelectedSafeEquipGemIcon(Equipment equipment)
    {
        ClearSafeEquipMatIcon();

        if(_isEquippedSafe)
        {
            EquipmentType equipType = equipment.EquipmentJson.equiptype;
            ItemRarity rarity = equipment.EquipmentJson.rarity;
            int currentStep = equipment.ReformStep;
            int currentLevel = equipment.UpgradeLevel;
            int nextLevel = currentLevel + 1;

            EquipmentUpgradeJson upgradeData = EquipmentModdingRepo.GetEquipmentUpgradeData(equipType, rarity, nextLevel);

            if (upgradeData == null)
            {
                return;
            }

            GameObject newEquipObj = ClientUtils.CreateChild(selectSafeEquipIconParent, equipIconPrefab);
            
            GameIcon_Equip equipIcon = newEquipObj.GetComponent<GameIcon_Equip>();
            equipIcon.Init(_selectedEquipment.ItemID, 0, currentStep, currentLevel, false, false, 
                false, OnClickOpenSelectSafeUpgradeEquipment);

            _safeEquipMatIcon = newEquipObj;
        }
        else
        {
            PlayerGhost player = GameInfo.gLocalPlayer;

            if(player == null)
            {
                return;
            }

            GameObject newMaterialObj = Instantiate(matIconPrefab);
            newMaterialObj.transform.SetParent(selectSafeEquipIconParent, false);

            GameIcon_MaterialConsumable matIcon = newMaterialObj.GetComponent<GameIcon_MaterialConsumable>();
            int selectedMatId = _safeMatSel == 0 ? GameConstantRepo.GetConstantInt("safe_gem_general") : GameConstantRepo.GetConstantInt("safe_gem_advanced");
            int matCount = player.clientItemInvCtrl.itemInvData.GetTotalStackCountByItemId((ushort)selectedMatId);
            matIcon.Init(selectedMatId, matCount, false, false, false, OnClickOpenSelectSafeUpgradeEquipment);

            _safeEquipMatIcon = newMaterialObj;
        }
    }

    private void GenerateNormalMaterialIcons(List<EquipUpgMaterial> materialList, Transform parent)
    {
        PlayerGhost player = GameInfo.gLocalPlayer;
        if(player == null)
        {
            return;
        }

        ClearMaterialIcons();
        for(int i = 0; i < materialList.Count; ++i)
        {
            EquipUpgMaterial matData = materialList[i];
            ushort matId = matData.mMat.itemId;

            GameObject newMatObj = ClientUtils.CreateChild(parent, matIconPrefab);

            int invcount = player.clientItemInvCtrl.itemInvData.GetTotalStackCountByItemId(matId);

            GameIcon_MaterialConsumable newMatIcon = newMatObj.GetComponent<GameIcon_MaterialConsumable>();
            newMatIcon.Init(matData.mMat.itemId, matData.mMat.stackCount, false, false, true);
            newMatIcon.SetStackCount(invcount, matData.mMat.stackCount);
            GameIconCmpt_SelectCheckmark selectCheckMark = newMatIcon.gameObject.GetComponentInChildren<GameIconCmpt_SelectCheckmark>();
            if (selectCheckMark != null)
            {
                int sel = i;
                Toggle checkmarkToggle = selectCheckMark.gameObject.GetComponent<Toggle>();
                checkmarkToggle.group = itemForUpgradeToggleGrp;
                checkmarkToggle.onValueChanged.AddListener(delegate
                {
                    OnClickSelectGeneralUpgradeMaterial(checkmarkToggle.isOn, sel);
                });
            }

            _materialIconsList.Add(newMatObj);
        }
    }

    private void GenerateSafeMaterialIcons(int safeMatGenId, int safeMatGenCount, int safeMatAdvId, int safeMatAdvCount, Transform parent)
    {
        GameObject newSafeMatGenIconObj = Instantiate(matIconPrefab);
        newSafeMatGenIconObj.transform.SetParent(parent, false);

        GameIcon_MaterialConsumable safeMatGenIcon = newSafeMatGenIconObj.GetComponent<GameIcon_MaterialConsumable>();
        safeMatGenIcon.Init(safeMatGenId, safeMatGenCount, false, false, true);
        GameIconCmpt_SelectCheckmark genIconCheckmark = safeMatGenIcon.GetToggleSelect();
        if (genIconCheckmark != null)
        {
            Toggle genIconToggle = genIconCheckmark.GetToggleSelect();
            genIconToggle.onValueChanged.AddListener(delegate
            {
                OnClickSelectSafeUpgradeMaterial(genIconToggle.isOn, 0);
            });
            genIconToggle.group = safeInvToggleGrp;
        }

        _safeMaterialIconList.Add(newSafeMatGenIconObj);

        GameObject newSafeMatAdvIconObj = ClientUtils.CreateChild(parent, matIconPrefab);

        GameIcon_MaterialConsumable safeMatAdvIcon = newSafeMatAdvIconObj.GetComponent<GameIcon_MaterialConsumable>();
        safeMatAdvIcon.Init(safeMatAdvId, safeMatAdvCount, false, false, true);
        GameIconCmpt_SelectCheckmark advIconCheckmark = safeMatAdvIcon.GetToggleSelect();
        if (advIconCheckmark != null)
        {
            Toggle advIconToggle = advIconCheckmark.GetToggleSelect();
            advIconToggle.onValueChanged.AddListener(delegate
            {
                OnClickSelectSafeUpgradeMaterial(advIconToggle.isOn, 1);
            });
            advIconToggle.group = safeInvToggleGrp;
        }

        _safeMaterialIconList.Add(newSafeMatAdvIconObj);
    }

    private void FillInventory(List<Equipment> equippedEquipList, List<IInventoryItem> invEquipList, bool isSafeUpgrade)
    {
        int equippedCount = equippedEquipList.Count;

        List<ModdingEquipment> fullEquipmentList = !isSafeUpgrade ?
            uiEquipModding.GetModdingEquipmentList(equippedEquipList, invEquipList) :
            uiEquipModding.GetModdingEquipmentList(equippedEquipList, invEquipList, new ModdingEquipment(_slotID, _selectedEquipment, _isEquipped));

        if(!isSafeUpgrade)
        {
            normalBagScrollView.Populate(this, fullEquipmentList, equippedCount, isSafeUpgrade, safeInvToggleGrp);
        }
        else
        {
            safeBagScrollView.Populate(this, fullEquipmentList, equippedCount, isSafeUpgrade, safeInvToggleGrp);
        }
    }

    private void GenerateEquipBaseStats(Equipment equipment)
    {
        List<int> baseStatsIdList = ClientUtils.GetIntListFromString(equipment.EquipmentJson.basese, ';');

        if(baseStatsIdList.Count == 0)
        {
            return;
        }

        EquipmentType equipType = equipment.EquipmentJson.equiptype;
        ItemRarity rarity = equipment.EquipmentJson.rarity;
        int upgradeLevel = equipment.UpgradeLevel;
        int nextLevel = upgradeLevel + 1;
        int upgradeLimit = equipment.EquipmentJson.upgradelimit;

        if(upgradeLimit == -1)
        {
            return;
        }

        for (int i = 0; i < baseStatsIdList.Count; ++i)
        {
            int seId = baseStatsIdList[i];
            SideEffectJson sideeffect = SideEffectRepo.GetSideEffect(seId);

            GameObject newStatsLine = ClientUtils.CreateChild(itemStatsParent, upgStatsValueLnPrefab);

            EquipmentUpgradeJson upgradeData = EquipmentModdingRepo.GetEquipmentUpgradeData(equipType, rarity, upgradeLevel);
            int increase = upgradeData == null ? 0 : (int)(sideeffect.max * upgradeData.increase);

            EquipmentUpgradeStats upgradeStatsObj = newStatsLine.GetComponent<EquipmentUpgradeStats>();
            if (upgradeStatsObj != null)
            {
                upgradeStatsObj.Init(upgradeLevel, upgradeLimit, sideeffect.localizedname, increase);
            }

            _selectEquipBaseStatsList.Add(newStatsLine);
        }
    }

    private void GenerateEquipExtraStats(Equipment equipment)
    {
        List<int> extraSEIdList = ClientUtils.GetIntListFromString(equipment.EquipmentJson.extrase, ';');

        for(int i = 0; i < extraSEIdList.Count; ++i)
        {
            int extraSEId = extraSEIdList[i];
            List<int> extraStatsIdList = EquipmentModdingRepo.GetEquipmentExtraSideEffectsList(extraSEId);

            if (extraStatsIdList.Count == 0)
            {
                continue;
            }

            EquipmentType equipType = equipment.EquipmentJson.equiptype;
            ItemRarity rarity = equipment.EquipmentJson.rarity;
            int upgradeLevel = equipment.UpgradeLevel;
            int nextLevel = upgradeLevel + 1;
            int upgradeLimit = equipment.EquipmentJson.upgradelimit;


            for(int j = 0; j < extraStatsIdList.Count; ++j)
            {
                int seId = extraStatsIdList[j];
                SideEffectJson sideeffect = SideEffectRepo.GetSideEffect(seId);

                GameObject newStatsLine = ClientUtils.CreateChild(itemStatsParent, upgStatsValueLnPrefab);

                EquipmentUpgradeJson upgradeData = EquipmentModdingRepo.GetEquipmentUpgradeData(equipType, rarity, upgradeLevel);
                int increase = upgradeData == null ? 0 : (int)(sideeffect.max * upgradeData.increase);

                EquipmentUpgradeStats upgradeStatsObj = newStatsLine.GetComponent<EquipmentUpgradeStats>();
                if (upgradeStatsObj != null)
                {
                    upgradeStatsObj.Init(upgradeLevel, upgradeLimit, sideeffect.localizedname, increase);
                }

                _selectEquipExtraStatsList.Add(newStatsLine);
            }
        }
    }

    private void GenerateEquipUpgBuffStats(List<int> buffSEIdList, int nextLevel)
    {
        if(buffSEIdList.Count == 0)
        {
            //GameObject emptyStatsEndLine = Instantiate(upgStatsLinePrefab);
            //emptyStatsEndLine.transform.SetParent(itemStatsParent, false);
            //_selectEquipStatsList.Add(emptyStatsEndLine);

            return;
        }

        for(int i = 0; i < buffSEIdList.Count; ++i)
        {
            int seId = buffSEIdList[i];
            SideEffectJson sideeffect = SideEffectRepo.GetSideEffect(seId);

            GameObject newStatsLine = ClientUtils.CreateChild(itemStatsParent, upgStatsValueLnPrefab);

            EquipmentUpgradeStats upgradeStatsObj = newStatsLine.GetComponent<EquipmentUpgradeStats>();
            if(upgradeStatsObj != null)
            {
                upgradeStatsObj.Init(EquipmentUpgradeStatsType.ToGet, nextLevel, sideeffect.localizedname);
            }

            _selectEquipBuffStatsList.Add(newStatsLine);
        }

        GameObject newStatsEndLine = Instantiate(upgStatsLinePrefab);
        newStatsEndLine.transform.SetParent(itemStatsParent, false);
        _selectEquipBuffStatsList.Add(newStatsEndLine);
    }

    private List<Equipment> GetEquipmentFromPlayerData(PlayerGhost player, Equipment selectedEquipment = null)
    {
        List<Equipment> slotEquipmentList = player.mEquipmentInvData.Slots;
        List<Equipment> equippedItemList = new List<Equipment>();
        if (selectedEquipment == null)
        {
            for (int i = 0; i < slotEquipmentList.Count; ++i)
            {
                Equipment equipment = slotEquipmentList[i];
                if (equipment != null)
                {
                    equippedItemList.Add(equipment);
                }
            }
        }
        else
        {
            for (int i = 0; i < slotEquipmentList.Count; ++i)
            {
                Equipment currEquip = slotEquipmentList[i] as Equipment;
                if (currEquip == null)
                {
                    continue;
                }

                if (selectedEquipment.ItemID == currEquip.ItemID)
                {
                    equippedItemList.Add(currEquip);
                }
            }
        }

        return equippedItemList;
    }

    private List<Equipment> GetItemInvListInEquipment(List<IInventoryItem> itemList, Equipment selectedEquipment = null)
    {
        List<Equipment> equipmentList = new List<Equipment>();
        if (selectedEquipment == null)
        {
            for (int i = 0; i < itemList.Count; ++i)
            {
                Equipment currEquip = itemList[i] as Equipment;
                if (currEquip != null)
                {
                    equipmentList.Add(currEquip);
                }
            }
        }
        else
        {
            for (int i = 0; i < itemList.Count; ++i)
            {
                Equipment currEquip = itemList[i] as Equipment;
                if(currEquip == null)
                {
                    continue;
                }

                if(selectedEquipment.ItemID == currEquip.ItemID)
                {
                    equipmentList.Add(currEquip);
                }
            }
        }

        return equipmentList;
    }

    private void ClearSelectedEquipment()
    {
        Destroy(_selectedEquipmentIcon);
        _selectedEquipmentIcon = null;
    }

    private void ClearMaterialIcons()
    {
        if(_materialIconsList == null)
        {
            _materialIconsList = new List<GameObject>();
            return;
        }

        for(int i = 0; i < _materialIconsList.Count; ++i)
        {
            Destroy(_materialIconsList[i]);
            _materialIconsList[i] = null;
        }
        _materialIconsList.Clear();
    }

    private void ClearSafeEquipMatIcon()
    {
        if(_safeEquipMatIcon != null)
        {
            Destroy(_safeEquipMatIcon);
            _safeEquipMatIcon = null;
        }
    }

    private void ClearSafeMaterialIcons()
    {
        if (_safeMaterialIconList == null)
        {
            _safeMaterialIconList = new List<GameObject>();
            return;
        }

        for (int i = 0; i < _safeMaterialIconList.Count; ++i)
        {
            Destroy(_safeMaterialIconList[i]);
            _safeMaterialIconList[i] = null;
        }
        _safeMaterialIconList.Clear();
    }

    public void ClearSelectEquipStatsLists()
    {
        ClearSelectEquipBaseStatsList();
        ClearSelectEquipExtraStatsList();
        ClearSelectEquipBuffStatsList();
    }

    public void ClearSelectEquipBaseStatsList()
    {
        if (_selectEquipBaseStatsList == null)
        {
            _selectEquipBaseStatsList = new List<GameObject>();
            return;
        }

        for (int i = 0; i < _selectEquipBaseStatsList.Count; ++i)
        {
            Destroy(_selectEquipBaseStatsList[i]);
            _selectEquipBaseStatsList[i] = null;
        }
        _selectEquipBaseStatsList.Clear();
    }

    public void ClearSelectEquipExtraStatsList()
    {
        if (_selectEquipExtraStatsList == null)
        {
            _selectEquipExtraStatsList = new List<GameObject>();
            return;
        }

        for (int i = 0; i < _selectEquipExtraStatsList.Count; ++i)
        {
            Destroy(_selectEquipExtraStatsList[i]);
            _selectEquipExtraStatsList[i] = null;
        }
        _selectEquipExtraStatsList.Clear();
    }

    public void ClearSelectEquipBuffStatsList()
    {
        if(_selectEquipBuffStatsList == null)
        {
            _selectEquipBuffStatsList = new List<GameObject>();
            return;
        }

        for(int i = 0; i < _selectEquipBuffStatsList.Count; ++i)
        {
            Destroy(_selectEquipBuffStatsList[i]);
            _selectEquipBuffStatsList[i] = null;
        }
        _selectEquipBuffStatsList.Clear();
    }
}