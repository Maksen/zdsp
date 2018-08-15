using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using Zealot.Common;
using Zealot.Client.Entities;
using Zealot.Repository;
using Kopio.JsonContracts;

public class EquipmentModdingRow : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject   equipIconPrefab;
    public Transform    equipIconParent;

    [Header("Prefabs")]
    public int rowSize = 1;

    // Private variables
    private List<GameObject> _equipIconList;

    public void Init()
    {
        ClearEquipIconsList();

        for(int i = 0; i < rowSize; ++i)
        {
            GameObject newEquipIconObj = Instantiate(equipIconPrefab);
            newEquipIconObj.transform.SetParent(equipIconParent, false);
            newEquipIconObj.SetActive(false);

            _equipIconList.Add(newEquipIconObj);
        }
    }

    public void AddData(UI_EquipmentUpgrade uiEquipUpgrade, int slotId, Equipment equipment, bool isSafeUpgrade, bool isEquipped, ToggleGroup toggleGroup)
    {
        if(IsFull())
        {
            return;
        }

        int newIconPos = GetNewIcon();

        if(newIconPos != -1 || (newIconPos >= 0 && newIconPos < _equipIconList.Count))
        {
            GameObject equipIconObj = _equipIconList[newIconPos];
            GameIcon_Equip equipIcon = equipIconObj.GetComponent<GameIcon_Equip>();
            UpdateIconData(equipIcon, uiEquipUpgrade, slotId, equipment, isSafeUpgrade, isEquipped, toggleGroup);
        }
    }

    public void UpdateData(int pos, UI_EquipmentUpgrade uiEquipUpgrade, int slotId, Equipment equipment, bool isSafeUpgrade, bool isEquipped, ToggleGroup toggleGroup)
    {
        if(pos < 0 || pos >= _equipIconList.Count)
        {
            return;
        }

        GameIcon_Equip equipIcon = _equipIconList[pos].GetComponent<GameIcon_Equip>();
        UpdateIconData(equipIcon, uiEquipUpgrade, slotId, equipment, isSafeUpgrade, isEquipped, toggleGroup);
    }

    public void AddRefresh(UI_EquipmentUpgrade uiEquipUpgrade, int slotId, Equipment equipment, bool isSafeUpgrade, bool isEquipped, ToggleGroup toggleGroup)
    {
        if (IsFull())
        {
            return;
        }

        int newIconPos = GetNewIcon();

        if (newIconPos != -1 || (newIconPos >= 0 && newIconPos < _equipIconList.Count))
        {
            GameObject equipIconObj = _equipIconList[newIconPos];
            GameIcon_Equip equipIcon = equipIconObj.GetComponent<GameIcon_Equip>();
            UpdateIconData(equipIcon, uiEquipUpgrade, slotId, equipment, isSafeUpgrade, isEquipped, toggleGroup);
        }
    }

    public void AddData(UI_EquipmentReform uiEquipReform, bool isReform, int slotId, Equipment equipment, bool isEquipped, ToggleGroup toggleGroup)
    {
        if (IsFull())
        {
            return;
        }

        int newIconPos = GetNewIcon();

        if (newIconPos != -1 || (newIconPos >= 0 && newIconPos < _equipIconList.Count))
        {
            GameObject equipIconObj = _equipIconList[newIconPos];
            GameIcon_Equip equipIcon = equipIconObj.GetComponent<GameIcon_Equip>();
            UpdateIconData(equipIcon, uiEquipReform, isReform, slotId, equipment, isEquipped, toggleGroup);
        }
    }

    public void UpdateData(int pos, UI_EquipmentReform uiEquipReform, bool isReform, int slotId, Equipment equipment, bool isEquipped, ToggleGroup toggleGroup)
    {
        if (pos < 0 || pos >= _equipIconList.Count)
        {
            return;
        }

        GameIcon_Equip equipIcon = _equipIconList[pos].GetComponent<GameIcon_Equip>();
        UpdateIconData(equipIcon, uiEquipReform, isReform, slotId, equipment, isEquipped, toggleGroup);
    }

    public void AddRefresh(UI_EquipmentReform uiEquipReform, bool isReform, int slotId, Equipment equipment, bool isEquipped, ToggleGroup toggleGroup)
    {
        if (IsFull())
        {
            return;
        }

        int newIconPos = GetNewIcon();

        if (newIconPos != -1 || (newIconPos >= 0 && newIconPos < _equipIconList.Count))
        {
            GameObject equipIconObj = _equipIconList[newIconPos];
            GameIcon_Equip equipIcon = equipIconObj.GetComponent<GameIcon_Equip>();
            UpdateIconData(equipIcon, uiEquipReform, isReform, slotId, equipment, isEquipped, toggleGroup);
        }
    }

    public bool IsFull()
    {
        bool isFull = true;

        for (int i = 0; i < _equipIconList.Count; ++i)
        {
            GameObject equipIconObj = _equipIconList[i];
            if(equipIconObj.activeSelf == false)
            {
                isFull = false;

                break;
            }
        }

        return isFull;
    }

    private int GetNewIcon()
    {
        int pos = 0;

        if(_equipIconList == null)
        {
            return -1;
        }

        for(int i = 0; i < rowSize; ++i)
        {
            if(_equipIconList[i].activeSelf == false)
            {
                _equipIconList[i].SetActive(true);
                pos = i;

                break;
            }
        }

        return pos;
    }

    private void UpdateIconData(GameIcon_Equip equipIcon, UI_EquipmentUpgrade uiEquipUpgrade, int slotId, Equipment equipment, bool isSafeUpgrade, bool isEquipped, ToggleGroup toggleGroup)
    {
        ModdingEquipment equipToUpgrade = new ModdingEquipment(slotId, equipment, isEquipped);

        if (equipIcon != null)
        {
            if (!isSafeUpgrade)
            {
                equipIcon.Init(equipment.ItemID, 0, equipment.ReformStep, equipment.UpgradeLevel, false, false, true);
                GameIconCmpt_SelectCheckmark selectCheckMark = equipIcon.GetToggleSelect();
                if (selectCheckMark != null)
                {
                    Toggle checkmarkToggle = selectCheckMark.GetToggleSelect();
                    checkmarkToggle.onValueChanged.RemoveAllListeners();
                    checkmarkToggle.onValueChanged.AddListener(delegate
                    {
                        uiEquipUpgrade.OnClickSelectUpgradeEquipment(checkmarkToggle.isOn, isEquipped, equipToUpgrade);
                    });
                    checkmarkToggle.group = toggleGroup;
                }
            }
            else
            {
                equipIcon.Init(equipment.ItemID, 0, equipment.ReformStep, equipment.UpgradeLevel, false, false, true);
                GameIconCmpt_SelectCheckmark selectCheckMark = equipIcon.GetToggleSelect();
                if (selectCheckMark != null)
                {
                    Toggle checkmarkToggle = selectCheckMark.GetToggleSelect();
                    checkmarkToggle.onValueChanged.AddListener(delegate
                    {
                        uiEquipUpgrade.OnClickSelectSafeUpgradeEquipment(checkmarkToggle.isOn, isEquipped, equipToUpgrade);
                    });
                    checkmarkToggle.group = toggleGroup;
                }
            }
        }
    }

    private void UpdateIconData(GameIcon_Equip equipIcon, UI_EquipmentReform uiEquipReform, bool isReform, int slotId, Equipment equipment, bool isEquipped, ToggleGroup toggleGroup)
    {
        ModdingEquipment equipToMod = new ModdingEquipment(slotId, equipment, isEquipped);

        if (equipIcon != null)
        {
            equipIcon.Init(equipment.ItemID, 0, equipment.ReformStep, equipment.UpgradeLevel, false, false, true);
            GameIconCmpt_SelectCheckmark selectCheckMark = equipIcon.GetToggleSelect();
            if (selectCheckMark != null)
            {
                Toggle checkmarkToggle = selectCheckMark.GetToggleSelect();
                if(isReform)
                {
                    checkmarkToggle.onValueChanged.AddListener(delegate
                    {
                        uiEquipReform.OnClickSelectReformEquipment(checkmarkToggle.isOn, isEquipped, equipToMod);
                    });
                }
                else
                {
                    checkmarkToggle.onValueChanged.AddListener(delegate
                    {
                        uiEquipReform.OnClickSelectRecycleEquipment(checkmarkToggle.isOn, isEquipped, equipToMod);
                    });
                }
                checkmarkToggle.group = toggleGroup;
            }
        }
    }

    public void Clear()
    {
        //ClearEquipIconsList();
        ClearRefresh();
    }

    public void ClearRefresh()
    {
        if(_equipIconList == null)
        {
            _equipIconList = new List<GameObject>();
            return;
        }

        for(int i = 0; i < _equipIconList.Count; ++i)
        {
            GameIcon_Equip equipIcon = _equipIconList[i].GetComponent<GameIcon_Equip>();
            if(equipIcon != null)
            {
                GameIconCmpt_SelectCheckmark selectCheckMark = equipIcon.GetToggleSelect();
                if (selectCheckMark != null)
                {
                    Toggle checkmarkToggle = selectCheckMark.GetToggleSelect();
                    checkmarkToggle.onValueChanged.RemoveAllListeners();
                }
            }

            _equipIconList[i].SetActive(false);
        }
    }

    private void ClearEquipIconsList()
    {
        if(_equipIconList == null)
        {
            _equipIconList = new List<GameObject>();
            return;
        }

        for(int i = 0; i < _equipIconList.Count; ++i)
        {
            Destroy(_equipIconList[i]);
            _equipIconList[i] = null;
        }
        _equipIconList.Clear();
    }
}