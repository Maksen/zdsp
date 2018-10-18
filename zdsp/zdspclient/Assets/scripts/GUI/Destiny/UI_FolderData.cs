using UnityEngine;
using UnityEngine.UI;
using Kopio.JsonContracts;
using Zealot.Common;
using Zealot.Repository;

public class UI_FolderData : MonoBehaviour
{
    [SerializeField]
    Image Icon;

    [SerializeField]
    Toggle DataToggle;

    [SerializeField]
    Text QuestName;

    [SerializeField]
    GameObject NewClue;

    [SerializeField]
    Sprite UnknownDisabledIcon;

    [SerializeField]
    Sprite AudioTypeIcon;

    [SerializeField]
    Sprite PhotoTypeIcon;

    [SerializeField]
    Sprite WordTypeIcon;

    [SerializeField]
    Sprite VideoTypeIcon;

    [SerializeField]
    Sprite CompletedIcon;

    [SerializeField]
    Sprite LockedIcon;

    [SerializeField]
    Sprite UncompleteIcon;

    private DestinyClueJson mClueJson;
    private ActivatedClueData mClueData;
    private UI_DestinyHistory mParent;

    public void Init(DestinyClueJson clueJson, ActivatedClueData clueData, ToggleGroup toggleGroup, UI_DestinyHistory parent)
    {
        mParent = parent;
        mClueJson = clueJson;
        mClueData = clueData;
        DataToggle.group = toggleGroup;
        UpdateUI();
    }

    public void UpdateData(DestinyClueJson clueJson, ActivatedClueData clueData)
    {
        mClueJson = clueJson;
        mClueData = clueData;
        UpdateUI();
    }

    private void UpdateUI()
    {
        bool completed = GameInfo.gLocalPlayer == null ? false : GameInfo.gLocalPlayer.QuestController.IsQuestCompleted(mClueJson.questid);
        Icon.sprite = GetClueIcon(mClueJson.category);
        DataToggle.interactable = mClueData == null ? false : true;
        QuestJson questJson = QuestRepo.GetQuestByID(mClueJson.questid);
        QuestName.color = completed ? new Color(223, 65, 71) : Color.white;
        QuestName.text = questJson == null ? "" : questJson.questname;
        NewClue.SetActive(mClueData == null ? false : mClueData.Status == (byte)ClueStatus.New);
    }

    private Sprite GetClueIcon(ClueCategory category)
    {
        if (mClueData != null)
        {
            switch (category)
            {
                case ClueCategory.Photo:
                    return PhotoTypeIcon;
                case ClueCategory.Sound:
                    return AudioTypeIcon;
                case ClueCategory.Video:
                    return VideoTypeIcon;
                default:
                    return WordTypeIcon;
            }
        }
        else
        {
            return UnknownDisabledIcon;
        }
    }

    public Sprite GetStatusIcon(bool completed)
    {
        if (mClueData != null)
        {
            return completed ? CompletedIcon : UncompleteIcon;
        }
        else
        {
            return LockedIcon;
        }
    }

    public void OnSelectedData()
    {
        if (DataToggle.isOn)
        {
            UIManager.OpenDialog(WindowType.DialogMessageFilter, (window) => window.GetComponent<UI_MessageFilter>().Init(mClueData));
        }
    }
}
