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
    [SerializeField] InterestCircleScroll circleScroll;
    [SerializeField] Button randomBtn;
    [SerializeField] Button confirmBtn;
    [SerializeField] GameObject resetBtn;

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
    private int halfIndex;
    private byte spinResult;

    private void Awake()
    {
        colorRed = descriptionText.color;
        halfIndex = Mathf.FloorToInt((float)HeroInterestType.TotalNum / 2);
    }

    public override void OnCloseWindow()
    {
        base.OnCloseWindow();
        spinning = false;
        EnableCicleScroll(true);
        waitForSpinConfirm = false;
        circleScroll.SetSelection(0);
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
            circleScroll.SetSelection(0);
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

    bool IsWithinNextSpinRange(int currentIndex, int nextIndex, int desiredIndex)
    {
        if (currentIndex < nextIndex)
            return currentIndex < desiredIndex && desiredIndex <= nextIndex;
        else
            return currentIndex < desiredIndex || desiredIndex <= nextIndex;
    }

    private void OnInterestSelected(byte type)
    {
        print("selected: " + type);

        if (spinning)
        {
            count++;
            if (count < 4)
                circleScroll.SelectCell((type + halfIndex) % (int)HeroInterestType.TotalNum);
            else
            {
                int nextIndex = (type + halfIndex) % (int)HeroInterestType.TotalNum;
                if (IsWithinNextSpinRange(type, nextIndex, spinResult))
                {
                    spinning = false;
                    //circleScroll.SetAutoSpin(false);
                    circleScroll.SelectCell(spinResult);
                    circleScroll.HighlightSelected(true);

                }
                else
                {
                    circleScroll.SelectCell(nextIndex);
                }

                //if (count == 4 && type != 0)
                //    circleScroll.SelectCell(0);
                //else
                //{
                //    int nextIndex = (type + halfIndex) % (int)HeroInterestType.TotalNum;
                //    if (spinResult <= nextIndex)
                //    {
                //        spinning = false;
                //        //circleScroll.HighlightSelected(true);
                //        circleScroll.SetAutoSpin(false);
                //        circleScroll.SelectCell(spinResult);
                //    }
                //    else
                //    {
                //        circleScroll.SelectCell(nextIndex);
                //    }
                //}
            }
        }
        else
        {
            circleScroll.SetAutoSpin(false);

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
                    SetAssign(canAssign);
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
        descriptionText.gameObject.SetActive(false);
    }

    private void SetAssign(bool interactable)
    {
        randomBtn.gameObject.SetActive(false);
        confirmBtn.gameObject.SetActive(true);
        confirmBtn.interactable = interactable;
        resetBtn.SetActive(true);
        string message = interactable ? "hro_confirmChangeInterest" : "hro_interestNotApplicable";
        descriptionText.text = GUILocalizationRepo.GetLocalizedString(message);
        descriptionText.color = interactable ? Color.white : colorRed;
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

    //public int index;
    bool spinning;
    int count = 0;
    public void OnClickRandomSpin()
    {
        //count = 0;
        //spinning = true;
        //circleScroll.HighlightSelected(false);

        //circleScroll.SelectCell(halfIndex);

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
        waitForSpinConfirm = false;
        circleScroll.SelectCell(0);
    }

    public void OnInterestRandomSpinResult(byte newType)
    {
        waitForSpinConfirm = true;
        //circleScroll.SelectCell(newType);
        spinResult = newType;
        count = 0;
        spinning = true;
        circleScroll.HighlightSelected(false);
        circleScroll.SetAutoSpin(true);
        //EnableCicleScroll(false);
        circleScroll.SelectCell(halfIndex);
    }

    private void EnableCicleScroll(bool value)
    {
        waitForResetComplete = !value;
        circleScroll.EnableScrollView(value); // prevent user drag scrollview
    }
}