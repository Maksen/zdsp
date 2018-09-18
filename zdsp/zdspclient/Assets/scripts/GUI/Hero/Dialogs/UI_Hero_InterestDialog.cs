using Kopio.JsonContracts;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Client.Entities;
using Zealot.Common;
using Zealot.Repository;

public class UI_Hero_InterestDialog : BaseWindowBehaviour
{
    [SerializeField] Image currentInterestImage;
    [SerializeField] Text currentInterestDescText;
    [SerializeField] Text interestNameText;
    [SerializeField] Transform itemIconTransform;
    [SerializeField] GameObject itemIconPrefab;
    [SerializeField] Text itemNameText;
    [SerializeField] Text descriptionText;
    [SerializeField] Button randomBtn;
    [SerializeField] Button confirmBtn;
    [SerializeField] GameObject resetBtn;

    [Header("Circle Scroll")]
    [SerializeField] InterestCircleScroll circleScroll;
    [SerializeField] float scrollToDuration = 0.4f;
    [SerializeField] int roundsToSpin = 4;
    [SerializeField] float minScrollDuration = 0.025f;
    [SerializeField] float maxScrollDuration = 0.4f;
    [SerializeField] AnimationCurve lerpCurve;

    private bool initialized;
    private HeroInterestType selectedType;
    private Hero hero;
    private GameIcon_MaterialConsumable item;
    private bool waitForResetComplete;
    private bool waitForSpinConfirm;
    private bool hasEnoughItem;
    private int bindItemId, unbindItemId;
    private bool showSpendConfirmation;
    private Color colorRed;
    private byte spinResult;

    // for simulate spinning
    private int indexStep = 1;
    private bool spinning;
    private int totalSteps;
    private int currentStep;
    private int totalCellCount;

    private void Awake()
    {
        colorRed = descriptionText.color;
        totalCellCount = Enum.GetNames(typeof(HeroInterestType)).Length;
    }

    public override void OnCloseWindow()
    {
        base.OnCloseWindow();
        spinning = false;
        EnableCicleScroll(true);
        waitForSpinConfirm = false;
        randomBtn.interactable = true;
        circleScroll.SelectCell(0, 0f);
    }

    public void Init(Hero hero, Sprite currentSprite, string currentDesc)
    {
        this.hero = hero;
        currentInterestImage.sprite = currentSprite;
        currentInterestDescText.text = currentDesc;

        if (item == null)
        {
            GameObject icon = ClientUtils.CreateChild(itemIconTransform, itemIconPrefab);
            item = icon.GetComponent<GameIcon_MaterialConsumable>();
        }

        if (!initialized)
        {
            circleScroll.SetUp(OnInterestSelected);
            circleScroll.SelectCell(0, 0f);
            initialized = true;
        }
        else
            OnInterestSelected(0);
        circleScroll.SetCellsApplicable(hero.HeroJson.interestgroup);
    }

    public void UpdateInterest(Hero hero, Sprite currentSprite, string currentDesc)
    {
        this.hero = hero;
        currentInterestImage.sprite = currentSprite;
        currentInterestDescText.text = currentDesc;
        ResetCircleScroll();
    }

    private float GetScrollDuration(int currentStep, int totalSteps)
    {
        float t = (float)currentStep / (totalSteps - 1);
        return Mathf.Lerp(minScrollDuration, maxScrollDuration, Mathf.Max(0, lerpCurve.Evaluate(t)));
    }

    private void OnInterestSelected(byte type)
    {
        //print("selected: " + type);
        if (spinning)
        {
            currentStep++;
            if (currentStep < totalSteps)
            {
                int nextIndex = (type + indexStep) % totalCellCount;
                circleScroll.SelectCell(nextIndex, GetScrollDuration(currentStep, totalSteps));
                if (currentStep == totalSteps - 1)  // last step
                    spinning = false;
            }
        }
        else
        {
            circleScroll.SetAutoSpin(false); // enable back highlight selected and scroll inertia

            selectedType = (HeroInterestType)type;
            HeroInterestJson interestJson = HeroRepo.GetInterestByType(selectedType);
            if (interestJson != null)
            {
                interestNameText.text = interestJson.localizedname;
                if (selectedType == HeroInterestType.Random)
                {
                    SetRandom();
                    SetItemIcon(hero.HeroJson.randomitemid);
                    if (waitForResetComplete)
                        EnableCicleScroll(true);
                }
                else
                {
                    bool canAssign = selectedType != hero.Interest && HeroRepo.IsInterestInGroup(hero.HeroJson.interestgroup, selectedType);
                    SetAssign(canAssign, selectedType == hero.Interest ? interestJson.localizedname : "");
                    if (!waitForSpinConfirm)
                        SetItemIcon(interestJson.assigneditemid);
                }
            }
            else
            {
                interestNameText.text = "No data";
                SetAssign(false);
            }
        }
    }

    private void SetRandom()
    {
        randomBtn.gameObject.SetActive(true);
        randomBtn.interactable = hero.CanChangeInterest();
        confirmBtn.gameObject.SetActive(false);
        resetBtn.SetActive(false);
        descriptionText.color = Color.white;
        descriptionText.text = GUILocalizationRepo.GetLocalizedString("hro_selectInterestToAssign");
        descriptionText.gameObject.SetActive(true);
    }

    private void SetAssign(bool interactable, string interestName = "")
    {
        randomBtn.gameObject.SetActive(false);
        confirmBtn.gameObject.SetActive(true);
        confirmBtn.interactable = interactable;
        resetBtn.SetActive(true);
        string message = "";
        if (!string.IsNullOrEmpty(interestName))  // not empty means selected same as current type
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("type", interestName);
            message = GUILocalizationRepo.GetLocalizedString("hro_sameAsCurrentInterest", param);
        }
        else
            message = interactable ? GUILocalizationRepo.GetLocalizedString("hro_confirmChangeInterest") : GUILocalizationRepo.GetLocalizedString("hro_interestNotApplicable");
        descriptionText.color = interactable ? Color.white : colorRed;
        descriptionText.text = message;
        descriptionText.gameObject.SetActive(true);
    }

    private void SetItemIcon(string itemIdStr)
    {
        PlayerGhost player = GameInfo.gLocalPlayer;
        if (player == null)
            return;

        hasEnoughItem = false;
        string[] itemids = itemIdStr.Split(';');
        if (itemids.Length > 0 && int.TryParse(itemids[0], out bindItemId))
        {
            item.InitWithToolTipView(bindItemId, 1);
            itemNameText.text = item.inventoryItem.JsonObject.localizedname;
            hasEnoughItem = player.clientItemInvCtrl.itemInvData.HasItem((ushort)bindItemId, 1);
            if (!hasEnoughItem && itemids.Length > 1 && int.TryParse(itemids[1], out unbindItemId))
            {
                hasEnoughItem = player.clientItemInvCtrl.itemInvData.HasItem((ushort)unbindItemId, 1);
                showSpendConfirmation = hasEnoughItem;
            }
            itemNameText.color = hasEnoughItem ? Color.white : colorRed;
        }
    }

    private void ShowUseUnbindItemConfirmation(Action confirmCallback)
    {
        IInventoryItem bindItem = item.inventoryItem;
        IInventoryItem unbindItem = GameRepo.ItemFactory.GetInventoryItem(unbindItemId);
        if (bindItem != null && unbindItem != null)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("bind", bindItem.JsonObject.localizedname);
            param.Add("unbind", unbindItem.JsonObject.localizedname);
            string message = GUILocalizationRepo.GetLocalizedString("hro_confirmUseUnbindToGift", param);
            UIManager.OpenYesNoDialog(message, confirmCallback);
        }
    }

    public void OnClickRandomSpin()
    {
        if (hasEnoughItem)
        {
            if (showSpendConfirmation)
                ShowUseUnbindItemConfirmation(OnConfirmRandomSpin);
            else
                OnConfirmRandomSpin();
        }
        else
            UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("sys_hero_GiftNotEnough"));
    }

    private void OnConfirmRandomSpin()
    {
        descriptionText.gameObject.SetActive(false);
        randomBtn.interactable = false;
        EnableCicleScroll(false);
        RPCFactory.CombatRPC.ChangeHeroInterest(hero.HeroId, 0);
    }

    public void OnClickConfirm()
    {
        if (hasEnoughItem)
        {
            EnableCicleScroll(false);
            if (waitForSpinConfirm)  // confirm on a random spin result
                RPCFactory.CombatRPC.ChangeHeroInterest(hero.HeroId, 0, true);
            else  // confirm on an assigned change
            {
                if (showSpendConfirmation)
                    ShowUseUnbindItemConfirmation(OnConfirmAssign);
                else
                    OnConfirmAssign();
            }
        }
        else
            UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("sys_hero_GiftNotEnough"));
    }

    private void OnConfirmAssign()
    {
        RPCFactory.CombatRPC.ChangeHeroInterest(hero.HeroId, (byte)selectedType);
    }

    public void OnClickReset()
    {
        EnableCicleScroll(false);
        ResetCircleScroll();
    }

    private void ResetCircleScroll()
    {
        randomBtn.interactable = true;
        waitForSpinConfirm = false;
        circleScroll.SelectCell(0, scrollToDuration);
    }

    public void OnInterestRandomSpinResult(byte newType)
    {
        waitForSpinConfirm = true;
        spinResult = newType;
        currentStep = 0;
        totalSteps = (totalCellCount * roundsToSpin) + spinResult;
        spinning = true;
        circleScroll.SetAutoSpin(true);
        circleScroll.SelectCell(indexStep, GetScrollDuration(currentStep, totalSteps));
    }

    private void EnableCicleScroll(bool value)
    {
        waitForResetComplete = !value;
        circleScroll.EnableScrollView(value); // prevent user drag scrollview
    }
}