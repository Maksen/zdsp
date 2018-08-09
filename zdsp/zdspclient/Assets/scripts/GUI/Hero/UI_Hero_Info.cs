using Kopio.JsonContracts;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Client.Entities;
using Zealot.Common;
using Zealot.Repository;

public class UI_Hero_Info : MonoBehaviour
{
    [Header("Common")]
    [SerializeField] Text heroNameText;
    [SerializeField] Model_3DAvatar modelAvatar;
    [SerializeField] Image modelImage;
    [SerializeField] Material grayscaleMat;
    [SerializeField] HeroCircleScroll circleScroll;
    [SerializeField] Hero_SkillIconData[] skillIcons;

    [Header("Locked")]
    [SerializeField] GameObject lockedHeroObj;
    [SerializeField] Transform unlockItemTranform;
    [SerializeField] GameObject unlockItemPrefab;
    [SerializeField] Text unlockItemCountText;

    [Header("Unlocked")]
    [SerializeField] GameObject unlockedHeroObj;
    [SerializeField] Text skillPointsText;
    [SerializeField] Text trustLevelText;
    [SerializeField] Image interestIconImage;
    [SerializeField] Text interestDescText;
    [SerializeField] Button summonBtn;
    [SerializeField] GameObject unsummonBtn;

    [Header("Timeline")]
    [SerializeField] UITimelineController timelineController;
    [SerializeField] GameObject[] objectsToHide;
    [SerializeField] GameObject clickBlocker;

    private HeroStatsClient heroStats;
    private int selectedHeroId = -1;
    private HeroJson selectedHeroData;
    private Hero selectedHero;
    private GameIcon_MaterialConsumable unlockItem;
    private Dictionary<int, int> currentHeroTier = new Dictionary<int, int>();

    public void Setup()
    {
        timelineController.OnBegin += new UITimelineController.TimelineDelegate(() => ShowUICameras(false));
        timelineController.OnEnd += new UITimelineController.TimelineDelegate(() => ShowUICameras(true));
    }

    private void ShowUICameras(bool value)
    {
        clickBlocker.SetActive(!value);
        for (int i = 0; i < objectsToHide.Length; i++)
            objectsToHide[i].SetActive(value);
    }

    private List<HeroCellDto> CreateHeroesData()
    {
        List<HeroCellDto> list = new List<HeroCellDto>();
        int index = 0;

        foreach (int heroid in HeroRepo.heroes.Keys)
        {
            HeroCellDto cell = new HeroCellDto(heroid, "Hero " + heroid, GameInfo.gLocalPlayer.HeroStats.IsHeroUnlocked(heroid));
            list.Add(cell);
            index++;
        }

        for (int i = index; i < 13; i++)
        {
            HeroCellDto cell = new HeroCellDto(i + 1, "Fake " + (i + 1), false);
            list.Add(cell);
        }

        return list;
    }

    private void Awake()
    {
        List<HeroCellDto> heroList = CreateHeroesData();
        circleScroll.SetUp(heroList, OnHeroSelected);
    }

    public void Init()
    {
        heroStats = GameInfo.gLocalPlayer.HeroStats;
        circleScroll.SelectHero(GetFirstHeroToSelect());
    }

    private int GetFirstHeroToSelect()
    {
        if (heroStats.SummonedHeroId > 0)
            return heroStats.SummonedHeroId;
        else
        {
            foreach (int heroid in HeroRepo.heroes.Keys)
            {
                if (heroStats.IsHeroUnlocked(heroid))
                    return heroid;
            }
            return -1;
        }
    }

    private void InitUnlockItem(int itemId, int itemCount)
    {
        if (unlockItem == null)
        {
            ClientUtils.DestroyChildren(unlockItemTranform);
            GameObject icon = ClientUtils.CreateChild(unlockItemTranform, unlockItemPrefab);
            unlockItem = icon.GetComponent<GameIcon_MaterialConsumable>();
        }

        unlockItem.Init(itemId, 1);
        unlockItemCountText.text = "x" + itemCount;
    }

    private void OnHeroSelected(int heroId)
    {
        //print("selected heroid " + heroId);
        selectedHeroId = heroId;
        selectedHeroData = HeroRepo.GetHeroById(heroId);

        if (selectedHeroData == null)
        {
            heroNameText.text = "Fake " + heroId;
            modelAvatar.transform.GetChild(0).GetChild(0).gameObject.SetActive(true);
            modelAvatar.Cleanup();
            SetLocked();
            InitUnlockItem(0, 0);
            SetSkillIcons(null);
        }
        else
        {
            modelAvatar.transform.GetChild(0).GetChild(0).gameObject.SetActive(false);  // to be removed
            heroNameText.text = selectedHeroData.localizedname;

            selectedHero = heroStats.GetHero(heroId);
            if (selectedHero != null)  // unlocked
            {
                SetUnlocked(heroStats.IsHeroSummoned(heroId));
                skillPointsText.text = selectedHero.SkillPoints.ToString();
                trustLevelText.text = selectedHero.TrustLevel.ToString();
                UpdateInterest(selectedHero.Interest);
            }
            else // locked
            {
                selectedHero = new Hero(heroId, selectedHeroData);
                SetLocked();
                InitUnlockItem(selectedHeroData.unlockitemid, selectedHeroData.unlockitemcount);
            }

            if (!currentHeroTier.ContainsKey(heroId))
                UpdateModelTier(selectedHero.ModelTier);
            else
                UpdateModelTier(currentHeroTier[heroId]);  // show the temp tier instead of the actual one

            SetSkillIcons(selectedHero);
        }
    }

    private void SetLocked()
    {
        unlockedHeroObj.SetActive(false);
        lockedHeroObj.SetActive(true);
        summonBtn.gameObject.SetActive(false);
        unsummonBtn.SetActive(false);
    }

    private void SetUnlocked(bool showUnsummonBtn)
    {
        unlockedHeroObj.SetActive(true);
        lockedHeroObj.SetActive(false);
        summonBtn.gameObject.SetActive(!showUnsummonBtn);
        unsummonBtn.SetActive(showUnsummonBtn);
    }

    private void SetSkillIcons(Hero hero)
    {
        for (int i = 0; i < skillIcons.Length; i++)
        {
            skillIcons[i].Init(hero, i + 1);
        }
    }

    public void OnClickUnlockHero()
    {
        RPCFactory.CombatRPC.UnlockHero(selectedHeroId);
    }

    public void OnSummonedHeroChanged()
    {
        if (heroStats == null)
            return;

        unsummonBtn.SetActive(selectedHeroId == heroStats.SummonedHeroId);
        summonBtn.gameObject.SetActive(selectedHeroId != heroStats.SummonedHeroId);
    }

    public void OnClickSummonHero()
    {
        RPCFactory.CombatRPC.SummonHero(selectedHeroId, currentHeroTier[selectedHeroId]);
        UIManager.CloseWindow(WindowType.Hero);
    }

    public void OnClickUnsummonHero()
    {
        RPCFactory.CombatRPC.SummonHero(0);
    }

    public void OnHeroAdded(Hero hero)
    {
        if (selectedHeroId == hero.HeroId)
        {
            selectedHero = hero;
            timelineController.Play(hero.HeroJson.unlockshow);
            circleScroll.UpdateCell(hero.HeroId, true);
            SetUnlocked(false);
            SetSkillIcons(hero);
            skillPointsText.text = hero.SkillPoints.ToString();
            trustLevelText.text = hero.TrustLevel.ToString();
            UpdateInterest(hero.Interest);
            UpdateModelTier(hero.ModelTier);
        }
    }

    public void OnHeroUpdated(Hero oldHero, Hero newHero)
    {
        if (selectedHeroId == newHero.HeroId)
        {
            selectedHero = newHero;
            SetSkillIcons(newHero);
            skillPointsText.text = newHero.SkillPoints.ToString();
            trustLevelText.text = newHero.TrustLevel.ToString();

            GameObject dialog = UIManager.GetWindowGameObject(WindowType.DialogHeroSkillPoints);
            if (dialog != null && dialog.activeInHierarchy)
            {
                dialog.GetComponent<UI_Hero_AddSkillPointsDialog>().Init(newHero);
                CheckModelTierUnlocked(currentHeroTier[selectedHeroId]);
            }

            dialog = UIManager.GetWindowGameObject(WindowType.DialogHeroTrust);
            if (dialog != null && dialog.activeInHierarchy)
                dialog.GetComponent<UI_Hero_AddTrustDialog>().UpdateList(newHero);

            if (oldHero.Interest != newHero.Interest)
                UpdateInterest(newHero.Interest);

            dialog = UIManager.GetWindowGameObject(WindowType.DialogHeroStats);
            if (dialog != null && dialog.activeInHierarchy)
                dialog.GetComponent<UI_Hero_StatsDialog>().Init(newHero);

            dialog = UIManager.GetWindowGameObject(WindowType.DialogHeroSkillDetails);
            if (dialog != null && dialog.activeInHierarchy)
                dialog.GetComponent<UI_Hero_SkillDetailsDialog>().Refresh(newHero);
        }
    }

    private void UpdateModelTier(int tier)
    {
        currentHeroTier[selectedHeroId] = tier;
        modelAvatar.ChangeHero(selectedHeroId, tier);
        string imagePath = "";
        switch (tier)
        {
            case 1: imagePath = selectedHeroData.t1imagepath; break;
            case 2: imagePath = selectedHeroData.t2imagepath; break;
            case 3: imagePath = selectedHeroData.t3imagepath; break;
            default:
                HeroItem skinItem = GameRepo.ItemFactory.GetInventoryItem(tier) as HeroItem;
                if (skinItem != null)
                    imagePath = skinItem.HeroItemJson.heroimagepath;
                break;
        }
        ClientUtils.LoadIconAsync(imagePath, OnModelImageLoaded);
        CheckModelTierUnlocked(tier);
    }

    private void CheckModelTierUnlocked(int currentTier)
    {
        bool unlocked = selectedHero.IsModelTierUnlocked(currentTier);
        modelImage.material = selectedHero.SlotIdx == -1 || !unlocked ? grayscaleMat : null;
        summonBtn.interactable = unlocked;
        if (heroStats.IsHeroSummoned(selectedHeroId))
        {
            unsummonBtn.SetActive(selectedHero.ModelTier == currentTier);
            summonBtn.gameObject.SetActive(selectedHero.ModelTier != currentTier);
        }
    }

    private void OnModelImageLoaded(Sprite sprite)
    {
        if (sprite != null)
            modelImage.sprite = sprite;
    }

    private void UpdateInterest(HeroInterestType type)
    {
        HeroInterestJson interestJson = HeroRepo.GetInterestByType(type);
        if (interestJson != null)
        {
            ClientUtils.LoadIconAsync(interestJson.iconpath, OnInterestIconLoaded);
            interestDescText.text = interestJson.description;
        }
    }

    private void OnInterestIconLoaded(Sprite sprite)
    {
        if (sprite != null)
        {
            interestIconImage.sprite = sprite;

            GameObject dialog = UIManager.GetWindowGameObject(WindowType.DialogHeroInterest);
            if (dialog != null &&  dialog.activeInHierarchy)
                dialog.GetComponent<UI_Hero_InterestDialog>().UpdateInterest(selectedHero, sprite, interestDescText.text);
        }
    }

    public void OnInterestRandomSpinResult(byte interest)
    {
        GameObject dialog = UIManager.GetWindowGameObject(WindowType.DialogHeroInterest);
        if (dialog != null && dialog.activeInHierarchy)
            dialog.GetComponent<UI_Hero_InterestDialog>().OnInterestRandomSpinResult(interest);
    }

    public void OnClickResetSkillPoints()
    {
        Dictionary<string, string> param = new Dictionary<string, string>();
        param.Add("cost", selectedHeroData.resetskillmoney.ToString());
        //string message = GUILocalizationRepo.GetLocalizedString("hero_confirmResetSkillPoints", param);
        string message = "Confirm spend " + param["cost"] + " to reset skill points?";
        UIManager.OpenYesNoDialog(message, () => RPCFactory.CombatRPC.ResetHeroSkillPoints(selectedHeroId));
    }

    public void OnClickAddSkillPoints()
    {
        UIManager.OpenDialog(WindowType.DialogHeroSkillPoints, 
            (window) => window.GetComponent<UI_Hero_AddSkillPointsDialog>().Init(selectedHero));
    }

    public void OnClickAddTrust()
    {
        UIManager.OpenDialog(WindowType.DialogHeroTrust,
            (window) => window.GetComponent<UI_Hero_AddTrustDialog>().Init(selectedHero));
    }

    public void OnClickChangeInterest()
    {
        UIManager.OpenDialog(WindowType.DialogHeroInterest,
            (window) => window.GetComponent<UI_Hero_InterestDialog>().Init(selectedHero, interestIconImage.sprite, interestDescText.text));
    }

    public void OnClickChangeModelTier()
    {
        if (selectedHeroData == null)
            return;

        UIManager.OpenDialog(WindowType.DialogHeroTier,
            (window) => window.GetComponent<UI_Hero_ModelTierDialog>().Init(selectedHero, currentHeroTier[selectedHeroId],
            modelImage.sprite, UpdateModelTier));
    }

    public void OnClickStats()
    {
        if (selectedHeroData == null)
            return;

        UIManager.OpenDialog(WindowType.DialogHeroStats,
            (window) => window.GetComponent<UI_Hero_StatsDialog>().Init(selectedHero));
    }

    public void OnClickBonds()
    {
        if (selectedHeroData == null)
            return;

        print("hero bonds");
    }

    public void CleanUp()
    {
        currentHeroTier.Clear();
        circleScroll.ResetSelectedIndex();
    }
}