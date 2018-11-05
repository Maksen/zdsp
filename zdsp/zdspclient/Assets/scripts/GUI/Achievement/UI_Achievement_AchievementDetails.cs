using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;

public class UI_Achievement_AchievementDetails : MonoBehaviour
{
    [SerializeField] ToggleGroup toggleGroup;
    [SerializeField] Toggle[] toggles;
    [SerializeField] UI_AchievementObjectivePanel objectivesPanel;

    private void Start()
    {
        for (int i = 0; i < toggles.Length; ++i)
        {
            AchievementType mainType = (AchievementType)i;
            toggles[i].onValueChanged.AddListener((isOn) => OnClickAchievementType(isOn, mainType));
        }
    }

    private void OnClickAchievementType(bool isOn, AchievementType mainType)
    {
        if (isOn)
        {
            objectivesPanel.gameObject.SetActive(true);
            objectivesPanel.Init(mainType);
        }
    }

    public void CleanUp()  // called by back button or when close window
    {
        bool allowSwitchOff = toggleGroup.allowSwitchOff;
        toggleGroup.allowSwitchOff = true;
        for (int i = 0; i < toggles.Length; ++i)
            toggles[i].isOn = false;
        toggleGroup.allowSwitchOff = allowSwitchOff;

        objectivesPanel.CleanUp();
        objectivesPanel.gameObject.SetActive(false);
    }
}