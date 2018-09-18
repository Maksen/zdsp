using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Repository;
public class UI_Mail : MonoBehaviour {

    #region leftpanel
    public Text mailCapacityLeft;
    public UI_Mail_ScrollView uiMailScrollView;

    public Button Button_TakeAllAttachment;
    public Button Button_DeleteAllMail;
    #endregion

    #region rightpanel
    public UI_MailAttachment_Scrollview uiAttachmentScrollView;

    public Text Text_Body;
    //public Text Text_Sender;
    public Text Text_Subject;

    public Button Button_DeleteMail;
    public Button Button_TakeAttachment;
    #endregion

    //public Text Text_Gold;
    //public Text Text_Crystal;
    
    private int deleteMailIndex;
    private int openedMailIndex;

    private int maxMail;
    private int curMail;

    private const float loadingTime = 5.0f;
    


    void Awake()
    {
        openedMailIndex = -1;
        deleteMailIndex = -1;

        maxMail = maxMail = GameConstantRepo.GetConstantInt("Mail_Capacity"); ;
        curMail = 0;
    }

    void OnEnable()
    {
        ClearRightPanelView();
        openedMailIndex = -1;
        deleteMailIndex = -1;
        
        RPCFactory.CombatRPC.RetrieveMail();//callback is OnRetrieveMail
        UIManager.StartHourglass(loadingTime);//stop hourglass after ui is inited
    }

    void OnDestroy()
    {
        uiMailScrollView = null;
        uiAttachmentScrollView = null;

        Button_DeleteAllMail = null;
        Button_TakeAttachment = null;
        Button_TakeAllAttachment = null;
        Button_DeleteMail = null;

        Text_Body = null;
        //Text_Sender = null;
        Text_Subject = null;

        //Text_Gold = null;
        //Text_Crystal = null;
    }


    #region onclick
    public void OnClickTakeAttachment()
    {
        Button_TakeAttachment.interactable = false;

        MailInvController.TakeAttachment(openedMailIndex);
    }

    public void OnClickTakeAllAttachment()
    {
        Button_TakeAttachment.interactable = false;
        Button_TakeAllAttachment.interactable = false;

        MailInvController.TakeAllAttachment();
    }

    public void OnClickDeleteMail()
    {
        MailInvController.DeleteMail(deleteMailIndex);
    }

    public void OnClickDeleteAllMail()
    {
        MailInvController.DeleteAllMail();
    }

    public void OnCancelDeleteMail()
    {
        UIManager.CloseDialog(WindowType.DialogYesNoOk);
    }
    #endregion


    #region callback
    public void OnRetrieveMail(List<MailData> lstMailData)
    {
        if (isActiveAndEnabled == false)
        {
            return;
        }

        Button_DeleteAllMail.interactable = true;
        Button_DeleteMail.interactable = true;
        Button_TakeAttachment.interactable = true;
        Button_TakeAllAttachment.interactable = true;

        //If there is no mail
        if (MailInvController.IsMailEmpty() == true)
        {
            Button_DeleteAllMail.interactable = false;
            Button_DeleteMail.interactable = false;
            Button_TakeAttachment.interactable = false;
            Button_TakeAllAttachment.interactable = false;
        }
        //if there are no mails with attachments
        else if (MailInvController.IsAllAttachmentTaken() == true)
        {
            Button_TakeAttachment.interactable = false;
            Button_TakeAllAttachment.interactable = false;
            Button_DeleteMail.interactable = false;
        }

        //Record the number of mails
        curMail = maxMail - lstMailData.Count;
        mailCapacityLeft.text = curMail.ToString();

        //Populate the mail list with retreived mails
        uiMailScrollView.OnSendMail(lstMailData);
        
        //after all ui is init-ed then...
        UIManager.StopHourglass();
    }

    public void OnOpenMail(MailData mailData, int mailIndex)
    {
        ClearRightPanelView();

        var mailJson = MailRepo.GetInfoByName(mailData.mailName);
        if (mailJson != null)
        {
            if (mailData.dicBodyParam != null)
                Text_Body.text = GameUtils.FormatString(mailJson.body, mailData.dicBodyParam);
            else
                Text_Body.text = mailJson.body;
            Text_Subject.text = mailJson.subject;
        }
        else
        {
            Text_Body.text = "Unknown Mail Body";
            Text_Subject.text = "Unknown Mail Subject";
        }
        Button_DeleteMail.interactable = true;
        Button_TakeAllAttachment.interactable = !MailInvController.IsAllAttachmentTaken();

        openedMailIndex = mailIndex;
        deleteMailIndex = mailIndex;

        //If mail attachment is not taken yet
        if (mailData.isTaken == false)
        {
            Button_TakeAttachment.interactable = true;

            //load up all the attachments
            uiAttachmentScrollView.PopulateAttachments(mailData.lstIInventoryItem);
            uiAttachmentScrollView.PopulateCurrencies(mailData.dicCurrencyAmt);
        }
        else
        {
            //No attachments
            Button_TakeAttachment.interactable = false;
            
            //Clear all item icons
            ClearRightPanelWidgets();
        }
        
        //Update mail icon on mail list
        uiMailScrollView.OnOpenMail(mailIndex, mailData.isTaken);
    }

    public void OnTakeAttachment(int mailIndex, bool isRetCodeSuccess)
    {
        if (isRetCodeSuccess == true)
        {
            ClearRightPanelWidgets();

            Button_TakeAttachment.interactable = false;

            uiMailScrollView.OnOpenMail(mailIndex, isRetCodeSuccess);

            //deleteMailIndex = mailIndex;

            //UIManager.OpenYesNoDialog(GUILocalizationRepo.GetLocalizedString("mail_DeleteMail"), OnClickDeleteMail, OnCancelDeleteMail);
        }
        else
        {
            Button_TakeAttachment.interactable = true;
        }

        Button_TakeAllAttachment.interactable = !MailInvController.IsAllAttachmentTaken();
    }

    public void OnTakeAllAttachment(bool isRetCodeSuccess)
    {
        if (isRetCodeSuccess == true)
        {
            Button_TakeAllAttachment.interactable = false;
            Button_TakeAttachment.interactable = false;
            ClearRightPanelWidgets();
            uiMailScrollView.OnOpenAllMail();
        }
        else
        {
            Button_TakeAllAttachment.interactable = true;
        }
    }

    public void OnTakeAllAttachmentPartial(List<int> takenMailIndexList)
    {
        if (takenMailIndexList != null)
        {
            for (int i = 0; i < takenMailIndexList.Count; ++i)
            {
                uiMailScrollView.OnOpenMail(takenMailIndexList[i], true);
            }
        }

        Button_TakeAllAttachment.interactable = true;
        ClearRightPanelWidgets();
    }

    public void OnDeleteMail(int mailIndex)
    {
        Button_TakeAttachment.interactable = false;
        Button_DeleteMail.interactable = false;

        if (MailInvController.IsMailEmpty() == true)
        {
            Button_DeleteMail.interactable = false;
            Button_TakeAllAttachment.interactable = false;
        }
        else if (MailInvController.IsAllAttachmentTaken())
        {
            Button_TakeAllAttachment.interactable = false;
        }

        ClearRightPanelView();
        
        openedMailIndex = -1;
        deleteMailIndex = -1;
        curMail++;
        uiMailScrollView.OnDeleteMail(mailIndex);

        mailCapacityLeft.text = curMail.ToString();
    }

    public void OnDeleteAllMail(List<MailData> lstMailData)
    {
        Button_DeleteAllMail.interactable = false;
        Button_DeleteMail.interactable = false;
        Button_TakeAttachment.interactable = false;
        Button_TakeAllAttachment.interactable = false;

        ClearRightPanelView();

        openedMailIndex = -1;
        deleteMailIndex = -1;
        uiMailScrollView.OnDeleteAllMail(lstMailData);
        curMail = maxMail - uiMailScrollView.GetMailGOCount();

        mailCapacityLeft.text = curMail.ToString();
    }
    #endregion


    #region helper
    private void ClearRightPanelWidgets()
    {
        //Text_Gold.text = null;
        //Text_Crystal.text = null;

        //foreach (GameIcon mailAttachIcon in GameIcon_LstMailAttachIcon)
        //{
        //    //mailAttachIcon.SetGameIcon(Sprite_ItemBox);
        //    //mailAttachIcon.SetStackcount(0);

        //    mailAttachIcon.Reset();
        //}

        //Clear attachment ui
        uiAttachmentScrollView.ClearAllAttachmentSlots();
        uiAttachmentScrollView.ClearAllCurrencySlots();
    }

    private void ClearRightPanelView()
    {
        Text_Body.text = null;
        //Text_Sender.text = null;
        Text_Subject.text = null;

        //openedMailIndex = -1;
        //deleteMailIndex = -1;

        ClearRightPanelWidgets();
    }

    public void SetInteraction(bool flag)
    {
        Button_DeleteAllMail.interactable = flag;
        Button_TakeAttachment.interactable = flag;
        Button_TakeAllAttachment.interactable = flag;
        Button_DeleteMail.interactable = flag;

        uiMailScrollView.SetInteraction(flag);
    }
    #endregion
}
