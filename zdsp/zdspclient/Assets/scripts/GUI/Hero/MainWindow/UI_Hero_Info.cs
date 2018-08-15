using Kopio.JsonContracts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
    [SerializeField] UI_Hero_LockedPanel lockedPanel;

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
    private Dictionary<int, int> currentHeroTier = new Dictionary<int, int>();

    public void Setup()
    {
        timelineController.OnBegin += new UITimelineController.TimelineDelegate(() => HideObjectsForTimeline(true));
        timelineController.OnEnd += new UITimelineController.TimelineDelegate(() => HideObjectsForTimeline(false));
    }

    private void HideObjectsForTimeline(bool hide)
    {
        clickBlocker.SetActive(hide);
        for (int i = 0; i < objectsToHide.Length; i++)
            objectsToHide[i].SetActive(!hide);
        UIManager.HideOpenedDialogs(hide);
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
            HeroCellDto cell = new HeroCellDto(i + 1, "Cell " + (i + 1), false);
            list.Add(cell);
        }

        return list;
    }

    private void Awake()
    {
        List<HeroCellDto> heroList = CreateHeroesData();
        circleScroll.SetUp(heroList, OnHeroSelected);
    }

    // called after Awake and OnEnable
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

    private void OnHeroSelected(int heroId)
    {
        //print("selected heroid " + heroId);
        selectedHeroId = heroId;
        selectedHeroData = HeroRepo.GetHeroById(heroId);

        if (selectedHeroData == null)
        {
            heroNameText.text = "Placeholder " + heroId;
            modelAvatar.Cleanup();
            SetLocked();
            lockedPanel.Init(0, 0, null);
            SetSkillIcons(null);
        }
        else
        {
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
                lockedPanel.Init(selectedHeroData.unlockitemid, selectedHeroData.unlockitemcount, OnClickUnlockHero);
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
        lockedPanel.Show(true);
        summonBtn.gameObject.SetActive(false);
        unsummonBtn.SetActive(false);
    }

    private void SetUnlocked(bool showUnsummonBtn)
    {
        unlockedHeroObj.SetActive(true);
        lockedPanel.Show(false);
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
        timelineController.Play(hero.HeroJson.unlockshow);
        circleScroll.UpdateCell(hero.HeroId, true);

        if (selectedHeroId == hero.HeroId)
        {
            selectedHero = hero;
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
            if (dialog != null && dialog.activeInHierarchy)
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

        UIManager.OpenDialog(WindowType.DialogHeroBonds,
            (window) => window.GetComponent<UI_Hero_BondsDialog>().Init(selectedHeroId));
    }

    public void CleanUp()
    {
        currentHeroTier.Clear();
        circleScroll.ResetSelectedIndex();
    }
}