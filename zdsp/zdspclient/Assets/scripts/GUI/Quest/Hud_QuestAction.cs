using UnityEngine;
using UnityEngine.UI;
using Zealot.Repository;
using Kopio.JsonContracts;
using Zealot.Common;

public class Hud_QuestAction : BaseWidgetBehaviour
{
    [SerializeField]
    Image Icon;

    [SerializeField]
    Text Action;

    [SerializeField]
    Button InteractButton;

    private int mQuestId;
    private QuestInteractiveDetailJson mInteractiveJson;

    public void Init(int interactiveid, int questid)
    {
        mQuestId = questid;
        mInteractiveJson = QuestRepo.GetQuestInteractiveByID(interactiveid);
        if (mInteractiveJson != null)
        {
            Icon.sprite = ClientUtils.LoadIcon(mInteractiveJson.iconid);
            Action.text = mInteractiveJson.icontext;
        }
        SetButtonStatus(true);
    }

    public void OnClickActionButton()
    {
        if (mInteractiveJson != null)
        {
            if (mInteractiveJson.interactivetime > 0)
            {
                SetButtonStatus(false);
                UIManager.SetWidgetActive(HUDWidgetType.ProgressBar, true);
                Hud_ProgressBar progressbar = UIManager.GetWidget(HUDWidgetType.ProgressBar).GetComponent<Hud_ProgressBar>();
                progressbar.InitTimeBar(mInteractiveJson.interactivetime, OnActionCompleted);
            }
            else
            {
                OnActionCompleted();
            }
        }
    }

    public void OnActionCompleted()
    {
        UIManager.SetWidgetActive(HUDWidgetType.ProgressBar, false);
        int rate = GameUtils.RandomInt(0, 100);
        if (rate <= mInteractiveJson.successrate)
        {
            RPCFactory.NonCombatRPC.InteractAction(mQuestId, mInteractiveJson.interactiveid);
            UIManager.ShowSystemMessage(CheckReplacementText(mInteractiveJson.successmsg, true));
        }
        else
        {
            SetButtonStatus(true);
            UIManager.ShowSystemMessage(CheckReplacementText(mInteractiveJson.failedmsg, false));
        }
    }

    private string CheckReplacementText(string message, bool success)
    {
        QuestObjectiveJson objectiveJson = null;
        int progress = 0;
        if (GameInfo.gLocalPlayer != null)
        {
            GameInfo.gLocalPlayer.QuestController.GetInteractData(mQuestId, mInteractiveJson.interactiveid, out objectiveJson, out progress);
            if (objectiveJson != null && success)
            {
                progress += 1;
            }
        }

        message = message.Replace("%pc%", progress.ToString());
        message = message.Replace("%o[p2]%", objectiveJson == null ? "" : objectiveJson.para2.ToString());
        if (objectiveJson != null)
        {
            StaticNPCJson staticNPCJson = StaticNPCRepo.GetNPCById(objectiveJson.para3);
            message = message.Replace("%o[p1]%", staticNPCJson == null ? "" : staticNPCJson.localizedname);
        }
        else
        {
            message = message.Replace("%o[p1]%", "");
        }

        return message;
    }

    public void SetButtonStatus(bool active)
    {
        InteractButton.interactable = active;
    }
}
