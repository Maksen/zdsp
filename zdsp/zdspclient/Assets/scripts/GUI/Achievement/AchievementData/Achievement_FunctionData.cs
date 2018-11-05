using Kopio.JsonContracts;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Repository;

public class Achievement_FunctionData : MonoBehaviour
{
    [SerializeField] protected Text nameText;
    [SerializeField] Text descriptionText;
    [SerializeField] Image iconImage;
    [SerializeField] string grayColorHex;

    private Color origNameColor;
    private Color origDescColor;
    private Color grayColor;
    private Toggle toggle;
    protected int triggerValue;

    private void Awake()
    {
        origNameColor = nameText.color;
        origDescColor = descriptionText.color;
        ColorUtility.TryParseHtmlString(grayColorHex, out grayColor);
        toggle = GetComponent<Toggle>();
    }

    public virtual void Init(LISAExternalFunctionJson data, ToggleGroup toggleGroup, bool unlocked)
    {
        toggle.group = toggleGroup;
        triggerValue = data.triggervalue;

        if (data.triggertype == LISAFunctionTriggerType.AchievementLV)
        {
            AchievementLevel levelInfo = AchievementRepo.GetAchievementLevelInfo(data.triggervalue);
            nameText.text = levelInfo != null ? levelInfo.name : "???";
        }

        descriptionText.text = data.localizeddescription.Replace("{value}", data.functionvalue.ToString());
        iconImage.sprite = ClientUtils.LoadIcon(data.iconpath);
        SetUnlocked(unlocked);
    }

    public virtual void Refresh()
    {
        SetUnlocked(GameInfo.gLocalPlayer.PlayerSynStats.AchievementLevel >= triggerValue);
    }

    public void SetUnlocked(bool unlocked)
    {
        nameText.color = unlocked ? origNameColor: grayColor;
        descriptionText.color = unlocked ? origDescColor : grayColor;
        iconImage.color = unlocked ? Color.white : grayColor;
    }

    public void SetToggleOn(bool value)
    {
        if (toggle.isOn != value)
            toggle.isOn = value;
    }
}