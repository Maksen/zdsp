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
    private bool waitForSpinConfirm;

    public void OnEnable()
    {
        Hero hero1 = new Hero(1, HeroInterestType.Metallurgy, 1, HeroRepo.GetHeroById(1));
        Init(hero1, null, "current interest desc");
    }

    public void Init(Hero hero, Sprite currentSprite, string currentDesc)
    {
        this.hero = hero;
        currentInterestImage.sprite = currentSprite;
        currentInterestDescText.text = currentDesc;

        if (item == null)
        {
            ClientUtils.DestroyChildren(itemIconTransform);
            GameObject icon = ClientUtils.CreateChild(itemIconTransform, itemIconPrefab);
            item = icon.GetComponent<GameIcon_MaterialConsumable>();
        }

        if (!initialized)
        {
            circleScroll.SetUp(OnInterestSelected);
            initialized = true;
        }
        else
        {
            ResetCircleScroll();
        }
    }

    private void OnInterestSelected(byte type)
    {
        selectedType = (HeroInterestType)type;
        print("selected type: " + selectedType);
        HeroInterestJson interestJson = HeroRepo.GetInterestByType(selectedType);
        if (interestJson != null)
        {
            interestNameText.text = interestJson.localizedname;
            if (selectedType == HeroInterestType.Random)
            {
                SetRandom();
                SetItemIcon(hero.HeroJson.randomitemid);
            }
            else
            {
                SetAssign(HeroRepo.IsInterestInGroup(hero.HeroJson.interestgroup, selectedType));
                if (waitForSpinConfirm)
                    circleScroll.EnableScrollView(false);
                else
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
        int id;
        string[] itemids = itemIdStr.Split(';');
        if (itemids.Length > 0 && int.TryParse(itemids[0], out id))
        {
            item.Init(id);
            itemNameText.text = item.inventoryItem.JsonObject.localizedname;
        }
    }

    public void OnClickRandomSpin()
    {
        int randIndex = GameUtils.RandomInt(1, 12);
        OnInterestRandomSpinResult((byte)randIndex);
        //RPCFactory.CombatRPC.ChangeHeroInterest(hero.HeroId, 0);
    }

    public void OnClickConfirm()
    {
        ResetCircleScroll();

        //if (waitForSpinConfirm)  // confirm on a random spin result
        //    RPCFactory.CombatRPC.ChangeHeroInterest(hero.HeroId, 0, true);
        //else  // confirm on an assigned change
        //    RPCFactory.CombatRPC.ChangeHeroInterest(hero.HeroId, (byte)selectedType);
    }

    public void OnClickReset()
    {
        ResetCircleScroll();
    }

    private void ResetCircleScroll()
    {
        waitForSpinConfirm = false;
        circleScroll.EnableScrollView(true);
        circleScroll.SelectCell(0);
    }

    public void OnInterestRandomSpinResult(byte newType)
    {
        waitForSpinConfirm = true;
        circleScroll.SelectCell(newType);
    }
}
