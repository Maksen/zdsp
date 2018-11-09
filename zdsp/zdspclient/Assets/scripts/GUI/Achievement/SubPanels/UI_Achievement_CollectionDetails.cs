using UIAddons;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;

public class UI_Achievement_CollectionDetails : MonoBehaviour
{
    [SerializeField] ToggleGroup toggleGroup;
    [SerializeField] Toggle[] toggles;

    private bool allowSwitchOff;

    private void Awake()
    {
        allowSwitchOff = toggleGroup.allowSwitchOff;
        for (int i = 0; i < toggles.Length; ++i)
        {
            CollectionType mainType = (CollectionType)i;
            toggles[i].onValueChanged.AddListener((isOn) => OnClickCollectionType(isOn, mainType));
        }
    }

    private void OnClickCollectionType(bool isOn, CollectionType mainType)
    {
        if (isOn)
        {
            UIManager.OpenDialog(WindowType.DialogCollection,
                (window) => window.GetComponent<UI_Achievement_CollectionDialog>().GoToTab(mainType, this));
        }
    }

    public void SetToggleOn(int index)
    {
        toggleGroup.allowSwitchOff = true;
        for (int i = 0; i < toggles.Length; ++i)
        {
            Toggle toggle = toggles[i];
            toggle.onValueChanged.RemoveAllListeners();
            bool on = (i == index);
            toggle.isOn = on;
            CustomToggle customToggle = toggle.GetComponent<CustomToggle>();
            if (customToggle != null)
                customToggle.OnValueChanged(on);
            CollectionType mainType = (CollectionType)i;
            toggle.onValueChanged.AddListener((isOn) => OnClickCollectionType(isOn, mainType));
        }
        toggleGroup.allowSwitchOff = allowSwitchOff;
    }

    public void CleanUp()  // called by back button or when close window
    {
        toggleGroup.allowSwitchOff = true;
        for (int i = 0; i < toggles.Length; ++i)
        {
            toggles[i].isOn = false;
            CustomToggle customToggle = toggles[i].GetComponent<CustomToggle>();
            if (customToggle != null)
                customToggle.OnValueChanged(false);
        }
        toggleGroup.allowSwitchOff = allowSwitchOff;
    }
}