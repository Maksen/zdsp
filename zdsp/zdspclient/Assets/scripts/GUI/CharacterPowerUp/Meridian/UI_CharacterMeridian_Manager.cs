using Kopio.JsonContracts;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections.Generic;
using Zealot.Client.Entities;
using Zealot.Common;
using Zealot.Repository;

struct MeridianObject
{
    public string typeName { get; private set; }
    public string effectName { get; private set; }
    public string effectValue  { get; private set; }
    public string animatorState  { get; private set; }
    public int level  { get; private set; }
    public int exp  { get; private set; }
    public int expPercent  { get; private set; }
    public int currency  { get; private set; }
    public List<ItemInfo> consumeItem  { get; private set; }
    public MeridianToggleData requireData { get; private set; }

    public MeridianObject(string mTypeName, string mEffectName, string mEffectValue, string mAnimatorState, int mLevel, int mExp, int mExpPercent, int mCurrency, List<ItemInfo> mConsumeItem, MeridianToggleData mRequireData)
    {
        typeName = mTypeName;
        effectName = mEffectName;
        effectValue = mEffectValue;
        animatorState = mAnimatorState;
        level = mLevel;
        exp = mExp;
        expPercent = mExpPercent;
        currency = mCurrency;
        consumeItem = mConsumeItem;
        requireData = mRequireData;
    }

    public void SetExpUp(int mExp, int mExpPercent, int mCurrency, List<ItemInfo> mConsumeItem)
    {
        exp = mExp;
        expPercent = mExpPercent;
        currency = mCurrency;
        consumeItem = mConsumeItem;
    }

    public void SetLevelUp(string mEffectValue, int mLevel, int mExp, int mExpPercent, int mCurrency, List<ItemInfo> mConsumeItem)
    {
        effectValue = mEffectValue;
        level = mLevel;
        exp = mExp;
        expPercent = mExpPercent;
        currency = mCurrency;
        consumeItem = mConsumeItem;
    }
}

public class UI_CharacterMeridian_Manager : MonoBehaviour
{
    private MeridianObject[] meridianObjects;
    private MeridianToggleData meridianCurrentObject, meridianNextObject;

    [SerializeField]
    private GameObject[] meridianToggles; //order is 1 2 5 6 4 3 8 7
    [SerializeField]
    private GameObject meridianCurrentToggles, meridianNextToggles;

    [SerializeField]
    private Text meridianExpText;
    [SerializeField]
    private Image meridianExpImage;
    [SerializeField]
    private Text meridianCurrencyText;

    private PlayerGhost player;
    private PowerUpController powerUpCtrl;

    [SerializeField]
    private Transform requireParent;
    [SerializeField]
    private GameObject requireObj;
    [SerializeField]
    private GameObject requireItemObj;

    [SerializeField]
    private GameObject onceMeridianButton;
    [SerializeField]
    private GameObject multiMeridianButton;
    [SerializeField]
    private Text multiMeridianText;

    #region Right Side Variable
    private string[] typesName = new string[] { "任脈", "衝脈", "陽蹻脈", "陽維脈", "督脈", "帶脈", "陰蹻脈", "陰維脈" };

    private int rsType;
    private int rsLevel;
    private int rsExp;
    private int rsExpPercent;
    private bool multiProgressing;
    #endregion

    #region BasicSetting
    private void Awake()
    {
        InitAwake();
        InitData();
    }

    private void OnEnable()
    {
        for (int i = 0; i < meridianObjects.Length; ++i)
        {
            if (meridianObjects[i].expPercent == 100 && meridianObjects[i].level > 0)
            {
                meridianObjects[i].requireData.PlayAnime("Toggle_Meridian_UnlockFx");
            }
        }
    }

    private void OnDisable()
    {
        multiProgressing = false;
    }

    public void SetMeridianObjSlot(int type)
    {
        SetMeridianUp(type);
    }
    #endregion

    #region Refresh
    private void InitAwake()
    {
        player = GameInfo.gLocalPlayer;
        powerUpCtrl = player.clientPowerUpCtrl;

        for (int i = 0; i < meridianToggles.Length; ++i)
        {
            int index = i;
            meridianToggles[i].GetComponent<Toggle>().onValueChanged.AddListener(delegate
            {
                SetMeridianRight(index);
            });
        }

        meridianCurrentObject = meridianCurrentToggles.GetComponent<MeridianToggleData>();
        meridianNextObject = meridianNextToggles.GetComponent<MeridianToggleData>();
    }

    private void InitData()
    {
        meridianObjects = new MeridianObject[8];

        for (int i = 0; i < meridianObjects.Length; ++i)
        {
            InitToggles(i);
        }
    }

    private void InitToggles(int type)
    {
        int level = powerUpCtrl.GetMeridianLevel(type);
        MeridianUnlockListJson unlockData = powerUpCtrl.GetMeridianUnlockJson(type);
        MeridianExpListJson expData = (level == 0) ? new MeridianExpListJson() :
            powerUpCtrl.GetMeridianExpJson(type);
        SideEffectJson sideEffect = SideEffectRepo.GetSideEffect(unlockData.effect);

        string typeName = typesName[type];
        string effectName = sideEffect.localizedname;
        string effectValue = unlockData.value.ToString();

        int exp = powerUpCtrl.GetMeridianExp(type);
        int expPercent = GetExpPercent(exp, expData.exp);
        string animatorState = GetAnimatorState(level, expPercent);
        List<ItemInfo> requireMaterials = GetRequireMaterials(type, expPercent);
        int currency = (expPercent < 100) ? expData.currency : unlockData.currency;

        MeridianObject currentObj = new MeridianObject(typeName, effectName, effectValue, animatorState,
            level, exp, expPercent, currency, requireMaterials, meridianToggles[type].GetComponent<MeridianToggleData>());
        meridianObjects[type] = currentObj;

        meridianObjects[type].requireData.Init(animatorState, typeName, level, effectName, effectValue);
    }

    private int GetExpPercent(int currentExp, int totalExp)
    {
        if (totalExp == 0)
            return 100;
        else
            return currentExp * 100 / totalExp;
    }

    private string GetBeginAnimatorState(int level, int expPercent)
    {
        if (level == 0)
            return "Toggle_Meridian_UnlockFx";
        else
            return GetAnimatorState(level, expPercent);
    }

    private string GetAnimatorState(int level, int expPercent)
    {
        if (level == 0)
            return "Toggle_Meridian_Locked";
        else if (expPercent < 100)
            return "Toggle_Meridian_UnLocked";
        else
            return "Toggle_Meridian_UnlockFx";
    }

    private List<ItemInfo> GetRequireMaterials(int type, int expPercent)
    {
        if (expPercent < 100)
            return powerUpCtrl.GetMeridianExpMaterial(type);
        else
            return powerUpCtrl.GetMeridianUnlockMaterial(type);
    }

    private void SetMeridianRight(int type)
    {
        rsType = type;

        ClientUtils.DestroyChildren(requireParent);
        MeridianObject selectedObject = meridianObjects[type];
        int level = selectedObject.level;
        int expPercent = selectedObject.expPercent;
        int currency = selectedObject.currency;

        string animationState = GetAnimatorState(level, expPercent);
        string typeName = selectedObject.typeName;
        string effectName = selectedObject.effectName;
        string effectValue = selectedObject.effectValue;

        meridianCurrentObject.Init(animationState, typeName, level, effectName, effectValue);

        MeridianUnlockListJson nextMeridian = PowerUpRepo.GetMeridianUnlockByTypesLevel(type, level + 1);
        if (nextMeridian == null)
        {
            meridianNextObject.Init(animationState, typeName, level, effectName, effectValue);
            meridianCurrencyText.text = 0.ToString();
            SetExpProgress(0);
            onceMeridianButton.SetActive(false);
            multiMeridianButton.SetActive(false);
            return;
        }

        animationState = GetBeginAnimatorState(level, expPercent);
        meridianCurrencyText.text = selectedObject.currency.ToString("N3");
        meridianNextObject.Init(animationState, typeName, level + 1, effectName, nextMeridian.value.ToString());
        SetExpProgress(selectedObject.expPercent);
        SetMaterial(selectedObject.consumeItem);
        SetButtonChange(false, expPercent);
    }

    private void SetExpProgress(int expPercent)
    {
        meridianExpText.text = ExpPercentText(expPercent);
        meridianExpImage.fillAmount = (float)expPercent / 100;
    }

    private string ExpPercentText(int expPercent)
    {
        return string.Format("{0}{1}", expPercent, " %");
    }

    private void SetMaterial(List<ItemInfo> items)
    {
        for (int i = 0; i < items.Count; ++i)
        {
            GameObject materialObj = ClientUtils.CreateChild(requireParent, requireObj);

            int amount = player.clientItemInvCtrl.itemInvData.GetTotalStackCountByItemId(items[i].itemId);

            RequireMeridianData reqMeridianData = materialObj.GetComponent<RequireMeridianData>();
            reqMeridianData.SetEquipIcon(requireItemObj);
            reqMeridianData.SetRequireStack(items[i].stackCount);
            reqMeridianData.equipIcon.InitWithToolTipView(items[i].itemId, amount);
        }
    }

    private void SetMeridianUp(int type)
    {
        if (rsType != type)
            return;

        MeridianObject obj = meridianObjects[type];

        MeridianExpListJson expJson = powerUpCtrl.GetMeridianExpJson(type);

        rsExp = powerUpCtrl.GetMeridianExp(type);
        rsExpPercent = GetExpPercent(rsExp, expJson.exp);
        List<ItemInfo> items = new List<ItemInfo>();
        int crtCurrency = 0;

        if (rsExpPercent == 100)
        {
            items = powerUpCtrl.GetMeridianUnlockMaterial(type);
            crtCurrency = powerUpCtrl.GetMeridianUnlockJson(type).currency;
        }
        else
        {
            if(rsExpPercent == 0)
            {
                rsLevel = powerUpCtrl.GetMeridianLevel(type);
                items = powerUpCtrl.GetMeridianExpMaterial(type);
                crtCurrency = powerUpCtrl.GetMeridianUnlockJson(type).currency;
            }
            else
            {
                items = obj.consumeItem;
                crtCurrency = obj.currency;
            }
        }

        meridianObjects[type].SetExpUp(rsExp, rsExpPercent, crtCurrency, items);
        SetButtonChange(multiProgressing, rsExpPercent);
    }

    private void SetButtonChange(bool processing = false, int expPercent = 0)
    {
        multiProgressing = processing;
        string buttonText = string.Empty;
        bool buttonActive = false;
        if (processing)
        {
            buttonText = "取消";
            ButtonCallBack(CancelMeridianMulti);
        }
        else
        {
            if (expPercent < 100)
            {
                buttonActive = true;
                buttonText = "一鍵養脈";
                ButtonCallBack(MeridianMulti);
            }
            else
            {
                buttonText = "解鎖";
                ButtonCallBack(MeridianUnlock);
            }
        }

        multiMeridianText.text = buttonText;
        onceMeridianButton.SetActive(buttonActive);
    }

    private void ButtonCallBack(UnityAction onClickCallback = null)
    {
        Button btn = multiMeridianButton.GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(delegate
        {
            onClickCallback();
        });
    }
    #endregion

    #region ClickEvent
    private void MeridianUnlock()
    {
        if (EnoughRequire())
            RPCFactory.NonCombatRPC.MeridianUp(rsType);
        else
            UIManager.ShowSystemMessage("解鎖道具或貨幣不足，無法解鎖");
    }

    private void MeridianOnce()
    {
        if (EnoughRequire())
            RPCFactory.NonCombatRPC.MeridianUp(rsType);
        else
            UIManager.ShowSystemMessage("解鎖道具或貨幣不足，無法養脈");
    }

    private void MeridianMulti()
    {
        SetButtonChange(true);
        MeridianContinue();
    }

    private void MeridianContinue()
    {
        if(multiProgressing && EnoughRequire())
        {
            Invoke("MeridianContinue", 0.7f);
            RPCFactory.NonCombatRPC.MeridianUp(rsType);
        }
    }

    private void CancelMeridianMulti()
    {
        SetButtonChange(false);
    }

    private bool EnoughRequire()
    {
        List<ItemInfo> items = meridianObjects[rsType].consumeItem;
        bool enoRequire = true;
        for (int i = 0; i < items.Count; ++i)
        {
            int invItem = player.clientItemInvCtrl.itemInvData.GetTotalStackCountByItemId(items[i].itemId);
            if (invItem < items[i].stackCount)
                enoRequire = false;
        }
        return enoRequire;
    }
    #endregion
}