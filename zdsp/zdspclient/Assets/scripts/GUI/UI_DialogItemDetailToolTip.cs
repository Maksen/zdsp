using Kopio.JsonContracts;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Zealot.Repository;
using Zealot.Common;
using Zealot.Client.Entities;

public class ItemDetailsButton
{
    public string name;
    public string icon;
    public UnityAction callback;
}

public class UI_DialogItemDetailToolTip : MonoBehaviour
{
    public class TooltipCommon
    {
        public Text storable;
        public Text description;
        public UI_DialogItemDetail_TextValue reqLv;
        public UI_DialogItemDetail_TextValue dailyUseLimit;
        public UI_DialogItemDetail_TextValue weeklyUseLimit;
        public UI_DialogItemDetail_TextValue dailyGetLimit;
        public UI_DialogItemDetail_TextValue weeklyGetLimit;

        public bool Failed()
        {
            return (storable == null || description == null || reqLv == null ||
                     dailyUseLimit == null || weeklyUseLimit == null ||
                     dailyGetLimit == null || dailyGetLimit == null);
        }
    }

    [Header("Tooltip Prefab list")]
    #region Tooltip Prefab
    [SerializeField]
    GameObject mTTLinePrefab;
    [SerializeField]
    GameObject mTTTextColonValuePrefab;
    [SerializeField]
    GameObject mTTValuePrefab;
    #endregion

    [Header("Left Panel Btn Prefab")]
    [SerializeField]
    GameObject mLPBtnPrefab;

    [SerializeField]
    List<Sprite> mButtonSprites;

    [Header("Parents")]
    #region Parent obj
    [SerializeField]
    Transform mIconParentTransform;
    [SerializeField]
    GameObject mStatsParent;
    [SerializeField]
    GameObject mLeftBtnPanelParent;
    #endregion

    [Header("Top bar")]
    #region Top bar
    [SerializeField]
    Text mItemName;
    [SerializeField]
    Text mItemTypeName;
    GameObject mGameIcon;
    #endregion

    [Header("Price")]
    #region Price
    [SerializeField]
    GameObject mObjAuctionPrice;
    [SerializeField]
    GameObject mObjSellPrice;
    [SerializeField]
    Text mAuctionPrice;
    [SerializeField]
    Text mSellPrice;
    [SerializeField]
    GameObject mAuctionIcon;
    [SerializeField]
    GameObject mSellIcon;
    #endregion

    #region Left Panel
    List<Button> mLeftPanelButtonLst = new List<Button>();

    [Header("Left Panel Toggles")]
    [SerializeField]
    Toggle mWhereToGetTog; //hide if itembase origin == -1
    [SerializeField]
    Toggle mExtraEffectTog; //Hide if equip has no sideeffect or is not equip
    [SerializeField]
    Toggle m3DViewTog; //Hide if no model path or not equipment
    #endregion

    [Header("Dialog")]
    [SerializeField]
    GameObject m3DView;
    [SerializeField]
    Model_3DAvatar m3DAvatar;
    [SerializeField]
    UI_DialogItemDetailTooltip_WhereToGet mWhereToGet;

    #region Limits
    UI_DialogItemDetail_TextValue mDailyGetTV;
    UI_DialogItemDetail_TextValue mDailyUseTV;
    UI_DialogItemDetail_TextValue mWeeklyGetTV;
    UI_DialogItemDetail_TextValue mWeeklyUseTV;
    #endregion

    IInventoryItem mItem = null;
    List<GameObject> mNormalStatsLst = new List<GameObject>();
    List<GameObject> mExtraSideEffectLst = new List<GameObject>();

    /// <summary>
    /// Pass custom item in from inventory
    /// </summary>
    public void InitTooltip(IInventoryItem item)
    {
        //Retrieve what you need from
        //InventoryItem
        //Inventory stats
        //Equipment class
        //var stat = GameInfo.gLocalPlayer.InventoryStats;

        if (item == null)
            return;
        if (mIconParentTransform == null)
        {
            Debug.LogError("HUD_ItemDetailToolTip.InitTooltip: Icon parent is null");
            return;
        }
        if (mStatsParent == null)
        {
            Debug.LogError("HUD_ItemDetailToolTip.InitTooltip: Stats Parent is null");
            return;
        }

        ClearTT();

        mItem = item;
        //Init tooltips based on item type
        switch (item.JsonObject.itemtype)
        {
            case ItemType.PotionFood:
                InitTT_Potion(item);
                break;
            case ItemType.Material:
                InitTT_Material(item);
                break;
            case ItemType.LuckyPick:
                InitTT_LuckyPick(item);
                break;
            case ItemType.Henshin:
                InitTT_Henshin(item);
                //todo
                break;
            case ItemType.Features:
                InitTT_Features(item);
                break;
            case ItemType.Equipment:
                InitTT_Equipment(item);
                break;
            case ItemType.DNA:
                InitTT_DNA(item);
                break;
            case ItemType.Relic:
                InitTT_Relic(item);
                break;
            case ItemType.QuestItem:
                InitTT_QuestItem(item);
                break;
            case ItemType.MercenaryItem:
                InitTT_MercenaryItem(item);
                break;
            case ItemType.InstanceItem:
                InitTT_InstanceItem(item);             
                break;
            case ItemType.ElementalStone:
                InitTT_ElementalStone(item);
                break;
        }

        RPCFactory.NonCombatRPC.Tooltip_DailyWeeklyLimit(mItem.JsonObject.itemid);
        DebugShowInfo();
    }

    public void OnDisable()
    {
        mWhereToGetTog.isOn = false;
        mExtraEffectTog.isOn = false;
        m3DViewTog.isOn = false;

        ClearTT();
    }

    #region Callback setter
    public void SetButtonCallback(List<ItemDetailsButton> buttons)
    {
        ClearTTBtn();
        if (buttons != null)
        {
            for (int index = 0; index < buttons.Count; index++)
                CreateLeftPanelButton(buttons[index]);
        }
    }
    #endregion

    #region Left Toggle function
    public void ShowExtra(bool toggle)
    {
        for (int i = 0; i < mExtraSideEffectLst.Count; ++i)
        {
            mExtraSideEffectLst[i].SetActive(toggle);
        }
        for (int i = 0; i < mNormalStatsLst.Count; ++i)
        {
            mNormalStatsLst[i].SetActive(!toggle);
        }
    }
    public void View3D(bool toggle)
    {
        Equipment eq = mItem as Equipment;
        if (is3DViewable(mItem))
        {

            //If first time opening dialog
            //if (m3DAvatar.GetOutfitModel() == null && toggle)
            if (toggle)
            {
                PlayerGhost pg = GameInfo.gLocalPlayer;
                m3DAvatar.Change(pg.mEquipmentInvData, (JobType)pg.PlayerSynStats.jobsect, pg.mGender);
                Toggle3DViewEquipment();
            }
            m3DView.SetActive(toggle);
        }
    }
    public void Location(bool toggle)
    {
        if (mItem == null)
        {
            mWhereToGet.gameObject.SetActive(toggle);
            return;
        }

        mWhereToGet.gameObject.SetActive(toggle);

        if (toggle)
        {
            //Show drop location dialog
            mWhereToGet.Init(mItem);
        }
    }
    #endregion

    #region ItemType Tooltip init
    private void ClearTT()
    {
        if (mGameIcon != null)
        {
            Destroy(mGameIcon);
            mGameIcon = null;
        }
        if (mStatsParent != null)
        {
            foreach (Transform t in mStatsParent.transform)
                Destroy(t.gameObject);

            mNormalStatsLst.Clear();
            mExtraSideEffectLst.Clear();
        }
        ClearTTBtn();
    }
    private void ClearTTBtn()
    {
        if (mLeftBtnPanelParent != null)
        {
            foreach (Transform t in mLeftBtnPanelParent.transform)
                Destroy(t.gameObject);

            mLeftPanelButtonLst.Clear();
        }
    }
    private void InitTT_Potion(IInventoryItem item)
    {
        //*** Icon - General item ***    
        InitGameIcon(item);

        //*** Stats ***
        Text sideeffectTxt = null; //Side effect
        TooltipCommon cTT = null;

        //Create common element
        InitTT_CommonShared(item, out cTT);
        CreateText(out sideeffectTxt);

        //Check if all obj has been created
        if (cTT.Failed() || sideeffectTxt == null)
            return;

        //Parent objects as needed
        cTT.reqLv.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        sideeffectTxt.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        cTT.storable.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        cTT.description.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        cTT.dailyUseLimit.transform.SetParent(mStatsParent.transform, false);
        cTT.dailyGetLimit.transform.SetParent(mStatsParent.transform, false);
        cTT.weeklyUseLimit.transform.SetParent(mStatsParent.transform, false);
        cTT.weeklyGetLimit.transform.SetParent(mStatsParent.transform, false);

        //Set data to our gameobjects
        PotionFood pf = item as PotionFood;
        int sideEffectID = -1;
        if (int.TryParse(pf.PotionFoodJson.seid, out sideEffectID))
        {
            SideEffectJson sej = SideEffectRepo.GetSideEffect(sideEffectID);
            sideeffectTxt.text = sej.description;
        }
        else
        {
            sideeffectTxt.text = "#unlocalized side effect#";
        }      
    }
    private void InitTT_Material(IInventoryItem item)
    {
        //*** Icon - General item ***
        InitGameIcon(item);

        //*** Stats ***
        TooltipCommon cTT = null;

        //Create common element
        InitTT_CommonShared(item, out cTT);

        //Check if all obj has been created
        if (cTT.Failed())
            return;

        //Set parent
        cTT.reqLv.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        cTT.storable.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        cTT.description.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        cTT.dailyUseLimit.transform.SetParent(mStatsParent.transform, false);
        cTT.dailyGetLimit.transform.SetParent(mStatsParent.transform, false);
        cTT.weeklyUseLimit.transform.SetParent(mStatsParent.transform, false);
        cTT.weeklyGetLimit.transform.SetParent(mStatsParent.transform, false);
    }
    private void InitTT_Exchange(IInventoryItem item)
    {
        //*** Icon - General item ***
        InitGameIcon(item);

        //*** Stats ***
        TooltipCommon cTT = null;
        Text txtExchangeItem = null; //Exchange ItemID
        Text txtReqValue = null; //Require Value

        //Create common element
        InitTT_CommonShared(item, out cTT);
        CreateText(out txtExchangeItem);
        CreateText(out txtReqValue);

        //Check if all obj has been created
        if (cTT.Failed() || txtExchangeItem == null || txtReqValue == null)
            return;

        //Parent object as needed
        cTT.reqLv.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        txtExchangeItem.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        txtReqValue.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        cTT.storable.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        cTT.description.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        cTT.dailyUseLimit.transform.SetParent(mStatsParent.transform, false);
        cTT.dailyGetLimit.transform.SetParent(mStatsParent.transform, false);
        cTT.weeklyUseLimit.transform.SetParent(mStatsParent.transform, false);
        cTT.weeklyGetLimit.transform.SetParent(mStatsParent.transform, false);

        //Set data to our gameobjects
        MaterialItem matItem = item as MaterialItem;
        ItemBaseJson exItem = GameRepo.ItemFactory.GetItemById(matItem.MaterialJson.exchitemid);
        string reqValStr = string.Format("{0} ({1}/{2})", 
                                            item.JsonObject.localizedname,
                                            GameInfo.gLocalPlayer.clientItemInvCtrl.itemInvData.GetTotalStackCountByItemId(item.ItemID),
                                            matItem.MaterialJson.reqval);
        txtExchangeItem.text = exItem.localizedname;
        txtReqValue.text = "" + reqValStr; //Material Needed: Item_Name (x/y)
    }
    private void InitTT_UpgradeItem(IInventoryItem item)
    {
        //*** Icon - General item ***
        InitGameIcon(item);

        //*** Stats ***
        TooltipCommon cTT = null;

        //Create common element
        InitTT_CommonShared(item, out cTT);

        //Check if all obj has been created
        if (cTT.Failed())
            return;

        //parent objects as needed
        cTT.reqLv.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        cTT.storable.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        cTT.description.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        cTT.dailyUseLimit.transform.SetParent(mStatsParent.transform, false);
        cTT.dailyGetLimit.transform.SetParent(mStatsParent.transform, false);
        cTT.weeklyUseLimit.transform.SetParent(mStatsParent.transform, false);
        cTT.weeklyGetLimit.transform.SetParent(mStatsParent.transform, false);
    }
    private void InitTT_Token(IInventoryItem item)
    {
        //Written in CR56
        //*** Icon - General item ***
        InitGameIcon(item);

        //*** Stats ***
        TooltipCommon cTT = null;

        //Create common element
        InitTT_CommonShared(item, out cTT);

        //Check if all obj has been created
        if (cTT.Failed())
            return;

        //parent objects as needed
        cTT.reqLv.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        cTT.storable.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        cTT.description.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        cTT.dailyUseLimit.transform.SetParent(mStatsParent.transform, false);
        cTT.dailyGetLimit.transform.SetParent(mStatsParent.transform, false);
        cTT.weeklyUseLimit.transform.SetParent(mStatsParent.transform, false);
        cTT.weeklyGetLimit.transform.SetParent(mStatsParent.transform, false);
    }
    private void InitTT_LuckyPick(IInventoryItem item)
    {
        //*** Icon - General item ***
        InitGameIcon(item);

        //*** Stats ***
        TooltipCommon cTT = null;

        //Create common element
        InitTT_CommonShared(item, out cTT);

        //Check if all obj has been created
        if (cTT.Failed())
            return;

        //parent objects as needed
        cTT.reqLv.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        cTT.storable.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        cTT.description.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        cTT.dailyUseLimit.transform.SetParent(mStatsParent.transform, false);
        cTT.dailyGetLimit.transform.SetParent(mStatsParent.transform, false);
        cTT.weeklyUseLimit.transform.SetParent(mStatsParent.transform, false);
        cTT.weeklyGetLimit.transform.SetParent(mStatsParent.transform, false);
    }
    private void InitTT_Henshin(IInventoryItem item)
    {
        //*** Icon - General item ***
        InitGameIcon(item);

        //*** Stats ***
        TooltipCommon cTT = null;

        //Create common element
        InitTT_CommonShared(item, out cTT);

        //Check if all obj has been created
        if (cTT.Failed())
            return;

        //parent objects as needed
        cTT.reqLv.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        cTT.storable.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        cTT.description.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        cTT.dailyUseLimit.transform.SetParent(mStatsParent.transform, false);
        cTT.dailyGetLimit.transform.SetParent(mStatsParent.transform, false);
        cTT.weeklyUseLimit.transform.SetParent(mStatsParent.transform, false);
        cTT.weeklyGetLimit.transform.SetParent(mStatsParent.transform, false);
    }
    private void InitTT_Special(IInventoryItem item)
    {
        //*** Icon - General item ***
        InitGameIcon(item);

        //*** Stats ***
        TooltipCommon cTT = null;

        //Create common element
        InitTT_CommonShared(item, out cTT);

        //Check if all obj has been created
        if (cTT.Failed())
            return;

        //parent objects as needed
        cTT.reqLv.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        cTT.storable.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        cTT.description.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        cTT.dailyUseLimit.transform.SetParent(mStatsParent.transform, false);
        cTT.dailyGetLimit.transform.SetParent(mStatsParent.transform, false);
        cTT.weeklyUseLimit.transform.SetParent(mStatsParent.transform, false);
        cTT.weeklyGetLimit.transform.SetParent(mStatsParent.transform, false);
    }
    private void InitTT_Features(IInventoryItem item)
    {
        //Written in CR56 and 58 now
        //*** Icon - General item ***
        InitGameIcon(item);

        //*** Stats ***
        TooltipCommon cTT = null;

        //Create common element
        InitTT_CommonShared(item, out cTT);

        //Check if all obj has been created
        if (cTT.Failed())
            return;

        //parent objects as needed
        cTT.reqLv.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        cTT.storable.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        cTT.description.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        cTT.dailyUseLimit.transform.SetParent(mStatsParent.transform, false);
        cTT.dailyGetLimit.transform.SetParent(mStatsParent.transform, false);
        cTT.weeklyUseLimit.transform.SetParent(mStatsParent.transform, false);
        cTT.weeklyGetLimit.transform.SetParent(mStatsParent.transform, false);
    }
    private void InitTT_Equipment(IInventoryItem item)
    {
        //*** Icon - Equipment ***
        InitGameIcon(item);
        Equipment eq = item as Equipment;

        //*** Stats ***
        //Common
        TooltipCommon cTT = null;
        //Unique
        Text txtStats = null;
        UI_DialogItemDetail_TextValue txtValStats = null;

        //Create common element
        InitTT_CommonShared(item, out cTT);
        mExtraEffectTog.gameObject.SetActive(true);
        m3DViewTog.gameObject.SetActive(true);

        //Common - Req Lv
        mNormalStatsLst.Add(cTT.reqLv.gameObject);
        mNormalStatsLst.Add(CreateLineNoParent());

        //Unique - Wpn Main Attr
        CreateTextColonValue(out txtValStats);
        txtValStats.Identifier = GUILocalizationRepo.GetLocalizedString("id_wpnmainattr");
        switch (eq.EquipmentJson.mainwepattrib)
        {
            case MainWeaponAttribute.Str:
                txtValStats.Value = GUILocalizationRepo.GetLocalizedString("ci_strength");
                break;
            case MainWeaponAttribute.Dex:
                txtValStats.Value = GUILocalizationRepo.GetLocalizedString("ci_dexterity");
                break;
            case MainWeaponAttribute.Int:
                txtValStats.Value = GUILocalizationRepo.GetLocalizedString("ci_inteligence");
                break;
        }
        mNormalStatsLst.Add(txtValStats.gameObject);
        mNormalStatsLst.Add(CreateLineNoParent());

        //Unique - req class, if -1 == all class, otherwise.. some classes
        int classid = -2;
        if (!int.TryParse(eq.EquipmentJson.equipclass, out classid))
            Debug.LogError("UI_DialogItemDetailTooltip.InitTT_Equipment: Error parsing class id.");
        CreateTextColonValue(out txtValStats);
        txtValStats.Identifier = GUILocalizationRepo.GetLocalizedString("id_class");
        txtValStats.Value = (classid != -1) ? JobSectRepo.GetJobLocalizedName((JobType)classid) : GUILocalizationRepo.GetLocalizedString("id_allclass");
        mNormalStatsLst.Add(txtValStats.gameObject);
        mNormalStatsLst.Add(CreateLineNoParent());

        //Unique - Base side effect
        CreateText(out txtStats);
        txtStats.text = "";
        string[] baseseGrpLst = eq.EquipmentJson.basese.Split(';');
        foreach (string s in baseseGrpLst)
        {
            int x;
            if (int.TryParse(s, out x) == false)
            {
                Debug.LogError("HUD_ItemDetailToolTip.InitTT_Equipment: Cannot convert Base side effect ID string chain in EquipmentJson.basese to int");
                continue;
            }
            if (x < 0) //hide if id == -1 or negative
            {
                txtStats.text = "";
                break;
            }

            txtStats.text += SideEffectRepo.GetSideEffect(x).description;
        }
        mNormalStatsLst.Add(txtStats.gameObject);
        mNormalStatsLst.Add(CreateLineNoParent());

        //Unique - power up/qiang hua level and effect
        PowerUpJson puj = GameInfo.gLocalPlayer.clientPowerUpCtrl.GetPowerUpJson(eq.EquipmentJson.partstype);
        if (puj != null)
        {
            SideEffectJson sej = SideEffectRepo.GetSideEffect(puj.effect);
            CreateText(out txtStats);
            txtStats.text = GUILocalizationRepo.GetLocalizedString("ItemTooltip_Powerup");
            mNormalStatsLst.Add(txtStats.gameObject);
            CreateTextColonValue(out txtValStats);
            txtValStats.Identifier = string.Format(GUILocalizationRepo.GetLocalizedString("ItemTooltip_Powerup2"), puj.power, GameInfo.gLocalPlayer.PlayerStats.Level);
            txtValStats.Value = string.Format(GUILocalizationRepo.GetLocalizedString("ItemTooltip_Powerup3"), sej.description, puj.value);
            mNormalStatsLst.Add(txtValStats.gameObject);
            mNormalStatsLst.Add(CreateLineNoParent());
        }

        //Unique - Refine effect
        if (eq.UpgradeLevel > 0)
        {
            float upgIncrease = EquipmentModdingRepo.GetEquipmentUpgradeData(eq.EquipmentJson.equiptype, eq.EquipmentJson.rarity, eq.UpgradeLevel).increase;

            CreateText(out txtStats);
            txtStats.text = GUILocalizationRepo.GetLocalizedString("ItemTooltip_RefineEffect");
            mNormalStatsLst.Add(txtStats.gameObject);
            CreateTextColonValue(out txtValStats);
            txtValStats.Identifier = string.Format(GUILocalizationRepo.GetLocalizedString("ItemTooltip_RefineEffect2"), eq.UpgradeLevel+7, eq.EquipmentJson.upgradelimit);
            txtValStats.Value = upgIncrease.ToString();
            mNormalStatsLst.Add(txtValStats.gameObject);
            mNormalStatsLst.Add(CreateLineNoParent());

            //Unique - Refinement lv effect
            CreateText(out txtStats);
            txtStats.text = GUILocalizationRepo.GetLocalizedString("ItemTooltip_RefineLv");
            mNormalStatsLst.Add(txtStats.gameObject);
            List<int> upgSeLst = EquipmentModdingRepo.GetEquipmentUpgradeBuff(eq.EquipmentJson.equiptype, eq.EquipmentJson.rarity, eq.UpgradeLevel+7);
            if (upgSeLst != null)
            {
                for (int i = 0; i < upgSeLst.Count; ++i)
                {
                    //Loop through all refinement effects that this weapon has unlocked based on its refine lvl +8 / +9 / +10
                    CreateTextColonValue(out txtValStats);
                    txtValStats.Identifier = string.Format(GUILocalizationRepo.GetLocalizedString("ItemTooltip_RefineLv2"), i); //refine level
                    txtValStats.Value = string.Format(GUILocalizationRepo.GetLocalizedString("ItemTooltip_RefineLv3"), SideEffectRepo.GetSideEffect(upgSeLst[i])); //equipmentUpgrade's buff
                    mNormalStatsLst.Add(txtValStats.gameObject);
                }
            }
            mNormalStatsLst.Add(CreateLineNoParent());
        }

        //Unique - socket item bonus + socket collection bonus
        //CreateText(out txtStats);
        //txtStats.text = "Socketed item bonus";
        //Loop through all socketed item bonus
        //mNormalStatsLst.Add(txtStats.gameObject);
        //CreateText(out txtStats);
        //txtStats.text = "Socketed item collection bonus";
        //Loop through all socketed item collection bonus
        //mNormalStatsLst.Add(txtStats.gameObject);
        //mNormalStatsLst.Add(CreateLine());

        //Unique - reform bonus - aka RO mobile Weapon Tier1,2,3
        if (eq.ReformStep > 0)
        //eq.ReformStep = 1;
        //if (EquipmentModdingRepo.GetEquipmentReformData(eq) != null)
        {
            List<EquipReformData> ersLst = EquipmentModdingRepo.GetEquipmentReformData(eq);
            string reformDesc = string.Empty;
            string reformStep = GUILocalizationRepo.GetLocalizedString("ItemTooltip_Evolve");

            //Loop all reform step
            for (int i = 0; i < ersLst.Count; ++i)
            {
                CreateTextColonValue(out txtValStats);
                txtValStats.Identifier = string.Format(reformStep, ClientUtils.GetRomanNumFromInt(i+1));

                //Retrieving the sideeffect description of each reform step
                reformDesc = string.Empty;
                string[] reformSE = ersLst[i].mReformData.sideeffect.Split(';');
                foreach (string se in reformSE)
                {
                    int x = -2;
                    if (int.TryParse(se, out x) == false)
                    {
                        Debug.LogError("HUD_ItemDetailToolTip.InitTT_Equipment: Cannot convert Base side effect ID string chain in EquipReformData to int");
                        continue;
                    }
                    if (x == -1)
                        continue;

                    reformDesc += SideEffectRepo.GetSideEffect(x).description;
                }
                txtValStats.Value = reformDesc;
                mNormalStatsLst.Add(txtValStats.gameObject);
            }
            mNormalStatsLst.Add(CreateLineNoParent());
        }


        //Common - can storage
        mNormalStatsLst.Add(cTT.storable.gameObject);
        mNormalStatsLst.Add(CreateLineNoParent());

        //Common - description
        mNormalStatsLst.Add(cTT.description.gameObject);

        //Common - use get limit
        mNormalStatsLst.Add(CreateLineNoParent());
        mNormalStatsLst.Add(cTT.dailyUseLimit.gameObject);
        mNormalStatsLst.Add(cTT.dailyGetLimit.gameObject);
        mNormalStatsLst.Add(cTT.weeklyUseLimit.gameObject);
        mNormalStatsLst.Add(cTT.weeklyGetLimit.gameObject);

        //Set Parent
        for (int i = 0; i < mNormalStatsLst.Count; ++i)
            mNormalStatsLst[i].transform.SetParent(mStatsParent.transform, false);

        //*** Extra Sideeffect Stats ***
        CreateText(out txtStats);
        string[] extraSeLst = eq.EquipmentJson.extrase.Split(';');
        foreach (string s in extraSeLst)
        {
            int x;
            if (int.TryParse(s, out x) == false)
            {
                string errstr = string.Format("HUD_ItemDetailToolTip.InitTT_Equipment: Cannot convert string {0} in EquipmentJson.extrase to int", s);
                Debug.LogError(errstr);
                continue;
            }
            if (x < 0) //hide if id == -1 or negative
            {
                txtStats.text = "";
                break;
            }

            //Loop all sideeffect in the sideeffect grp, get their description in SDG
            txtStats.text += SideEffectRepo.GetSideEffect(x);
        }
        mExtraSideEffectLst.Add(txtStats.gameObject);
        mExtraSideEffectLst.Add(CreateLineNoParent());

        //Set parent of extra side effect
        for (int i = 0; i < mExtraSideEffectLst.Count; ++i)
            mExtraSideEffectLst[i].transform.SetParent(mStatsParent.transform, false);
    }
    private void InitTT_DNA(IInventoryItem item)
    {
        //*** Icon - Gene ***
        InitGameIcon(item);
        //DNA dnaItem = item as DNA;
        //mcIcon.Level = geneItem.GeneJson.;
        //mcIcon.EvolveLevel = geneItem.GeneJson.;

        //*** Stats ***
        Text txtPosGene = null; //Positive gene effect
        Text txtNegGene = null; //Negative gene effect
        TooltipCommon cTT = null;

        //Create common element
        InitTT_CommonShared(item, out cTT);
        CreateText(out txtPosGene);
        CreateText(out txtNegGene);

        //Check if all obj has been created
        if (txtPosGene == null || txtNegGene == null || cTT.Failed())
            return;

        //Parent objects as needed
        txtPosGene.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        txtNegGene.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        cTT.reqLv.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        cTT.storable.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        cTT.description.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        cTT.dailyUseLimit.transform.SetParent(mStatsParent.transform, false);
        cTT.dailyGetLimit.transform.SetParent(mStatsParent.transform, false);
        cTT.weeklyUseLimit.transform.SetParent(mStatsParent.transform, false);
        cTT.weeklyGetLimit.transform.SetParent(mStatsParent.transform, false);

        //set data to our gameobjects
        DNA dna = item as DNA;
        txtPosGene.text = dna.DNAJson.postive;
        txtNegGene.text = dna.DNAJson.negative;
    }
    private void InitTT_Relic(IInventoryItem item)
    {
        //*** Icon - General item ***
        InitGameIcon(item);

        //*** Stats ***
        Text txtCanRecycle = null;
        Text txtSocketTitle = null;
        Text txtSocketAbility = null;
        Text txtCollectionTitle = null;
        Text txtCollectionAbility = null;
        TooltipCommon cTT = null;

        //Create common element
        InitTT_CommonShared(item, out cTT);
        CreateText(out txtCanRecycle);
        CreateText(out txtSocketTitle);
        CreateText(out txtSocketAbility);
        CreateText(out txtCollectionTitle);
        CreateText(out txtCollectionAbility);

        //Check if all obj has been created
        if (txtCanRecycle == null || txtSocketAbility == null || txtCollectionAbility == null || cTT.Failed())
            return;

        //Set parent
        txtSocketTitle.transform.SetParent(mStatsParent.transform, false);
        txtSocketAbility.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        txtCollectionTitle.transform.SetParent(mStatsParent.transform, false);
        txtCollectionAbility.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        txtCanRecycle.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        cTT.reqLv.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        cTT.storable.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        cTT.description.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        cTT.dailyUseLimit.transform.SetParent(mStatsParent.transform, false);
        cTT.dailyGetLimit.transform.SetParent(mStatsParent.transform, false);
        cTT.weeklyUseLimit.transform.SetParent(mStatsParent.transform, false);
        cTT.weeklyGetLimit.transform.SetParent(mStatsParent.transform, false);

        //Set data to our gameobjects
        Relic relic = item as Relic;
        //Can recycle?
        if (relic.RelicJson.recycle)
            txtCanRecycle.text = GUILocalizationRepo.GetLocalizedString("id_canrecycle");
        else
            txtCanRecycle.text = GUILocalizationRepo.GetLocalizedString("id_cannotrecyle");
        //Socket ability?
        txtSocketTitle.text = GUILocalizationRepo.GetLocalizedString("ItemTooltip_RelicSocket");
        txtSocketAbility.text = relic.RelicJson.sockability;
        //Collection ability
        txtCollectionTitle.text = GUILocalizationRepo.GetLocalizedString("ItemTooltip_RelicCollection");
        txtCollectionAbility.text = relic.RelicJson.collectability;
    }
    private void InitTT_QuestItem(IInventoryItem item)
    {
        //*** Icon - General item ***
        InitGameIcon(item);

        //*** Stats ***
        TooltipCommon cTT = null;

        //Create common element
        InitTT_CommonShared(item, out cTT);

        //Check if all obj has been created
        if (cTT.Failed())
            return;

        //Set parent
        cTT.reqLv.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        cTT.storable.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        cTT.description.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        cTT.dailyUseLimit.transform.SetParent(mStatsParent.transform, false);
        cTT.dailyGetLimit.transform.SetParent(mStatsParent.transform, false);
        cTT.weeklyUseLimit.transform.SetParent(mStatsParent.transform, false);
        cTT.weeklyGetLimit.transform.SetParent(mStatsParent.transform, false);
    }
    private void InitTT_MercenaryItem(IInventoryItem item)
    {
        //*** Icon - General item ***
        InitGameIcon(item);

        //*** Stats ***
        TooltipCommon cTT = null;

        //Create common element
        InitTT_CommonShared(item, out cTT);

        //Check if all obj has been created
        if (cTT.Failed())
            return;

        //Set parent
        cTT.reqLv.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        cTT.storable.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        cTT.description.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        cTT.dailyUseLimit.transform.SetParent(mStatsParent.transform, false);
        cTT.dailyGetLimit.transform.SetParent(mStatsParent.transform, false);
        cTT.weeklyUseLimit.transform.SetParent(mStatsParent.transform, false);
        cTT.weeklyGetLimit.transform.SetParent(mStatsParent.transform, false);
    }
    private void InitTT_InstanceItem(IInventoryItem item)
    {
        //*** Icon - General item ***
        InitGameIcon(item);

        //*** Stats ***
        TooltipCommon cTT = null;
        UI_DialogItemDetail_TextValue txtVal = null;

        //Create common element
        InitTT_CommonShared(item, out cTT);
        CreateTextColonValue(out txtVal);

        //Check if all obj has been created
        if (cTT.Failed() || txtVal == null)
            return;

        //Set Parent
        txtVal.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        cTT.reqLv.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        cTT.storable.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        cTT.description.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        cTT.dailyUseLimit.transform.SetParent(mStatsParent.transform, false);
        cTT.dailyGetLimit.transform.SetParent(mStatsParent.transform, false);
        cTT.weeklyUseLimit.transform.SetParent(mStatsParent.transform, false);
        cTT.weeklyGetLimit.transform.SetParent(mStatsParent.transform, false);

        //Set data to our gameobjects
        InstanceItem instanceItem = item as InstanceItem;
        //coordinates
        txtVal.Identifier = GUILocalizationRepo.GetLocalizedString("id_location");
        txtVal.ShowColon = true;
        txtVal.Value = instanceItem.InstanceItemJson.coordinate;
    }
    private void InitTT_ElementalStone(IInventoryItem item)
    {
        //*** Icon - General item ***
        InitGameIcon(item);

        //*** Stats ***
        TooltipCommon cTT = null;
        Text sideeffectTxt = null;
        UI_DialogItemDetail_TextValue stoneSlotTxt = null;
        InitTT_CommonShared(item, out cTT);
        CreateText(out sideeffectTxt);
        CreateTextColonValue(out stoneSlotTxt);

        if (cTT.Failed() || sideeffectTxt == null || stoneSlotTxt == null)
            return;

        //Parent objects as needed
        stoneSlotTxt.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        cTT.reqLv.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        sideeffectTxt.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        cTT.storable.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        cTT.description.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        cTT.dailyUseLimit.transform.SetParent(mStatsParent.transform, false);
        cTT.dailyGetLimit.transform.SetParent(mStatsParent.transform, false);
        cTT.weeklyUseLimit.transform.SetParent(mStatsParent.transform, false);
        cTT.weeklyGetLimit.transform.SetParent(mStatsParent.transform, false);

        ElementalStone es = item as ElementalStone;
        //Unique - Equip slot
        stoneSlotTxt.Identifier = GUILocalizationRepo.GetLocalizedString("ItemTooltip_ElemStone");
        stoneSlotTxt.Value = EquipFusionRepo.StoneTypeGetName(es.ElementalStoneJson.type).ToString();
        //Unique - Sideeffects
        sideeffectTxt.text = string.Empty;
        List<string> effectGroup = EquipFusionController.DecodeEffect(es.FusionData);
        for (int i = 0; i < effectGroup.Count; ++i)
            sideeffectTxt.text += effectGroup[i] + '\n';
    }
    private void InitTT_CommonShared(IInventoryItem item, out TooltipCommon cTT)
    {
        cTT = new TooltipCommon();
        if (!CreateText(out cTT.storable) || !CreateText(out cTT.description) ||
            !CreateTextColonValue(out cTT.reqLv) || !CreateTextColonValue(out cTT.dailyUseLimit) ||
            !CreateTextColonValue(out cTT.dailyGetLimit) || !CreateTextColonValue(out cTT.weeklyUseLimit) ||
            !CreateTextColonValue(out cTT.weeklyGetLimit))
        {
            cTT.storable = cTT.description = null;
            cTT.reqLv = cTT.dailyUseLimit = cTT.dailyGetLimit = cTT.weeklyUseLimit = cTT.weeklyGetLimit = null;
            return;
        }

        string depositStr = (item.JsonObject.deposit) ? "id_canstorage" : "id_cannotstorage";
        cTT.storable.text = GUILocalizationRepo.GetLocalizedString(depositStr);
        cTT.description.text = item.JsonObject.description;
        cTT.reqLv.Identifier = GUILocalizationRepo.GetLocalizedString("id_requirelevel");
        cTT.reqLv.Value = item.JsonObject.requirelvl.ToString();

        string localstr = GUILocalizationRepo.GetLocalizedString("ItemTooltip_UseGetLimit");
        if (item.JsonObject.dailyuselimit > 0)
        {
            cTT.dailyUseLimit.Identifier = GUILocalizationRepo.GetLocalizedString("ItemTooltip_DailyUseLimit");
            cTT.dailyUseLimit.Value = string.Format(localstr, item.JsonObject.dailyuselimit, item.JsonObject.dailyuselimit);
        }
        if (item.JsonObject.dailygetlimit > 0)
        {
            cTT.dailyGetLimit.Identifier = GUILocalizationRepo.GetLocalizedString("ItemTooltip_DailyGetLimit"); ;
            cTT.dailyGetLimit.Value = string.Format(localstr, item.JsonObject.dailygetlimit, item.JsonObject.dailygetlimit);
        }
        if (item.JsonObject.weeklyuselimit > 0)
        {
            cTT.weeklyUseLimit.Identifier = GUILocalizationRepo.GetLocalizedString("ItemTooltip_WeeklyUseLimit"); ;
            cTT.weeklyUseLimit.Value = string.Format(localstr, item.JsonObject.weeklyuselimit, item.JsonObject.weeklyuselimit);
        }
        if (item.JsonObject.weeklygetlimit > 0)
        {
            cTT.weeklyGetLimit.Identifier = GUILocalizationRepo.GetLocalizedString("ItemTooltip_WeeklyGetLimit"); ;
            cTT.weeklyGetLimit.Value = string.Format(localstr, item.JsonObject.weeklygetlimit, item.JsonObject.weeklygetlimit);
        }
        cTT.dailyUseLimit.gameObject.SetActive(item.JsonObject.dailyuselimit > 0);
        cTT.dailyGetLimit.gameObject.SetActive(item.JsonObject.dailygetlimit > 0);
        cTT.weeklyUseLimit.gameObject.SetActive(item.JsonObject.weeklyuselimit > 0);
        cTT.weeklyGetLimit.gameObject.SetActive(item.JsonObject.weeklygetlimit > 0);
        mDailyGetTV = cTT.dailyGetLimit;
        mDailyUseTV = cTT.dailyUseLimit;
        mWeeklyGetTV = cTT.weeklyGetLimit;
        mWeeklyUseTV = cTT.weeklyUseLimit;

        int auctionVal;
        string[] auctionStr = item.JsonObject.auction.Split('|');
        if (!int.TryParse(auctionStr[0], out auctionVal))
        {
            Debug.LogError("UI_DialogItemDetailToolTip.InitTT_CommonShared: Walaoeh! Unable to parse item.JsonObject.auction!!");
            auctionVal = -1;
        }
        mAuctionIcon.SetActive(auctionVal != -1);
        mAuctionPrice.text = (auctionVal == -1) ? GUILocalizationRepo.GetLocalizedString("id_cannotauction") : auctionStr[0];
        mSellIcon.SetActive(item.JsonObject.sellprice != -1);
        mSellPrice.text = (item.JsonObject.sellprice != -1) ? item.JsonObject.sellprice.ToString() : GUILocalizationRepo.GetLocalizedString("id_cannotsell");

        //mWhereToGetTog;
        mExtraEffectTog.gameObject.SetActive(false);
        m3DViewTog.gameObject.SetActive(false);
    }

    #region shortcut create
    /// <summary>
    /// Output object will not be parented to any object
    /// </summary>
    /// <param name="tiv"></param>
    /// <returns></returns>
    private bool CreateTextColonValue(out UI_DialogItemDetail_TextValue txtVal)
    {
        GameObject obj = Instantiate(mTTTextColonValuePrefab, Vector3.zero, Quaternion.identity);
        txtVal = obj.GetComponent<UI_DialogItemDetail_TextValue>();
        if (txtVal == null)
        {
            Debug.LogError("HUD_ItemDetailToolTip.CreateTextColonValue: mTTTextColonValuePrefab has no HUD_ItemDetail_TextColonValue script file");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Output object will not be parented to any object
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    private bool CreateText(out Text t)
    {
        GameObject obj = Instantiate(mTTValuePrefab, Vector3.zero, Quaternion.identity);
        t = obj.GetComponent<Text>();
        if (t == null)
        {
            Debug.LogError("HUD_ItemDetailToolTip.CreateText: mTTValuePrefab has no Text component");
            return false;
        }

        return true;
    }
    private GameObject CreateLine()
    {
        GameObject obj = Instantiate(mTTLinePrefab, Vector3.zero, Quaternion.identity);
        obj.transform.SetParent(mStatsParent.transform, false);

        return obj;
    }
    private GameObject CreateLineNoParent()
    {
        GameObject obj = Instantiate(mTTLinePrefab, Vector3.zero, Quaternion.identity);
        return obj;
    }

    private void InitGameIcon(IInventoryItem item)
    {
        if (item == null)
            return;

        ItemSortJson itemSortJson = item.ItemSortJson;
        mGameIcon = Instantiate(ClientUtils.LoadGameIcon(itemSortJson.gameicontype));
        mGameIcon.transform.SetParent(mIconParentTransform, false);
        ClientUtils.InitGameIcon(mGameIcon, null, item.ItemID, itemSortJson.gameicontype, item.StackCount, false);

        mItemName.text = item.JsonObject.localizedname;
        mItemTypeName.text = itemSortJson.localizedname;
    }

    private void CreateLeftPanelButton(ItemDetailsButton button)
    {
        GameObject obj = Instantiate(mLPBtnPrefab);
        obj.transform.SetParent(mLeftBtnPanelParent.transform, false);
        obj.GetComponent<UI_DialogItemDetailToolTip_Btn>().Init(
        button.name,
        mButtonSprites.Find(x => x.name == button.icon),
        button.callback);
    }
    #endregion  //shortcut create stats
    #endregion

    public void Toggle3DViewEquipment()
    {
        //Change avatar's model according to what the item is
        //Take off all bathrobe equip if looking at regular equip while in bathrobe
        //
        PlayerGhost pg = GameInfo.gLocalPlayer;
        GameObject avaObj = m3DAvatar.GetOutfitModel();
        AvatarController ac = avaObj.GetComponent<AvatarController>();
        EquipmentInventoryData equipmentInvData = pg.mEquipmentInvData.CloneJson();
        List<int> appearance = pg.mEquipmentInvData.AppearanceSlots;
        Equipment eq = mItem as Equipment;

        switch (eq.EquipmentJson.partstype)
        {
            case PartsType.Sword:
            case PartsType.Xbow:
            case PartsType.Hammer:
            case PartsType.Dagger:
            case PartsType.Blade:
            case PartsType.Fan:
            case PartsType.Lance:
            case PartsType.Sanxian:
                equipmentInvData.SetEquipmentToSlot((int)EquipmentSlot.Weapon, eq);
                break;
            case PartsType.Helm:
                equipmentInvData.SetEquipmentToSlot((int)EquipmentSlot.Helm, eq);
                break;
            case PartsType.Body:
                equipmentInvData.SetEquipmentToSlot((int)EquipmentSlot.Body, eq);
                break;
            case PartsType.Wing:
                equipmentInvData.SetEquipmentToSlot((int)EquipmentSlot.Back, eq);
                break;
            case PartsType.Bathrobe:
                equipmentInvData.SetFashionToSlot((int)FashionSlot.Bathrobe, eq);
                break;
            default:
                break;
        }

        ac.InitAvatar(equipmentInvData, pg.GetJobSect(), pg.mGender);
    }
    private bool is3DViewable(IInventoryItem item)
    {
        if (item.JsonObject.itemtype != ItemType.Equipment)
            return false;

        Equipment eq = item as Equipment;
        return (eq.EquipmentJson.equiptype == EquipmentType.Weapon || eq.EquipmentJson.fashionsuit == true);
    }
    public void SetDailyWeeklyLimit(int itemID, int dailyGet, int dailyUse, int weeklyGet, int weeklyUse)
    {
        if (mItem.JsonObject.itemid != itemID)
            return;

        string localstr = GUILocalizationRepo.GetLocalizedString("ItemTooltip_UseGetLimit");
        if (dailyGet > -1)
            mDailyGetTV.Value = string.Format(localstr, dailyGet, mItem.JsonObject.dailygetlimit);
        if (dailyUse > -1)
            mDailyUseTV.Value = string.Format(localstr, dailyUse, mItem.JsonObject.dailyuselimit);
        if (weeklyGet > -1)
            mWeeklyGetTV.Value = string.Format(localstr, weeklyGet, mItem.JsonObject.weeklygetlimit);
        if (weeklyUse > -1)
            mWeeklyUseTV.Value = string.Format(localstr, weeklyUse, mItem.JsonObject.weeklyuselimit);
    }

    #region Debug
    private void DebugShowInfo()
    {
#if ZEALOT_DEVELOPMENT
        if (ConsoleVariables.ShowItemID)
        {
            mItemTypeName.text += ClientUtils.FormatStringColor(string.Format(" (#ID: {0})", mItem.JsonObject.itemid), "#ff00ffff");
        }
#endif
    }
    #endregion
}

/*
#if UNITY_EDITOR
[CustomEditor(typeof(HUD_ItemDetailToolTip))]
public class HUD_ItemDetailToolTipEditor : Editor
{
    public override void OnInspectorGUI()
    {
        HUD_ItemDetailToolTip idtt = (HUD_ItemDetailToolTip)target;
        if (GUILayout.Button("Potion"))
        {
            PotionFood pot;
        }

        base.OnInspectorGUI();
    }
}
#endif
*/