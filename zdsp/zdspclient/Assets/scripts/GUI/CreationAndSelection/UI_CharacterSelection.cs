using Kopio.JsonContracts;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Repository;

public class UI_CharacterSelection : MonoBehaviour
{
    [SerializeField]
    Button Enter;

    [SerializeField]
    Text EnterText;

    [SerializeField]
    Button Delete;

    [SerializeField]
    Text DeleteText;

    [SerializeField]
    ToggleGroup CharacterGroup;

    [SerializeField]
    Transform[] CharacterSlot;

    [SerializeField]
    GameObject CharacterData;

    [SerializeField]
    Model_3DAvatar PlayerAvatar;

    [SerializeField]
    Text Level;

    [SerializeField]
    Text Job;

    [SerializeField]
    Text Energy;

    [SerializeField]
    Text Location;

    [SerializeField]
    GameObject InfoPanel;

    private List<CharacterCreationData> mCharacterList;
    private CharacterCreationData mSelectedCharacter;
    private int mMaxCharacter = 3;
    private int mCharacterCount = 0;
    private Dictionary<string, GameObject> mCharacterObjects;
    private DateTime RemoveEndTime;

    public void Init(List<CharacterCreationData> charListData)
    {
        mCharacterList = new List<CharacterCreationData>();
        foreach (CharacterCreationData data in charListData)
        {
            mCharacterList.Add(data);
        }
        mCharacterCount = charListData.Count;

        for (int i = mCharacterList.Count; i < mMaxCharacter; i++)
        {
            mCharacterList.Add(null);
        }

        mCharacterObjects = new Dictionary<string, GameObject>();
        mSelectedCharacter = mCharacterList[GameInfo.gLobby.mLastLoginIndex] == null ? mCharacterList[0] : mCharacterList[GameInfo.gLobby.mLastLoginIndex];
        UpdateCharacterData();
        UpdateCharacterModel();
        UpdateCharacterInfo();
    }

    private void UpdateCharacterData()
    {
        ClearCharacterObject();
        for (int i = 0; i < mMaxCharacter; i++)
        {
            CharacterCreationData data = mCharacterList[i];
            GameObject characterdata = Instantiate(CharacterData);
            characterdata.GetComponent<UI_CharacterData>().Init(data, this);
            characterdata.transform.SetParent(GetCharacterSlot(i), false);
            mCharacterObjects.Add(data == null ? i.ToString() : data.Name, characterdata);
        }
    }

    private Transform GetCharacterSlot(int count)
    {
        return CharacterSlot[count];
    }

    public void OnSelectedCharacter(CharacterCreationData characterData)
    {
        mSelectedCharacter = characterData;
        if (mSelectedCharacter == null)
        {
            InfoPanel.SetActive(false);
            SceneLoader.Instance.LoadLevel("JobExhibition");
        }
        else
        {
            InfoPanel.SetActive(true);
            UpdateCharacterModel();
            UpdateCharacterInfo();
        }
    }

    private void UpdateCharacterModel()
    {
        PlayerAvatar.CreationChange(mSelectedCharacter.EquipmentInventory, (JobType)mSelectedCharacter.JobSect, (Gender)mSelectedCharacter.Gender, 0, "Cutscene");
    }

    private void UpdateCharacterInfo()
    {
        Level.text = mSelectedCharacter.ProgressLevel.ToString();
        Job.text = JobSectRepo.GetJobLocalizedName((JobType)mSelectedCharacter.JobSect);
        Energy.text = "沒資料";
        if (!string.IsNullOrEmpty(mSelectedCharacter.RemoveCharDT))
        {
            RemoveEndTime = DateTime.ParseExact(mSelectedCharacter.RemoveCharDT, "yyyy.MM.dd-HH:mm", null);
            StartCoroutine(RemoveEndTimeCountDown());
        }
        else
        {
            StopCoroutine(RemoveEndTimeCountDown());
            RemoveEndTime = DateTime.MinValue;
            LevelJson levelInfo = LevelRepo.GetInfoById(mSelectedCharacter.Levelid);
            Location.text = levelInfo != null ? levelInfo.localizedname : "";
            Delete.interactable = true;
            DeleteText.text = GUILocalizationRepo.GetLocalizedString("csl_delete_character");
            EnterText.text = GUILocalizationRepo.GetLocalizedString("csl_enter_game");
        }
    }

    private IEnumerator RemoveEndTimeCountDown()
    {
        yield return new WaitForSecondsRealtime(1);

        UpdateRemoveEndTime();
    }

    private void UpdateRemoveEndTime()
    {
        if (string.IsNullOrEmpty(mSelectedCharacter.RemoveCharDT))
        {
            return;
        }

        double remainhours = (RemoveEndTime - DateTime.Now).TotalHours;
        double remainseconds = (RemoveEndTime - DateTime.Now).TotalSeconds;

        if (remainseconds > 0)
        {
            if (remainhours > 1)
            {
                Location.text = (int)remainhours + GUILocalizationRepo.GetLocalizedString("time_hour");
            }
            else
            {
                Location.text = GUILocalizationRepo.GetLocalizedTimeString((int)remainseconds, 2);
            }
            StartCoroutine(RemoveEndTimeCountDown());
            Delete.interactable = false;
            DeleteText.text = GUILocalizationRepo.GetLocalizedString("csl_complete_delete");
            EnterText.text = GUILocalizationRepo.GetLocalizedString("csl_cancel_delete");
        }
        else
        {
            Delete.interactable = true;
            DeleteText.text = GUILocalizationRepo.GetLocalizedString("csl_complete_delete");
            EnterText.text = GUILocalizationRepo.GetLocalizedString("csl_cancel_delete");
        }
    }

    public void OnClickBack()
    {
        UIManager.StartHourglass();
        PhotonNetwork.networkingPeer.Disconnect();
    }

    public void OnClickDelete()
    {
        if (mSelectedCharacter != null)
        {
            if (RemoveEndTime == DateTime.MinValue)
            {
                ShowConfirmationDialog(0, mSelectedCharacter.Name);
            }
            else
            {
                ShowConfirmationDialog(1, mSelectedCharacter.Name);
            }
        }
    }

    public void OnConfirmDelete()
    {
        UIManager.StartHourglass();
        RPCFactory.LobbyRPC.DeleteCharacter(mSelectedCharacter.Name);
    }

    public void OnCharacterDeleted(int result, string name, string endtime)
    {
        if (result == 0)
        {
            mSelectedCharacter.RemoveCharDT = endtime;
            UpdateCharacterInfo();
        }
        else if (result == 1)
        {
            UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("csl_deletecharacter_countdown", null));
        }
        else if (result  == 2)
        {
            mCharacterList = GameInfo.gLobby.CharacterDeleted(name);
            mCharacterCount = mCharacterList.Count;
            for (int i = mCharacterList.Count; i < mMaxCharacter; i++)
            {
                mCharacterList.Add(null);
            }
            CheckRemainCharacterCount();
        }
        else if (result == 3)
        {
            UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("csl_deletecharacter_failed", null));
        }
    }

    private void CheckRemainCharacterCount()
    {
        if (mCharacterCount <= 0)
        {
            SceneLoader.Instance.LoadLevel("JobExhibition");
        }
        else
        {
            UpdateCharacterData();
            foreach (CharacterCreationData characterdata in mCharacterList)
            {
                if (characterdata != null)
                {
                    mSelectedCharacter = characterdata;
                    UpdateCharacterModel();
                    UpdateCharacterInfo();
                    break;
                }
            }
        }
    }

    public void OnClickEnter()
    {
        if (mSelectedCharacter != null)
        {
            if (string.IsNullOrEmpty(mSelectedCharacter.RemoveCharDT))
            {
                UIManager.StartHourglass();
                GameInfo.mChar = mSelectedCharacter.Name;
                GameInfo.gLobby.EnterGame();
            }
            else
            {
                ShowConfirmationDialog(2, mSelectedCharacter.Name);
            }
        }
    }

    private void OnDestroy()
    {
        ClearCharacterObject();
    }

    private void ClearCharacterObject()
    {
        foreach(KeyValuePair<string, GameObject> entry in mCharacterObjects)
        {
            Destroy(entry.Value);
        }
        mCharacterObjects = new Dictionary<string, GameObject>();
    }

    private void ShowConfirmationDialog(int type, string charname)
    {
        Dictionary<string, string> param = new Dictionary<string, string>();
        param.Add("name", charname);
        if (type == 0)
        {
            UIManager.OpenYesNoDialog(GUILocalizationRepo.GetLocalizedString("csl_request_delete_confirmation", param), OnConfirmDelete);
        }
        else if (type == 1)
        {
            UIManager.OpenYesNoDialog(GUILocalizationRepo.GetLocalizedString("csl_complete_delete_confirmation", param), OnConfirmDelete);
        }
        else if(type == 2)
        {
            UIManager.OpenYesNoDialog(GUILocalizationRepo.GetLocalizedString("csl_cancel_delete_confirmation", param), OnCancelDelete);
        }
    }

    public void OnCancelDelete()
    {
        UIManager.StartHourglass();
        RPCFactory.LobbyRPC.CancelDeleteCharacter(mSelectedCharacter.Name);
    }

    public void OnCancelDeleteResult(bool result, string charname)
    {
        UIManager.StopHourglass();
        if (result)
        {
            mSelectedCharacter.RemoveCharDT = "";
            UpdateCharacterInfo();
            DeleteText.text = GUILocalizationRepo.GetLocalizedString("csl_delete_character");
            EnterText.text = GUILocalizationRepo.GetLocalizedString("csl_enter_game");
        }
        else
        {
            UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("csl_canceldelete_failed", null));
        }
    }
}
