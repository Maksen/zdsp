using Kopio.JsonContracts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using Zealot.Common.Entities;
using Zealot.Repository;

public class UI_Hero_BondsDialog : BaseWindowBehaviour
{
    [Header("Left Side")]
    [SerializeField] GameObject bondDataPrefab;
    [SerializeField] HorizontalScrollSnap horizontalScroll;
    [SerializeField] UI_ScrollRectOcclusion scrollRectOcclusion;
    [SerializeField] PaginationManager pageManager;
    [SerializeField] Scroll_TxtCurrentPage currentPage;

    [Header("Right Side")]
    [SerializeField] UI_Hero_TotalBuffPanel totalBuffPanel;

    [Header("Sub Panels")]
    [SerializeField] UI_Hero_LockedPanel lockedPanel;
    [SerializeField] UI_Hero_UnlockedPanel unlockedPanel;

    private List<Hero_BondData> bondList = new List<Hero_BondData>();

    private void Start()
    {
        horizontalScroll.OnSelectionChangeStartEvent.AddListener(ToggleHeroSelectionOff);
    }

    public void Init(int heroId, int jumpToBondGrpId = 0)
    {
        int startingPageIndex = 0;
        List<HeroBond> involvedBonds = HeroRepo.GetInvolvedBondsByHeroId(heroId);
        for (int i = 0; i < involvedBonds.Count; i++)
        {
            GameObject obj = Instantiate(bondDataPrefab);
            Hero_BondData bondData = obj.GetComponent<Hero_BondData>();
            bondData.Init(involvedBonds[i], this);
            horizontalScroll.AddChild(obj);
            bondList.Add(bondData);
            pageManager.transform.GetChild(i).gameObject.SetActive(true);
            if (involvedBonds[i].heroBondGroupJson.id == jumpToBondGrpId)
                startingPageIndex = i;
        }

        scrollRectOcclusion.Init();
        if (involvedBonds.Count > 0)
            currentPage.Init();
        else
            currentPage.gameObject.SetActive(false); // disable in case no data
        totalBuffPanel.Init();

        if (startingPageIndex > 0)
            StartCoroutine(GoToPage(startingPageIndex));
    }

    private IEnumerator GoToPage(int index)
    {
        yield return null;
        horizontalScroll.GoToScreen(index);
    }

    public void Refresh(Hero hero, bool isNewlyAdded)
    {
        for (int i = 0; i < bondList.Count; i++)
            bondList[i].Refresh();

        if (isNewlyAdded)
            ToggleHeroSelectionOff();
        else
            unlockedPanel.UpdatePanel(hero);

        totalBuffPanel.Init();
    }

    public void HideSubPanels()
    {
        lockedPanel.Show(false);
        unlockedPanel.Show(false);
    }

    private void ToggleHeroSelectionOff()
    {
        int index = horizontalScroll.CurrentPage;
        if (index >= 0 && index < bondList.Count)
            bondList[index].ToggleHeroSelectionOff();
    }

    public void ShowLockedPanel(int heroId)
    {
        lockedPanel.Show(true);
        unlockedPanel.Show(false);

        HeroJson heroJson = HeroRepo.GetHeroById(heroId);
        if (heroJson != null)
            lockedPanel.Init(heroJson.localizedname, heroJson.unlockitemid, heroJson.unlockitemcount, () => RPCFactory.CombatRPC.UnlockHero(heroId));
    }

    public void ShowSkillPointsPanel(Hero hero, int reqPts)
    {
        lockedPanel.Show(false);
        unlockedPanel.ShowSkillPointsPanel(hero, reqPts);
    }

    public void ShowLevelUpPanel(Hero hero, int reqLvl)
    {
        lockedPanel.Show(false);
        unlockedPanel.ShowLevelUpPanel(hero, reqLvl);
    }

    public void ShowFullPanel(Hero hero, int reqPts, int reqLvl)
    {
        lockedPanel.Show(false);
        unlockedPanel.ShowFullPanel(hero, reqPts, reqLvl);
    }

    public override void OnCloseWindow()
    {
        base.OnCloseWindow();
        CleanUp();
    }

    public void CleanUp()
    {
        pageManager.OffAllToggles();
        scrollRectOcclusion.CleanUp();
        bondList.Clear();
        GameObject[] children;
        horizontalScroll.RemoveAllChildren(out children);
        for (int i = 0; i < children.Length; i++)
            Destroy(children[i]);
        HideSubPanels();
        totalBuffPanel.Clear();
    }
}