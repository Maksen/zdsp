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

    [Header("Locked")]
    [SerializeField] GameObject lockedHeroObj;
    [SerializeField] Transform unlockItemTranform;
    [SerializeField] GameObject unlockItemPrefab;
    [SerializeField] Text unlockItemCountText;

    [Header("Unlocked")]
    [SerializeField] GameObject unlockedHeroObj;
    [SerializeField] Text skillPointsText;
    [SerializeField] Hero_SkillIconData[] skillIcons;
    [SerializeField] Text trustLevelText;
    [SerializeField] Image interestIconImage;
    [SerializeField] Text interestDescText;
    [SerializeField] GameObject summonBtn;
    [SerializeField] GameObject unsummonBtn;

    [Header("Timeline")]
    [SerializeField] UITimelineController timelineController;
    [SerializeField] GameObject[] uiCameras;

    private PlayerGhost player;
    private bool scrollSetup;
    private int selectedHeroId = -1;
    private HeroJson selectedHeroData;
    private Hero selectedHero;
    private GameIcon_MaterialConsumable unlockItem;

    public void Setup()
    {
        timelineController.OnBegin += new UITimelineController.TimelineDelegate(() => ShowUICameras(false));
        timelineController.OnEnd += new UITimelineController.TimelineDelegate(() => ShowUICameras(true));
    }

    private void ShowUICameras(bool value)
    {
        for (int i = 0; i < uiCameras.Length; i++)
            uiCameras[i].SetActive(value);
    }

    private List<HeroCellDto> CreateHeroesData()
    {
        List<HeroCellDto> list = new List<HeroCellDto>();
        int index = 0;

        foreach (int heroid in HeroRepo.heroes.Keys)
        {
            HeroCellDto cell = new HeroCellDto(heroid, "Hero " + heroid, player.HeroStats.IsHeroUnlocked(heroid));
            list.Add(cell);
            index++;
        }

        for (int i = index; i < 20; i++)
        {
            HeroCellDto cell = new HeroCellDto(i + 1, "Fake " + (i + 1), false);
            list.Add(cell);
        }

        return list;
    }

    public void Init()
    {
        player = GameInfo.gLocalPlayer;

        if (!scrollSetup)
        {
            List<HeroCellDto> heroList = CreateHeroesData();
            circleScroll.SetUp(heroList, OnHeroSelected);
            scrollSetup = true;
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

        unlockItem.Init(itemId, 0);
        unlockItemCountText.text = "x" + itemCount;
    }

    private void OnHeroSelected(int heroId)
    {
        //print("selected heroid " + selectedHeroId);
        selectedHeroId = heroId;
        selectedHeroData = HeroRepo.GetHeroById(heroId);

        if (selectedHeroData == null)
        {
            heroNameText.text = "Fake " + heroId;
            modelAvatar.transform.GetChild(0).GetChild(0).gameObject.SetActive(true);
            modelAvatar.Cleanup();
            SetLocked();
            InitUnlockItem(0, 0);
        }
        else
        {
            modelAvatar.transform.GetChild(0).GetChild(0).gameObject.SetActive(false);  // to be removed

            heroNameText.text = selectedHeroData.localizedname;

            selectedHero = player.HeroStats.GetHero(heroId);
            if (selectedHero != null)  // unlocked
            {
                SetUnlocked(player.HeroStats.IsHeroSummoned(heroId));
                UpdateModelTier(selectedHero.ModelTier);
                skillPointsText.text = selectedHero.SkillPoints.ToString();
                trustLevelText.text = selectedHero.TrustLevel.ToString();
                UpdateInterest(selectedHero.Interest);
            }
            else // locked
            {
                SetLocked();
                UpdateModelTier(1);
                InitUnlockItem(selectedHeroData.unlockitemid, selectedHeroData.unlockitemcount);
            }
            SetSkillIcons(selectedHero, selectedHeroData);
        }
    }

    private void SetLocked()
    {
        unlockedHeroObj.SetActive(false);
        lockedHeroObj.SetActive(true);
        summonBtn.SetActive(false);
        unsummonBtn.SetActive(false);
    }

    private void SetUnlocked(bool showUnsummonBtn)
    {
        unlockedHeroObj.SetActive(true);
        lockedHeroObj.SetActive(false);
        summonBtn.SetActive(!showUnsummonBtn);
        unsummonBtn.SetActive(showUnsummonBtn);
    }

    private void SetSkillIcons(Hero hero, HeroJson heroData)
    {
        if (hero != null)
        {
            skillIcons[0].Init(1, heroData.skill1grp, hero.Skill1Level);
            skillIcons[1].Init(2, heroData.skill2grp, hero.Skill2Level);
            skillIcons[2].Init(3, heroData.skill3grp, hero.Skill3Level);
        }
        else
        {
            skillIcons[0].Init(1, heroData.skill1grp, 0);
            skillIcons[1].Init(2, heroData.skill2grp, 0);
            skillIcons[2].Init(3, heroData.skill3grp, 0);
        }
    }

    public void OnClickUnlockHero()
    {
        RPCFactory.CombatRPC.UnlockHero(selectedHeroId);
    }

    public void OnSummonedHeroChanged()
    {
        if (selectedHeroId == player.HeroStats.SummonedHeroId)
        {
            summonBtn.SetActive(false);
            unsummonBtn.SetActive(true);
        }
        else
        {
            summonBtn.SetActive(true);
            unsummonBtn.SetActive(false);
        }
    }

    public void OnClickSummonHero()
    {
        RPCFactory.CombatRPC.SummonHero(selectedHeroId);
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
            SetSkillIcons(hero, hero.HeroJson);
            skillPointsText.text = hero.SkillPoints.ToString();
            trustLevelText.text = hero.TrustLevel.ToString();
            modelImage.material = null;
        }
    }

    public void OnHeroUpdated(Hero oldHero, Hero newHero)
    {
        if (selectedHeroId == newHero.HeroId)
        {
            selectedHero = newHero;
            SetSkillIcons(newHero, newHero.HeroJson);
            skillPointsText.text = newHero.SkillPoints.ToString();
            trustLevelText.text = newHero.TrustLevel.ToString();

            GameObject dialog = UIManager.GetWindowGameObject(WindowType.DialogHeroSkillPoints);
            if (dialog.activeInHierarchy)
                dialog.GetComponent<UI_Hero_AddSkillPointsDialog>().Init(newHero);

            if (oldHero.ModelTier != newHero.ModelTier)
                UpdateModelTier(newHero.ModelTier);

            dialog = UIManager.GetWindowGameObject(WindowType.DialogHeroTrust);
            if (dialog.activeInHierarchy)
                dialog.GetComponent<UI_Hero_AddTrustDialog>().UpdateList(newHero);

            if (oldHero.Interest != newHero.Interest)
                UpdateInterest(newHero.Interest);

            dialog = UIManager.GetWindowGameObject(WindowType.DialogHeroStats);
            if (dialog.activeInHierarchy)
                dialog.GetComponent<UI_Hero_StatsDialog>().Init(newHero);
        }
    }

    private void UpdateModelTier(int tier)
    {
        modelAvatar.ChangeHero(selectedHeroId, tier);
        switch (tier)
        {
            case 1: ClientUtils.LoadIconAsync(selectedHeroData.t1imagepath, OnModelImageLoaded); break;
            case 2: ClientUtils.LoadIconAsync(selectedHeroData.t2imagepath, OnModelImageLoaded); break;
            case 3: ClientUtils.LoadIconAsync(selectedHeroData.t3imagepath, OnModelImageLoaded); break;
        }
        modelImage.material = selectedHero == null ? grayscaleMat : null;
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
            interestDescText.text = type.ToString();
        }
    }

    private void OnInterestIconLoaded(Sprite sprite)
    {
        if (sprite != null)
        {
            interestIconImage.sprite = sprite;

            GameObject dialog = UIManager.GetWindowGameObject(WindowType.DialogHeroInterest);
            if (dialog.activeInHierarchy)
                dialog.GetComponent<UI_Hero_InterestDialog>().Init(selectedHero, sprite, interestDescText.text);
        }
    }

    public void OnInterestRandomSpinResult(byte interest)
    {
        GameObject dialog = UIManager.GetWindowGameObject(WindowType.DialogHeroInterest);
        if (dialog.activeInHierarchy)
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
        if (selectedHero != null)
        {
            UIManager.OpenDialog(WindowType.DialogHeroTier,
                (window) => window.GetComponent<UI_Hero_ModelTierDialog>().Init(selectedHero, modelImage.sprite));
        }
        else
            UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("sys_hero_HeroIsLocked"));
    }

    public void OnClickStats()
    {
        UIManager.OpenDialog(WindowType.DialogHeroStats,
            (window) => window.GetComponent<UI_Hero_StatsDialog>().Init(selectedHero));
    }
}