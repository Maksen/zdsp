using Kopio.JsonContracts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zealot.Common;
using Zealot.Server.Rules;
using Zealot.Repository;
using Zealot.Server.Entities;

// Example Usage
//MailObject mailObj = new MailObject();
//mailObj.gold = 1000;
//mailObj.crystal = 1000;
//mailObj.rcvName = "Dickbutt";
//mailObj.mailName = "Mail Test";

//int itemid = 2;
//int stackcount = 301;
//bool superability = false;

//IInventoryItem itemToAttach = GameRules.GenerateItem(itemid, null, stackcount, superability);
//mailObj.lstAttachment.Add(itemToAttach);

//itemid = 101;
//stackcount = 200;
//superability = false;

//itemToAttach = GameRules.GenerateItem(itemid, null, stackcount, superability);
//mailObj.lstAttachment.Add(itemToAttach);

//itemid = 1;
//stackcount = 1;
//superability = false;

//itemToAttach = GameRules.GenerateItem(itemid, null, stackcount, superability);
//mailObj.lstAttachment.Add(itemToAttach);

//itemid = 3;
//stackcount = 1;
//superability = false;

//itemToAttach = GameRules.GenerateItem(itemid, null, stackcount, superability);
//mailObj.lstAttachment.Add(itemToAttach);

//MailManager.Instance.SendMail(mailObj);

namespace Photon.LoadBalancing.GameServer.Mail
{
    public enum MailResult : byte
    {
        MailFailed_ServerNotFound,
        MailFailed_UnknownClient,

        MailSuccess_Offline,
        MailFailed_Offline,
        MailSuccess_MailSendOnline,
        MailFailed_InboxFull_Offline,
        MailFailed_InboxFull_Online,

        MailFailed_UnknownTitle,

        MailFailed_Unknown
    }

    public sealed class MailManager
    {
        public int MAIL_LIMIT { get; private set; }
        public const int EXPIRY_DAYS = 15;
        public const int ATTACHMENT_SIZE = 12;
        public const string MAIL_DATEFORMAT = "yyyy/MM/dd HH:mm:ss";
        public static CultureInfo MAIL_CULTUREINFO = CultureInfo.InvariantCulture;

        private static readonly MailManager instance = new MailManager();

        private JsonSerializerSettings jsonSettingDB;
        private JsonSerializerSettings jsonSettingClient;

        private MailManager()
        {
            jsonSettingDB = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.None,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
            };

            jsonSettingDB.Converters.Add(new DBInventoryItemConverter());

            jsonSettingClient = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.None,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
            };

            jsonSettingClient.Converters.Add(new ClientInventoryItemConverter());

            MAIL_LIMIT = GameConstantRepo.GetConstantInt("Mail_Capacity");
            if (MAIL_LIMIT == 0)
                MAIL_LIMIT = 10;
        }

        public static MailManager Instance
        {
            get
            {
                return instance;
            }
        }

        #region Log
        private void ZLog_MailLogSent(MailObject mailObj, DateTime created, GameClientPeer rcvPeer)
        {
            string attachment = "";
            StringBuilder sb = new StringBuilder();
            foreach (IInventoryItem attachItem in mailObj.lstAttachment)
                sb.AppendFormat("id={0}|amt={1};", attachItem.ItemID, attachItem.StackCount);
            attachment = sb.ToString();

            string currency = "";
            StringBuilder sb_currency = new StringBuilder();
            foreach (var kvp in mailObj.dicCurrencyAmt)
                sb.AppendFormat("type={0}|amt={1};", kvp.Key, kvp.Value);
            currency = sb_currency.ToString();

            MailContentJson mailJson = MailRepo.GetInfoByName(mailObj.mailName);
            Zealot.Logging.Client.LogClasses.MailSent mailSentLog = new Zealot.Logging.Client.LogClasses.MailSent();
            //LogClass
            if (rcvPeer != null)
            {
                mailSentLog.userId = rcvPeer.mUserId;
                mailSentLog.charId = rcvPeer.GetCharId();
            }
            else
            {
                mailSentLog.userId = "";
                mailSentLog.charId = "";
            }
            mailSentLog.message = "";
            //MailSent
            mailSentLog.sentDate = created;
            mailSentLog.rcvName = mailObj.rcvName;
            mailSentLog.mailName = mailObj.mailName;
            mailSentLog.attachment = attachment;
            mailSentLog.currency = currency;

            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(mailSentLog);
        }

        private void ZLog_MailLogTaken(MailData mailData, string rcvName)
        {
            GameClientPeer rcvPeer = GameApplication.Instance.GetCharPeer(rcvName);

            MailContentJson mailJson = MailRepo.GetInfoByName(mailData.mailName);
            Zealot.Logging.Client.LogClasses.MailTaken mailTakenLog = new Zealot.Logging.Client.LogClasses.MailTaken();
            //LogClass
            mailTakenLog.userId = rcvPeer.mUserId;
            mailTakenLog.charId = rcvPeer.GetCharId();
            mailTakenLog.message = "";
            //MailTaken
            mailTakenLog.sentDate = new DateTime(mailData.expiryTicks).AddDays(-EXPIRY_DAYS);
            mailTakenLog.mailName = mailData.mailName;

            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(mailTakenLog);
        }

        private void ZLog_MailLogRemove(MailData mailData, string rcvName)
        {
            GameClientPeer rcvPeer = GameApplication.Instance.GetCharPeer(rcvName);

            MailContentJson mailJson = MailRepo.GetInfoByName(mailData.mailName);
            Zealot.Logging.Client.LogClasses.MailRemove mailRemoveLog = new Zealot.Logging.Client.LogClasses.MailRemove();
            //LogClass
            mailRemoveLog.userId = rcvPeer.mUserId;
            mailRemoveLog.charId = rcvPeer.GetCharId();
            mailRemoveLog.message = "";
            //MailRemove
            mailRemoveLog.sentDate = new DateTime(mailData.expiryTicks).AddDays(-EXPIRY_DAYS);
            mailRemoveLog.mailName = mailData.mailName;

            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(mailRemoveLog);
        }
        #endregion

        #region Helper
        private List<IInventoryItem> GenerateStackedItemToAttachList(List<IInventoryItem> lstItemToAttach)
        {
            List<IInventoryItem> lstEquipItemToAttach = new List<IInventoryItem>();
            List<IInventoryItem> lstStackedItemToAttach = new List<IInventoryItem>();
            List<IInventoryItem> lstNonEquipItemToAttach = new List<IInventoryItem>();

            foreach (IInventoryItem itemToAttach in lstItemToAttach)
            {
                lstNonEquipItemToAttach.Add(itemToAttach);
            }

            foreach (IInventoryItem equipItemToAttach in lstEquipItemToAttach)
            {
                lstStackedItemToAttach.Add(equipItemToAttach);
            }

            for (int index = 0; index < lstNonEquipItemToAttach.Count; index++)
            {
                IInventoryItem nonEquipItemToAttach = lstNonEquipItemToAttach[index];

                int totalStackCount = 0;

                ushort itemId = nonEquipItemToAttach.ItemID;

                totalStackCount = nonEquipItemToAttach.StackCount;

                for (int lastIndex = lstNonEquipItemToAttach.Count - 1; lastIndex != index; lastIndex--)
                {
                    IInventoryItem nextNonEquipItemToAttach = lstNonEquipItemToAttach[lastIndex];

                    if (nextNonEquipItemToAttach.ItemID == itemId)
                    {
                        totalStackCount += nextNonEquipItemToAttach.StackCount;

                        lstNonEquipItemToAttach.Remove(nextNonEquipItemToAttach);
                    }
                }

                if (totalStackCount > nonEquipItemToAttach.MaxStackCount)
                {
                    int quotient = totalStackCount / nonEquipItemToAttach.MaxStackCount;
                    int remainder = totalStackCount % nonEquipItemToAttach.MaxStackCount;

                    while (quotient > 0)
                    {
                        bool hasSuperAbility = false;
                        int overflowStackCount = nonEquipItemToAttach.MaxStackCount;
                        IInventoryItem overflowItem = GameRules.GenerateItem(itemId, null, overflowStackCount, hasSuperAbility);

                        lstStackedItemToAttach.Add(overflowItem);

                        quotient--;
                    }

                    if (remainder > 0)
                    {
                        bool hasSuperAbility = false;
                        int overflowStackCount = remainder;
                        IInventoryItem overflowItem = GameRules.GenerateItem(itemId, null, overflowStackCount, hasSuperAbility);

                        lstStackedItemToAttach.Add(overflowItem);
                    }
                }
                else
                {
                    nonEquipItemToAttach.StackCount = (ushort)totalStackCount;

                    lstStackedItemToAttach.Add(nonEquipItemToAttach);
                }
            }

            return lstStackedItemToAttach;
        }

        private List<List<IInventoryItem>> GenerateAttachmentListPerMailList(List<IInventoryItem> lstStackedItemToAttach)
        {
            List<List<IInventoryItem>> lstAttachmentList = new List<List<IInventoryItem>>();
            List<IInventoryItem> lstItem = null;
            for (int index = 0; index < lstStackedItemToAttach.Count; index++)
            {
                if (index % ATTACHMENT_SIZE == 0)
                {
                    lstItem = new List<IInventoryItem>();
                    lstAttachmentList.Add(lstItem);
                }
                lstItem.Add(lstStackedItemToAttach[index]);
            }

            return lstAttachmentList;
        }

        private List<MailData> GenerateMailsViaMailObject(MailObject mailObj, DateTime created)
        {
            List<MailData> lstMailData = new List<MailData>();
            if (mailObj == null)
                return lstMailData;

            List<IInventoryItem> lstItemToAttach = mailObj.lstAttachment;
            List<IInventoryItem> lstStackedItemToAttach = GenerateStackedItemToAttachList(lstItemToAttach);
            List<List<IInventoryItem>> lstAttachmentList = GenerateAttachmentListPerMailList(lstStackedItemToAttach);

            if (lstAttachmentList.Count == 0)//if there is no attachment
            {
                MailData mailData = new MailData();
                mailData.mailName = mailObj.mailName;
                mailData.mailStatus = MailStatus.Unread;
                if (mailObj.dicCurrencyAmt.Count == 0)
                    mailData.isTaken = true;
                else
                    mailData.isTaken = false;
                mailData.lstIInventoryItem = new List<IInventoryItem>();
                mailData.dicCurrencyAmt = mailObj.dicCurrencyAmt;
                mailData.hasTopupGold = mailObj.hasTopupGold;
                mailData.dicBodyParam = mailObj.dicBodyParam;

                //mailData.sentDateTime = mailObj.sentDateTime;
                //mailData.sentDate = mailObj.sentDateTime.ToString(MAIL_DATEFORMAT, MAIL_CULTUREINFO);
                //mailData.expiryDateTime = DateTime.Now.AddDays(EXPIRY_DAYS);
                //mailData.expiryDate = mailData.expiryDateTime.ToString(MAIL_DATEFORMAT, MAIL_CULTUREINFO);
                //mailData.expiryTicks = mailData.expiryDateTime.Ticks - DateTime.Now.Ticks;
                mailData.expiryTicks = created.AddDays(EXPIRY_DAYS).Ticks;

                lstMailData.Add(mailData);
            }
            else // if there is attachment
            {
                //create more mails if attachment is more than 5 items
                bool currencySent = false;
                foreach (List<IInventoryItem> lstAttachment in lstAttachmentList)
                {
                    MailData mailData = new MailData();
                    mailData.mailName = mailObj.mailName;
                    mailData.mailStatus = MailStatus.Unread;
                    mailData.isTaken = false;
                    mailData.lstIInventoryItem = lstAttachment;
                    if (!currencySent)
                    {
                        mailData.dicCurrencyAmt = mailObj.dicCurrencyAmt;
                        mailData.hasTopupGold = mailObj.hasTopupGold;
                        currencySent = true;
                    }
                    mailData.dicBodyParam = mailObj.dicBodyParam;

                    //mailData.sentDateTime = mailObj.sentDateTime;
                    //mailData.sentDate = mailObj.sentDateTime.ToString(MAIL_DATEFORMAT, MAIL_CULTUREINFO);
                    //mailData.expiryDateTime = DateTime.Now.AddDays(EXPIRY_DAYS);
                    //mailData.expiryDate = mailData.expiryDateTime.ToString(MAIL_DATEFORMAT, MAIL_CULTUREINFO);
                    //mailData.expiryTicks = mailData.expiryDateTime.Ticks - DateTime.Now.Ticks;
                    mailData.expiryTicks = created.AddDays(EXPIRY_DAYS).Ticks;

                    lstMailData.Add(mailData);

                    //EFLog_PrepMail(mailData, mailObj.rcvName);
                }
            }

            return lstMailData;
        }

        private void PrepareOnlineMail(MailObject mailObj, DateTime created, List<MailData> lstMailData, GameClientPeer rcvPeer)
        {
            //Generate maildata list if not provided
            if (lstMailData == null)
                lstMailData = GenerateMailsViaMailObject(mailObj, created);            
            //add mails to characterdata
            rcvPeer.CharacterData.MailInventory.lstMailData.AddRange(lstMailData);
        }

        /// <summary>
        /// when player is offline, create a mail and send to db
        /// </summary>
        /// <param name="mailObj"></param>
        private async void PrepareOfflineMailAsync(MailObject mailObj)
        {
            MailContentJson mailContentJson = MailRepo.GetInfoByName(mailObj.mailName);
            if (mailContentJson == null)
                return;

            string serializedOfflineMailData = JsonConvert.SerializeObject(mailObj, Formatting.None, jsonSettingDB);

            Guid? guild = await GameApplication.dbRepository.MailOffline.InsertMailOffline(mailObj.rcvName, serializedOfflineMailData);
            if (guild != null)
            {
                //todo log offline mail
                GameApplication.Instance.executionFiber.Enqueue(() => {
                    ZLog_MailLogSent(mailObj, DateTime.Now, null);
                });
            }
        }
        #endregion

        public async Task ClientInit(GameClientPeer rcvPeer)
        {
            //making sure that the player is online
            if (rcvPeer.RoomReference != null && rcvPeer.mPlayer != null)
            {
                string charName = rcvPeer.mChar;
                DateTime now = DateTime.Now;
                DateTime expiry = now.AddDays(-15);
                //get from db, the mails that player missed when he was offline
                List<Dictionary<string, object>> lstMailOfflineObj = await GameApplication.dbRepository.MailOffline.GetMailOffline(charName, expiry);
                GameApplication.Instance.executionFiber.Enqueue(() =>
                {
                    //If player disconnects before init
                    rcvPeer = GameApplication.Instance.GetCharPeer(charName);
                    if (rcvPeer == null)
                        return;
                    Player player = rcvPeer.mPlayer;
                    if (player == null)
                        return;
                    rcvPeer.CharacterData.MailInventory.hasNewMail = false;//reset 

                    //If there are offline mails
                    int mailcount = lstMailOfflineObj.Count;
                    if (mailcount > 0)
                    {
                        //Create online mail from offline mail and send to player
                        for(int index = 0; index < mailcount; index++)
                        {
                            Dictionary<string, object> mailinfo = lstMailOfflineObj[index];
                            MailObject offlineMailObj = JsonConvert.DeserializeObject<MailObject>((string)mailinfo["maildata"], jsonSettingDB);
                            offlineMailObj.rcvName = charName;
                            MailContentJson mailContentJson = MailRepo.GetInfoByName(offlineMailObj.mailName);
                            if (mailContentJson == null)
                                continue;
                            PrepareOnlineMail(offlineMailObj, (DateTime)mailinfo["created"], null, rcvPeer); // Add to character data and log
                        }

                        //Delete all offline mail from db
                        var isDeleteOK = GameApplication.dbRepository.MailOffline.DeleteMailOffline(charName);

                        //Check if there are new mails
                        rcvPeer.CharacterData.MailInventory.hasNewMail = true;
                        //var mailInv = rcvPeer.CharacterData.MailInventory;
                        //foreach (var mail in mailInv.lstMailData)
                        //{
                        //    if (mail.mailStatus == MailStatus.New || mail.mailStatus == MailStatus.Unread || mail.isTaken == false)
                        //    {
                        //        rcvPeer.CharacterData.MailInventory.hasNewMail = true;
                        //        break;
                        //    }
                        //}

                        rcvPeer.ZRPC.CombatRPC.Ret_HasNewMail(rcvPeer.CharacterData.MailInventory.hasNewMail, null, rcvPeer);//inform client
                    }
                    else
                    {
                        //If there are no offline mails
                        List<MailData> Mailbox = rcvPeer.CharacterData.MailInventory.lstMailData;

                        for (int i = 0; i < Mailbox.Count; ++i)
                        {
                            //If there are unread/attachment mails, turn on red dot for client
                            if (Mailbox[i].mailStatus == MailStatus.Unread || Mailbox[i].isTaken == false)
                            {
                                rcvPeer.CharacterData.MailInventory.hasNewMail = true;
                                rcvPeer.ZRPC.CombatRPC.Ret_HasNewMail(rcvPeer.CharacterData.MailInventory.hasNewMail, "", rcvPeer);//inform client
                                break;
                            }
                        }
                    }
                });
            }
        }

        /// <summary>
        /// Create mailobject and store in db if player is offline or deliver the mail if online
        /// </summary>
        /// <param name="mailObj">The new mail object</param>
        /// <returns>success / fail</returns>
        public MailResult SendMail(MailObject mailObj)
        {
            MailContentJson mailContentJson = MailRepo.GetInfoByName(mailObj.mailName);
            if (mailContentJson == null)
                return MailResult.MailFailed_UnknownTitle;

            string rcvName = mailObj.rcvName;
            GameClientPeer rcvPeer = GameApplication.Instance.GetCharPeer(rcvName);

            if (rcvPeer != null)//is online
            {
                //if player is online, deliver the mail
                DateTime createdTime = DateTime.Now;
                PrepareOnlineMail(mailObj, createdTime, null, rcvPeer); // Add mailobj to character's data
                ZLog_MailLogSent(mailObj, createdTime, rcvPeer);

                //Mails beyond limit will be removed at retrievemail
                bool limitbreak = rcvPeer.CharacterData.MailInventory.lstMailData.Count > MAIL_LIMIT;

                //inform player there is new mail...
                if (!limitbreak)
                {
                    rcvPeer.CharacterData.MailInventory.hasNewMail = true;
                    rcvPeer.ZRPC.CombatRPC.Ret_HasNewMail(true, mailContentJson.hud.ToString(), rcvPeer);
                }

                return (limitbreak) ? MailResult.MailFailed_InboxFull_Online :  MailResult.MailSuccess_MailSendOnline;
            }
            else
            {
                //player is offline, save the mailobj
                PrepareOfflineMailAsync(mailObj);
                return MailResult.MailSuccess_Offline;
            }
        }

        public MailResult SendRedemptionMail(MailObject mailObj)
        {
            MailContentJson mailContentJson = MailRepo.GetInfoByName(mailObj.mailName);

            if (mailContentJson == null)
            {
                //EFLog_InvalidMail(mailObj);
                return MailResult.MailFailed_UnknownTitle;
            }

            string rcvName = mailObj.rcvName;
            GameClientPeer rcvPeer = GameApplication.Instance.GetCharPeer(rcvName);

            if (rcvPeer != null)//is online
            {
                //Find how many mails this mailobject will create
                List<MailData> lstMailData = GenerateMailsViaMailObject(mailObj, DateTime.Now);

                //Exit if not enough mail space
                if (rcvPeer.CharacterData.MailInventory.lstMailData.Count + lstMailData.Count > MAIL_LIMIT)
                    return MailResult.MailFailed_InboxFull_Online;

                //if player is online, deliver the mail
                DateTime createdTime = DateTime.Now;
                PrepareOnlineMail(mailObj, createdTime, lstMailData, rcvPeer); // Add mailobj to character's data
                ZLog_MailLogSent(mailObj, createdTime, rcvPeer);

                //inform player there is new mail...
                if (rcvPeer.CharacterData.MailInventory.hasNewMail == false)
                {
                    rcvPeer.CharacterData.MailInventory.hasNewMail = true;
                    rcvPeer.ZRPC.CombatRPC.Ret_HasNewMail(true, mailContentJson.hud.ToString(), rcvPeer);
                }
                return MailResult.MailSuccess_MailSendOnline;
            }
            else
            {
                return MailResult.MailFailed_Offline;
            }
        }

        public void RetrieveMail(GameClientPeer rcvPeer)
        {
            MailInventoryData mailInvData = rcvPeer.CharacterData.MailInventory;    //Store all the mail
            List<MailData> lstMailData = mailInvData.lstMailData;

            //filter expired mail and sort
            //filter mail exceeding limit
            lstMailData.RemoveAll(mailData => DateTime.Now.Ticks > mailData.expiryTicks);   //remove expired mail
            lstMailData = lstMailData.OrderBy(o => o.expiryTicks).ToList();                 //Order mail from most recent => least recent
            if (lstMailData.Count > MAIL_LIMIT)
                lstMailData.RemoveRange(MAIL_LIMIT, lstMailData.Count - MAIL_LIMIT);         //Remove all mails beyond threshold

            foreach (var mailData in lstMailData)
            {
                mailData.mailStatus = MailStatus.Unread;
            }
            mailInvData.lstMailData = lstMailData;
            mailInvData.hasNewMail = false;//no more new mail

            string serializedMailString = JsonConvert.SerializeObject(mailInvData, Formatting.None, jsonSettingClient);
            //inform client, sending  non-expiried mails
            rcvPeer.ZRPC.CombatRPC.Ret_RetrieveMail(serializedMailString, rcvPeer);
        }

        public int OpenMail(GameClientPeer rcvPeer, int mailListIdx)
        {
            MailInventoryData mailInvData = rcvPeer.CharacterData.MailInventory;
            if (mailListIdx >= mailInvData.lstMailData.Count || mailListIdx < 0)
                return (int)MailReturnCode.OpenMail_Fail_InvalidIndex;
            MailData mailData = mailInvData.lstMailData[mailListIdx];
            mailData.mailStatus = MailStatus.Read;
            return (int)MailReturnCode.OpenMail_Success;
        }

        public int TakeAttachment(GameClientPeer rcvPeer, int mailIdx)
        {
            int mailReturnCode;
            MailInventoryData mailInvData = rcvPeer.CharacterData.MailInventory;
            //Make sure mail index is valid
            if (mailIdx >= mailInvData.lstMailData.Count || mailIdx < 0)
            {
                return (int)MailReturnCode.TakeAttachment_Fail_InvalidIndex;
            }

            MailData mailData = mailInvData.lstMailData[mailIdx];

            //Add items into inventory
            InvRetval invRetVal = rcvPeer.mInventory.AddItemsToInventory(mailData.lstIInventoryItem, true, "Mail");
            switch (invRetVal.retCode)
            {
                case InvReturnCode.AddSuccess:
                    //Add currency into inventory
                    foreach (var curr in mailData.dicCurrencyAmt)
                    {
                        rcvPeer.mPlayer.AddCurrency(curr.Key, curr.Value, "Mail");
                    }
                    if (mailData.dicCurrencyAmt.ContainsKey(CurrencyType.Gold) && mailData.hasTopupGold)
                    {
                        int gold = mailData.dicCurrencyAmt[CurrencyType.Gold];
                        if (gold > 0)
                            rcvPeer.mWelfareCtrlr.OnCredited(gold);
                    }
                    ZLog_MailLogTaken(mailData, rcvPeer.mChar);

                    mailData.isTaken = true;
                    DeleteMail(rcvPeer, mailIdx);
                    mailReturnCode = (int)MailReturnCode.TakeAttachment_Success;
                    break;
                case InvReturnCode.AddFailed:
                    mailReturnCode = (int)MailReturnCode.TakeAttachment_Fail_InventoryAddFailed;
                    break;
                case InvReturnCode.Full:
                    mailReturnCode = (int)MailReturnCode.TakeAttachment_Fail_InventoryFull;
                    break;
                default:
                    mailReturnCode = (int)MailReturnCode.TakeAttachment_Fail_InventoryUnknownRetCode;
                    break;
            }
            return mailReturnCode;
        }

        public int TakeAllAttachment(GameClientPeer rcvPeer, out string lstTakenMailIndexSerialStr)
        {
            int mailReturnCode;
            MailInventoryData mailInvData = rcvPeer.CharacterData.MailInventory;
            InvRetval invRetVal = null;
            int mailIdx = 0;
            List<int> lstTakenMailIndex = new List<int>();

            //Loop and add items into inventory
            foreach (MailData mailData in mailInvData.lstMailData)
            {
                if (mailData.isTaken)
                {
                    mailIdx++;
                    continue;
                }

                //Add item attachments into player's inventory
                invRetVal = rcvPeer.mInventory.AddItemsToInventory(mailData.lstIInventoryItem, true, "Mail");
                //Do nothing if items cannot be added
                if (invRetVal.retCode != InvReturnCode.AddSuccess)
                    break;

                //Add currency attachments into player's inventory
                foreach (var curr in mailData.dicCurrencyAmt)
                    rcvPeer.mPlayer.AddCurrency(curr.Key, curr.Value, "Mail");

                ZLog_MailLogTaken(mailData, rcvPeer.mChar);
                mailData.isTaken = true;
                mailData.mailStatus = MailStatus.Read;

                //Tell client which mail index has been taken, client will display items taken base on clientside mail data
                lstTakenMailIndex.Add(mailIdx++);
            }

            //Delete all mails that have their attachment taken
            DeleteAllMail(rcvPeer);

            //If there is no attachment to take
            if (invRetVal == null)
            {
                mailReturnCode = (int)MailReturnCode.TakeAllAttachment_Success;
                lstTakenMailIndexSerialStr = "";
                return mailReturnCode;
            }

            switch (invRetVal.retCode)
            {
                case InvReturnCode.AddSuccess:
                    mailReturnCode = (int)MailReturnCode.TakeAllAttachment_Success;
                    break;
                case InvReturnCode.AddFailed:
                    mailReturnCode = (int)MailReturnCode.TakeAllAttachment_Fail_InventoryAddFailed;
                    break;
                case InvReturnCode.Full:
                    mailReturnCode = (int)MailReturnCode.TakeAllAttachment_Fail_InventoryFull;
                    break;
                default:
                    mailReturnCode = (int)MailReturnCode.TakeAllAttachment_Fail_InventoryUnknownRetCode;
                    break;
            }
            lstTakenMailIndexSerialStr = JsonConvert.SerializeObject(lstTakenMailIndex);
            return mailReturnCode;
        }

        public int DeleteMail(GameClientPeer rcvPeer, int mailIndex)
        {
            MailInventoryData mailInvData = rcvPeer.CharacterData.MailInventory;
            if (mailIndex >= mailInvData.lstMailData.Count || mailIndex < 0)
            {
                return (int)MailReturnCode.DeleteMail_Fail_InvalidIndex;
            }
            else if (!mailInvData.lstMailData[mailIndex].isTaken)
            {
                return (int)MailReturnCode.DeleteMail_Fail_HasAttachment;
            }
            ZLog_MailLogRemove(mailInvData.lstMailData[mailIndex], rcvPeer.mChar);
            mailInvData.lstMailData.RemoveAt(mailIndex);

            return (int)MailReturnCode.DeleteMail_Success;
        }

        public int DeleteAllMail(GameClientPeer rcvPeer)
        {
            MailInventoryData mailInvData = rcvPeer.CharacterData.MailInventory;
            for (int i = mailInvData.lstMailData.Count - 1; i >= 0; i--)
            {
                if (mailInvData.lstMailData[i].isTaken)
                {
                    ZLog_MailLogRemove(mailInvData.lstMailData[i], rcvPeer.mChar);
                    mailInvData.lstMailData.RemoveAt(i);
                }
            }
            return (int)MailReturnCode.DeleteAllMail_Success;
        }
    }
}
