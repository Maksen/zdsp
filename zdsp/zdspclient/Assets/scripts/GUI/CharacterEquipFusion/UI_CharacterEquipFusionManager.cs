using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Client.Entities;
using Zealot.Repository;
using Zealot.Common;
using System.Text;

struct SelectItemData
{
    public int InventoryIndex;
    public int ObjectInventoryIndex;
    public IInventoryItem Item;

    public SelectItemData (int _InventoryIndex, int _ObjectInventoryIndex, IInventoryItem _Item)
    {
        InventoryIndex = _InventoryIndex;
        ObjectInventoryIndex = _ObjectInventoryIndex;
        Item = _Item;
    }
}

public class UI_CharacterEquipFusionManager : MonoBehaviour {

    public static UI_CharacterEquipFusionManager mine;

    [SerializeField] private Text equipmentName;
    [SerializeField] private Text equipmentUpgradeValue;
    [SerializeField] private Text equipmentEvolveValue;
    [SerializeField] private Text equipmentRank;
    [SerializeField] private GameObject[] addButton;

    [SerializeField] private GameObject selectPrefab;
    [SerializeField] private Transform[] selectParent;
    private GameObject[] addObject;
    [SerializeField] private Text[] equipmentEffect;
    [SerializeField] private Text[] gemEffect1;
    [SerializeField] private Text[] gemEffect2;
    [SerializeField] private Text[] gemEffect3;

    [SerializeField] private GameObject bagPrefab;
    [SerializeField] private Transform materialParent;
    [SerializeField] private GameObject materialPanel;
    [SerializeField] private Button fusionButton;
    [SerializeField] private Text costCurrencyText;

    PlayerGhost player;

    List<SelectItemData> equipSlots = new List<SelectItemData>();
    List<SelectItemData> stoneSlots = new List<SelectItemData>();
    
    SelectItemData[] selectedMaterial = new SelectItemData[5];

    private List<bool> buttonDown = new List<bool>();
    private List<bool> compeletStep = new List<bool>();

    List<GameObject> equipList = new List<GameObject>();
    List<GameObject> elementalList = new List<GameObject>();

    #region BasicSetting
    void Awake()
    {
        InitAwake();
    }

    void OnEnable ()
    {
        GetInventoryItem();
    }

    void OnDisable()
    {
        FinishedFusion();
    }
    #endregion

    #region Refresh
    void InitAwake ()
    {
        mine = this;
        addButton[0].GetComponent<Button>().onClick.AddListener(OnClickLeftEquipmentBtn);
        addButton[1].GetComponent<Button>().onClick.AddListener(OnClickLeftMaterialEquipmentBtn);
        addButton[2].GetComponent<Button>().onClick.AddListener(delegate { OnClickLeftStone(3); });
        addButton[3].GetComponent<Button>().onClick.AddListener(delegate { OnClickLeftStone(4); });
        addButton[4].GetComponent<Button>().onClick.AddListener(delegate { OnClickLeftStone(5); });

        addObject = new GameObject[5];
        for (int i = 0; i < 5; ++i)
        {
            Destroy(selectParent[i].GetChild(0).gameObject);
            GameObject GameIconEquip = ClientUtils.CreateChild(selectParent[i], selectPrefab);
            GameIconEquip.transform.GetChild(2).GetComponent<Text>().text = string.Empty;
            addObject[i] = GameIconEquip;
            addObject[i].SetActive(false);
        }

        addObject[0].GetComponent<Button>().onClick.AddListener(OnClickLeftEquipmentBtn);
        addObject[1].GetComponent<Button>().onClick.AddListener(OnClickLeftMaterialEquipmentBtn);
        addObject[2].GetComponent<Button>().onClick.AddListener(delegate { ClearSelectButton(3); });
        addObject[3].GetComponent<Button>().onClick.AddListener(delegate { ClearSelectButton(4); });
        addObject[4].GetComponent<Button>().onClick.AddListener(delegate { ClearSelectButton(5); });

        fusionButton.onClick.AddListener(EquipFusion);

        ClientUtils.DestroyChildren(materialParent.transform);
    }

    void GetInventoryItem()
    {
        ClientUtils.DestroyChildren(materialParent.transform);

        player = GameInfo.gLocalPlayer;
        RefreshUI();
        equipList.Clear();
        elementalList.Clear();

        fusionButton.interactable = false;

        selectedMaterial = new SelectItemData[] { new SelectItemData(-1, -1, null), new SelectItemData(-1, -1, null), new SelectItemData(-1, -1, null), new SelectItemData(-1, -1, null), new SelectItemData(-1, -1, null) };
        buttonDown = new List<bool>() { false, false, false, false, false };
        compeletStep = new List<bool>() { false, false, false, false, false };

        SearchFusionInventory();
    }

    void RefreshUI()
    {
        SetFusionEquipStats(string.Empty, string.Empty, string.Empty, string.Empty);
        costCurrencyText.text = "0";
        for (int i = 0; i < 6; ++i)
        {
            equipmentEffect[i].text = string.Empty;
            gemEffect1[i].text = string.Empty;
            gemEffect2[i].text = string.Empty;
            gemEffect3[i].text = string.Empty;
        }
    }

    public void SearchFusionInventory()
    {
        equipSlots.Clear();
        stoneSlots.Clear();
        List<IInventoryItem> allItems = player.clientItemInvCtrl.itemInvData.Slots;
        for (int i = 0; i < allItems.Count; ++i)
        {
            int index = i;
            if (allItems[index] == null)
            {
                continue;
            }
            if (allItems[index].JsonObject.itemtype == ItemType.Equipment)
            {
                equipSlots.Add(new SelectItemData(index, equipSlots.Count, allItems[index]));
            }
            else if (allItems[index].JsonObject.itemtype == ItemType.ElementalStone)
            {
                stoneSlots.Add(new SelectItemData(index, stoneSlots.Count, allItems[index]));
            }
        }
    }
    #endregion

    #region LeftSideClickEvent
    public void OnClickLeftEquipmentBtn()
    {
        ClickSelectButton(1);
        CloseElementalList();

        if (equipList.Count == 0)
        {
            for (int i = 0; i < equipSlots.Count; ++i)
            {
                int index = i;
                GameObject equip = ClientUtils.CreateChild(materialParent, bagPrefab);
                equipList.Add(equip);
                Equipment equipData = equipSlots[index].Item as Equipment;
                equip.GetComponent<Toggle>().onValueChanged.AddListener(delegate
                {
                    OnClickRightEquipment(equipSlots[index].InventoryIndex, index, equipData.ItemID);
                });
                FusionData_BagItem bagData = equip.GetComponent<FusionData_BagItem>();
                List<string> equipEffect = EquipFusionRepo.DecodeEffect(equipData.FusionEffect);
                List<string> equipStats = EquipFusionRepo.BuildEquipStats(equipData);
                bagData.SetEquipStats(ClientUtils.LoadItemIcon(equipData.ItemID), equipStats[0], equipStats[1], equipStats[2], equipStats[3], equipEffect);
            }
        } else
        {
            for (int i = 0; i < equipList.Count; ++i)
            {
                equipList[i].SetActive(true);
            }
            ShowEquipmentInRight();
        }
    }

    public void OnClickLeftMaterialEquipmentBtn ()
    {
        if (!compeletStep[0])
        {
            //view you should select equipment before click this button
            return;
        }
        CloseElementalList();
        ClickSelectButton(2);

        for (int i = 0; i < equipSlots.Count; ++i)
        {
            int index = i;
            equipList[index].SetActive((equipSlots[index].Item.ItemID != selectedMaterial[0].Item.ItemID) ? false : true);
        }
        ShowEquipmentInRight();
    }

    void ShowEquipmentInRight ()
    {
        if (selectedMaterial[0].ObjectInventoryIndex != -1)
        {
            equipList[selectedMaterial[0].ObjectInventoryIndex].SetActive(false);
        }

        if (selectedMaterial[1].ObjectInventoryIndex != -1)
        {
            equipList[selectedMaterial[1].ObjectInventoryIndex].SetActive(false);
        }
    }

    public void OnClickLeftStone (int order)
    {
        CloseEquipList();
        ClickSelectButton(order);

        if(compeletStep[order - 1] || !compeletStep[0])
        {
            ClearSelectButton(order);
            return;
        }

        if (elementalList.Count == 0)
        {
            for (int i = 0; i < stoneSlots.Count; ++i)
            {
                int index = i;
                GameObject stone = ClientUtils.CreateChild(materialParent, bagPrefab);
                elementalList.Add(stone);
                ElementalStone stoneData = stoneSlots[index].Item as ElementalStone;
                List<string> effectGroup = EquipFusionRepo.DecodeEffect(stoneData.FusionData);
                stone.GetComponent<Toggle>().onValueChanged.AddListener(delegate
                {
                    OnClickRightStone(stoneSlots[index].InventoryIndex, index, stoneSlots[index].Item.ItemID);
                });
                FusionData_BagItem bagData = stone.GetComponent<FusionData_BagItem>();
                bagData.SetGemStats(ClientUtils.LoadItemIcon(stoneSlots[index].Item.ItemID), stoneSlots[index].Item.JsonObject.localizedname, effectGroup);
            }
            OpenEquipList();
        }
        else
        {
            OpenEquipList();
            for (int i = 2; i < 5; ++i)
            {
                if (selectedMaterial[i].ObjectInventoryIndex != -1)
                {
                    elementalList[selectedMaterial[i].ObjectInventoryIndex].SetActive(false);
                }
            }
        }
    }

    void OpenEquipList()
    {
        int type = EquipFusionRepo.ConvertStoneType(selectedMaterial[0].Item.JsonObject.itemsort);

        for (int i = 0; i < elementalList.Count; ++i)
        {
            int gemType = EquipFusionRepo.ConvertStoneType(stoneSlots[i].Item.ItemID);
            elementalList[i].SetActive((type == gemType) ? true : false);
        }
    }

    void CloseEquipList ()
    {
        for (int i = 0; i < equipList.Count; ++i)
        {
            equipList[i].SetActive(false);
        }
    }

    void CloseElementalList()
    {
        if (elementalList.Count != 0)
        {
            for (int i = 0; i < elementalList.Count; ++i)
            {
                elementalList[i].SetActive(false);
            }
        }
    }
    #endregion

    #region RightSideClickEvent
    public void OnClickRightEquipment (int Index, int EquipListIndex, int ItemId)
    {
        equipList[EquipListIndex].GetComponent<Toggle>().isOn = false;
        materialPanel.SetActive(false);
        equipList[EquipListIndex].SetActive(false);

        if (buttonDown[0])
        {
            if (selectedMaterial[0].Item != null)
            {
                if (selectedMaterial[0].Item.ItemID != ItemId)
                {
                    ClearSelectButton(2);
                    ClearSelectButton(3);
                    ClearSelectButton(4);
                    ClearSelectButton(5);
                }
            }
            selectedMaterial[0] = new SelectItemData(Index, EquipListIndex, equipSlots[EquipListIndex].Item);
            SelectButton(0, ItemId);
            Equipment equip = equipSlots[EquipListIndex].Item as Equipment;
            List<string> equipEffect = EquipFusionRepo.DecodeEffect(equip.FusionEffect);
            List<string> equipStats = EquipFusionRepo.BuildEquipStats(equip);
            SetFusionEquipStats(equipStats[0], equipStats[1], equipStats[2], equipStats[3]);
            for (int i = 0; i < 6; ++i)
            {
                equipmentEffect[i].text = equipEffect[i];
            }
        } else if (buttonDown[1])
        {
            selectedMaterial[1] = new SelectItemData(Index, EquipListIndex, equipSlots[EquipListIndex].Item);
            SelectButton(1, ItemId);
        }
    }

    public void OnClickRightStone (int Index, int StoneListIndex, int ItemId)
    {
        elementalList[StoneListIndex].GetComponent<Toggle>().isOn = false;
        materialPanel.SetActive(false);
        elementalList[StoneListIndex].SetActive(false);

        for (int i = 2; i < buttonDown.Count; ++i)
        {
            if (buttonDown[i])
            {
                int index = i;
                selectedMaterial[index] = new SelectItemData(Index, StoneListIndex, stoneSlots[StoneListIndex].Item);
                SelectButton(index, ItemId);
                ElementalStone stone = selectedMaterial[index].Item as ElementalStone;
                List<string> stoneEffect = EquipFusionRepo.DecodeEffect(stone.FusionData);
                for (int j = 0; j < 6; ++j)
                {
                    switch (index)
                    {
                        case 2:
                            gemEffect1[j].text = stoneEffect[j];
                            break;
                        case 3:
                            gemEffect2[j].text = stoneEffect[j];
                            break;
                        case 4:
                            gemEffect3[j].text = stoneEffect[j];
                            break;
                    }
                    
                }
            }
        }
        if(compeletStep[0] && compeletStep[1] && compeletStep[2] && compeletStep[3] && compeletStep[4])
        {
            CompeletMaterial();
        } else
        {
            costCurrencyText.text = "0";
            fusionButton.interactable = false;
        }
    }

    void SelectButton (int order, int itemId)
    {
        addObject[order].SetActive(true);
        addButton[order].SetActive(false);
        addObject[order].transform.GetChild(1).GetComponent<Image>().sprite = ClientUtils.LoadItemIcon(itemId);
        compeletStep[order] = true;
        buttonDown[order] = false;
    }

    void CompeletMaterial ()
    {
        int totalCurrency = EquipFusionRepo.GetTotalCurrencyCount(selectedMaterial[2].Item.ItemID, selectedMaterial[3].Item.ItemID, selectedMaterial[4].Item.ItemID);
        costCurrencyText.text = totalCurrency.ToString("N0");
        fusionButton.interactable = (player.SecondaryStats.Money >= totalCurrency) ? true : false;
    }
    #endregion

    #region MiddleClickEvent
    public void EquipFusion ()
    {
        string str = BuildMaterialItem();
        Equipment equip = selectedMaterial[0].Item as Equipment;

        if (equip.FusionEffect != "0|0,0|0,0|0|0,0|0,0|0|0,0|0,0")
        {
            RPCFactory.NonCombatRPC.EquipFusionGive(selectedMaterial[0].InventoryIndex, str);
            AddWindowsListener(equip);

        } else
        {
            RPCFactory.NonCombatRPC.EquipFusion(selectedMaterial[0].InventoryIndex, str, true);
        }
    }
    
    public void FinishedFusion ()
    {
        for (int i = 0; i < 6; ++i)
        {
            ClearSelectButton(i);
        }
    }

    public static void RefreshFusion()
    {
        if (mine != null)
        {
            mine.FinishedFusion();
            mine.GetInventoryItem();
        }
    }

    public void NotEnoughMaterial ()
    {
        //Add buy material windows
    }

    string BuildMaterialItem ()
    {
        StringBuilder str = new StringBuilder(selectedMaterial[1].InventoryIndex.ToString());
        str.Append('|');
        str.Append(selectedMaterial[2].InventoryIndex.ToString());
        str.Append('|');
        str.Append(selectedMaterial[3].InventoryIndex.ToString());
        str.Append('|');
        str.Append(selectedMaterial[4].InventoryIndex.ToString());

        return str.ToString();
    }
    #endregion

    #region WindowsClickEvent
    public void AddWindowsListener(Equipment myEquip)
    {
        Dialog_EquipFusionManager windowsObject = UIManager.GetWindowGameObject(WindowType.DialogEquipFusion).GetComponent<Dialog_EquipFusionManager>();
        windowsObject.confirmChangeFusion.GetComponent<Button>().onClick.AddListener(ConfirmChange);
        windowsObject.cancelConfirm.GetComponent<Button>().onClick.AddListener(CancelChange);
    }

    public void ConfirmChange ()
    {
        RPCFactory.NonCombatRPC.EquipFusion(selectedMaterial[0].InventoryIndex, BuildMaterialItem(), true);
    }

    public void CancelChange ()
    {
        RPCFactory.NonCombatRPC.EquipFusion(selectedMaterial[0].InventoryIndex, BuildMaterialItem(), false);
    }
    #endregion

    #region UtilitiesFunction
    void ClickSelectButton (int index)
    {
        for (int i = 0; i < buttonDown.Count; ++i)
        {
            buttonDown[i] = (i == index - 1) ? true : false;
        }
        materialPanel.SetActive(true);
    }

    void ClearSelectButton(int Order)
    {
        switch (Order)
        {
            case 0:
                materialPanel.SetActive(false);
                break;
            case 1:
                ClearBtn(0);
                SetFusionEquipStats(string.Empty, string.Empty, string.Empty, string.Empty);
                for (int i = 0; i < 6; ++i)
                {
                    equipmentEffect[i].text = string.Empty;
                }
                break;
            case 2:
                ClearBtn(1);
                break;
            case 3:
                ClearBtn(2);
                for (int i = 0; i < 6; ++i)
                {
                    gemEffect1[i].text = string.Empty;
                }
                break;
            case 4:
                ClearBtn(3);
                for (int i = 0; i < 6; ++i)
                {
                    gemEffect2[i].text = string.Empty;
                }
                break;
            case 5:
                ClearBtn(4);
                for (int i = 0; i < 6; ++i)
                {
                    gemEffect3[i].text = string.Empty;
                }
                break;
            default:
                break;
        }
    }

    void ClearBtn (int order)
    {
        selectedMaterial[order] = new SelectItemData(-1, -1, null);
        addObject[order].SetActive(false);
        addButton[order].SetActive(true);
        compeletStep[order] = false;
    }

    void SetFusionEquipStats (string name, string upgrade, string evolve, string rank)
    {
        equipmentName.text = name;
        equipmentUpgradeValue.text = upgrade;
        equipmentEvolveValue.text = evolve;
        equipmentRank.text = rank;
    }
    #endregion
}
