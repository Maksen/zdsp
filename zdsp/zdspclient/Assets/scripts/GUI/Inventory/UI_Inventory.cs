using Kopio.JsonContracts;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Client.Entities;
using Zealot.Repository;

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
    Image imgJobIcon = null;
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

    public BagTabType CurrentInventoryTab { get; private set; }
    public List<InvDisplayItem> DisplayItemList { get; private set; }

    [NonSerialized]
    public bool InitOnEnable = true;

    GameTimer invSortCooldown = null;
    DNAType dnaTypeFilter = DNAType.None;

    // Use this for initialization
    void Awake()
    {
        CurrentInventoryTab = BagTabType.All;
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
            Init(BagTabType.All);
    }

    void OnDisable()
    {
        InvSellPanel.gameObject.SetActive(false);
        invScrollView.Clear();
        DisplayItemList.Clear();
        toggleCheckboxQuickSlot.isOn = false;
        dnaTypeFilter = DNAType.None;

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

    public void Init(BagTabType inventoryTab, DNAType dnaTypeFilter = DNAType.None)
    {
        InvSellPanel.UIInventory = this;
        InvQuickSlot.UIInventory = this;

        PlayerGhost player = GameInfo.gLocalPlayer;
        if (player != null)
        {
            invScrollView.InitScrollView(this);
            this.dnaTypeFilter = dnaTypeFilter;
            if (CurrentInventoryTab == inventoryTab)
            {
                toggleCheckboxQuickSlot.isOn = (CurrentInventoryTab == BagTabType.Consumable);
                RefreshRight(player);  
            }
            defaultToggleingrpInvTabs.GoToPage((byte)inventoryTab);

            ClientUtils.LoadJobIconAsync(player.GetJobSect(), OnJobIconLoaded);
            RefreshLeft(player);
            StartCoroutine(InitOnNextFrame(player));
        }
    }

    IEnumerator InitOnNextFrame(PlayerGhost player)
    {
        yield return null;
        hideHelm.isOn = !player.mEquipmentInvData.HideHelm;
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
            if (CurrentInventoryTab == BagTabType.All || (invItem != null && CurrentInventoryTab == invItem.ItemSortJson.bagtabtype))
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

                bool showItem = true;
                if (invItem == null || (invItem.JsonObject.itemtype == ItemType.DNA && dnaTypeFilter != DNAType.None &&
                                        dnaTypeFilter != ((DNA)invItem).DNAJson.dnatype))
                {
                    showItem = false;
                }
                if (CurrentInventoryTab == BagTabType.All || showItem)
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

    public void UpdateVisibleInvRows(bool toggleQuickSlot)
    {
        PlayerGhost player = GameInfo.gLocalPlayer;
        if (player != null)
        {
            if (toggleQuickSlot)
                toggleCheckboxQuickSlot.isOn = (CurrentInventoryTab == BagTabType.Consumable);
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
        if (CurrentInventoryTab == (BagTabType)index)
            return;

        CurrentInventoryTab = (BagTabType)index;
        UpdateVisibleInvRows(true);
    }

    public void OnValueChangedToggleQuickSlot(bool value)
    {
        if (value && CurrentInventoryTab != BagTabType.Consumable)
            defaultToggleingrpInvTabs.GoToPage((byte)BagTabType.Consumable);
    }

    public void OnClickSortInventory()
    {
        if (invSortCooldown != null)
        {
            UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("ret_OnCooldown"));
            return;
        }

        invSortCooldown = GameInfo.gCombat.mTimers.SetTimer(10000, (arg) => { invSortCooldown = null; }, null);
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
        avatar.Change(equipmentInvData, player.GetJobSect(), player.mGender);
        RefreshEquipIcons(equipmentInvData.Slots, equipSlots);
        RefreshEquipIcons(equipmentInvData.FashionSlots, fashionSlots);
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

    private void OnJobIconLoaded(Sprite sprite)
    {
        imgJobIcon.sprite = sprite;
    }

    public void OnHelmToggleChanged(bool isOn)
    {
        PlayerGhost player = GameInfo.gLocalPlayer;
        bool hideHelm = !isOn;
        if (player != null && player.mEquipmentInvData.HideHelm != hideHelm)
            RPCFactory.NonCombatRPC.HideHelm(hideHelm);
    }

    public void OnEquipmentSlotClickedCB(int slotId)
    {
        PlayerGhost player = GameInfo.gLocalPlayer;
        var _item = player.mEquipmentInvData.GetEquipmentBySlotId(slotId);
        if (_item != null)
            UIManager.OpenDialog(WindowType.DialogItemDetail, (window) => {
                OnClicked_InitTooltipEquipment(window.GetComponent<UI_DialogItemDetailToolTip>(), slotId, _item, false);
            });
    }

    private void OnFashionSlotClickedCB(int slotId)
    {
        PlayerGhost player = GameInfo.gLocalPlayer;
        var _item = player.mEquipmentInvData.GetFashionSlot(slotId);
        if (_item != null)
            UIManager.OpenDialog(WindowType.DialogItemDetail, (window) => {
                OnClicked_InitTooltipEquipment(window.GetComponent<UI_DialogItemDetailToolTip>(), slotId, _item, true);
            });
    }
    #endregion

    #region OnClickInventoryItem
    public void OnClicked_InventoryItem(int slotId, IInventoryItem item)
    {
        UIManager.OpenDialog(WindowType.DialogItemDetail, (window) => {
            OnClicked_InitTooltip(window.GetComponent<UI_DialogItemDetailToolTip>(), slotId, item);
        });
    }

    private void OnClicked_InitTooltipEquipment(UI_DialogItemDetailToolTip component, int slotId, IInventoryItem item, bool fashionslot)
    {
        component.InitTooltip(item, true);
        List<ItemDetailsButton> _buttons = new List<ItemDetailsButton>();

        var _equipmentJson = (EquipmentJson)item.JsonObject;
        AddUnEquip(_buttons, slotId, item, fashionslot);
        //if (!string.IsNullOrEmpty(_equipmentJson.socketspace) && _equipmentJson.socketspace != "-1")
        //    AddSocket(_buttons, item);
        if (!string.IsNullOrEmpty(_equipmentJson.evolvegrp) && _equipmentJson.evolvegrp != "-1")
            AddEvolve(_buttons, slotId);
        if (_equipmentJson.upgradelimit > 0)
            AddUpgrade(_buttons, slotId);

        component.SetButtonCallback(_buttons);
    }

    private void OnClicked_InitTooltip(UI_DialogItemDetailToolTip component, int slotId, IInventoryItem item)
    {
        component.InitTooltip(item);
        List<ItemDetailsButton> _buttons = new List<ItemDetailsButton>();
        switch (item.JsonObject.itemtype)
        {
            case ItemType.PotionFood:
            case ItemType.LuckyPick:
            case ItemType.Henshin:
                AddUse(_buttons, slotId, item);
                break;
            case ItemType.Material:
                var _materialJson = (MaterialJson)item.JsonObject;
                if (_materialJson.mattype == MaterialType.Exchange)
                    AddExchange(_buttons, slotId, item);
                else if (_materialJson.mattype == MaterialType.UpgradeItem)
                    AddUpgrade(_buttons, slotId);
                else if (_materialJson.mattype == MaterialType.Special)
                    AddUse(_buttons, slotId, item);
                else if (_materialJson.mattype == MaterialType.Token)
                    AddUILink(_buttons, item, _materialJson.uiid);
                break;
            case ItemType.Features:
                AddUILink(_buttons, item, ((FeaturesJson)item.JsonObject).uiid);
                break;
            case ItemType.Equipment:
                var _equipmentJson = (EquipmentJson)item.JsonObject;
                //Add equip buttons according to what the player wears
                switch (_equipmentJson.partstype)
                {
                    case PartsType.Ring:
                        if (GameInfo.gLocalPlayer.mEquipmentInvData.GetEquipmentBySlotId((int)EquipmentSlot.Ring1) != null ||
                            GameInfo.gLocalPlayer.mEquipmentInvData.GetEquipmentBySlotId((int)EquipmentSlot.Ring2) != null)
                        {
                            AddEquipLeft(_buttons, slotId, item);
                            AddEquipRight(_buttons, slotId, item);
                        }
                        else
                            AddEquip(_buttons, slotId, item);
                        break;
                    case PartsType.Accessory:
                        if (GameInfo.gLocalPlayer.mEquipmentInvData.GetEquipmentBySlotId((int)EquipmentSlot.Accessory1) == null ||
                            GameInfo.gLocalPlayer.mEquipmentInvData.GetEquipmentBySlotId((int)EquipmentSlot.Accessory2) == null)
                        {
                            AddEquipLeft(_buttons, slotId, item);
                            AddEquipRight(_buttons, slotId, item);
                        }
                        else
                            AddEquip(_buttons, slotId, item);
                        break;
                    default:
                        AddEquip(_buttons, slotId, item);
                        break;
                }

                if (!string.IsNullOrEmpty(_equipmentJson.socketspace) && _equipmentJson.socketspace != "-1")
                    AddSocket(_buttons, item);
                if (!string.IsNullOrEmpty(_equipmentJson.evolvegrp) && _equipmentJson.evolvegrp != "-1")
                    AddEvolve(_buttons, slotId);
                if (_equipmentJson.upgradelimit > 0)
                    AddUpgrade(_buttons, slotId);

                //TODO: Need to add double equip button if its ring or accessory
                break;
            case ItemType.DNA:
                AddDNA(_buttons, item);
                break;
            case ItemType.Relic:
                AddSocket(_buttons, item);
                AddAchievement(_buttons, item);
                break;
            case ItemType.QuestItem:
                AddUse(_buttons, slotId, item);
                break;
            case ItemType.MercenaryItem:                    
                AddHeroUI(_buttons, slotId, item);
                AddHeroGift(_buttons, slotId, item);
                AddHeroSkin(_buttons, slotId, item);
                break;
            case ItemType.InstanceItem:
                AddTeleport(_buttons, slotId, item);
                break;
        }
        component.SetButtonCallback(_buttons);
    }

    private void AddUse(List<ItemDetailsButton> buttons, int slotId, IInventoryItem item)
    {
        ItemDetailsButton _button = new ItemDetailsButton();
        _button.name = GUILocalizationRepo.GetLocalizedString("ItemTooltipButton_Use");
        _button.icon = "ButtonB_UseItem";
        _button.callback = () => GameInfo.gLocalPlayer.clientItemInvCtrl.OnClicked_UseItem(slotId, item);
        buttons.Add(_button);
    }

    private void AddExchange(List<ItemDetailsButton> buttons, int slotId, IInventoryItem item)
    {
        ItemDetailsButton _button = new ItemDetailsButton();
        _button.name = GUILocalizationRepo.GetLocalizedString("ItemTooltipButton_Exchange");
        _button.icon = "ButtonB_UseItem"; //no exchange icon
        _button.callback = () => ClientUtils.OpenUIWindowByLinkUI(LinkUIType.Equipment_Craft);
        buttons.Add(_button);
    }

    private void AddUpgrade(List<ItemDetailsButton> buttons, int slotId)
    {
        ItemDetailsButton _button = new ItemDetailsButton();
        _button.name = GUILocalizationRepo.GetLocalizedString("ItemTooltipButton_OpenUpgradeUI");
        _button.icon = "ButtonB_Upgrade";
        _button.callback = () => ClientUtils.OpenUIWindowByLinkUI(LinkUIType.Equipment_Upgrade, slotId.ToString());
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

    private void AddEvolve(List<ItemDetailsButton> buttons, int slotId)
    {
        ItemDetailsButton _button = new ItemDetailsButton();
        _button.name = GUILocalizationRepo.GetLocalizedString("ItemTooltipButton_OpenEvolveUI");
        _button.icon = "ButtonB_Evolve";
        _button.callback = () => ClientUtils.OpenUIWindowByLinkUI(LinkUIType.Equipment_Reform, slotId.ToString());
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

    private void AddHeroUI(List<ItemDetailsButton> buttons, int slotId, IInventoryItem item)
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

    private void AddHeroGift(List<ItemDetailsButton> buttons, int slotId, IInventoryItem item)
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
            _button.callback = () => GameInfo.gLocalPlayer.clientItemInvCtrl.OnClicked_UseItem(slotId, item);
        }
        else
        {
            _button.name = GUILocalizationRepo.GetLocalizedString("ItemTooltipButton_GoToGift");
            _button.icon = "ButtonB_UseItem";
            _button.callback = () => ClientUtils.OpenUIWindowByLinkUI(LinkUIType.Hero);
        }
    }

    private void AddHeroSkin(List<ItemDetailsButton> buttons, int slotId, IInventoryItem item)
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
            _button.callback = () => GameInfo.gLocalPlayer.clientItemInvCtrl.OnClicked_UseItem(slotId, item);
        }
    }

    private void AddTeleport(List<ItemDetailsButton> buttons, int slotId, IInventoryItem item)
    {
        string _coordinate = ((InstanceItemJson)item.JsonObject).coordinate;
        if (string.IsNullOrEmpty(_coordinate) || _coordinate == "-1")
            return;
        ItemDetailsButton _button = new ItemDetailsButton();
        _button.name = GUILocalizationRepo.GetLocalizedString("ItemTooltipButton_Transfer");
        _button.icon = "ButtonB_UseItem";
        _button.callback = () => GameInfo.gLocalPlayer.clientItemInvCtrl.OnClicked_UseItem(slotId, item);
        buttons.Add(_button);
    }

    private void AddEquip(List<ItemDetailsButton> buttons, int slotId, IInventoryItem item, int eqSlotIdx = -1)
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
            GameInfo.gLocalPlayer.clientItemInvCtrl.OnClicked_Equip(slotId, item, _fashionOn, eqSlotIdx);
        };
        buttons.Add(_button);
    }

    private void AddEquipLeft(List<ItemDetailsButton> buttons, int slotId, IInventoryItem item)
    {
        AddEquip(buttons, slotId, item, 0);
        buttons[buttons.Count - 1].name = GUILocalizationRepo.GetLocalizedString("ItemTooltipButton_EquipLeft");
    }

    private void AddEquipRight(List<ItemDetailsButton> buttons, int slotId, IInventoryItem item)
    {
        AddEquip(buttons, slotId, item, 1);
        buttons[buttons.Count - 1].name = GUILocalizationRepo.GetLocalizedString("ItemTooltipButton_EquipRight");
    }

    private void AddUnEquip(List<ItemDetailsButton> buttons, int slotId, IInventoryItem item, bool fashionslot)
    {
        ItemDetailsButton _button = new ItemDetailsButton();
        _button.name = GUILocalizationRepo.GetLocalizedString("ItemTooltipButton_UnEquip");
        _button.icon = "ButtonB_UnEquip";
        _button.callback = () => GameInfo.gLocalPlayer.clientItemInvCtrl.OnClicked_UnEquip(slotId, item, fashionslot);
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
