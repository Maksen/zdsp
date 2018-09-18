using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Client.Entities;
using Zealot.Repository;
using Zealot.Common;
using System.Text;

public struct SelectItemData
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

public class UI_CharacterEquipFushionManager : MonoBehaviour {

    public static UI_CharacterEquipFushionManager mine;

    [SerializeField]
    private Text equipmentName;
    [SerializeField]
    private Text equipmentUpgradeValue;
    [SerializeField]
    private Text equipmentEvolveValue;
    [SerializeField]
    private Text equipmentRank;
    [SerializeField]
    private Button[] addButton;
    [SerializeField]
    private Transform[] addObject;
    [SerializeField]
    private Transform[] equipmentEffect;
    [SerializeField]
    private Transform[] gemEffect;
    [SerializeField]
    private GameObject equipPrefab;
    [SerializeField]
    private GameObject gemPrefab;
    [SerializeField]
    private Transform materialParent;
    [SerializeField]
    private GameObject materialPanel;
    [SerializeField]
    private Button fushionButton;
    [SerializeField]
    private Text costCurrencyText;

    PlayerGhost player;

    List<SelectItemData> equipSlots = new List<SelectItemData>();
    List<SelectItemData> stoneSlots = new List<SelectItemData>();
    
    SelectItemData[] selectedMaterial;

    private List<bool> buttonDown = new List<bool>();
    private List<bool> compeletStep = new List<bool>();

    List<GameObject> equipList = new List<GameObject>();
    List<GameObject> elementalList = new List<GameObject>();

    void Awake()
    {
        mine = this;
        addButton[0].onClick.AddListener(OnClickLeftEquipmentBtn);
        addButton[1].onClick.AddListener(OnClickLeftMaterialEquipmentBtn);
        addButton[2].onClick.AddListener(delegate { OnClickLeftStone(3); });
        addButton[3].onClick.AddListener(delegate { OnClickLeftStone(4); });
        addButton[4].onClick.AddListener(delegate { OnClickLeftStone(5); });

        addObject[0].GetComponent<Button>().onClick.AddListener(OnClickLeftEquipmentBtn);
        addObject[1].GetComponent<Button>().onClick.AddListener(OnClickLeftMaterialEquipmentBtn);
        addObject[2].GetComponent<Button>().onClick.AddListener(delegate { OnCloseLeftStone(3); });
        addObject[3].GetComponent<Button>().onClick.AddListener(delegate { OnCloseLeftStone(4); });
        addObject[4].GetComponent<Button>().onClick.AddListener(delegate { OnCloseLeftStone(5); });

        fushionButton.onClick.AddListener(EquipFushion);

        ClientUtils.DestroyChildren(materialParent.transform);
    }

    void OnEnable ()
    {
        GetInventoryItem();
    }

    void OnDisable()
    {
        FinishedFushion();
    }

    void GetInventoryItem ()
    {
        ClientUtils.DestroyChildren(materialParent.transform);

        player = GameInfo.gLocalPlayer;
        RefreshUI();
        equipList.Clear();
        elementalList.Clear();

        fushionButton.interactable = false;

        selectedMaterial = new SelectItemData[] { new SelectItemData(-1, -1, null), new SelectItemData(-1, -1, null), new SelectItemData(-1, -1, null), new SelectItemData(-1, -1, null), new SelectItemData(-1, -1, null) };
        buttonDown = new List<bool>() { false, false, false, false, false };
        compeletStep = new List<bool>() { false, false, false, false, false };
        
        SearchFushionInventory();
    }
    
    void RefreshUI ()
    {
        equipmentName.text = string.Empty;
        equipmentUpgradeValue.text = string.Empty;
        equipmentEvolveValue.text = string.Empty;
        equipmentRank.text = string.Empty;
        costCurrencyText.text = "0";
        for (int i = 0; i < 3; ++i)
        {
            equipmentEffect[i].GetChild(0).GetComponent<Text>().text = string.Empty;
            equipmentEffect[i].GetChild(1).GetComponent<Text>().text = string.Empty;
        }
        for (int i = 0; i < 3; ++i)
        {
            for (int j = 4; j < 10; ++j)
            {
                gemEffect[i].GetChild(j).GetComponent<Text>().text = string.Empty;
            }
        }
    }

    public void SearchFushionInventory()
    {
        equipSlots.Clear();
        stoneSlots.Clear();
        List<IInventoryItem> allItems = player.clientItemInvCtrl.itemInvData.Slots;
        for (int i = 0; i < allItems.Count; ++i)
        {
            int index = i;
            if(allItems[index] == null)
            {
                continue;
            }
            if(allItems[index].JsonObject.itemtype == ItemType.Equipment)
            {
                equipSlots.Add(new SelectItemData(index, equipSlots.Count, allItems[index]));
            }
            else if (allItems[index].JsonObject.itemtype == ItemType.ElementalStone)
            {
                stoneSlots.Add(new SelectItemData(index, stoneSlots.Count, allItems[index]));
            }
        }
    }

    #region LeftSide
    public void OnClickLeftEquipmentBtn()
    {
        ClickSelectButton(1);

        if (elementalList.Count != 0)
        {
            CloseElementalList();
        }

        if (equipList.Count == 0)
        {
            for (int i = 0; i < equipSlots.Count; ++i)
            {
                int index = i;
                GameObject equip = ClientUtils.CreateChild(materialParent, equipPrefab);
                equipList.Add(equip);
                Equipment equipData = equipSlots[index].Item as Equipment;
                equip.GetComponent<Toggle>().onValueChanged.AddListener(delegate
                {
                    OnClickRightEquipment(equipSlots[index].InventoryIndex, index, equipData.JsonObject.itemid);
                });
                equip.transform.GetChild(3).GetComponent<Image>().sprite = ClientUtils.LoadItemIcon(equipData.JsonObject.itemid);
                Transform equipText = equip.transform.GetChild(2).GetChild(3);
                equipText.GetChild(0).GetComponent<Text>().text = equipData.JsonObject.localizedname;
                StringBuilder st = new StringBuilder("+");
                st.Append(equipData.UpgradeLevel.ToString());
                equipText.GetChild(2).GetComponent<Text>().text = st.ToString();
                equipText.GetChild(4).GetComponent<Text>().text = equipData.ReformStep.ToString();
                equipText.GetChild(5).GetComponent<Text>().text = "階";
                List<string> equipEffect = EquipFushionRepo.DecodeEffect(equipData.FushionEffect);
                for (int j = 0; j < 3; ++j)
                {
                    equip.transform.GetChild(4 + j).GetChild(0).GetComponent<Text>().text = (j < equipEffect.Count) ? equipEffect[j] : string.Empty;
                    equip.transform.GetChild(4 + j).GetChild(1).GetComponent<Text>().text = string.Empty;
                }
            }
        } else
        {
            for (int i = 0; i < equipList.Count; ++i)
            {
                equipList[i].SetActive(true);
            }

            if(selectedMaterial[0].ObjectInventoryIndex != -1)
            {
                equipList[selectedMaterial[0].ObjectInventoryIndex].SetActive(false);
            }

            if (selectedMaterial[1].ObjectInventoryIndex != -1)
            {
                equipList[selectedMaterial[1].ObjectInventoryIndex].SetActive(false);
            }
        }
    }

    public void OnClickLeftMaterialEquipmentBtn ()
    {
        if (!compeletStep[0])
        {
            //view you should select equipment before click this button
            return;
        }

        if (elementalList.Count != 0)
        {
            CloseElementalList();
        }

        ClickSelectButton(2);

        for (int i = 0; i < equipSlots.Count; ++i)
        {
            int index = i;
            equipList[index].SetActive((equipSlots[index].Item.JsonObject.itemid != selectedMaterial[0].Item.JsonObject.itemid) ? false : true);
        }

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
                GameObject stone = ClientUtils.CreateChild(materialParent, gemPrefab);
                elementalList.Add(stone);
                stone.transform.GetChild(3).GetComponent<Image>().sprite = ClientUtils.LoadItemIcon(stoneSlots[index].Item.JsonObject.itemid);
                stone.transform.GetChild(2).GetChild(2).GetComponent<Text>().text = stoneSlots[index].Item.JsonObject.localizedname;
                ElementalStone stoneData = stoneSlots[index].Item as ElementalStone;
                List<string> effectGroup = EquipFushionRepo.DecodeEffect(stoneData.FushionData);
                for (int j = 0; j < effectGroup.Count; ++j)
                {
                    stone.transform.GetChild(4 + j * 2).GetComponent<Text>().text = effectGroup[j];
                }
                for (int j = 0; j < 3 - effectGroup.Count; ++j)
                {
                    stone.transform.GetChild(8 - j * 2).GetComponent<Text>().text = string.Empty;
                }
                for (int j = 0; j < 3; ++j)
                {
                    stone.transform.GetChild(9 - j * 2).GetComponent<Text>().text = string.Empty;
                }
                stone.GetComponent<Toggle>().onValueChanged.AddListener(delegate
                {
                    OnClickRightStone(stoneSlots[index].InventoryIndex, index, stoneSlots[index].Item.JsonObject.itemid);
                });
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

    public void OnCloseLeftStone (int order)
    {
        ClearSelectButton(order);
    }

    void OpenEquipList()
    {
        int type = EquipFushionRepo.ConvertStoneType(selectedMaterial[0].Item.JsonObject.itemsort);

        for (int i = 0; i < elementalList.Count; ++i)
        {
            int gemType = EquipFushionRepo.ConvertStoneType(stoneSlots[i].Item.JsonObject.itemid);
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
        for (int i = 0; i < elementalList.Count; ++i)
        {
            elementalList[i].SetActive(false);
        }
    }
    #endregion

    #region RightSide
    public void OnClickRightEquipment (int Index, int EquipListIndex, int ItemId)
    {
        equipList[EquipListIndex].GetComponent<Toggle>().isOn = false;
        materialPanel.SetActive(false);
        equipList[EquipListIndex].SetActive(false);

        if (buttonDown[0])
        {
            if (selectedMaterial[0].Item != null)
            {
                if (selectedMaterial[0].Item.JsonObject.itemid != ItemId)
                {
                    ClearSelectButton(2);
                    ClearSelectButton(3);
                    ClearSelectButton(4);
                    ClearSelectButton(5);
                }
            }
            selectedMaterial[0] = new SelectItemData(Index, EquipListIndex, equipSlots[EquipListIndex].Item);
            addObject[0].gameObject.SetActive(true);
            addButton[0].gameObject.SetActive(false);
            addObject[0].GetChild(1).GetComponent<Image>().sprite = ClientUtils.LoadItemIcon(ItemId);
            Equipment equip = equipSlots[EquipListIndex].Item as Equipment;
            List<string> equipEffect = EquipFushionRepo.DecodeEffect(equip.FushionEffect);
            equipmentName.text = equip.GetEquipmentName();
            StringBuilder st = new StringBuilder("+");
            st.Append(equip.UpgradeLevel.ToString());
            equipmentUpgradeValue.text = st.ToString();
            equipmentEvolveValue.text = equip.ReformStep.ToString();
            equipmentRank.text = "階";
            for (int i = 0; i < 3; ++i)
            {
                equipmentEffect[i].GetChild(0).GetComponent<Text>().text = (i < equipEffect.Count) ? equipEffect[i] : string.Empty;
            }
            compeletStep[0] = true; buttonDown[0] = false;
        } else if (buttonDown[1])
        {
            selectedMaterial[1] = new SelectItemData(Index, EquipListIndex, equipSlots[EquipListIndex].Item);
            addObject[1].gameObject.SetActive(true);
            addButton[1].gameObject.SetActive(false);
            addObject[1].GetChild(1).GetComponent<Image>().sprite = ClientUtils.LoadItemIcon(ItemId);
            compeletStep[1] = true; buttonDown[1] = false;
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
                addObject[index].gameObject.SetActive(true);
                addButton[index].gameObject.SetActive(false);
                addObject[index].GetChild(1).GetComponent<Image>().sprite = ClientUtils.LoadItemIcon(ItemId);
                compeletStep[i] = true; buttonDown[i] = false;
                ElementalStone stone = selectedMaterial[index].Item as ElementalStone;
                List<string> stoneEffect = EquipFushionRepo.DecodeEffect(stone.FushionData);
                for (int j = 0; j < stoneEffect.Count; ++j)
                {
                    gemEffect[index - 2].GetChild(4 + j * 2).GetComponent<Text>().text = stoneEffect[j];
                }
            }
        }
        if(compeletStep[0] && compeletStep[1] && compeletStep[2] && compeletStep[3] && compeletStep[4])
        {
            CompeletMaterial();
        } else
        {
            costCurrencyText.text = "0";
            fushionButton.interactable = false;
        }
    }

    void CompeletMaterial ()
    {
        int totalCurrency = EquipFushionRepo.GetTotalCurrencyCount(selectedMaterial[2].Item.JsonObject.itemid, selectedMaterial[3].Item.JsonObject.itemid, selectedMaterial[4].Item.JsonObject.itemid);
        costCurrencyText.text = totalCurrency.ToString("N0");
        fushionButton.interactable = (player.SecondaryStats.Money >= totalCurrency) ? true : false;
    }
    #endregion

    #region Middle
    public void EquipFushion ()
    {
        StringBuilder str = new StringBuilder(selectedMaterial[1].InventoryIndex.ToString());
        str.Append('|');
        str.Append(selectedMaterial[2].InventoryIndex.ToString());
        str.Append('|');
        str.Append(selectedMaterial[3].InventoryIndex.ToString());
        str.Append('|');
        str.Append(selectedMaterial[4].InventoryIndex.ToString());

        RPCFactory.NonCombatRPC.EquipFushion(selectedMaterial[0].InventoryIndex, str.ToString());
    }
    
    public void FinishedFushion ()
    {
        for (int i = 0; i < 6; ++i)
        {
            ClearSelectButton(i);
        }
    }

    public static void RefreshFushion()
    {
        if (mine != null)
        {
            mine.FinishedFushion();
            mine.GetInventoryItem();
        }
    }

    public void NotEnoughMaterial ()
    {

    }
    #endregion
    
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
                selectedMaterial[0] = new SelectItemData(-1, -1, null);
                addObject[0].gameObject.SetActive(false);
                addButton[0].gameObject.SetActive(true);
                compeletStep[0] = false;
                equipmentName.text = string.Empty;
                equipmentUpgradeValue.text = string.Empty;
                equipmentEvolveValue.text = string.Empty;
                equipmentRank.text = string.Empty;
                for (int i = 0; i < 3; ++i)
                {
                    equipmentEffect[i].GetChild(0).GetComponent<Text>().text = string.Empty;
                    equipmentEffect[i].GetChild(1).GetComponent<Text>().text = string.Empty;
                }
                break;
            case 2:
                selectedMaterial[1] = new SelectItemData(-1, -1, null);
                addObject[1].gameObject.SetActive(false);
                addButton[1].gameObject.SetActive(true);
                compeletStep[1] = false;
                break;
            case 3:
                selectedMaterial[2] = new SelectItemData(-1, -1, null);
                addObject[2].gameObject.SetActive(false);
                addButton[2].gameObject.SetActive(true);
                compeletStep[2] = false;
                for (int i = 4; i < 10; ++i)
                {
                    gemEffect[0].GetChild(i).GetComponent<Text>().text = string.Empty;
                }
                break;
            case 4:
                selectedMaterial[3] = new SelectItemData(-1, -1, null);
                addObject[3].gameObject.SetActive(false);
                addButton[3].gameObject.SetActive(true);
                compeletStep[3] = false;
                for (int i = 4; i < 10; ++i)
                {
                    gemEffect[1].GetChild(i).GetComponent<Text>().text = string.Empty;
                }
                break;
            case 5:
                selectedMaterial[4] = new SelectItemData(-1, -1, null);
                addObject[4].gameObject.SetActive(false);
                addButton[4].gameObject.SetActive(true);
                compeletStep[4] = false;
                for (int i = 4; i < 10; ++i)
                {
                    gemEffect[2].GetChild(i).GetComponent<Text>().text = string.Empty;
                }
                break;
            default:
                break;
        }
    }

    void NotEnoughItem ()
    {

    }
}
