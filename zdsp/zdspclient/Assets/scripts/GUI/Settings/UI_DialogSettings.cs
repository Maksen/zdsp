using UnityEngine;

enum SettingsTab : byte
{
    Game = 0,
    Account
}

public class UI_DialogSettings : MonoBehaviour
{
    [Header("Tab Content")]
    [SerializeField]
    UI_DialogSettings_Game gameSettings = null;
    [SerializeField]
    UI_DialogSettings_Account accountSettings = null;

    int settingsTabIndex = -1;

    void OnDisable()
    {
        settingsTabIndex = -1;
    }

    public void OnValueChangedToggleSettings(int index)
    {
        if (settingsTabIndex == index)
            return;

        settingsTabIndex = index;
        SettingsTab settingsTab = (SettingsTab)index;
        switch (settingsTab)
        {
            case SettingsTab.Game:
                gameSettings.Init();
                break;
            case SettingsTab.Account:
                accountSettings.Init();
                break;
        }
    }
}
