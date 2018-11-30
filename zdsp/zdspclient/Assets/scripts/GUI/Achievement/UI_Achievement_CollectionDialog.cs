using Kopio.JsonContracts;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Bot;
using Zealot.Client.Entities;
using Zealot.Common;
using Zealot.Entities;
using Zealot.Repository;

public class UI_Achievement_CollectionDialog : BaseWindowBehaviour
{
    [SerializeField] Toggle[] toggles;
    [SerializeField] DefaultToggleInGroup tabController;
    [SerializeField] AchievementCollectionScrollView collectScrollView;
    [SerializeField] DragSpin3DAvatar dragSpinAvatar;
    [SerializeField] Model_3DAvatar modelAvatar;

    [Header("Common")]
    [SerializeField] GameObject lockImageObj;
    [SerializeField] Text collectDateText;
    [SerializeField] UI_Achievement_CollectionDescPanel descPanel;

    [Header("Monster")]
    [SerializeField] Image monsterElementImage;
    [SerializeField] GameObject monsterTooltipObj;
    [SerializeField] Text monsterTooltipText;
    [SerializeField] ComboBoxA monsterTypeFilter;

    [Header("Fashion")]
    [SerializeField] ComboBoxA fashionPartsTypeFilter;
    [SerializeField] GameObject fashionStoreButton;
    [SerializeField] GameObject fashionTakeOutButton;

    [Header("NPC")]
    [SerializeField] Text npcLocationText;

    [Header("Relic")]
    [SerializeField] ComboBoxA relicRarityFilter;
    [SerializeField] ComboBoxA relicTypeFilter;
    [SerializeField] GameObject relicStoreButton;
    [SerializeField] GameObject relicTakeOutButton;

    [Header("DNA")]
    [SerializeField] ComboBoxA dnaRarityFilter;
    [SerializeField] ComboBoxA dnaTypeFilter;

    private List<CollectionObjective> objectivesList;
    private List<CollectionInfo> displayList = new List<CollectionInfo>();
    private PlayerGhost player;
    private AchievementStatsClient achStats;
    private StringBuilder sb = new StringBuilder();
    private UI_Achievement uiAchieve;
    private UI_Achievement_CollectionDetails uiColDetails;
    private CollectionObjective selectedObjective;
    private int selectedIdx;

    private void Awake()
    {
        for (int i = 0; i < toggles.Length; ++i)
        {
            CollectionType type = (CollectionType)i;
            toggles[i].onValueChanged.AddListener((isOn) => Init(isOn, type));
        }

        collectScrollView.InitScrollView(OnClickObjective);
    }

    public override void OnOpenWindow()
    {
        base.OnOpenWindow();

        if (uiAchieve == null)
            uiAchieve = UIManager.GetWindowGameObject(WindowType.Achievement).GetComponent<UI_Achievement>();
        uiAchieve.ShowAvatar(false);
    }

    public override void OnCloseWindow()
    {
        base.OnCloseWindow();

        descPanel.ClosePanel();
        dragSpinAvatar.Reset();
        modelAvatar.Cleanup();
        collectScrollView.Clear();
        uiAchieve.ShowAvatar(true);
        uiColDetails.CleanUp();
        selectedObjective = null;
        displayList.Clear();
    }

    public void GoToTab(CollectionType type, UI_Achievement_CollectionDetails parent)
    {
        uiColDetails = parent;
        tabController.GoToPage((int)type);
    }

    public void Init(bool isOn, CollectionType type)  // called when collection type toggled
    {
        if (isOn)
        {
            player = GameInfo.gLocalPlayer;
            achStats = player.AchievementStats;
            uiColDetails.SetToggleOn((int)type);
            StartCoroutine(LateSelectCollectionType(type));
        }
    }

    private IEnumerator LateSelectCollectionType(CollectionType type)
    {
        yield return null;  // wait one frame for comboboxes to be setup

        // select 1st item in combobox if none selected
        if (type == CollectionType.Monster)
        {
            if (monsterTypeFilter.SelectedIndex == -1)
                monsterTypeFilter.SelectedIndex = 0;
        }
        else if (type == CollectionType.Fashion)
        {
            if (fashionPartsTypeFilter.SelectedIndex == -1)
                fashionPartsTypeFilter.SelectedIndex = 0;
        }
        else if (type == CollectionType.Relic)
        {
            if (relicRarityFilter.SelectedIndex == -1)
                relicRarityFilter.SelectedIndex = 0;
            if (relicTypeFilter.SelectedIndex == -1)
                relicTypeFilter.SelectedIndex = 0;
        }
        else if (type == CollectionType.DNA)
        {
            if (dnaRarityFilter.SelectedIndex == -1)
                dnaRarityFilter.SelectedIndex = 0;
            if (dnaTypeFilter.SelectedIndex == -1)
                dnaTypeFilter.SelectedIndex = 0;
        }

        OnClickCollectionType(type);
    }

    private void OnClickCollectionType(CollectionType type)
    {
        objectivesList = AchievementRepo.GetCollectionObjectivesByType(type);
        switch (type)
        {
            case CollectionType.Monster:
                PopulateObjectiveList(monsterTypeFilter.SelectedIndex);
                break;
            case CollectionType.Fashion:
                PopulateObjectiveList(fashionPartsTypeFilter.SelectedIndex);
                break;
            case CollectionType.Hero:
            case CollectionType.NPC:
                PopulateObjectiveList(0);
                break;
            case CollectionType.Relic:
                PopulateObjectiveList(relicTypeFilter.SelectedIndex, relicRarityFilter.SelectedIndex);
                break;
            case CollectionType.DNA:
                PopulateObjectiveList(dnaTypeFilter.SelectedIndex, dnaRarityFilter.SelectedIndex);
                break;
            case CollectionType.Photo:
                PopulateObjectiveList(0);  // todo: jm to replace with photo region filter
                break;
        }
    }

    private void PopulateObjectiveList(int typeFilterIndex, int rarityFilterIndex = 0, bool resetSelected = true)
    {
        displayList.Clear();

        List<CollectionObjective> filteredList;
        if (typeFilterIndex == 0 && rarityFilterIndex == 0)
            filteredList = objectivesList;
        else
            filteredList = objectivesList.Where(x => FilterCondition(x, typeFilterIndex, rarityFilterIndex)).ToList();

        int length = filteredList.Count;
        for (int i = 0; i < length; ++i)
        {
            CollectionObjective obj = filteredList[i];
            CollectStatus status = achStats.GetCollectionObjectiveStatus(obj);
            CollectionInfo info = new CollectionInfo(obj, status);
            displayList.Add(info);
        }

        if (displayList.Count > 0)
        {
            int index = 0;
            if (!resetSelected && selectedObjective != null)
            {
                int foundIndex = displayList.FindIndex(x => x.objective.id == selectedObjective.id);
                if (foundIndex != -1)
                    index = foundIndex;
            }
            collectScrollView.Populate(displayList, index);
        }
        else
        {
            Empty();
        }
    }

    private void Empty()
    {
        modelAvatar.Cleanup();
        selectedObjective = null;
        collectScrollView.Clear();
        descPanel.Empty();
        collectDateText.text = "";
        lockImageObj.SetActive(false);
    }

    public void RefreshObjectiveList()
    {
        bool needRefresh = false;
        int length = displayList.Count;
        for (int i = 0; i < length; ++i)
        {
            CollectionInfo info = displayList[i];
            CollectStatus newStatus = achStats.GetCollectionObjectiveStatus(info.objective);
            if (info.status != newStatus)
            {
                info.status = newStatus;
                needRefresh = true;
            }
        }

        if (needRefresh)
        {
            collectScrollView.Refresh(displayList);
            if (selectedObjective != null)
                RefreshSelectedObjective();
        }
    }

    private bool FilterCondition(CollectionObjective obj, int typeFilterIndex, int rarityFilterIndex)
    {
        if (obj.type == CollectionType.Monster)
        {
            return (int)obj.json.monstertype == typeFilterIndex - 1;
        }
        else if (obj.type == CollectionType.Fashion)
        {
            Equipment equipment = obj.targetJsonObject as Equipment;
            if (equipment != null)
            {
                FashionSlot fashionSlot = InventoryHelper.GetFashionSlotByPartType(equipment.EquipmentJson.partstype);
                if (typeFilterIndex == 1)
                    return fashionSlot == FashionSlot.Helm;
                else if (typeFilterIndex == 2)
                    return fashionSlot == FashionSlot.Weapon;
                else if (typeFilterIndex == 3)
                    return fashionSlot == FashionSlot.Body;
                else
                    return fashionSlot == FashionSlot.Back;
            }
            return false;
        }
        else if (obj.type == CollectionType.Relic)
        {
            Relic relic = obj.targetJsonObject as Relic;
            if (relic != null)
            {
                if (rarityFilterIndex == 0) // all rarity, filter by type
                    return (int)relic.RelicJson.relictype == typeFilterIndex; // Relictype start from 1
                else if (typeFilterIndex == 0)  // all type, filter by rarity
                    return (int)relic.RelicJson.rarity == rarityFilterIndex - 1;
                else  // filter by type and rarity
                    return (int)relic.RelicJson.relictype == typeFilterIndex && (int)relic.RelicJson.rarity == rarityFilterIndex - 1;
            }
            return false;
        }
        else if (obj.type == CollectionType.DNA)
        {
            DNA dna = obj.targetJsonObject as DNA;
            if (dna != null)
            {
                if (rarityFilterIndex == 0) // all rarity, filter by type
                    return (int)dna.DNAJson.dnatype == typeFilterIndex; // DNAtype start from 1
                else if (typeFilterIndex == 0)  // all type, filter by rarity
                    return (int)dna.DNAJson.rarity == rarityFilterIndex - 1;
                else  // filter by type and rarity
                    return (int)dna.DNAJson.dnatype == typeFilterIndex && (int)dna.DNAJson.rarity == rarityFilterIndex - 1;
            }
            return false;
        }
        else if (obj.type == CollectionType.Photo)
        {
            return (int)obj.json.regiontype == typeFilterIndex - 1;
        }
        else
            return true;
    }

    public void OnClickObjective(bool isOn, CollectionObjective obj)
    {
        if (!isOn || selectedObjective == obj)
            return;
        //print("click on obj: " + obj.id);
        selectedIdx = displayList.FindIndex(x => x.objective.id == obj.id);
        collectScrollView.SetSelectedIndex(selectedIdx);

        selectedObjective = obj;

        CollectionElement elem = achStats.GetCollectionById(obj.id);
        bool isUnlocked = elem != null;

        lockImageObj.SetActive(!isUnlocked);
        collectDateText.text = isUnlocked ? GUILocalizationRepo.GetLocalizedDateTime(elem.CollectDate, 2) :
                                            AchievementRepo.GetCollectionTypeLockString(obj.type);
        collectDateText.color = isUnlocked ? Color.white : ClientUtils.ColorGray;
        descPanel.Init(obj, elem);

        UpdateAvatarModel(obj);

        switch (obj.type)
        {
            case CollectionType.Monster:
                monsterTooltipObj.SetActive(isUnlocked);
                CombatNPCJson combatNPCJson = obj.targetJsonObject as CombatNPCJson;
                if (combatNPCJson != null)
                {
                    if (combatNPCJson.element == Element.None)
                        monsterElementImage.transform.parent.gameObject.SetActive(false);
                    else
                    {
                        monsterElementImage.transform.parent.gameObject.SetActive(true);
                        monsterElementImage.sprite = ClientUtils.LoadMonsterElementIcon(combatNPCJson.element);
                    }
                    monsterTooltipText.text = ConstructMonsterStatsText(combatNPCJson);
                }
                break;
            case CollectionType.Fashion:
                fashionStoreButton.SetActive(isUnlocked && !elem.Stored);
                fashionTakeOutButton.SetActive(isUnlocked && elem.Stored);
                break;
            case CollectionType.Hero:
                break;
            case CollectionType.NPC:
                npcLocationText.text = obj.json.localizedlocation;
                break;
            case CollectionType.Relic:
                relicStoreButton.SetActive(isUnlocked && !elem.Stored);
                relicTakeOutButton.SetActive(isUnlocked && elem.Stored);
                break;
            case CollectionType.DNA:
                break;
            case CollectionType.Photo:
                break;
        }
    }

    private void RefreshSelectedObjective()
    {
        CollectionElement elem = achStats.GetCollectionById(selectedObjective.id);
        bool isUnlocked = elem != null;

        lockImageObj.SetActive(!isUnlocked);
        collectDateText.text = isUnlocked ? GUILocalizationRepo.GetLocalizedDateTime(elem.CollectDate, 2) :
                                            AchievementRepo.GetCollectionTypeLockString(selectedObjective.type);
        collectDateText.color = isUnlocked ? Color.white : ClientUtils.ColorGray;
        descPanel.Init(selectedObjective, elem);

        switch (selectedObjective.type)
        {
            case CollectionType.Monster:
                monsterTooltipObj.SetActive(isUnlocked);
                break;
            case CollectionType.Fashion:
                fashionStoreButton.SetActive(isUnlocked && !elem.Stored);
                fashionTakeOutButton.SetActive(isUnlocked && elem.Stored);
                break;
            case CollectionType.Relic:
                relicStoreButton.SetActive(isUnlocked && !elem.Stored);
                relicTakeOutButton.SetActive(isUnlocked && elem.Stored);
                break;
            case CollectionType.Photo:
                break;
        }
    }

    private void UpdateAvatarModel(CollectionObjective obj)
    {
        dragSpinAvatar.Reset();

        switch (obj.type)
        {
            case CollectionType.Monster:
                CombatNPCJson combatNPCJson = obj.targetJsonObject as CombatNPCJson;
                if (combatNPCJson != null)
                    modelAvatar.Change(combatNPCJson.containerprefabpath, OnModelLoaded);
                break;
            case CollectionType.Fashion:
                if (modelAvatar.gameObject.activeInHierarchy) // need wait for active in order to play avatar animation
                {
                    Equipment equipment = obj.targetJsonObject as Equipment;
                    if (equipment != null && equipment.EquipmentJson.fashionsuit)
                    {
                        EquipmentInventoryData cloneEquipInvData = player.mEquipmentInvData.CloneJsonWithItemConverter();
                        FashionSlot slot = InventoryHelper.GetFashionSlotByPartType(equipment.EquipmentJson.partstype);
                        if (slot != FashionSlot.MAXSLOTS)
                        {
                            if (slot == FashionSlot.Helm)
                                cloneEquipInvData.HideHelm = false;
                            if (slot == FashionSlot.Weapon)
                                cloneEquipInvData.SetEquipmentToSlot((int)EquipmentSlot.Weapon, equipment);
                            else
                                cloneEquipInvData.SetFashionToSlot((int)slot, equipment);
                            modelAvatar.Change(cloneEquipInvData, player.GetJobSect(), player.mGender, OnModelLoaded);
                        }
                    }
                }
                else
                    StartCoroutine(LateChangeAvatar(obj));
                break;
            case CollectionType.Hero:
                HeroStatsClient heroStats = player.HeroStats;
                Hero hero = heroStats.GetHero(obj.targetId);
                if (hero == null)  // locked
                {
                    HeroJson heroJson = obj.targetJsonObject as HeroJson;
                    hero = new Hero(obj.targetId, heroJson);
                }
                modelAvatar.ChangeHero(obj.targetId, hero.ModelTier, OnModelLoaded);
                break;
            case CollectionType.NPC:
                StaticNPCJson staticNPCJson = obj.targetJsonObject as StaticNPCJson;
                if (staticNPCJson != null)
                    modelAvatar.Change(staticNPCJson.containerprefabpath, OnModelLoaded);
                break;
            case CollectionType.Relic:
            case CollectionType.DNA:
            case CollectionType.Photo:
                modelAvatar.Cleanup();
                break;
        }
    }

    private IEnumerator LateChangeAvatar(CollectionObjective obj)
    {
        yield return new WaitUntil(() => modelAvatar.gameObject.activeInHierarchy);
        UpdateAvatarModel(obj);
    }

    private void OnModelLoaded(GameObject model)
    {
        float[] camera = StaticNPCRepo.ParseCameraPosInTalk(selectedObjective.json.uipos);
        Vector3 pos = model.transform.parent.localPosition;
        model.transform.parent.localPosition = new Vector3(camera[0], camera[1], pos.z);
        model.transform.localRotation = Quaternion.Euler(new Vector3(0, camera[2], 0));
        model.transform.localScale = new Vector3(camera[3], camera[3], camera[3]);
    }

    private string ConstructMonsterStatsText(CombatNPCJson json)
    {
        sb.Length = 0;
        sb.AppendFormat("{0}   {1}", GUILocalizationRepo.GetLocalizedString("com_health"), json.healthmax).AppendLine();
        sb.AppendFormat("{0}   {1}", GUILocalizationRepo.GetLocalizedString("com_level"), json.level).AppendLine();
        sb.AppendFormat("{0}   {1}", GUILocalizationRepo.GetLocalizedString("com_race"), ClientUtils.GetLocalizedRace(json.race)).AppendLine();
        sb.AppendFormat("{0}   {1}", GUILocalizationRepo.GetLocalizedString("com_weakness"), ClientUtils.GetLocalizedAttackStyle(json.weakness)).AppendLine();
        sb.AppendFormat("{0}   {1}", GUILocalizationRepo.GetLocalizedString("com_attack"), json.attack).AppendLine();
        sb.AppendFormat("{0}   {1}", GUILocalizationRepo.GetLocalizedString("stats_armor"), json.armor).AppendLine();
        sb.AppendFormat("{0}   {1}", GUILocalizationRepo.GetLocalizedString("stats_accuracy"), json.accuracy).AppendLine();
        sb.AppendFormat("{0}   {1}", GUILocalizationRepo.GetLocalizedString("stats_evasion"), json.evasion);
        return sb.ToString();
    }

    public void OnClickGetOrigin()
    {
        if (selectedObjective == null)
            return;
        IInventoryItem item = selectedObjective.targetJsonObject as IInventoryItem;
        if (item != null)
        {
            UIManager.OpenDialog(WindowType.DialogCollectionOrigin,
                (window) => window.GetComponent<UI_Achievement_CollectionOriginDialog>().Init(item.JsonObject.origin));
        }
    }

    public void OnClickStoreCollectionItem(bool store)
    {
        if (selectedObjective == null)
            return;
        RPCFactory.CombatRPC.StoreCollectionItem(selectedObjective.id, store);
    }

    public void OnClickOpenHeroUI()
    {
        if (selectedObjective == null)
            return;
        ClientUtils.OpenUIWindowByLinkUI(LinkUIType.Hero, selectedObjective.targetId.ToString());
        GetComponent<UIDialog>().ClickClose();
    }

    public void OnClickPathFindToNPC()
    {
        if (selectedObjective == null)
            return;

        string currentLevel = ClientUtils.GetCurrentLevelName();
        string targetLevel = "";
        Vector3 targetPos = Vector3.zero;
        StaticNPCJson staticNPCJson = selectedObjective.targetJsonObject as StaticNPCJson;
        if (staticNPCJson != null && NPCPosMap.FindNearestStaticNPC(staticNPCJson.archetype, currentLevel, player.Position, ref targetLevel, ref targetPos))
        {
            bool foundtarget = true;
            if (currentLevel == targetLevel)
                player.ProceedToTarget(targetPos, staticNPCJson.id, CallBackAction.Interact);
            else
            {
                BotController.TheDijkstra.DoRouter(currentLevel, targetLevel, out foundtarget);
                if (foundtarget)
                {
                    BotController.DestLevel = targetLevel;
                    BotController.DestMapPos = targetPos;
                    BotController.DestAction = ReachTargetAction.NPC_Interact;
                    BotController.DestArchtypeID = staticNPCJson.id;
                    player.Bot.SeekingWithRouter();
                }
                else
                    UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("sys_CannotFindTarget"));
            }

            if (foundtarget)
            {
                UIManager.CloseAllDialogs();
                UIManager.CloseAllWindows();
            }
        }
        else
            UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("sys_CannotFindTarget"));
    }

    public void OnTypeSelectionChanged(int index)  // used for monster and fashion
    {
        PopulateObjectiveList(index, 0, false);
    }

    public void OnRelicTypeSelectionChanged(int index)
    {
        PopulateObjectiveList(index, relicRarityFilter.SelectedIndex, false);
    }

    public void OnRelicRaritySelectionChanged(int index)
    {
        PopulateObjectiveList(relicTypeFilter.SelectedIndex, index, false);
    }

    public void OnDNATypeSelectionChanged(int index)
    {
        PopulateObjectiveList(index, dnaRarityFilter.SelectedIndex, false);
    }

    public void OnDNARaritySelectionChanged(int index)
    {
        PopulateObjectiveList(dnaTypeFilter.SelectedIndex, index, false);
    }
}