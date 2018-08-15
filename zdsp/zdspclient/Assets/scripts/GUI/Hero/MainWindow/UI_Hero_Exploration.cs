using Kopio.JsonContracts;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Repository;

public class UI_Hero_Exploration : MonoBehaviour
{
    [Header("Left Side")]
    [SerializeField] Text mapNameText;
    [SerializeField] Text remainingTimeText;
    [SerializeField] Image backgroundImage;
    [SerializeField] Transform reqmtParent;
    [SerializeField] GameObject reqmtDataPrefab;

    [Header("Target")]
    [SerializeField] Button targetBtn;
    [SerializeField] Image targetIconImage;

    [Header("Heroes")]
    [SerializeField] Transform heroDataParent;
    [SerializeField] GameObject heroDataPrefab;
    [SerializeField] Text efficiencyText;

    [Header("Rewards")]
    [SerializeField] Transform rewardItemParent;
    [SerializeField] GameObject[] itemPrefabs; // 0 - equipment  1 - material/consumable, 2 - DNA
    [SerializeField] ScrollRect itemListScrollRect;

    [Header("Bottom")]
    [SerializeField] Transform reqItemParent;
    [SerializeField] Text reqItemCount;
    [SerializeField] GameObject startButton;
    [SerializeField] GameObject claimButton;
    [SerializeField] Text tooltipText;

    [Header("Right Side")]
    [SerializeField] Text exploreLimitText;
    [SerializeField] ComboBoxA mapComboBox;
    [SerializeField] Transform mapDataParent;
    [SerializeField] GameObject mapDataPrefab;
    [SerializeField] ScrollRect mapListScrollRect;

    private List<ExplorationMapJson> allMapsList;
    private bool initialized;
    private string mapFilter = "all";
    private int selectedMapId = -1;
    private ExplorationMapJson selectedMapData;
    private int selectedTargetId = -1;
    private string backgroundPath = "";
    private List<Hero_MapRequirementData> requirementList = new List<Hero_MapRequirementData>();
    private List<Hero_MapHeroData> heroList = new List<Hero_MapHeroData>();
    private GameIcon_MaterialConsumable requiredItem;
    private Dictionary<int, int> currentMapTargets = new Dictionary<int, int>(); // mapid -> targetid

    private void Awake()
    {
        allMapsList = HeroRepo.explorationMapsList;

        // create map requirements icons
        for (int i = 0; i < 3; i++)
        {
            GameObject obj = ClientUtils.CreateChild(reqmtParent, reqmtDataPrefab);
            Hero_MapRequirementData data = obj.GetComponent<Hero_MapRequirementData>();
            obj.SetActive(false);
            requirementList.Add(data);
        }

        // create hero icons
        for (int i = 0; i < 3; i++)
        {
            GameObject obj = ClientUtils.CreateChild(heroDataParent, heroDataPrefab);
            Hero_MapHeroData data = obj.GetComponent<Hero_MapHeroData>();
            obj.SetActive(false);
            heroList.Add(data);
        }
    }

    private void OnEnable()
    {
        if (!initialized)
        {
            SetupDropdownList();
            PopulateMapList();
            initialized = true;
        }
    }

    public void Refresh()
    {
    }


    private void SetupDropdownList()
    {
        List<string> subNameList = new List<string>();
        for (int i = 0; i < allMapsList.Count; i++)
        {
            if (!subNameList.Contains(allMapsList[i].localizedsubname))
                subNameList.Add(allMapsList[i].localizedsubname);
        }

        mapComboBox.ClearItemList();
        mapComboBox.AddItem(GUILocalizationRepo.GetLocalizedString("hro_allmaps"), "all");  // all maps
        for (int i = 0; i < subNameList.Count; i++)
        {
            mapComboBox.AddItem(subNameList[i], subNameList[i]);
        }

        mapComboBox.SelectedIndex = 0;
    }

    public void OnMapSelectionChanged(int index)
    {
        if (mapFilter != mapComboBox.SelectedValue)
        {
            mapFilter = mapComboBox.SelectedValue;
            PopulateMapList();
        }
    }

    private void PopulateMapList()
    {
        ClientUtils.DestroyChildren(mapDataParent);
        mapListScrollRect.verticalNormalizedPosition = 1f;

        bool hasToggled = false;
        ToggleGroup toggleGroup = mapDataParent.GetComponent<ToggleGroup>();
        for (int i = 0; i < allMapsList.Count; i++)
        {
            if (mapFilter == "all" || mapFilter == allMapsList[i].localizedsubname)
            {
                GameObject obj = ClientUtils.CreateChild(mapDataParent, mapDataPrefab);
                Hero_MapData data = obj.GetComponent<Hero_MapData>();
                data.Init(allMapsList[i], toggleGroup, OnMapSelected);
                if (allMapsList[i].mapid == selectedMapId)
                {
                    data.SetToggleOn(true);
                    hasToggled = true;
                }
            }
        }

        if (mapDataParent.childCount > 0 && !hasToggled)
            mapDataParent.GetChild(0).GetComponent<Hero_MapData>().SetToggleOn(true);
    }

    private void OnMapSelected(int mapId)
    {
        if (selectedMapId != mapId)
        {
            selectedMapId = mapId;
            selectedMapData = HeroRepo.GetExplorationMapById(mapId);

            if (selectedMapData != null)
            {
                mapNameText.text = selectedMapData.localizedname;
                SetBackgroundImage();
                SetMapRequirements();
                SetToolTipText();
                SetExplorationTime();
                SetHeroList();
                SetRequiredItem();
                SetRewardList();

                if (!currentMapTargets.ContainsKey(mapId))
                    UpdateTarget(0);
                else
                    UpdateTarget(currentMapTargets[mapId]);
            }
        }
    }

    private void SetBackgroundImage()
    {
        TerrainJson terrainData = HeroRepo.GetTerrainByType(selectedMapData.terraintype);
        if (terrainData != null && backgroundPath != terrainData.backgroundpath)
        {
            ClientUtils.LoadIconAsync(terrainData.backgroundpath, OnBackgroundImageLoaded);
            backgroundPath = terrainData.backgroundpath;
        }
    }

    private void OnBackgroundImageLoaded(Sprite sprite)
    {
        if (sprite != null)
            backgroundImage.sprite = sprite;
    }

    private void SetMapRequirements()
    {
        requirementList[0].Init(selectedMapData.chestreqtype1, selectedMapData.chestreqvalue1);
        requirementList[1].Init(selectedMapData.chestreqtype2, selectedMapData.chestreqvalue2);
        requirementList[2].Init(selectedMapData.chestreqtype3, selectedMapData.chestreqvalue3);
    }

    private void SetToolTipText()
    {
        Dictionary<string, string> param = new Dictionary<string, string>();
        param.Add("time", selectedMapData.battletimecost.ToString());
        tooltipText.text = GUILocalizationRepo.GetLocalizedString("hro_exploration_tooltip", param);
    }

    private void SetExplorationTime()
    {
        double totalSeconds = selectedMapData.completetime * 60;
        remainingTimeText.text = GUILocalizationRepo.GetShortLocalizedTimeString(totalSeconds);
    }

    private void SetRequiredItem()
    {
        if (requiredItem == null)
            requiredItem = ClientUtils.CreateChild(reqItemParent, itemPrefabs[1]).GetComponent<GameIcon_MaterialConsumable>();

        requiredItem.InitWithTooltipViewOnly(selectedMapData.reqitemid, 1);
        reqItemCount.text = "x" + selectedMapData.reqitemcount;
    }

    private void SetHeroList()
    {
        int maxHeroCount = selectedMapData.maxherocount;
        for (int i = 0; i < heroList.Count; i++)
        {
            heroList[i].gameObject.SetActive(i < maxHeroCount);
        }
    }

    private void SetRewardList()
    {
        ClientUtils.DestroyChildren(rewardItemParent);
        itemListScrollRect.horizontalNormalizedPosition = 0f;
        CreateItemIcon(selectedMapData.chestitemid, 1);
    }

    private void CreateItemIcon(int itemId, int itemCount)
    {
        ItemBaseJson itemJson = GameRepo.ItemFactory.GetItemById(itemId);
        if (itemJson == null)
            return;

        switch (itemJson.bagtype)
        {
            case BagType.Equipment:
                ClientUtils.CreateChild(rewardItemParent, itemPrefabs[0]).GetComponent<GameIcon_Equip>().InitWithTooltipViewOnly(itemId);
                break;
            case BagType.Consumable:
            case BagType.Material:
                ClientUtils.CreateChild(rewardItemParent, itemPrefabs[1]).GetComponent<GameIcon_MaterialConsumable>().InitWithTooltipViewOnly(itemId, itemCount);
                break;
            case BagType.DNA:
                ClientUtils.CreateChild(rewardItemParent, itemPrefabs[2]).GetComponent<GameIcon_DNA>().InitWithTooltipViewOnly(itemId, 0, 0);
                break;
        }
    }

    public void CleanUp()
    {
        initialized = false;
        selectedMapId = -1;
        mapFilter = "all";
        selectedTargetId = -1;
    }

    public void OnClickSelectTarget()
    {
        UIManager.OpenDialog(WindowType.DialogHeroTargetList,
            (window) => window.GetComponent<UI_Hero_MapTargetDialog>().Init(selectedMapData, UpdateTarget));
    }

    private void UpdateTarget(int targetId)
    {
        currentMapTargets[selectedMapId] = targetId;

        if (selectedTargetId != targetId)
        {
            selectedTargetId = targetId;
            print("selected target: " + selectedTargetId);
            if (selectedTargetId == 0)  // all
                targetIconImage.sprite = ClientUtils.LoadIcon("UI_ZDSP_Icons/Portraits/zzz_Test.png");  // temp
            else
            {
                ExplorationTargetJson targetData = HeroRepo.GetExplorationTargetById(selectedTargetId);
                if (targetData != null)
                    targetIconImage.sprite = ClientUtils.LoadIcon(targetData.iconpath);
            }
        }
    }
}