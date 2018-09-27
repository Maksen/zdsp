using Kopio.JsonContracts;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Zealot.Common;
using Zealot.Repository;

public class ReformBagData : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject equipIconPrefab;
    public Transform equipIconParent;

    [Header("Toggles")]
    public Toggle selectToggle;

    [Header("Text")]
    public Text reformStatsTxt;

    // Private variables
    private int _selection;
    private List<GameObject> _materialList;

    public void Init(UI_EquipmentReform uiEquipReform, int selection, ToggleGroup reformSelGrp, int reformStep, EquipmentReformGroupJson reformData)
    {
        _selection = selection;

        //selectToggle.onValueChanged.RemoveAllListeners();
        selectToggle.onValueChanged.AddListener(delegate
        {
            uiEquipReform.OnClickSelectReformStep(_selection, selectToggle.isOn);
        });
        selectToggle.group = reformSelGrp;

        // Reform Stats
        EquipReformData equipReformData = new EquipReformData(reformStep, reformData);
        reformStatsTxt.text = uiEquipReform.FormatSideEffectString(reformStep, equipReformData.GetSideEffects());

        // Reform Materials
        List<ItemInfo> materialList = EquipmentModdingRepo.GetModdingMaterialsFromStr(reformData.requirement);

        ClearMaterialList();

        if(materialList != null)
        {
            for(int i = 0; i < materialList.Count; ++i)
            {
                ItemInfo matData = materialList[i];
                if(matData != null)
                {
                    GameObject equipIconObj = Instantiate(equipIconPrefab);
                    equipIconObj.transform.SetParent(equipIconParent, false);

                    GameIcon_Equip equipIcon = equipIconObj.GetComponent<GameIcon_Equip>();
                    equipIcon.InitWithoutCallback(matData.itemId, 0, 0, 0);

                    _materialList.Add(equipIconObj);
                }
            }
        }
    }

    private void ClearMaterialList()
    {
        if(_materialList == null)
        {
            _materialList = new List<GameObject>();

            return;
        }

        for(int i = 0; i < _materialList.Count; ++i)
        {
            Destroy(_materialList[i]);
            _materialList[i] = null;
        }
        _materialList.Clear();
    }
}
