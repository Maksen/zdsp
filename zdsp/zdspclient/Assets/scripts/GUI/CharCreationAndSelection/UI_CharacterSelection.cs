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
    Button Back;

    [SerializeField]
    ToggleGroup CharacterGroup;

    [SerializeField]
    Transform[] CharacterSlot;

    [SerializeField]
    GameObject CharacterData;

    [SerializeField]
    GameObject CreateCharacterData;

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
        UpdateCharacterInfo();
    }

    private void UpdateCharacterData()
    {
        ClearCharacterObject();
        for (int i = 0; i < mMaxCharacter; i++)
        {
            CharacterCreationData data = mCharacterList[i];
            if (data == null)
            {
                GameObject characterdata = Instantiate(CreateCharacterData);
                characterdata.GetComponent<UI_CreateCharacterData>().Init(data, this);
                characterdata.transform.SetParent(GetCharacterSlot(i), false);
                mCharacterObjects.Add(data == null ? i.ToString() : data.Name, characterdata);
            }
            else
            {
                GameObject characterdata = Instantiate(CharacterData);
                characterdata.GetComponent<UI_CharacterData>().Init(data, CharacterGroup, mSelectedCharacter == data ? true : false, this);
                characterdata.transform.SetParent(GetCharacterSlot(i), false);
                mCharacterObjects.Add(data == null ? i.ToString() : data.Name, characterdata);
            }
        }
    }

    private Transform GetCharacterSlot(int count)
    {
        return CharacterSlot[count];
    }

    public void OnSelectedCharacter(CharacterCreationData characterData)
    {
        if (characterData == null)
        {
            InfoPanel.SetActive(false);
            PlayerAvatar.Cleanup();
            GameInfo.gLobby.PlayCharacterCreationCutscene();
        }
        else if (mSelectedCharacter != characterData || (mSelectedCharacter == characterData && PlayerAvatar.GetOutfitModel() == null))
        {
            InfoPanel.SetActive(false);
            UIManager.StartHourglass();
            InfoPanel.SetActive(true);
            ChangeCharacter();
            UpdateCharacterInfo();
            UIManager.StopHourglass();
        }
        mSelectedCharacter = characterData;
    }

    private void LockButton()
    {
        foreach(KeyValuePair<string, GameObject> entry in mCharacterObjects)
        {
            UI_CharacterData characterData = entry.Value.GetComponent<UI_CharacterData>();
            UI_CreateCharacterData createCharacterData = entry.Value.GetComponent<UI_CreateCharacterData>();
            if (characterData != null)
            {
                characterData.DisableToggle();
            }
            if (createCharacterData != null)
            {
                createCharacterData.DisableButton();
            }
        }
        Enter.interactable = false;
        Back.interactable = false;
    }

    private void UnlockButton()
    {
        foreach (KeyValuePair<string, GameObject> entry in mCharacterObjects)
        {
            UI_CharacterData characterData = entry.Value.GetComponent<UI_CharacterData>();
            UI_CreateCharacterData createCharacterData = entry.Value.GetComponent<UI_CreateCharacterData>();
            if (characterData != null)
            {
                characterData.EnableToggle();
            }
            if (createCharacterData != null)
            {
                createCharacterData.EnableButton();
            }
        }
        Enter.interactable = true;
        Back.interactable = true;
    }

    private void ChangeCharacter()
    {
        if (PlayerAvatar.GetOutfitModel() != null)
        {
            LockButton();
            PlayerAvatar.PlayAnimation("pc_show3", UpdateCharacterModel);
        }
        else
        {
            UpdateCharacterModel();
        }
    }

    private void UpdateCharacterModel()
    {
        PlayerAvatar.PlayAnimation("pc_show6");
        PlayerAvatar.gameObject.SetActive(false);
        PlayerAvatar.CreationChange(mSelectedCharacter.EquipmentInventory, (JobType)mSelectedCharacter.JobSect, (Gender)mSelectedCharacter.Gender, 0, "Cutscene", OnCharacterLoaded);
    }

    public void OnCharacterLoaded()
    {
        PlayerAvatar.gameObject.SetActive(true);
        PlayerAvatar.PlayAnimation("pc_show1", PlayIdle);
        Camera.main.Render();
    }

    public void PlayIdle()
    {
        UnlockButton();
        PlayerAvatar.PlayAnimation("pc_show2");
    }

    private void UpdateCharacterInfo()
    {
        Level.text = mSelectedCharacter.ProgressLevel.ToString();
        Job.text = JobSectRepo.GetJobLocalizedName((JobType)mSelectedCharacter.JobSect);
        Energy.text = "沒資料";
        int status = 0;
        if (!string.IsNullOrEmpty(mSelectedCharacter.RemoveCharDT))
        {
            RemoveEndTime = DateTime.ParseExact(mSelectedCharacter.RemoveCharDT, "yyyy.MM.dd-HH:mm", null);
            double remainseconds = (RemoveEndTime - DateTime.Now).TotalSeconds;
            if (remainseconds > 0)
            {
                status = 1;
            }
            else
            {
                status = 2;
            }
        }
        else
        {
            status = 0;
        }

        if (status == 0)
        {
            RemoveEndTime = DateTime.MinValue;
            LevelJson levelInfo = LevelRepo.GetInfoById(mSelectedCharacter.Levelid);
            Location.text = levelInfo != null ? levelInfo.localizedname : "";
            Delete.interactable = true;
            DeleteText.text = GUILocalizationRepo.GetLocalizedString("csl_delete_character");
            EnterText.text = GUILocalizationRepo.GetLocalizedString("csl_enter_game");
            StopCoroutine(RemoveEndTimeCountDown());
        }
        else if (status == 1)
        {
            Delete.interactable = false;
            DeleteText.text = GUILocalizationRepo.GetLocalizedString("csl_complete_delete");
            EnterText.text = GUILocalizationRepo.GetLocalizedString("csl_cancel_delete");
            StartCoroutine(RemoveEndTimeCountDown());
        }
        else
        {
            Location.text = GUILocalizationRepo.GetLocalizedTimeString(0, 2);
            Delete.interactable = true;
            DeleteText.text = GUILocalizationRepo.GetLocalizedString("csl_complete_delete");
            EnterText.text = GUILocalizationRepo.GetLocalizedString("csl_cancel_delete");
            StopCoroutine(RemoveEndTimeCountDown());
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
            GameInfo.gLobby.PlayCharacterCreationCutscene();
        }
        else
        {
            UpdateCharacterData();
            foreach (CharacterCreationData characterdata in mCharacterList)
            {
                if (characterdata != null)
                {
                    mSelectedCharacter = characterdata;
                    ChangeCharacter();
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
                LockButton();
                PlayerAvatar.PlayAnimation("pc_show5", EnterGame);
            }
            else
            {
                ShowConfirmationDialog(2, mSelectedCharacter.Name);
            }
        }
    }

    public void EnterGame()
    {
        UIManager.StartHourglass();
        GameInfo.mChar = mSelectedCharacter.Name;
        GameInfo.gLobby.EnterGame();
    }

    private void OnDestroy()
    {
        ClearCharacterObject();
    }

    private void ClearCharacterObject()
    {
        if (mCharacterObjects != null)
        {
            foreach (KeyValuePair<string, GameObject> entry in mCharacterObjects)
            {
                Destroy(entry.Value);
            }
            mCharacterObjects = new Dictionary<string, GameObject>();
        }
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
