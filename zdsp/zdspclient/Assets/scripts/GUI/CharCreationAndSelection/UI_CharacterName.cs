using System;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Repository;

public class UI_CharacterName : MonoBehaviour
{
    [SerializeField]
    Button Confirm;

    [SerializeField]
    InputField Name;

    [SerializeField]
    Text WarningMessage;

    [SerializeField]
    Image Valid;

    [SerializeField]
    Sprite Tick;

    [SerializeField]
    Sprite Cross;

    private UI_CharacterCreation mParent;
    private string mName;

    public void Init(UI_CharacterCreation parent)
    {
        mParent = parent;
        mName = Name.text = CharacterCreationRepo.GetRandomName();
        NameCheck();
    }

    public void OnClickRandom()
    {
        WarningMessage.gameObject.SetActive(false);
        mName = Name.text = CharacterCreationRepo.GetRandomName();
        NameCheck();
    }

    public void OnEndEdit(string value)
    {
        mName = value;
        NameCheck();
    }

    private void NameCheck()
    {
        bool invalidname = false;
        Regex regex = new Regex(@"^[\p{IsCJKUnifiedIdeographs}0-9A-Za-z]+$");

        int namelength = GetNameLength(mName);

        if (namelength < 4 || namelength > 16)
        {
            invalidname = true;
            WarningMessage.gameObject.SetActive(true);
            WarningMessage.text = GUILocalizationRepo.GetLocalizedString("charactercreation_shortorlong");
        }
        else if (WordFilterRepo.CheckString(mName, WordFilterType.Naming))
        {
            invalidname = true;
            WarningMessage.gameObject.SetActive(true);
            WarningMessage.text = GUILocalizationRepo.GetLocalizedString("charactercreation_forbiddenword");
        }
        else if (mName.Length > 0 && Char.IsDigit(mName[0]))
        {
            invalidname = true;
            WarningMessage.gameObject.SetActive(true);
            WarningMessage.text = GUILocalizationRepo.GetLocalizedString("charactercreation_digitstart");
        }
        else if (!regex.IsMatch(mName))
        {
            invalidname = true;
            WarningMessage.gameObject.SetActive(true);
            WarningMessage.text = GUILocalizationRepo.GetLocalizedString("charactercreation_symbol");
        }

        if (invalidname)
        {
            Valid.sprite = Cross;
            Confirm.interactable = false;
        }
        else
        {
            Valid.sprite = Tick;
            WarningMessage.gameObject.SetActive(false);
            Confirm.interactable = true;
        }
    }

    private int GetNameLength(string value)
    {
        int length = 0;
        Regex regex = new Regex(@"^[\p{IsCJKUnifiedIdeographs}]");
        foreach (char c in value)
        {
            if (Char.IsDigit(c))
            {
                length += 1;
            }
            else
            {
                Match match = regex.Match(c.ToString());
                if (match.Success)
                {
                    length += 2;
                }
                else
                {
                    length += 1;
                }
            }
        }
        return length;
    }

    public void OnClickBack()
    {
        UIManager.CloseDialog(WindowType.DialogCharacterName);
    }

    public void OnClickConfirm()
    {
        UIManager.StartHourglass();
        RPCFactory.LobbyRPC.CheckCharacterName(mName);
    }

    public void OnCheckCharacterNameReturn(bool result)
    {
        UIManager.StopHourglass();
        if (!result)
        {
            Valid.sprite = Tick;
            UIManager.CloseDialog(WindowType.DialogCharacterName);
            mParent.OnConfirmName(mName);
        }
        else
        {
            Valid.sprite = Cross;
            WarningMessage.gameObject.SetActive(true);
            WarningMessage.text = GUILocalizationRepo.GetLocalizedString("charactercreation_nameexist");
            Confirm.interactable = false;
        }
    }
}
