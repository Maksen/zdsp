using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;

using Zealot.Repository;
using Zealot.Common;
using Kopio.JsonContracts;
using Zealot.Client.Entities;

public class ItemDetailsButton
{
    public string name;
    public string icon;
    public UnityAction callback;
}

public class HUD_ItemDetailToolTip : MonoBehaviour
{
    [Header("Game Icon Prefab")]
    #region Game Icon Prefabs
    [SerializeField]
    GameObject mEquipIconPrefab;
    [SerializeField]
    GameObject mMaterialConsumableIconPrefab;
    [SerializeField]
    GameObject mGeneIconPrefab;
    #endregion

    [Header("Tooltip Prefab list")]
    #region Tooltip Prefab
    [SerializeField]
    GameObject mTTLinePrefab;
    [SerializeField]
    GameObject mTTTextColonValuePrefab;
    [SerializeField]
    GameObject mTTValuePrefab;
    [SerializeField]
    GameObject mTTLocationPrefab;
    #endregion

    [Header("Left Panel Btn Prefab")]
    [SerializeField]
    GameObject mLPBtnPrefab;

    [SerializeField]
    List<Sprite> mButtonSprites;

    [Header("Parents")]
    #region Parent obj
    [SerializeField]
    GameObject mIconParent;
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
    GameIcon_Base mIcon;
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
    //[SerializeField]
    //Dialog_WhereToGet mWhereToGet;

    IInventoryItem mItem = null;
    List<GameObject> mNormalStatsLst = new List<GameObject>();
    List<GameObject> mExtraSideEffectLst = new List<GameObject>();

    /// <summary>
    /// Pass custom item in from inventory
    /// </summary>
    public void InitTooltip (IInventoryItem item)
    {
        //Retrieve what you need from
        //InventoryItem
        //Inventory stats
        //Equipment class
        //var stat = GameInfo.gLocalPlayer.InventoryStats;

        if (item == null)
            return;
        if (mIconParent == null)
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
        }
    }
    public void OnDisable()
    {
        mWhereToGetTog.isOn = false;
        mExtraEffectTog.isOn = false;
        m3DViewTog.isOn = false;
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
            if (m3DAvatar.GetOutfitModel() == null && toggle)
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
        //Show drop location dialog
        //mWhereToGet.InitWithItem(mItem);
    }
    #endregion
    #region ItemType Tooltip init
    private void ClearTT()
    {
        if (mIconParent != null)
        {
            foreach (Transform t in mIconParent.transform)
                Destroy(t.gameObject);
        }
        if (mStatsParent != null)
        {
            mNormalStatsLst.Clear();
            mExtraSideEffectLst.Clear();
            foreach (Transform t in mStatsParent.transform)
                Destroy(t.gameObject);
        }
        ClearTTBtn();
    }
    private void ClearTTBtn()
    {
        if (mLeftBtnPanelParent != null)
        {
            mLeftPanelButtonLst.Clear();
            foreach (Transform t in mLeftBtnPanelParent.transform)
                Destroy(t.gameObject);
        }
    }
    private void InitTT_Potion(IInventoryItem item)
    {
        //*** Icon - General item ***
        GameObject obj = CreateIcon(mMaterialConsumableIconPrefab, item);
        mIcon = obj.GetComponent<GameIcon_MaterialConsumable>();
        GameIcon_MaterialConsumable mcIcon = obj.GetComponent<GameIcon_MaterialConsumable>();
        mcIcon.Init(item.ItemID, item.StackCount, false);

        //*** Stats ***
        Text sideeffectTxt = null; //Side effect
        HUD_ItemDetail_TextColonValue tcv = null; //Req Level
        Text txtStorage = null; //Can put into storage
        Text txtDescription = null; //Description

        //Create common element
        InitTT_CommonWithReqLevel(item, out txtStorage, out txtDescription, out tcv);
        CreateText(out sideeffectTxt);

        //Check if all obj has been created
        if (txtStorage == null || sideeffectTxt == null || txtDescription == null || tcv == null)
            return;

        //Parent objects as needed
        tcv.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        sideeffectTxt.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        txtStorage.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        txtDescription.transform.SetParent(mStatsParent.transform, false);
        
        

        //Set data to our gameobjects
        sideeffectTxt.text = "";
        PotionFood pf = item as PotionFood;
        string[] seGrpLst = pf.PotionFoodJson.segrpid.Split(';'); //break string and find all sideeffect groups
        foreach (string s in seGrpLst)
        {
            int x;
            if (int.TryParse(s, out x) == false)
            {
                Debug.LogError("HUD_ItemDetailToolTip.InitTT_Potion: Cannot convert sideGroupId from string to int");
                sideeffectTxt.text = "#unlocalized side effect#";
                break;
            }

            //Loop all sideeffect in the sideeffect grp, get their description in SDG
            Dictionary<int, SideEffectJson> seGrp = SideEffectRepo.mSideEffectGroups[x].sideeffects;
            foreach (SideEffectJson se in seGrp.Values)
                sideeffectTxt.text += SDGRepo.GetSDGText(se);
        }
    }
    private void InitTT_Material(IInventoryItem item)
    {
        //*** Icon - General item ***
        GameObject obj = CreateIcon(mMaterialConsumableIconPrefab, item);
        mIcon = obj.GetComponent<GameIcon_MaterialConsumable>();
        GameIcon_MaterialConsumable mcIcon = obj.GetComponent<GameIcon_MaterialConsumable>();
        mcIcon.Init(item.ItemID, item.StackCount, false);

        //*** Stats ***
        Text txtStorage = null; //Can put into storage
        Text txtDescription = null; //Description

        //Create common element
        InitTT_CommonShared(item, out txtStorage, out txtDescription);

        //Check if all obj has been created
        if (txtStorage == null || txtDescription == null)
            return;

        //Set parent
        txtStorage.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        txtDescription.transform.SetParent(mStatsParent.transform, false);
    }
    private void InitTT_Exchange(IInventoryItem item)
    {
        //*** Icon - General item ***
        GameObject obj = CreateIcon(mMaterialConsumableIconPrefab, item);
        mIcon = obj.GetComponent<GameIcon_MaterialConsumable>();
        GameIcon_MaterialConsumable mcIcon = obj.GetComponent<GameIcon_MaterialConsumable>();
        mcIcon.Init(item.ItemID, item.StackCount, false);

        //*** Stats ***
        HUD_ItemDetail_TextColonValue tcvReqLv = null; //Req level
        Text txtStorage = null; //Can put into storage
        Text txtDescription = null; //Description
        Text txtExchangeItem = null; //Exchange ItemID
        Text txtReqValue = null; //Require Value

        //Create common element
        InitTT_CommonWithReqLevel(item, out txtStorage, out txtDescription, out tcvReqLv);
        CreateText(out txtExchangeItem);
        CreateText(out txtReqValue);

        //Check if all obj has been created
        if (txtStorage == null || txtDescription == null || txtExchangeItem == null || txtReqValue == null)
            return;

        //Parent object as needed
        tcvReqLv.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        txtExchangeItem.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        txtReqValue.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        txtStorage.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        txtDescription.transform.SetParent(mStatsParent.transform, false);
        

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
        GameObject obj = CreateIcon(mMaterialConsumableIconPrefab, item);
        mIcon = obj.GetComponent<GameIcon_MaterialConsumable>();
        GameIcon_MaterialConsumable mcIcon = obj.GetComponent<GameIcon_MaterialConsumable>();
        mcIcon.Init(item.ItemID, item.StackCount, false);

        //*** Stats ***
        Text txtStorage = null; //Can put into storage
        Text txtDescription = null; //Description

        //Create common element
        InitTT_CommonShared(item, out txtStorage, out txtDescription);

        //Check if all obj has been created
        if (txtStorage == null || txtDescription == null)
            return;

        //parent objects as needed
        txtStorage.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        txtDescription.transform.SetParent(mStatsParent.transform, false);
    }
    private void InitTT_Token(IInventoryItem item)
    {
        //Written in CR56
        //*** Icon - General item ***
        GameObject obj = CreateIcon(mMaterialConsumableIconPrefab, item);
        mIcon = obj.GetComponent<GameIcon_MaterialConsumable>();
        GameIcon_MaterialConsumable mcIcon = obj.GetComponent<GameIcon_MaterialConsumable>();
        mcIcon.Init(item.ItemID, item.StackCount, false);

        //*** Stats ***
        HUD_ItemDetail_TextColonValue tcv = null; //Req Level
        Text txtStorage = null; //Can put into storage
        Text txtDescription = null; //Description

        //Create common element
        InitTT_CommonWithReqLevel(item, out txtStorage, out txtDescription, out tcv);

        //Check if all obj has been created
        if (txtStorage == null || txtDescription == null || tcv == null)
            return;

        //parent objects as needed
        tcv.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        txtStorage.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        txtDescription.transform.SetParent(mStatsParent.transform, false);
    }
    private void InitTT_LuckyPick(IInventoryItem item)
    {
        //*** Icon - General item ***
        GameObject obj = CreateIcon(mMaterialConsumableIconPrefab, item);
        mIcon = obj.GetComponent<GameIcon_MaterialConsumable>();
        GameIcon_MaterialConsumable mcIcon = obj.GetComponent<GameIcon_MaterialConsumable>();
        mcIcon.Init(item.ItemID, item.StackCount, false);

        //*** Stats ***
        HUD_ItemDetail_TextColonValue tcv = null; //Req Level
        Text txtStorage = null; //Can put into storage
        Text txtDescription = null; //Description

        //Create common element
        InitTT_CommonWithReqLevel(item, out txtStorage, out txtDescription, out tcv);

        //Check if all obj has been created
        if (txtStorage == null || txtDescription == null || tcv == null)
            return;

        //parent objects as needed
        tcv.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        txtStorage.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        txtDescription.transform.SetParent(mStatsParent.transform, false);
    }
    private void InitTT_Henshin(IInventoryItem item)
    {
        //*** Icon - General item ***
        GameObject obj = CreateIcon(mMaterialConsumableIconPrefab, item);
        mIcon = obj.GetComponent<GameIcon_MaterialConsumable>();
        GameIcon_MaterialConsumable mcIcon = obj.GetComponent<GameIcon_MaterialConsumable>();
        mcIcon.Init(item.ItemID, item.StackCount, false);

        //*** Stats ***
        HUD_ItemDetail_TextColonValue tcv = null; //Req Level
        Text txtStorage = null; //Can put into storage
        Text txtDescription = null; //Description

        //Create common element
        InitTT_CommonWithReqLevel(item, out txtStorage, out txtDescription, out tcv);

        //Check if all obj has been created
        if (txtStorage == null || txtDescription == null || tcv == null)
            return;

        //parent objects as needed
        tcv.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        txtStorage.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        txtDescription.transform.SetParent(mStatsParent.transform, false);
    }
    private void InitTT_Special(IInventoryItem item)
    {
        //*** Icon - General item ***
        GameObject obj = CreateIcon(mMaterialConsumableIconPrefab, item);
        mIcon = obj.GetComponent<GameIcon_MaterialConsumable>();
        GameIcon_MaterialConsumable mcIcon = obj.GetComponent<GameIcon_MaterialConsumable>();
        mcIcon.Init(item.ItemID, item.StackCount, false);

        //*** Stats ***
        HUD_ItemDetail_TextColonValue tcv = null; //Req Level
        Text txtStorage = null; //Can put into storage
        Text txtDescription = null; //Description

        //Create common element
        InitTT_CommonWithReqLevel(item, out txtStorage, out txtDescription, out tcv);

        //Check if all obj has been created
        if (txtStorage == null || txtDescription == null || tcv == null)
            return;

        //parent objects as needed
        tcv.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        txtStorage.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        txtDescription.transform.SetParent(mStatsParent.transform, false);
    }
    private void InitTT_Features(IInventoryItem item)
    {
        //Written in CR56 and 58 now
        //*** Icon - General item ***
        GameObject obj = CreateIcon(mMaterialConsumableIconPrefab, item);
        mIcon = obj.GetComponent<GameIcon_MaterialConsumable>();
        GameIcon_MaterialConsumable mcIcon = obj.GetComponent<GameIcon_MaterialConsumable>();
        mcIcon.Init(item.ItemID, item.StackCount, false);

        //*** Stats ***
        HUD_ItemDetail_TextColonValue tcv = null; //Req Level
        Text txtStorage = null; //Can put into storage
        Text txtDescription = null; //Description

        //Create common element
        InitTT_CommonWithReqLevel(item, out txtStorage, out txtDescription, out tcv);

        //Check if all obj has been created
        if (txtStorage == null || txtDescription == null || tcv == null)
            return;

        //parent objects as needed
        tcv.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        txtStorage.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        txtDescription.transform.SetParent(mStatsParent.transform, false);
    }
    private void InitTT_Equipment(IInventoryItem item)
    {
        //*** Icon - Equipment ***
        GameObject obj = CreateIcon(mEquipIconPrefab, item);
        mIcon = obj.GetComponent<GameIcon_Equip>();
        GameIcon_Equip iconEq = obj.GetComponent<GameIcon_Equip>();
        Equipment eq = item as Equipment;
        iconEq.Init(eq.ItemID, 0, 0, eq.UpgradeLevel, false, false);



        //*** Stats ***
        //Common
        HUD_ItemDetail_TextColonValue tcvReqLvl = null; //Req Level
        Text txtStorage = null; //Can put into storage
        Text txtDescription = null; //Description
        //Unique
        Text txtStats = null;
        HUD_ItemDetail_TextColonValue tcvStats = null;

        //Create common element
        InitTT_CommonWithReqLevel(item, out txtStorage, out txtDescription, out tcvReqLvl);

        //Common - Req Lv
        mNormalStatsLst.Add(tcvReqLvl.gameObject);
        mNormalStatsLst.Add(CreateLineNoParent());

        //Unique - Wpn Main Attr
        CreateTextColonValue(out tcvStats);
        tcvStats.Identifier = GUILocalizationRepo.GetLocalizedString("id_wpnmainattr");
        switch (eq.EquipmentJson.mainwepattrib)
        {
            case MainWeaponAttribute.Str:
                tcvStats.Value = GUILocalizationRepo.GetLocalizedString("ci_strength");
                break;
            case MainWeaponAttribute.Dex:
                tcvStats.Value = GUILocalizationRepo.GetLocalizedString("ci_dexterity");
                break;
            case MainWeaponAttribute.Int:
                tcvStats.Value = GUILocalizationRepo.GetLocalizedString("ci_inteligence");
                break;
        }
        mNormalStatsLst.Add(tcvStats.gameObject);
        mNormalStatsLst.Add(CreateLineNoParent());

        //Unique - req class, if -1 == all class, otherwise.. some classes
        //Missing localization
        CreateTextColonValue(out tcvStats);
        tcvStats.Identifier = GUILocalizationRepo.GetLocalizedString("id_class");
        tcvStats.Value = eq.EquipmentJson.equipclass; //NOTE: Incorrect
        mNormalStatsLst.Add(tcvStats.gameObject);
        mNormalStatsLst.Add(CreateLineNoParent());

        //Unique - Base side effect
        CreateText(out txtStats);
        txtStats.text = "";
        string[] baseseGrpLst = eq.EquipmentJson.basese.Split(';');
        foreach (string s in baseseGrpLst)
        {
            int x;
            if (int.TryParse(s, out x) == false || x < 0)
            {
                //Debug.LogError("HUD_ItemDetailToolTip.InitTT_Equipment: Cannot convert string in baseSideEffectGroupList to int");
                break;
            }

            //Loop all sideeffect in the sideeffect grp, get their description in SDG
            Dictionary<int, SideEffectJson> seGrp = SideEffectRepo.mSideEffectGroups[x].sideeffects;
            foreach (SideEffectJson se in seGrp.Values)
                txtStats.text += SDGRepo.GetSDGText(se);
        }
        mNormalStatsLst.Add(txtStats.gameObject);
        mNormalStatsLst.Add(CreateLineNoParent());

        //Unique - qiang hua level and effect
        //CreateText(out txtStats);
        //txtStats.text = GUILocalizationRepo.GetLocalizedString("id_powerup");
        //mNormalStatsLst.Add(txtStats.gameObject);
        //CreateTextColonValue(out tcvStats);
        //tcvStats.Identifier = string.Format("{0}/{1}", 0, 0);   //qiang hua lvl, player max lv
        //tcvStats.Value = string.Format("{0} + {1}",0 , 0); //effect, qiang hua value
        //mNormalStatsLst.Add(tcvStats.gameObject);
        //mNormalStatsLst.Add(CreateLine());

        //Unique - Refine effect
        //CreateText(out txtStats);
        //txtStats.text = "Refine effect";
        //mNormalStatsLst.Add(txtStats.gameObject);
        //CreateTextColonValue(out tcvStats);
        //tcvStats.Identifier = string.Format("{0}/{1}", 0, eq.EquipmentJson.upgradelimit); //refine level, upgrade limit
        //tcvStats.Value = string.Format("{0} + {1}", eq.EquipmentJson.basese, 0); //basesideeffect, refine bonus
        //mNormalStatsLst.Add(tcvStats.gameObject);
        //mNormalStatsLst.Add(CreateLine());

        //Unique - Refinement lv effect
        //CreateText(out txtStats);
        //txtStats.text = "Refine Lv effect";
        //mNormalStatsLst.Add(txtStats.gameObject);
        //Loop through all refinement effects that this weapon has unlocked based on its refine lvl +8/+9/+10
        //CreateTextColonValue(out tcvStats);
        //tcvStats.Identifier = string.Format(GUILocalizationRepo.GetLocalizedString("id_refinelv"), 0); //refine level
        //tcvStats.Value = string.Format(GUILocalizationRepo.GetLocalizedString("id_refinelv2"), 0); //equipmentUpgrade's buff
        //mNormalStatsLst.Add(tcvStats.gameObject);
        //mNormalStatsLst.Add(CreateLine());

        //Unique - Chu Mo effect - hidden until further notice

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

        //Unique - evolve bonus - aka RO mobile Weapon Tier1,2,3
        //Loop through all evolve stages
        //CreateTextColonValue(out tcvStats);
        //tcvStats.Identifier = "step N";
        //tcvStats.Value = "value N";
        //mNormalStatsLst.Add(tcvStats.gameObject);
        //mNormalStatsLst.Add(CreateLine());

        //Common - can storage
        mNormalStatsLst.Add(txtStorage.gameObject);
        mNormalStatsLst.Add(CreateLineNoParent());

        //Common - description
        mNormalStatsLst.Add(txtDescription.gameObject);

        //Set Parent
        for (int i = 0; i < mNormalStatsLst.Count; ++i)
            mNormalStatsLst[i].transform.SetParent(mStatsParent.transform, false);





        //*** Extra Sideeffect Stats ***
        txtStats.text = "";
        string[] extraseGrpLst = eq.EquipmentJson.extrase.Split(';');
        foreach (string s in extraseGrpLst)
        {
            int x;
            if (int.TryParse(s, out x) == false || x < 0)
            {
                //Debug.LogError("HUD_ItemDetailToolTip.InitTT_Equipment: Cannot convert string in extraSideEffectGroupList to int");
                break;
            }

            //Loop all sideeffect in the sideeffect grp, get their description in SDG
            Dictionary<int, SideEffectJson> seGrp = SideEffectRepo.mSideEffectGroups[x].sideeffects;
            foreach (SideEffectJson se in seGrp.Values)
                txtStats.text += SDGRepo.GetSDGText(se);
        }
        mExtraSideEffectLst.Add(txtStats.gameObject);
        mExtraSideEffectLst.Add(CreateLine());
    }
    private void InitTT_DNA(IInventoryItem item)
    {
        //*** Icon - Gene ***
        GameObject obj = CreateIcon(mGeneIconPrefab, item);
        mIcon = obj.GetComponent<GameIcon_DNA>();
        GameIcon_DNA mcIcon = obj.GetComponent<GameIcon_DNA>();
        mcIcon.Init(item.ItemID);
        DNA dnaItem = item as DNA;
        //mcIcon.Level = geneItem.GeneJson.;
        //mcIcon.EvolveLevel = geneItem.GeneJson.;

        //*** Stats ***
        Text txtPosGene = null; //Positive gene effect
        Text txtNegGene = null; //Negative gene effect
        Text txtStorage = null; //Can put into storage
        Text txtDescription = null; //Description

        //Create common element
        InitTT_CommonShared(item, out txtStorage, out txtDescription);
        CreateText(out txtPosGene);
        CreateText(out txtNegGene);

        //Check if all obj has been created
        if (txtPosGene == null || txtNegGene == null || txtStorage == null || txtDescription == null)
            return;

        //Parent objects as needed
        txtPosGene.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        txtNegGene.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        txtStorage.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        txtDescription.transform.SetParent(mStatsParent.transform, false);
        

        //set data to our gameobjects
        DNA dna = item as DNA;
        txtPosGene.text = dna.DNAJson.postive;
        txtNegGene.text = dna.DNAJson.negative;
    }
    private void InitTT_Relic(IInventoryItem item)
    {
        //*** Icon - General item ***
        GameObject obj = Instantiate(mMaterialConsumableIconPrefab, Vector3.zero, Quaternion.identity);
        mIcon = obj.GetComponent<GameIcon_MaterialConsumable>();
        mIcon.transform.SetParent(mIconParent.transform, false);
        GameIcon_MaterialConsumable mcIcon = obj.GetComponent<GameIcon_MaterialConsumable>();
        mcIcon.Init(item.ItemID, item.StackCount, false);
        mItemName.text = item.JsonObject.localizedname;
        mItemTypeName.text = GameRepo.ItemFactory.ItemSortTable[item.JsonObject.itemsort].localizedname;

        //*** Stats ***
        Text txtCanRecycle = null;
        Text txtSocketAbility = null;
        Text txtCollectionAbility = null;
        Text txtStorage = null; //Can put into storage
        Text txtDescription = null; //Description

        //Create common element
        InitTT_CommonShared(item, out txtStorage, out txtDescription);
        CreateText(out txtCanRecycle);
        CreateText(out txtSocketAbility);
        CreateText(out txtCollectionAbility);

        //Check if all obj has been created
        if (txtCanRecycle == null || txtSocketAbility == null || txtCollectionAbility == null || txtStorage == null || txtDescription == null)
            return;

        //Set parent
        txtSocketAbility.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        txtCollectionAbility.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        txtCanRecycle.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        txtStorage.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        txtDescription.transform.SetParent(mStatsParent.transform, false);

        //Set data to our gameobjects
        Relic relic = item as Relic;
        //Can recycle?
        if (relic.RelicJson.recycle)
            txtCanRecycle.text = GUILocalizationRepo.GetLocalizedString("Can_Recycle");
        else
            txtCanRecycle.text = GUILocalizationRepo.GetLocalizedString("Can_Recycle");
        //Socket ability?
        txtSocketAbility.text = relic.RelicJson.sockability;
        //Collection ability
        txtCollectionAbility.text = relic.RelicJson.collectability;
    }
    private void InitTT_QuestItem(IInventoryItem item)
    {
        //*** Icon - General item ***
        GameObject obj = Instantiate(mMaterialConsumableIconPrefab, Vector3.zero, Quaternion.identity);
        mIcon = obj.GetComponent<GameIcon_MaterialConsumable>();
        mIcon.transform.SetParent(mIconParent.transform, false);
        GameIcon_MaterialConsumable mcIcon = obj.GetComponent<GameIcon_MaterialConsumable>();
        mcIcon.Init(item.ItemID, item.StackCount, false);
        mItemName.text = item.JsonObject.localizedname;
        mItemTypeName.text = GameRepo.ItemFactory.ItemSortTable[item.JsonObject.itemsort].localizedname;

        //*** Stats ***
        HUD_ItemDetail_TextColonValue tcv = null; //Req Level
        Text txtStorage = null; //Can put into storage
        Text txtDescription = null; //Description

        //Create common element
        InitTT_CommonWithReqLevel(item, out txtStorage, out txtDescription, out tcv);

        //Check if all obj has been created
        if (tcv == null || txtStorage == null || txtDescription == null)
            return;

        //Set parent
        tcv.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        txtStorage.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        txtDescription.transform.SetParent(mStatsParent.transform, false);
    }
    private void InitTT_MercenaryItem(IInventoryItem item)
    {
        //*** Icon - General item ***
        GameObject obj = Instantiate(mMaterialConsumableIconPrefab, Vector3.zero, Quaternion.identity);
        mIcon = obj.GetComponent<GameIcon_MaterialConsumable>();
        mIcon.transform.SetParent(mIconParent.transform, false);
        GameIcon_MaterialConsumable mcIcon = obj.GetComponent<GameIcon_MaterialConsumable>();
        mcIcon.Init(item.ItemID, item.StackCount, false);
        mItemName.text = item.JsonObject.localizedname;
        mItemTypeName.text = GameRepo.ItemFactory.ItemSortTable[item.JsonObject.itemsort].localizedname;

        //*** Stats ***
        Text txtStorage = null; //Can put into storage
        Text txtDescription = null; //Description

        //Create common element
        InitTT_CommonShared(item, out txtStorage, out txtDescription);

        //Check if all obj has been created
        if (txtStorage == null || txtDescription == null)
            return;

        //Set parent
        txtStorage.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        txtDescription.transform.SetParent(mStatsParent.transform, false);
    }
    private void InitTT_InstanceItem(IInventoryItem item)
    {
        //*** Icon - General item ***
        GameObject obj = Instantiate(mMaterialConsumableIconPrefab, Vector3.zero, Quaternion.identity);
        mIcon = obj.GetComponent<GameIcon_MaterialConsumable>();
        mIcon.transform.SetParent(mIconParent.transform, false);
        GameIcon_MaterialConsumable mcIcon = obj.GetComponent<GameIcon_MaterialConsumable>();
        mcIcon.Init(item.ItemID, item.StackCount, false);
        mItemName.text = item.JsonObject.localizedname;
        mItemTypeName.text = GameRepo.ItemFactory.ItemSortTable[item.JsonObject.itemsort].localizedname;

        //*** Stats ***
        Text txtStorage = null; //Can put into storage
        Text txtDescription = null; //Description
        HUD_ItemDetail_TextColonValue tiv = null;

        //Create common element
        InitTT_CommonShared(item, out txtStorage, out txtDescription);
        CreateTextColonValue(out tiv);

        //Check if all obj has been created
        if (txtStorage == null || txtDescription == null || tiv == null)
            return;

        //Set Parent
        tiv.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        txtStorage.transform.SetParent(mStatsParent.transform, false);
        CreateLine();
        txtDescription.transform.SetParent(mStatsParent.transform, false);

        //Set data to our gameobjects
        InstanceItem instanceItem = item as InstanceItem;
        //coordinates
        tiv.Identifier = GUILocalizationRepo.GetLocalizedString("id_location");
        tiv.ShowColon = true;
        tiv.Value = instanceItem.InstanceItemJson.coordinate;
    }
    private void InitTT_CommonShared(IInventoryItem item, out Text putIntoStorage, out Text description)
    {
        //*** Stats ***
        //can put into storage
        if (!CreateText(out putIntoStorage))
        {
            putIntoStorage = null;
            description = null;
            return;
        }

        //Description
        if (!CreateText(out description))
        {
            putIntoStorage = null;
            description = null;
            return;
        }

        //json's first string after split = base price or -1 (cannot auction)
        string cannotAuction = GUILocalizationRepo.GetLocalizedString("id_cannotauction");
        string[] auctionStr = item.JsonObject.auction.Split('|');
        int value;
        int.TryParse(auctionStr[0], out value);

        putIntoStorage.text = GUILocalizationRepo.GetLocalizedString("id_canstorage");
        description.text = item.JsonObject.description;
        mAuctionPrice.text = (value == -1) ? cannotAuction : auctionStr[0];
        mSellPrice.text = (item.JsonObject.sellprice != -1) ? item.JsonObject.sellprice.ToString() : GUILocalizationRepo.GetLocalizedString("id_cannotsell");
        mAuctionIcon.SetActive(value != -1);
        mSellIcon.SetActive(item.JsonObject.sellprice != -1);

        //Hide unnecessary buttons
        mWhereToGetTog.gameObject.SetActive(item.JsonObject.origin == "-1");
        mExtraEffectTog.gameObject.SetActive(item.JsonObject.itemtype == ItemType.Equipment);
        m3DViewTog.gameObject.SetActive(is3DViewable(item));
    }
    private void InitTT_CommonWithReqLevel(IInventoryItem item, 
                                            out Text putIntoStorage,
                                            out Text description, 
                                            out HUD_ItemDetail_TextColonValue tcv)
    {
        //*** Stats ***
        //Req Lv
        if (CreateTextColonValue(out tcv))
        {
            putIntoStorage = null;
            description = null;
            tcv = null;
            return;
        }
        tcv.Identifier = GUILocalizationRepo.GetLocalizedString("id_requirelevel");
        tcv.Value = item.JsonObject.requirelvl.ToString();


        InitTT_CommonShared(item, out putIntoStorage, out description);
    }

    #region shortcut create
    /// <summary>
    /// Output object will not be parented to any object
    /// </summary>
    /// <param name="tiv"></param>
    /// <returns></returns>
    private bool CreateTextColonValue(out HUD_ItemDetail_TextColonValue tiv)
    {
        GameObject obj = Instantiate(mTTTextColonValuePrefab, Vector3.zero, Quaternion.identity);
        tiv = obj.GetComponent<HUD_ItemDetail_TextColonValue>();
        if (tiv == null)
        {
            Debug.LogError("HUD_ItemDetailToolTip.CreateTextColonValue: mTTTextColonValuePrefab has no HUD_ItemDetail_TextColonValue script file");
            return true;
        }

        return false;
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
    private GameObject CreateIcon(GameObject prefab, IInventoryItem item)
    {
        GameObject obj = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        obj.transform.SetParent(mIconParent.transform, false);

        mItemName.text = item.JsonObject.localizedname;
        mItemTypeName.text = GameRepo.ItemFactory.ItemSortTable[item.JsonObject.itemsort].localizedname;

        return obj;
    }
    private void CreateLeftPanelButton(ItemDetailsButton button)
    {
        GameObject obj = Instantiate(mLPBtnPrefab);
        obj.transform.SetParent(mLeftBtnPanelParent.transform, false);
        obj.GetComponent<HUD_ItemDetailToolTip_Btn>().Init(
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
                ac.OnWeaponChanged(eq.EquipmentJson.prefabpath);
                break;
            case PartsType.Helm:
                ac.OnSkinChanged("helm", eq.EquipmentJson, pg.mGender);
                break;
            case PartsType.Body:
                ac.OnSkinChanged("body", eq.EquipmentJson, pg.mGender);
                break;
            case PartsType.Wing:
                ac.OnBackChanged(eq.EquipmentJson.prefabpath);
                break;
            case PartsType.Bathrobe:
                int len = eq.EquipmentJson.malemeshpath.Length + eq.EquipmentJson.femalemeshpath.Length +
                          eq.EquipmentJson.malematerialpath.Length + eq.EquipmentJson.femalematerialpath.Length;
                if (len == 0)
                {
                    Debug.LogError("HUD_ItemDetailTooltip.Toggle3DViewEquipment: Bathrobe has no mesh & material path");
                    return;
                }

                string meshPath = (pg.mGender == Gender.Male) ? eq.EquipmentJson.malemeshpath : eq.EquipmentJson.femalemeshpath;
                string matPath = (pg.mGender == Gender.Male) ? eq.EquipmentJson.malematerialpath : eq.EquipmentJson.femalematerialpath;

                //Remove all appearance equipment
                ac.Unequip("weapon_r");
                ac.Unequip("helm");
                ac.Unequip("body");
                //Wear robe if there is robe, robe has mesh and skin
                ac.OnSkinChanged("body", meshPath, matPath);
                //Wear headtowel if there is path written in kopio, headtowel has mesh and skin
                ac.OnSkinChanged("helm", meshPath, matPath);
                break;
            default:
                break;
        }
    }
    private bool is3DViewable(IInventoryItem item)
    {
        if (item.JsonObject.itemtype != ItemType.Equipment)
            return false;

        Equipment eq = item as Equipment;
        return (eq.EquipmentJson.equiptype == EquipmentType.Weapon || eq.EquipmentJson.fashionsuit == true);
    }
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