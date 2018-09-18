using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using Zealot.Common;
using Zealot.Repository;

public static class MailInvController
{
    public static GameObject GameObject_UI_Mail;

    private static UI_Mail uiMail;

    private static List<MailData> lstMailData;
    
    private static JsonSerializerSettings jsonSetting;

    private const float loadingTime = 5.0f;

    private static int deleteMailIndex = -1;

    private static bool _hasMail;
    public static bool hasMail
    {
        get { return _hasMail; }
        set
        {
            _hasMail = value;
            UIManager.AlertManager2.SetAlert(AlertType.Mail, value);
        }
    }

    public static void Init()
    {
        GameObject_UI_Mail = UIManager.GetWindowGameObject(WindowType.Mail);
        uiMail = GameObject_UI_Mail.GetComponent<UI_Mail>();

        lstMailData = new List<MailData>();
        
        jsonSetting = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.None,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore
        };

        jsonSetting.Converters.Add(new ClientInventoryItemConverter());
    }


    #region Action
    public static void DeleteMail(int _deleteMailIndex)
    {
        deleteMailIndex = _deleteMailIndex;

        MailData mailData = lstMailData[_deleteMailIndex];

        if (mailData.isTaken == false)
        {
            UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("sys_Mail_AttachmentNotTaken"));
        }
        else
        {
            if (PhotonNetwork.connected)
            {
                UIManager.StartHourglass(loadingTime);

                uiMail.SetInteraction(false);

                RPCFactory.CombatRPC.DeleteMail(_deleteMailIndex);
            }
        }
    }

    public static void OnCancelDeleteAll()
    {
        UIManager.CloseDialog(WindowType.DialogYesNoOk);
    }

    public static void OnProceedDeleteAll()
    {
        UIManager.CloseDialog(WindowType.DialogYesNoOk);

        if (PhotonNetwork.connected)
        {
            UIManager.StartHourglass(loadingTime);

            uiMail.SetInteraction(false);

            RPCFactory.CombatRPC.DeleteAllMail();
        }
    }

    public static void DeleteAllMail()
    {
        if (IsAllAttachmentTaken() == false)
        {
            UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("sys_Mail_AttachmentNotTaken"));
            OnProceedDeleteAll();
        }
        else
        {
            if (PhotonNetwork.connected)
            {
                UIManager.StartHourglass(loadingTime);

                uiMail.SetInteraction(false);

                RPCFactory.CombatRPC.DeleteAllMail();
            }
        }
    }

    public static void TakeAttachment(int openedMailIndex)
    {
        if (PhotonNetwork.connected)
        {
            UIManager.StartHourglass(loadingTime);

            uiMail.SetInteraction(false);

            RPCFactory.CombatRPC.TakeAttachment(openedMailIndex);
        }
    }

    public static void TakeAllAttachment()
    {
        if (PhotonNetwork.connected)
        {
            UIManager.StartHourglass(loadingTime);

            uiMail.SetInteraction(false);

            RPCFactory.CombatRPC.TakeAllAttachment();
        }
    }

    public static void OpenMail(int mailIndex)
    {
        MailData mailData = lstMailData[mailIndex];

        if (mailData.mailStatus == MailStatus.Unread )
        {
            if (PhotonNetwork.connected)
            {
                UIManager.StartHourglass(loadingTime);

                uiMail.SetInteraction(false);

                RPCFactory.CombatRPC.OpenMail(mailIndex);
            }
        }
        else
        {
            uiMail.OnOpenMail(mailData, mailIndex);
        }
    }
    #endregion


    #region Helper
    private static void ProcessOnTakeAttachment(int mailIndex)
    {
        MailData mailData = lstMailData[mailIndex];
        mailData.isTaken = true;
        mailData.mailStatus = MailStatus.Read;

        UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("sys_Mail_TakeAttachSuccess"));
    }

    private static void ProcessOnTakeAllAttachment()
    {
        //For each mail
        foreach (MailData mailData in lstMailData)
        {
            if (mailData.isTaken == true)
                continue;

            mailData.isTaken = true;
            mailData.mailStatus = MailStatus.Read;

            UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("sys_Mail_TakeAttachSuccess"));
        }
    }

    private static void ProcessOnTakeAllAttachment(List<int> removeList)
    {
        if (removeList == null || removeList.Count == 0)
            return;

        for (int i = 0; i < removeList.Count; ++i)
        {
            if (lstMailData[removeList[i]].isTaken)
                continue;

            lstMailData[removeList[i]].isTaken = true;
            lstMailData[removeList[i]].mailStatus = MailStatus.Read;

            UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("sys_Mail_TakeAttachSuccess"));
        }

        uiMail.OnTakeAllAttachmentPartial(removeList);
    }

    public static bool IsMailEmpty()
    {
        return lstMailData.Count == 0;
    }

    public static bool IsAllAttachmentTaken()
    {
        bool isAllAttachmentTaken = true;

        foreach (MailData mailData in lstMailData)
        {
            if (mailData.isTaken == false)
            {
                isAllAttachmentTaken = false;
                break;
            }
        }

        return isAllAttachmentTaken;
    }

    public static bool IsAllMailRead()
    {
        bool isAllMailRead = true;

        foreach (MailData mailData in lstMailData)
        {
            if (mailData.mailStatus == MailStatus.Unread)
            {
                isAllMailRead = false;
                break;
            }
        }

        return isAllMailRead;
    }

    public static bool IsAllMailReadAndTaken()
    {
        foreach (MailData mailData in lstMailData)
        {
            if (mailData.mailStatus == MailStatus.Unread || mailData.isTaken == false)
            {
                return false;
            }
        }
        return true;
    }

    public static List<MailData> GetMailDataList()
    {
        return lstMailData;
    }
    #endregion


    #region callbacks
    public static void HasNewMail_Callback(bool _hasMail_, string hudID)
    {
        hasMail = _hasMail_;
        //Debug.Log("HUD.Combat.HUDMenu.SetHintActive(HUD_Menu.HintIconType.Mail, hasMail);");
        //string[] ids = hudID.Split(';');
        //for (int i = 0; i < ids.Length;  ++i)
        //{
        //    int _id = 0;
        //    bool result = int.TryParse(ids[i], out _id);
        //    //if(result)//success
        //    //    UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgById(_id, null));
        //}
    }

    public static void RetrieveMail_Callback(string serializedMailString)
    {
        MailInventoryData mailInvData = JsonConvert.DeserializeObject<MailInventoryData>(serializedMailString, jsonSetting);
        lstMailData = mailInvData.lstMailData;
        uiMail.OnRetrieveMail(lstMailData);

        //Show red dot as long as there is a unread mail or untaken attachment
        hasMail = !IsAllMailReadAndTaken();
    }

    public static void OpenMail_Callback(int mailReturnCode, int mailIndex)
    {
        UIManager.StopHourglass();

        uiMail.SetInteraction(true);

        switch ((MailReturnCode)mailReturnCode)
        {
            case MailReturnCode.OpenMail_Success:
                MailData mailData = lstMailData[mailIndex];
                mailData.mailStatus = MailStatus.Read;

                uiMail.OnOpenMail(mailData, mailIndex);

                hasMail = !IsAllMailReadAndTaken();    //Turn on/off red dot
                break;
            case MailReturnCode.OpenMail_Fail_InvalidIndex:
                break;
            default:
                break;
        }
    }

    public static void TakeAttachment_Callback(int mailReturnCode, int mailIndex)
    {
        UIManager.StopHourglass();

        uiMail.SetInteraction(true);

        bool isRetCodeSuccess = false;

        switch ((MailReturnCode)mailReturnCode)
        {
            case MailReturnCode.TakeAttachment_Success:
                isRetCodeSuccess = true;
                ProcessOnTakeAttachment(mailIndex);
                uiMail.OnTakeAttachment(mailIndex, isRetCodeSuccess);
                hasMail = !IsAllMailReadAndTaken();    //Turn on/off red dot

                //Delete mail upon taking attachment
                lstMailData.RemoveAt(mailIndex);
                uiMail.OnDeleteMail(mailIndex);
                hasMail = !IsMailEmpty();    //Turn on/off red dot
                break;
            case MailReturnCode.TakeAttachment_Fail_InventoryFull:
            case MailReturnCode.TakeAttachment_Fail_InventoryAddFailed:
                UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("sys_BagInventoryFull", null));

                break;
            case MailReturnCode.TakeAttachment_Fail_InvalidIndex:
            case MailReturnCode.TakeAttachment_Fail_InventoryUnknownRetCode:
            default:
                break;
        }
    }

    public static void TakeAllAttachment_Callback(int mailReturnCode, string lstTakenMailIndexSerialStr)
    {
        UIManager.StopHourglass();

        uiMail.SetInteraction(true);

        bool isRetCodeSuccess = false;

        switch ((MailReturnCode)mailReturnCode)
        {
            case MailReturnCode.TakeAllAttachment_Success:
                isRetCodeSuccess = true;
                uiMail.OnTakeAllAttachment(isRetCodeSuccess);
                ProcessOnTakeAllAttachment();

                //MANUALDELETEMAIL TIP: Revert this to go back to manual delete mail system
                //Delete mail upon taking attachment
                //hasMail = !IsAllMailReadAndTaken();    //Turn on/off red dot
                DeleteAllMail_Callback((int)MailReturnCode.DeleteAllMail_Success);
                break;
            case MailReturnCode.TakeAllAttachment_Fail_InventoryFull:
            case MailReturnCode.TakeAllAttachment_Fail_InventoryAddFailed:
                UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("sys_BagInventoryFull", null));

                //Remove attachments from mentioned mails
                List<int> removeList = JsonConvert.DeserializeObject<List<int>>(lstTakenMailIndexSerialStr);
                ProcessOnTakeAllAttachment(removeList);

                //MANUALDELETEMAIL TIP: Remove this to go back to manual delete mail system
                //Delete mail upon taking attachments
                DeleteAllMail_Callback((int)MailReturnCode.DeleteAllMail_Success);
                break;
            case MailReturnCode.TakeAllAttachment_Fail_InventoryUnknownRetCode:
            default:
                break;
        }
    }

    public static void DeleteMail_Callback(int mailReturnCode, int mailIndex)
    {
        UIManager.StopHourglass();

        uiMail.SetInteraction(true);

        switch ((MailReturnCode)mailReturnCode)
        {
            case MailReturnCode.DeleteMail_Success:
                lstMailData.RemoveAt(mailIndex);

                uiMail.OnDeleteMail(mailIndex);

                hasMail = !IsMailEmpty();    //Turn on/off red dot
                break;
            case MailReturnCode.DeleteMail_Fail_InvalidIndex:
            default:
                break;
        }

        deleteMailIndex = -1;
    }

    public static void DeleteAllMail_Callback(int mailReturnCode)
    {
        UIManager.StopHourglass();

        switch ((MailReturnCode)mailReturnCode)
        {
            case MailReturnCode.DeleteAllMail_Success:

                //Remove from UI
                uiMail.OnDeleteAllMail(lstMailData);

                //Remove from mail internal controller
                for (int i = lstMailData.Count - 1; i >= 0; --i)
                {
                    if (lstMailData[i].isTaken)
                        lstMailData.RemoveAt(i);
                }

                hasMail = !IsMailEmpty();    //Turn off red dot
                uiMail.SetInteraction(hasMail); //Turn on/off interaction if there is mail or not
                break;
            default:
                break;
        }

        deleteMailIndex = -1;
    }
#endregion
}
