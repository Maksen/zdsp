using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using UIAddons;

public class DefaultToggleInGroup : MonoBehaviour
{
    [SerializeField]
    List<Toggle> list_toggle;
    [SerializeField]
    bool defaultAutoEnable = true;
    [SerializeField]
    Toggle defaultToggle;

    private bool enableDefault = true;
    private ToggleGroup toggleGroup;
    private bool allowSwitchOff = false;

    void Awake()
    {
        if (list_toggle.Count > 0)
        {
            toggleGroup = list_toggle[0].group;
            if (toggleGroup != null)
                allowSwitchOff = toggleGroup.allowSwitchOff;
        }
    }

    void OnEnable()
    {
        if (defaultAutoEnable && defaultToggle != null)
        {
            enableDefault = true;
            StartCoroutine(LateEnable());
        }
    }

    IEnumerator LateEnable()
    {
        yield return null;
        if (enableDefault)
            defaultToggle.isOn = true;
    }

    void OnDisable()
    {
        if (toggleGroup != null)
            toggleGroup.allowSwitchOff = true;
        foreach (var toggle in list_toggle)
        {
            toggle.isOn = false;
        }
        if (toggleGroup != null)
            toggleGroup.allowSwitchOff = allowSwitchOff;
    }

    public void GoToPage(int index)
    {
        enableDefault = false;
        if (toggleGroup != null)
            toggleGroup.allowSwitchOff = true;
        for (int local_index = 0; local_index < list_toggle.Count; local_index++)
        {
            if (local_index != index)
            {
                list_toggle[local_index].isOn = false;
                CustomToggle customToggle = list_toggle[local_index].GetComponent<CustomToggle>();
                if (customToggle != null)
                    customToggle.OnValueChanged(false);
            }
            else
            {
                list_toggle[local_index].isOn = true;
            }
        }
        if (toggleGroup != null)
            toggleGroup.allowSwitchOff = allowSwitchOff;
    }
}
