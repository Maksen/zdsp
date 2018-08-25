using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Client.Entities;
using Zealot.Repository;
using Kopio.JsonContracts;

public class InvDisplayItem
{
    public int OriginSlotId = 0;
    public IInventoryItem InvItem = null; // Reference to inventory item
    public int OriginStackCount = 0;      // For reference when selling
    public int DisplayStackCount = 0;
}

public class UI_Inventory : BaseWindowBehaviour
{
    [Header("Top Section")]
    [SerializeField]
    UI_Currency uiCurrencyMoney = null;
    [SerializeField]
    UI_Currency uiCurrencyGold = null;

    [Header("Left Section")]
    [SerializeField]
    Model_3DAvatar avatar = null;
    [SerializeField]
    Toggle toggleEquipFashion = null;
    [SerializeField]
    Toggle hideHelm = null;
    [SerializeField]
    GameObject[] equipSlots = null;
    [SerializeField]
    GameObject[] fashionSlots = null;
    [SerializeField]
    GameObject equipIconPrefab = null;

    [Header("Right Section")]
    [SerializeField]
    Text txtBagCount = null;
    [SerializeField]
    InventoryScrollView invScrollView = null;
    [SerializeField]
    DefaultToggleInGroup defaultToggleingrpInvTabs = null;
    [SerializeField]
    Toggle toggleCheckboxQuickSlot = null;

    public Button ButtonSell = null;
    public Button ButtonPowerup = null;

    [Header("UI Reference")]   
    public UI_Inventory_SellPanel InvSellPanel = null;
    public UI_Inventory_QuickSlot InvQuickSlot = null;

    public BagType CurrentInventoryTab { get; private set; }
    public List<InvDisplayItem> DisplayItemList { get; private set; }

    [NonSerialized]
    public bool InitOnEnable = true;

    // Use this for initialization
    void Awake()
    {
        CurrentInventoryTab = BagType.Any;
        DisplayItemList = new List<InvDisplayItem>();
        int length = equipSlots.Length;
        for (int index = 0; index < length; ++index)
        {
            int slotIdx = index;
            GameObject _icon = Instantiate(equipIconPrefab);
            _icon.name = "GameIcon_Equip";
            _icon.transform.SetParent(equipSlots[index].transform, false);
            _icon.SetActive(false);
            _icon.GetComponent<GameIcon_Equip>().SetClickCallback(() => OnEquipmentSlotClickedCB(slotIdx));
        }
        length = fashionSlots.Length;
        for (int index = 0; index < length; ++index)
        {
            int slotIdx = index;
            GameObject _icon = Instantiate(equipIconPrefab);
            _icon.name = "GameIcon_Equip";
            _icon.transform.SetParent(fashionSlots[index].transform, false);
            _icon.SetActive(false);
            _icon.GetComponent<GameIcon_Equip>().SetClickCallback(() => OnFashionSlotClickedCB(slotIdx));
        }
    }

    void OnEnable()
    {
        if (InitOnEnable)
            Init(BagType.Any);
    }

    void OnDisable()
    {
        InvSellPanel.gameObject.SetActive(false);
        toggleCheckboxQuickSlot.isOn = false;

        invScrollView.Clear();

        DisplayItemList.Clear();
        InitOnEnable = true;
    }

    public override void OnShowWindow()
    {
        base.OnShowWindow();
        
        // Need to re-play standby animation because animation is reset as model is disabled when UI canvas is hidden
        PlayerGhost player = GameInfo.gLocalPlayer;
        if (player != null)
            avatar.PlayAnimation(ClientUtils.GetStandbyAnimationByWeaponType(player.WeaponTypeUsed));
    }

    public void Init(BagType inventoryTab)
    {
        InvSellPanel.UIInventory = this;
        InvQuickSlot.UIInventory = this;

        PlayerGhost player = GameInfo.gLocalPlayer;
        if (player != null)
        {
            invScrollView.InitScrollView(this);
            if (CurrentInventoryTab == inventoryTab)
            {
                toggleCheckboxQuickSlot.isOn = (CurrentInventoryTab == BagType.Consumable);
                RefreshRight(player);
            }
            else
                defaultToggleingrpInvTabs.GoToPage((byte)inventoryTab);

            RefreshLeft(player);
            hideHelm.isOn = !player.mEquipmentInvData.HideHelm;
        }
    }

    #region Inventory right side

    public void SetUsedSlotsCount(int usedSlotCount, int UnlockSlotCount)
    {
        txtBagCount.text = string.Format("{0}/{1}", usedSlotCount, UnlockSlotCount);
    }

    public int GetUsedSlotsCount(List<IInventoryItem> itemList)
    {
        int usedSlots = 0, count = itemList.Count;
        for (int i = 0; i < count; ++i)
        {
            if (itemList[i] != null)
                ++usedSlots;
        }
        return usedSlots;
    }

    public void UpdateDisplayItemListByBagType(List<IInventoryItem> invItemList)
    {
        DisplayItemList.Clear();

        int invItemListCnt = invItemList.Count;
        List<Dictionary<int, int>> sellRefList = InvSellPanel.SellRefList;
        List<InvDisplayItem> sellItemList = InvSellPanel.SellItemList;
        int sellRefListCnt = (sellRefList != null) ? sellRefList.Count : 0;
        for (int i = 0; i < invItemListCnt; ++i)
        {
            IInventoryItem invItem = invItemList[i];
            if (CurrentInventoryTab == BagType.Any || (invItem != null && CurrentInventoryTab == invItem.JsonObject.bagtype))
            {
                int stackCount = (invItem != null) ? invItem.StackCount : 0;
                for (int j = 0; j < sellRefListCnt; ++j)
                {
                    Dictionary<int, int> refDict = sellRefList[j];            
                    if (refDict.ContainsKey(i))
                    {
                        InvDisplayItem invDisplayItem = sellItemList[j];
                        int result = invDisplayItem.OriginStackCount - invDisplayItem.DisplayStackCount;
                        if (stackCount == result || stackCount == 0)
                            refDict.Remove(i);
                        else
                            stackCount -= refDict[i];
                    }
                }

                if (stackCount == 0)
                    invItem = null;
                if (CurrentInventoryTab == BagType.Any || invItem != null)
                    DisplayItemList.Add(new InvDisplayItem { OriginSlotId = i , InvItem = invItem,
                                                             OriginStackCount = stackCount,
                                                             DisplayStackCount = stackCount });
            }
        }
    }

    public void RefreshRight(PlayerGhost player)
    {
        List<IInventoryItem> invItemList = player.clientItemInvCtrl.itemInvData.Slots;
        SetUsedSlotsCount(GetUsedSlotsCount(invItemList),
                          player.SecondaryStats.UnlockedSlotCount);  

        UpdateDisplayItemListByBagType(invItemList);

        if (InvSellPanel.gameObject.activeInHierarchy)
            InvSellPanel.UpdateSellListFromRefList();

        invScrollView.PopulateRows();
    }

    public void UpdateVisibleInvRows(bool toggleQuickSlot = true)
    {
        PlayerGhost player = GameInfo.gLocalPlayer;
        if (player != null)
        {
            if (toggleQuickSlot)
                toggleCheckboxQuickSlot.isOn = (CurrentInventoryTab == BagType.Consumable);
            RefreshRight(player);
        }
    }

    public void UpdateScrollViewCallback()
    {
        invScrollView.UpdateVisibleRowsCallback();
    }

    public void UpdateCurrencyAmount(CurrencyType type)
    {
        if (type == CurrencyType.Money)
            uiCurrencyMoney.UpdateCurrencyAmount();
        else
            uiCurrencyGold.UpdateCurrencyAmount();
    }

    public void OnValueChangedInventoryTab(int index)
    {
        if (CurrentInventoryTab == (BagType)index)
            return;

        CurrentInventoryTab = (BagType)index;
        UpdateVisibleInvRows();
    }

    public void OnValueChangedToggleQuickSlot(bool value)
    {
        if (value)
        {
            if (CurrentInventoryTab != BagType.Consumable)
                defaultToggleingrpInvTabs.GoToPage((byte)BagType.Consumable);
        }
    }

    public void OnClickSortInventory()
    {
        RPCFactory.CombatRPC.SortItem();
    }

    public void OnClickOpenSellPanel()
    {
        InvSellPanel.gameObject.SetActive(true);
        UpdateScrollViewCallback();
    }

    #endregion

    #region Inventory left side
    public void RefreshLeft(PlayerGhost player)
    {
        EquipmentInventoryData equipmentInvData = player.mEquipmentInvData;
        avatar.Change(equipmentInvData, (JobType)player.PlayerSynStats.jobsect, player.mGender);
        RefreshEquipIcons(equipmentInvData.Slots, equipSlots);
        RefreshEquipIcons(equipmentInvData.Fashions, fashionSlots);
    }

    private void RefreshEquipIcons(List<Equipment> _equipmentSlots, GameObject[] _uiSlots)
    {
        Equipment _item;
        GameObject _baseIcon, _equipIcon;
        int length = _uiSlots.Length;
        for (int index = 0; index < length; ++index)
        {
            _item = _equipmentSlots[index];
            _baseIcon = _uiSlots[index].transform.Find("Image_Base").gameObject;
            _equipIcon = _uiSlots[index].transform.Find("GameIcon_Equip").gameObject;
            if (_item == null)
            {
                _baseIcon.SetActive(true);
                _equipIcon.SetActive(false);
            }
            else
            {
                _baseIcon.SetActive(false);
                _equipIcon.SetActive(true);
                _equipIcon.GetComponent<GameIcon_Equip>().InitWithoutCallback(_item.ItemID, 0, 0, _item.UpgradeLevel);
            }
        }
    }

    public void OnHelmToggleChanged(bool ison)
    {
        PlayerGhost player = GameInfo.gLocalPlayer;
        bool hidehelm = !ison;
        if (player != null && player.mEquipmentInvData.HideHelm != hidehelm)
            RPCFactory.NonCombatRPC.HideHelm(hidehelm);
    }

    private void OnEquipmentSlotClickedCB(int slotid)
    {
        PlayerGhost player = GameInfo.gLocalPlayer;
        var _item = player.mEquipmentInvData.GetEquipmentBySlotId(slotid);
        if (_item != null)
            UIManager.OpenDialog(WindowType.DialogItemDetail, (window) => {
                OnClicked_InitTooltipEquipment(window.GetComponent<UI_DialogItemDetailToolTip>(), slotid, _item, false);
            });
    }

    private void OnFashionSlotClickedCB(int slotid)
    {
        PlayerGhost player = GameInfo.gLocalPlayer;
        var _item = player.mEquipmentInvData.GetFashionSlot(slotid);
        if (_item != null)
            UIManager.OpenDialog(WindowType.DialogItemDetail, (window) => {
                OnClicked_InitTooltipEquipment(window.GetComponent<UI_DialogItemDetailToolTip>(), slotid, _item, true);
            });
    }
    #endregion

    #region OnClickInventoryItem
    public void OnClicked_InventoryItem(int slotid, IInventoryItem item)
    {
        UIManager.OpenDialog(WindowType.DialogItemDetail, (window) => {
            OnClicked_InitTooltip(window.GetComponent<UI_DialogItemDetailToolTip>(), slotid, item);
        });
    }

    private void OnClicked_InitTooltipEquipment(UI_DialogItemDetailToolTip component, int slotid, IInventoryItem item, bool fashionslot)
    {
        component.InitTooltip(item);
        List<ItemDetailsButton> _buttons = new List<ItemDetailsButton>();

        var _equipmentJson = (EquipmentJson)item.JsonObject;
        if (!string.IsNullOrEmpty(_equipmentJson.socketspace) && _equipmentJson.socketspace != "-1")
            AddSocket(_buttons, item);
        if (!string.IsNullOrEmpty(_equipmentJson.evolvegrp) && _equipmentJson.evolvegrp != "-1")
            AddEvolve(_buttons, item);
        if (_equipmentJson.upgradelimit > 0)
            AddUpgrade(_buttons, item);
        AddUnEquip(_buttons, slotid, item, fashionslot);

        component.SetButtonCallback(_buttons);
    }

    private void OnClicked_InitTooltip(UI_DialogItemDetailToolTip component, int slotid, IInventoryItem item)
    {
        component.InitTooltip(item);
        List<ItemDetailsButton> _buttons = new List<ItemDetailsButton>();
        switch (item.JsonObject.itemtype)
        {
            case ItemType.PotionFood:
            case ItemType.LuckyPick:
            case ItemType.Henshin:
                AddUse(_buttons, slotid, item);
                break;
            case ItemType.Material:
                var _materialJson = (MaterialJson)item.JsonObject;
                if (_materialJson.mattype == MaterialType.Exchange)
                    AddExchange(_buttons, slotid, item);
                else if (_materialJson.mattype == MaterialType.UpgradeItem)
                    AddUpgrade(_buttons, item);
                else if (_materialJson.mattype == MaterialType.Special)
                    AddUse(_buttons, slotid, item);
                else if (_materialJson.mattype == MaterialType.Token)
                    AddUILink(_buttons, item, _materialJson.uiid);
                break;
            case ItemType.Features:
                AddUILink(_buttons, item, ((FeaturesJson)item.JsonObject).uiid);
                break;
            case ItemType.Equipment:
                var _equipmentJson = (EquipmentJson)item.JsonObject;
                if (!string.IsNullOrEmpty(_equipmentJson.socketspace) && _equipmentJson.socketspace != "-1")
                    AddSocket(_buttons, item);
                if (!string.IsNullOrEmpty(_equipmentJson.evolvegrp) && _equipmentJson.evolvegrp != "-1")
                    AddEvolve(_buttons, item);
                if (_equipmentJson.upgradelimit > 0)
                    AddUpgrade(_buttons, item);
                AddEquip(_buttons, slotid, item);
                break;
            case ItemType.DNA:
                AddDNA(_buttons, item);
                break;
            case ItemType.Relic:
                AddSocket(_buttons, item);
                AddAchievement(_buttons, item);
                break;
            case ItemType.QuestItem:
                AddUse(_buttons, slotid, item);
                break;
            case ItemType.MercenaryItem:                    
                AddHeroUI(_buttons, slotid, item);
                AddHeroGift(_buttons, slotid, item);
                AddHeroSkin(_buttons, slotid, item);
                break;
            case ItemType.InstanceItem:
                AddTeleport(_buttons, slotid, item);
                break;
        }
        component.SetButtonCallback(_buttons);
    }

    private void AddUse(List<ItemDetailsButton> buttons, int slotid, IInventoryItem item)
    {
        ItemDetailsButton _button = new ItemDetailsButton();
        _button.name = GUILocalizationRepo.GetLocalizedString("ItemTooltipButton_Use");
        _button.icon = "ButtonB_UseItem";
        _button.callback = () => GameInfo.gLocalPlayer.clientItemInvCtrl.OnClicked_UseItem(slotid, item);
        buttons.Add(_button);
    }

    private void AddExchange(List<ItemDetailsButton> buttons, int slotid, IInventoryItem item)
    {
        ItemDetailsButton _button = new ItemDetailsButton();
        _button.name = GUILocalizationRepo.GetLocalizedString("ItemTooltipButton_Exchange");
        _button.icon = "ButtonB_UseItem"; //no exchange icon
        _button.callback = () => GameInfo.gLocalPlayer.clientItemInvCtrl.OnClicked_UseItem(slotid, item);
        buttons.Add(_button);
    }

    private void AddUpgrade(List<ItemDetailsButton> buttons, IInventoryItem item)
    {
        ItemDetailsButton _button = new ItemDetailsButton();
        _button.name = GUILocalizationRepo.GetLocalizedString("ItemTooltipButton_OpenUpgradeUI");
        _button.icon = "ButtonB_Upgrade";
        _button.callback = () => ClientUtils.OpenUIWindowByLinkUI(LinkUIType.Equipment_Upgrade);
        buttons.Add(_button);
    }

    private void AddSocket(List<ItemDetailsButton> buttons, IInventoryItem item)
    {
        ItemDetailsButton _button = new ItemDetailsButton();
        _button.name = GUILocalizationRepo.GetLocalizedString("ItemTooltipButton_OpenRelicUI");
        _button.icon = "ButtonB_Socket";
        _button.callback = () => ClientUtils.OpenUIWindowByLinkUI(LinkUIType.Equipment_Socket);
        buttons.Add(_button);
    }

    private void AddEvolve(List<ItemDetailsButton> buttons, IInventoryItem item)
    {
        ItemDetailsButton _button = new ItemDetailsButton();
        _button.name = GUILocalizationRepo.GetLocalizedString("ItemTooltipButton_OpenEvolveUI");
        _button.icon = "ButtonB_Evolve";
        _button.callback = () => ClientUtils.OpenUIWindowByLinkUI(LinkUIType.Equipment_Reform);
        buttons.Add(_button);
    }

    private void AddDNA(List<ItemDetailsButton> buttons, IInventoryItem item)
    {
        ItemDetailsButton _button = new ItemDetailsButton();
        _button.name = GUILocalizationRepo.GetLocalizedString("ItemTooltipButton_OpenGeneUI");
        _button.icon = "ButtonB_UseItem";
        _button.callback = () => ClientUtils.OpenUIWindowByLinkUI(LinkUIType.DNA);
        buttons.Add(_button);
    }

    private void AddAchievement(List<ItemDetailsButton> buttons, IInventoryItem item)
    {
        ItemDetailsButton _button = new ItemDetailsButton();
        _button.name = GUILocalizationRepo.GetLocalizedString("ItemTooltipButton_OpenAchievementUI");
        _button.icon = "ButtonB_UseItem";
        _button.callback = () => ClientUtils.OpenUIWindowByLinkUI(LinkUIType.Achievement);
        buttons.Add(_button);
    }

    private void AddHeroUI(List<ItemDetailsButton> buttons, int slotid, IInventoryItem item)
    {
        HeroItemJson heroItemJson = (HeroItemJson)item.JsonObject;
        ItemDetailsButton _button = new ItemDetailsButton();
        _button.name = GUILocalizationRepo.GetLocalizedString("ItemTooltipButton_OpenHeroUI");
        _button.icon = "ButtonB_UseItem";
        _button.callback = () =>
        {
            switch (heroItemJson.heroitemtype)
            {
                case HeroItemType.Shard:
                case HeroItemType.HeroSkin:
                    int _heroid;
                    if (int.TryParse(heroItemJson.heroid, out _heroid) && _heroid > 0)
                        ClientUtils.OpenUIWindowByLinkUI(LinkUIType.Hero, _heroid.ToString());
                    else
                        ClientUtils.OpenUIWindowByLinkUI(LinkUIType.Hero);
                    break;
                case HeroItemType.Gift:
                    ClientUtils.OpenUIWindowByLinkUI(LinkUIType.Hero);
                    break;
                case HeroItemType.Onigiri:
                    ClientUtils.OpenUIWindowByLinkUI(LinkUIType.Hero_Explore);
                    break;
            }
        };
        buttons.Add(_button);
    }

    private void AddHeroGift(List<ItemDetailsButton> buttons, int slotid, IInventoryItem item)
    {
        HeroItemJson heroItemJson = (HeroItemJson)item.JsonObject;
        if (heroItemJson.heroitemtype != HeroItemType.Gift || heroItemJson.ischangelike != 0 || string.IsNullOrEmpty(heroItemJson.heroid))
            return;

        ItemDetailsButton _button = new ItemDetailsButton();
        buttons.Add(_button);
        int _heroid;
        if (int.TryParse(heroItemJson.heroid, out _heroid) && _heroid > 0)
        {
            _button.name = GUILocalizationRepo.GetLocalizedString("ItemTooltipButton_DirectGift");
            _button.icon = "ButtonB_UseItem";
            _button.callback = () => GameInfo.gLocalPlayer.clientItemInvCtrl.OnClicked_UseItem(slotid, item);
        }
        else
        {
            _button.name = GUILocalizationRepo.GetLocalizedString("ItemTooltipButton_GoToGift");
            _button.icon = "ButtonB_UseItem";
            _button.callback = () => ClientUtils.OpenUIWindowByLinkUI(LinkUIType.Hero);
        }
    }

    private void AddHeroSkin(List<ItemDetailsButton> buttons, int slotid, IInventoryItem item)
    {
        HeroItemJson heroItemJson = (HeroItemJson)item.JsonObject;
        if (heroItemJson.heroitemtype != HeroItemType.HeroSkin || string.IsNullOrEmpty(heroItemJson.heroid) || heroItemJson.heroid == "-1")
            return;

        ItemDetailsButton _button = new ItemDetailsButton();
        buttons.Add(_button);
        int _heroid;
        if (int.TryParse(heroItemJson.heroid, out _heroid))
        {
            _button.name = GUILocalizationRepo.GetLocalizedString("ItemTooltipButton_GetHeroSkin");
            _button.icon = "ButtonB_UseItem";
            _button.callback = () => GameInfo.gLocalPlayer.clientItemInvCtrl.OnClicked_UseItem(slotid, item);
        }
    }

    private void AddTeleport(List<ItemDetailsButton> buttons, int slotid, IInventoryItem item)
    {
        string _coordinate = ((InstanceItemJson)item.JsonObject).coordinate;
        if (string.IsNullOrEmpty(_coordinate) || _coordinate == "-1")
            return;
        ItemDetailsButton _button = new ItemDetailsButton();
        _button.name = GUILocalizationRepo.GetLocalizedString("ItemTooltipButton_Transfer");
        _button.icon = "ButtonB_UseItem";
        _button.callback = () => GameInfo.gLocalPlayer.clientItemInvCtrl.OnClicked_UseItem(slotid, item);
        buttons.Add(_button);
    }

    private void AddEquip(List<ItemDetailsButton> buttons, int slotid, IInventoryItem item)
    {
        ItemDetailsButton _button = new ItemDetailsButton();
        _button.name = GUILocalizationRepo.GetLocalizedString("ItemTooltipButton_Equip");
        _button.icon = "ButtonB_Equip";
        _button.callback = () =>
        {
            var _equipmentJson = (EquipmentJson)item.JsonObject;
            bool _fashionOn = !toggleEquipFashion.isOn;
            if (_fashionOn) //fashion slots visible
            {
                if (!InventoryHelper.HasFashionSlot(_equipmentJson.partstype))
                {
                    UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("ret_EquipFail_NotFashionPart"));
                    return;
                }
            }
            else  //equip slots visible 
            {
                if (_equipmentJson.fashionsuit)
                {
                    UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("ret_EquipFail_FashionOnly"));
                    return;
                }
            }
            GameInfo.gLocalPlayer.clientItemInvCtrl.OnClicked_Equip(slotid, item, _fashionOn);
        };
        buttons.Add(_button);
    }

    private void AddUnEquip(List<ItemDetailsButton> buttons, int slotid, IInventoryItem item, bool fashionslot)
    {
        ItemDetailsButton _button = new ItemDetailsButton();
        _button.name = GUILocalizationRepo.GetLocalizedString("ItemTooltipButton_UnEquip");
        _button.icon = "ButtonB_UnEquip";
        _button.callback = () => GameInfo.gLocalPlayer.clientItemInvCtrl.OnClicked_UnEquip(slotid, item, fashionslot);
        buttons.Add(_button);
    }

    private void AddUILink(List<ItemDetailsButton> buttons, IInventoryItem item, int uiid)
    {
        LinkUIJson _linkUIJson = GameRepo.ItemFactory.GetLinkUI(uiid);
        if (_linkUIJson != null)
        {
            ItemDetailsButton _button = new ItemDetailsButton();
            _button.name = _linkUIJson.localizedname;
            _button.icon = "ButtonB_UseItem"; //no ui link icon
            _button.callback = () => ClientUtils.OpenUIWindowByLinkUI(_linkUIJson.uitype);
            buttons.Add(_button);
        }
    }
    #endregion
}
