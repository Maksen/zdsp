using Kopio.JsonContracts;
using System;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Repository;

public class Achievement_FunctionData : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text descriptionText;
    [SerializeField] Image iconImage;
    [SerializeField] string grayColorHex;

    private Color grayColor;
    private Toggle toggle;
    private LISAExternalFunctionJson json;
    private Action<int> SelectedCallback;

    private void Awake()
    {
        ColorUtility.TryParseHtmlString(grayColorHex, out grayColor);
        toggle = GetComponent<Toggle>();
    }

    public void Init(LISAExternalFunctionJson data, ToggleGroup toggleGroup, bool unlocked, Action<int> callback)
    {
        json = data;
        toggle.group = toggleGroup;
        SelectedCallback = callback;
        if (data.triggertype == LISAFunctionTriggerType.AchievementLV)
        {
            AchievementLevel levelInfo = AchievementRepo.GetAchievementLevelInfo(data.triggervalue);
            if (levelInfo != null)
                nameText.text = levelInfo.name;
            else
                nameText.text = "???";
        }
        else
        {
            AchievementObjective obj = AchievementRepo.GetAchievementObjectiveById(data.triggervalue);
            if (obj != null)
                nameText.text = obj.localizedName;
            else
                nameText.text = "???";
        }
        descriptionText.text = data.localizeddescription.Replace("{value}", data.functionvalue.ToString());
        iconImage.sprite = ClientUtils.LoadIcon(data.iconpath);
        if (!unlocked)
        {
            nameText.color = grayColor;
            descriptionText.color = grayColor;
        }
    }

    public void OnToggle(bool isOn)
    {
        if (isOn)
        {
            if (SelectedCallback != null)
                SelectedCallback(json.triggervalue);
        }
    }
}