using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Repository;

public class UI_AchievementObjectivePanel : MonoBehaviour
{
    [SerializeField] Text subTypeNameText;
    [SerializeField] Transform subTypeDataParent;
    [SerializeField] GameObject subTypeDataPrefab;
    [SerializeField] ToggleGroup subTypeToggleGroup;
    [SerializeField] ComboBoxA filterComboBox;

    private List<AchievementObjective> objectivesList;

    public void Init(AchievementType mainType)
    {
        ClientUtils.DestroyChildren(subTypeDataParent);
        var subTypeList = AchievementRepo.GetAchievementSubTypesByMainType(mainType);
        for (int i = 0; i < subTypeList.Count; ++i)
        {
            GameObject obj = ClientUtils.CreateChild(subTypeDataParent, subTypeDataPrefab);
            obj.GetComponent<Achievement_SubTypeData>().Init(subTypeList[i], subTypeToggleGroup, OnClickSubType);
        }
        StartCoroutine(SelectFirstSubType());
    }

    private IEnumerator SelectFirstSubType()
    {
        yield return null;
        if (subTypeDataParent.childCount > 0)
            subTypeDataParent.GetChild(0).GetComponent<Achievement_SubTypeData>().SetToggleOn(true);
    }

    private void OnClickSubType(int subType)
    {
        var data = AchievementRepo.GetAchievementSubTypeByID(subType);
        if (data != null)
        {
            subTypeNameText.text = data.localizedname;
            objectivesList = AchievementRepo.GetAchievementObjectivesBySubType(subType);
        }
        else
        {
            subTypeNameText.text = "";
            objectivesList.Clear();
        }
        PopulateObjectiveList(filterComboBox.SelectedIndex);
    }

    public void PopulateObjectiveList(int filterIndex)
    {
    }

    public void CleanUp()
    {
        ClientUtils.DestroyChildren(subTypeDataParent);
    }
}