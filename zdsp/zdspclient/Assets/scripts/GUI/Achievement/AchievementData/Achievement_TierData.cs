using Kopio.JsonContracts;
using System;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Repository;

public class Achievement_TierData : MonoBehaviour
{
    [SerializeField] Image iconImage;
    [SerializeField] string lockedColorHex;
    [SerializeField] Text versionText;
    [SerializeField] Text tierNameText;
    [SerializeField] Text tierDescText;

    private Toggle toggle;
    private Action<int> OnSelectedCallback;
    private int tier;
    private Color lockedColor;

    private void Awake()
    {
        toggle = GetComponent<Toggle>();
        ColorUtility.TryParseHtmlString(lockedColorHex, out lockedColor);
    }

    public void Init(LISATransformTierJson data, ToggleGroup toggleGrp, Action<int> callback)
    {
        tier = data.tierid;
        toggle.group = toggleGrp;
        OnSelectedCallback = callback;

        AchievementLevel achLevel = AchievementRepo.GetAchievementLevelInfo(data.reqlvl);
        if (achLevel != null)
            versionText.text = "Ver " + achLevel.name;

        tierNameText.text = data.localizedname;
        tierDescText.text = data.localizeddescription;
        iconImage.sprite = ClientUtils.LoadIcon(data.iconpath);
    }

    public void SetUnlocked(bool unlocked)
    {
        iconImage.color = unlocked ? Color.white : lockedColor;
        toggle.interactable = unlocked;
    }

    public void EnableToggleCallback(bool value)
    {
        if (value)
            toggle.onValueChanged.AddListener(OnToggled);
        else
            toggle.onValueChanged.RemoveListener(OnToggled);
    }

    private void OnToggled(bool isOn)
    {
        if (isOn)
        {
            if (OnSelectedCallback != null)
                OnSelectedCallback(tier);
        }
    }

    public void SetToggleOn(bool value)
    {
        toggle.isOn = value;
    }
}