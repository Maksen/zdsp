using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Repository;

public class UI_Party_SettingsDialog : BaseWindowBehaviour
{
    [SerializeField] ComboBoxA locationComboBox;
    [SerializeField] InputField minLevelInputField;
    [SerializeField] InputField maxLevelInputField;
    [SerializeField] ComboBoxA autoAcceptComboBox;
    [SerializeField] InputField notesInputField;

    private PartySetting currentSettings;
    private int MAX_LEVEL;

    private void Awake()
    {
        MAX_LEVEL = CharacterLevelRepo.GetMaxLevel();

        // setup location combobox
        locationComboBox.AddItem(PartyRepo.GetLocationName(0), "0");  // nearby
        for (LocationType type = LocationType.Dungeon; type <= LocationType.PVP; type++)
        {
            var list = PartyRepo.GetLocationsByType(type);
            if (list != null)
            {
                for (int i = 0; i < list.Count; i++)
                    locationComboBox.AddItem(list[i].localizedName, list[i].locationId.ToString());
            }
        }
    }

    private void Start()
    {
        minLevelInputField.onValidateInput += delegate (string input, int charIndex, char addedChar) { return ValidatePositiveInteger(addedChar); };
        maxLevelInputField.onValidateInput += delegate (string input, int charIndex, char addedChar) { return ValidatePositiveInteger(addedChar); };
    }

    private char ValidatePositiveInteger(char charToValidate)
    {
        //Checks if non-digit is entered
        if (charToValidate < '0' || charToValidate > '9')
        {
            // if it is change it to an empty character.
            charToValidate = '\0';
        }
        return charToValidate;
    }

    public void ValidateMinLevelInputField(string str)
    {
        if (!string.IsNullOrEmpty(str))
        {
            int inputLvl;
            if (int.TryParse(str, out inputLvl))
            {
                if (inputLvl <= 0)
                    minLevelInputField.text = "1";
                else if (inputLvl > MAX_LEVEL)
                    minLevelInputField.text = MAX_LEVEL.ToString();
            }
        }
    }

    public void ValidateMaxLevelInputField(string str)
    {
        if (!string.IsNullOrEmpty(str))
        {
            int inputLvl;
            if (int.TryParse(str, out inputLvl))
            {
                if (inputLvl <= 0)
                    maxLevelInputField.text = "1";
                else if (inputLvl > MAX_LEVEL)
                    maxLevelInputField.text = MAX_LEVEL.ToString();
            }
        }
    }

    public override void OnOpenWindow()
    {
        base.OnOpenWindow();
        if (GameInfo.gLocalPlayer.PartyStats != null)
        {
            currentSettings = GameInfo.gLocalPlayer.PartyStats.mPartySetting;
            StartCoroutine(LateInit(currentSettings));
        }
    }

    private IEnumerator LateInit(PartySetting settings)
    {
        yield return null;
        Init(settings);
    }

    public void Init(PartySetting settings)
    {
        locationComboBox.SelectedValue = settings.locationId.ToString();
        minLevelInputField.text = settings.minLevel.ToString();
        maxLevelInputField.text = settings.maxLevel.ToString();
        autoAcceptComboBox.SelectedIndex = (int)settings.autoAcceptType;
        notesInputField.text = settings.notes;
    }

    public void OnClickConfirm()
    {
        int locationId;
        int.TryParse(locationComboBox.SelectedValue, out locationId);
        int minLvl;
        int.TryParse(minLevelInputField.text, out minLvl);
        int maxLvl;
        int.TryParse(maxLevelInputField.text, out maxLvl);
        if (minLvl > maxLvl)  // swap if min is more and max
        {
            int temp = maxLvl;
            maxLvl = minLvl;
            minLvl = temp;
        }
        AutoAcceptType acceptType = (AutoAcceptType)autoAcceptComboBox.SelectedIndex;
        string notes = notesInputField.text;

        PartySetting newSettings = new PartySetting(locationId, minLvl, maxLvl, acceptType, notes);
        if (newSettings != currentSettings)
            RPCFactory.CombatRPC.ChangePartySetting(newSettings.ToString());

        GetComponent<UIDialog>().ClickClose();
    }

}
