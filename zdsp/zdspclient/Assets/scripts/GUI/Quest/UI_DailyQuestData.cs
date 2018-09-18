using UnityEngine;
using UnityEngine.UI;
using Kopio.JsonContracts;
using Zealot.Repository;
using Zealot.Common;

public class UI_DailyQuestData : MonoBehaviour
{
    [SerializeField]
    Image NpcIcon;

    [SerializeField]
    Text QuestName;

    [SerializeField]
    GameObject Completed;

    [SerializeField]
    Text Description;

    [SerializeField]
    Transform PartyContent;

    [SerializeField]
    GameObject PartyMemberData;

    [SerializeField]
    UI_QuestReward QuestReward;

    [SerializeField]
    Text ButtonText;

    [SerializeField]
    Button FeatureButton;

    private QuestSignboardJson mSignboardJson;

    public void Init(QuestSignboardJson signboardJson, QuestClientController questController)
    {
        mSignboardJson = signboardJson;
        QuestJson questJson = QuestRepo.GetQuestByID(signboardJson.questid);
        NpcIcon.sprite = ClientUtils.LoadIcon(signboardJson.iconpath);
        QuestName.text = questJson == null ? "" : questJson.questname;
        bool completed =  questController.IsQuestCompleted(signboardJson.questid);
        Completed.SetActive(completed);
        CurrentQuestData questData = questController.GetQuestData(QuestType.Signboard, signboardJson.questid);
        Description.text = questData == null ? (questJson == null ? "" : questJson.description) : questController.DeserializedDescription(questData);
        int rewardgroup = QuestRepo.GetQuestReward(signboardJson.questid, 0);
        int jobsect = GameInfo.gLocalPlayer == null ? -1 : GameInfo.gLocalPlayer.PlayerSynStats.jobsect;
        Reward reward = RewardListRepo.GetRewardByGrpIDJobID(rewardgroup, jobsect);
        if (reward != null)
        {
            QuestReward.Init(reward);
        }

        FeatureButton.onClick.RemoveAllListeners();

        if (completed)
        {
            ButtonText.text = GUILocalizationRepo.GetLocalizedString("qst_completed");
            FeatureButton.interactable = false;
        }
        else
        {
            if (questData == null)
            {
                ButtonText.text = GUILocalizationRepo.GetLocalizedString("dqt_accept_quest");
                FeatureButton.interactable = true;
                FeatureButton.onClick.AddListener(OnClickTigger);
            }
            else
            {
                bool submit = questController.IsQuestCanSubmit(signboardJson.questid);
                if (submit)
                {
                    ButtonText.text = GUILocalizationRepo.GetLocalizedString("quest_submit");
                    FeatureButton.interactable = true;
                    FeatureButton.onClick.AddListener(OnClickSubmit);
                }
                else
                {
                    ButtonText.text = GUILocalizationRepo.GetLocalizedString("quest_abandon");
                    FeatureButton.interactable = true;
                    FeatureButton.onClick.AddListener(OnClickDelete);
                }
            }
        }
    }

    public void OnClickTigger()
    {
        RPCFactory.NonCombatRPC.StartQuest(mSignboardJson.questid, mSignboardJson.signboardid, 0);
    }

    public void OnClickSubmit()
    {
        UIManager.StartHourglass();
        RPCFactory.NonCombatRPC.CompleteQuest(mSignboardJson.questid, true);
    }

    public void OnClickDelete()
    {
        RPCFactory.NonCombatRPC.DeleteQuest(mSignboardJson.questid, "null");
    }
}
