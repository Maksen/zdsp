using UnityEngine;
using UnityEngine.UI;
using System.Collections;
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
            UIManager.ShowSystemMessage(mInteractiveJson.successmsg);
        }
        else
        {
            SetButtonStatus(true);
            UIManager.ShowSystemMessage(mInteractiveJson.failedmsg);
        }
    }

    public void SetButtonStatus(bool active)
    {
        InteractButton.interactable = active;
    }
}
