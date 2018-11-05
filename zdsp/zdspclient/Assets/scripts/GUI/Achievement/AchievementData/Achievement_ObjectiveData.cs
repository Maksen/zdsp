using System;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Repository;

public class Achievement_ObjectiveData : Achievement_ObjectiveDataBase
{
    [SerializeField] Text progressText;
    [SerializeField] GameObject infoButtonObj;
    [SerializeField] Text infoTooltipText;
    [SerializeField] Toggle toggle;

    private Action<bool, int> OnToggleCallback;
    private int index;

    protected override void Awake()
    {
        base.Awake();
        toggle.onValueChanged.AddListener(OnToggle);
    }

    public void Init(AchievementInfo info, ToggleGroup toggleGroup, int idx, Action<bool, int> callback)
    {
        if (toggle.group == null)
            toggle.group = toggleGroup;

        index = idx;
        OnToggleCallback = callback;

        Init(info.objective, info.count);
        AchievementObjective obj = info.objective;
        if (obj.rewardFunction == LISAFunction.None)
            infoButtonObj.SetActive(false);
        else
        {
            infoButtonObj.SetActive(true);
            infoTooltipText.text = obj.rewardFunctionDesc.Replace("{value}", obj.rewardFunctionValue.ToString());
        }
        float progress = (float)info.count / info.objective.completeCount;
        progressText.text = progress.ToString("P0");
    }

    public void SetToggleOn(bool value)
    {
        toggle.onValueChanged.RemoveListener(OnToggle);
        if (toggle.isOn != value)
            toggle.isOn = value;
        toggle.onValueChanged.AddListener(OnToggle);
    }

    public void OnToggle(bool isOn)
    {
        if (OnToggleCallback != null)
            OnToggleCallback(isOn, index);
    }
}