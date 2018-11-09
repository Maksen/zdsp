using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Repository;

public class UI_Achievement_ObjectivePanel : MonoBehaviour
{
    [SerializeField] Text subTypeNameText;
    [SerializeField] Transform subTypeDataParent;
    [SerializeField] GameObject subTypeDataPrefab;
    [SerializeField] ToggleGroup subTypeToggleGroup;
    [SerializeField] ComboBoxA filterComboBox;
    [SerializeField] AchievementListScrollView achScrollView;

    private List<AchievementObjective> objectivesList = new List<AchievementObjective>();
    private int lastFilterIndex = -1;
    private List<AchievementInfo> displayList = new List<AchievementInfo>();
    private AchievementStatsClient achStats;

    public void Init(AchievementType mainType)
    {
        achStats = GameInfo.gLocalPlayer.AchievementStats;
        CleanUp();

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

        if (lastFilterIndex == -1)
            filterComboBox.SelectedIndex = 0;

        if (subTypeDataParent.childCount > 0)
            subTypeDataParent.GetChild(0).GetComponent<Achievement_SubTypeData>().SetToggleOn(true);
    }

    private void OnClickSubType(int subType)
    {
        objectivesList.Clear();

        var data = AchievementRepo.GetAchievementSubTypeByID(subType);
        if (data != null)
        {
            subTypeNameText.text = data.localizedname;
            Dictionary<int, List<AchievementObjective>> objListByGrp = AchievementRepo.GetAchievementObjectivesGroupsBySubType(subType);
            if (objListByGrp != null && objListByGrp.Values != null)
            {
                foreach (var list in objListByGrp.Values)
                {
                    AchievementObjective last;
                    AchievementObjective next;
                    achStats.GetLastCompletedAndNextAchievement(list, out last, out next);
                    if (last != null)
                        objectivesList.Add(last);
                    if (next != null)
                        objectivesList.Add(next);
                }
            }
        }
        else
            subTypeNameText.text = "?";

        PopulateObjectiveList(filterComboBox.SelectedIndex);
    }

    public void PopulateObjectiveList(int filterIndex)
    {
        displayList.Clear();

        for (int i = 0; i < objectivesList.Count; ++i)
        {
            AchievementObjective obj = objectivesList[i];

            int count = 0;
            bool isCompleted = false;
            AchievementElement elem = achStats.GetAchievementById(obj.id);
            if (elem != null)
            {
                count = elem.Count;
                isCompleted = elem.IsCompleted();
            }

            if (filterIndex == 1 && !isCompleted) // filter completed but not completed
                continue;
            else if (filterIndex == 2 && isCompleted)  // filter uncompleted but completed
                continue;

            AchievementInfo info = new AchievementInfo(obj, count);
            displayList.Add(info);
        }

        if (filterIndex == 0)
            displayList = displayList.OrderBy(x => x, new AchievementInfoComparer()).ToList();

        achScrollView.Populate(displayList);
    }

    private class AchievementInfoComparer : IComparer<AchievementInfo>
    {
        public int Compare(AchievementInfo x, AchievementInfo y)
        {
            if (!x.IsCompleted() && y.IsCompleted())
                return -1;
            else if (x.IsCompleted() && !y.IsCompleted())
                return 1;
            else
                return 0;
        }
    }

    public void OnFilterSelectionChanged(int index)
    {
        lastFilterIndex = index;
        PopulateObjectiveList(index);
    }

    public void CleanUp()
    {
        ClientUtils.DestroyChildren(subTypeDataParent);
        achScrollView.Clear();
        displayList.Clear();
    }
}