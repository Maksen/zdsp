using Kopio.JsonContracts;
using UnityEngine;
using UnityEngine.UI;
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
    [SerializeField] GameObject randomBtn;
    [SerializeField] Button confirmBtn;
    [SerializeField] GameObject resetBtn;

    private bool initialized;
    private HeroInterestType selectedType;
    private Hero hero;
    private GameIcon_MaterialConsumable item;
    private bool waitForResetComplete;
    private bool waitForSpinConfirm;
    private bool hasEnoughItem;

    public override void OnCloseWindow()
    {
        base.OnCloseWindow();
        EnableCicleScroll(true);
        waitForSpinConfirm = false;
        circleScroll.ResetSelection();
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
            initialized = true;
        }
    }

    public void UpdateInterest(Hero hero, Sprite currentSprite, string currentDesc)
    {
        this.hero = hero;
        currentInterestImage.sprite = currentSprite;
        currentInterestDescText.text = currentDesc;
        ResetCircleScroll();
    }

    private void OnInterestSelected(byte type)
    {
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

    private void SetRandom()
    {
        randomBtn.SetActive(true);
        confirmBtn.gameObject.SetActive(false);
        resetBtn.SetActive(false);
        descriptionText.gameObject.SetActive(false);
    }

    private void SetAssign(bool interactable)
    {
        randomBtn.SetActive(false);
        confirmBtn.gameObject.SetActive(true);
        confirmBtn.interactable = interactable;
        resetBtn.SetActive(true);
        string message = interactable ? "hero_confirmChangeInterest" : "hero_interestNotApplicable";
        descriptionText.text = GUILocalizationRepo.GetLocalizedString(message);
        descriptionText.gameObject.SetActive(true);
    }

    private void SetItemIcon(string itemIdStr)
    {
        hasEnoughItem = false;
        int bindItemId, unbindItemId;
        string[] itemids = itemIdStr.Split(';');
        if (itemids.Length > 0 && int.TryParse(itemids[0], out bindItemId))
        {
            item.Init(bindItemId);
            itemNameText.text = item.inventoryItem.JsonObject.localizedname;
            hasEnoughItem = GameInfo.gLocalPlayer.clientItemInvCtrl.itemInvData.HasItem((ushort)bindItemId, 1);
            if (!hasEnoughItem && itemids.Length > 1 && int.TryParse(itemids[1], out unbindItemId))
                hasEnoughItem = GameInfo.gLocalPlayer.clientItemInvCtrl.itemInvData.HasItem((ushort)unbindItemId, 1);
        }
    }

    public void OnClickRandomSpin()
    {
        if (hasEnoughItem)
        {
            EnableCicleScroll(false);
            RPCFactory.CombatRPC.ChangeHeroInterest(hero.HeroId, 0);
        }
        else
            UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("sys_hero_GiftNotEnough"));
    }

    public void OnClickConfirm()
    {
        if (hasEnoughItem)
        {
            EnableCicleScroll(false);
            if (waitForSpinConfirm)  // confirm on a random spin result
                RPCFactory.CombatRPC.ChangeHeroInterest(hero.HeroId, 0, true);
            else  // confirm on an assigned change
                RPCFactory.CombatRPC.ChangeHeroInterest(hero.HeroId, (byte)selectedType);
        }
        else
            UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("sys_hero_GiftNotEnough"));
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
        circleScroll.SelectCell(newType);
    }

    private void EnableCicleScroll(bool value)
    {
        waitForResetComplete = !value;
        circleScroll.EnableScrollView(value); // prevent user drag scrollview
    }
}