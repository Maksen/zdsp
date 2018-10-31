using Kopio.JsonContracts;
using System;
using UnityEngine;
using UnityEngine.UI;

public class Achievement_SubTypeData : MonoBehaviour
{
    [SerializeField] Text titleText;

    private Toggle toggle;
    private int subType;
    private Action<int> OnSelectCallback;

    private void Awake()
    {
        toggle = GetComponent<Toggle>();
    }

    public void Init(AchievementSubTypeJson data, ToggleGroup toggleGroup, Action<int> callback)
    {
        subType = data.id;
        toggle.group = toggleGroup;
        OnSelectCallback = callback;
        titleText.text = data.localizedname.Substring(0, 1);
    }

    public void OnToggle(bool isOn)
    {
        if (isOn)
        {
            if (OnSelectCallback != null)
                OnSelectCallback(subType);
        }
    }

    public void SetToggleOn(bool isOn)
    {
        toggle.isOn = isOn;
    }
}