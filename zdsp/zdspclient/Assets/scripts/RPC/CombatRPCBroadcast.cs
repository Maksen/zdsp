using Kopio.JsonContracts;
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using Zealot.Common;
using Zealot.Common.RPC;
using Zealot.Repository;
using Newtonsoft.Json;

public partial class ClientMain : MonoBehaviour
{
    [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.BroadcastMessageToClient)]
    public void BroadcastMessageToClient(byte type, string parameters)
    {
        if (!GameInfo.mIsPlayerReady)
            return;
        string[] parameters_array = parameters.Split(';');
        switch ((BroadcastMessageType)type)
        {
            case BroadcastMessageType.BossSpawn:
                BroadcastBossSpawn(int.Parse(parameters_array[0]));
                break;
            case BroadcastMessageType.BossKilled:
                BroadcastBossKilled(int.Parse(parameters_array[0]), parameters_array[1]);
                break;
            case BroadcastMessageType.BossKilledMyDmg:
                BroadcastMyDamage(int.Parse(parameters_array[0]), parameters_array[1]);
                break;
            case BroadcastMessageType.BossKilledMyScore:
                BroadcastMyScore(parameters_array);
                break;
            case BroadcastMessageType.MonsterSpawn:
                BroadcastMonsterSpawn(int.Parse(parameters_array[0]), int.Parse(parameters_array[1]));
                break;
            case BroadcastMessageType.MonsterKilled:
                BroadcastMonsterKilled(int.Parse(parameters_array[0]), int.Parse(parameters_array[1]), parameters_array[2]);
                break;
            case BroadcastMessageType.GMActivityConfigChanged:
                BroadcastGMActivityConfigChanged(parameters);
                break;
            case BroadcastMessageType.NotifyActivityStart:
                BroadcastActivityStart(byte.Parse(parameters_array[0]));
                break;
            case BroadcastMessageType.StatusActivityStart:
                BroadcastActivityStatusStart(byte.Parse(parameters_array[0]));
                break;
            case BroadcastMessageType.StatusActivityEnd:
                BroadcastActivityStatusEnd(byte.Parse(parameters_array[0]));
                break;
            case BroadcastMessageType.NewServerEventConfigChanged:
                GameUtils.mNewServerEventEnabled = bool.Parse(parameters_array[0]);
                break;
            case BroadcastMessageType.ArenaRankUp:
                BroadcastArenaRankUp(parameters_array[0], parameters_array[1], parameters_array[2]);
                break;
            case BroadcastMessageType.TickerTapeMessage:
                //UIManager.GetWidget(HUDWidgetType.TickerTape).GetComponent<HUD_TickerTape>().SetAnnoucementText(parameters);
                break;
            case BroadcastMessageType.AuctionBegin:
                BroadcastAuctionBegin(int.Parse(parameters_array[0]), parameters_array[1]);
                break;
            case BroadcastMessageType.AuctionEnd:
                BroadcastAuctionEnd(byte.Parse(parameters_array[0]), int.Parse(parameters_array[1]), parameters_array[2],
                                    parameters_array[3], int.Parse(parameters_array[4]), parameters_array[5]);
                break;
            case BroadcastMessageType.AuctionChanged:
                BroadcastAuctionChanged(parameters);
                break;
            //case BroadcastMessageType.RandomBoxRewardActive:
            //    {
            //        HUD_RandomChest hudRandomChest = UIManager.GetWidget(HUDWidgetType.RandomChest).GetComponent<HUD_RandomChest>();
            //        if (hudRandomChest != null)
            //        {
            //            hudRandomChest.OnActived(parameters);
            //        }
            //    }
            //    break;
            //case BroadcastMessageType.RandomBoxRewardExprie:
            //    {
            //        HUD_RandomChest hudRandomChest = UIManager.GetWidget(HUDWidgetType.RandomChest).GetComponent<HUD_RandomChest>();
            //        if (hudRandomChest != null)
            //        {
            //            hudRandomChest.OnExpried(parameters);

            //        }
            //    }
            //    break;
            case BroadcastMessageType.RareItemNotification:
                int itemid = int.Parse(parameters_array[0]);
                string playername = parameters_array[1];
                string itemname = string.Empty;
                ItemBaseJson itemjson = GameRepo.ItemFactory.GetItemById(itemid);
                if (itemjson != null)
                {
                    itemname = itemjson.localizedname;
                }
                Dictionary<string, string> param = new Dictionary<string, string>();
                param.Add("item", itemname);
                param.Add("name", playername);
                string message = GUILocalizationRepo.GetLocalizedSysMsgByName("sys_RareItem_Message", param);
                //UIManager.GetWidget(HUDWidgetType.TickerTape).GetComponent<HUD_TickerTape>().SetAnnoucementText(message);
                break;
            case BroadcastMessageType.NewDay:
                Debug.Log("NewDay");
                GameInfo.gLocalPlayer.OnNewDay();
                break;
            case BroadcastMessageType.SystemSwitchChange:
                {
                    string chStr = "[SystemSwitch:BroadcastChange] " + parameters;
                    Debug.Log(chStr);
                    SystemSwitchData mSysSwitch = GameInfo.gLocalPlayer.mSysSwitch;
                    mSysSwitch.OnChange(parameters);
                    Debug.Log("[closeSwitch] " + mSysSwitch.GetSemicolonList());
                }
                break;
            case BroadcastMessageType.GainExperience:
                {
                    //Debug.Log("Gain Experience");
                   // UIManager.GetWidget(HUDWidgetType.ExpGain).GetComponent<HUD_ShowGetXP>().AddXPGain(parameters);
                    //UIManager.ShowSystemMessage("*** Gained " + parameters + " experience ***", false);
                }
                break;
            case BroadcastMessageType.GMMessageChanged:
                GMMessageChanged(parameters);
                break;
            case BroadcastMessageType.MessageBroadcaster:
                MessageBroadcaster(int.Parse(parameters_array[0]) == 1, parameters_array[1]);
                break;
        }
    }

    private void BroadcastBossSpawn(int id)
    {
        SpecialBossJson info = SpecialBossRepo.GetInfoById(id);
        if (info == null)
            return;
        LevelJson level_info = LevelRepo.GetInfoById(info.location);
        NPCJson archetypeInfo = CombatNPCRepo.GetNPCById(info.archetypeid);
        Dictionary<string, string> param = new Dictionary<string, string>();
        param.Add("area", level_info.localizedname);
        param.Add("boss", archetypeInfo.localizedname);
        string message = GUILocalizationRepo.GetLocalizedSysMsgByName("sys_Boss_Spawn", param);
        //HUD.Combat.SetTickerMessage(message);
        UIManager.ShowSystemMessage(message, true);
    }

    private string GetMonsterKilledMessage(string lvlName, string monName, string killer)
    {
        Dictionary<string, string> param = new Dictionary<string, string>();
        param.Add("area", lvlName);
        param.Add("boss", monName);
        param.Add("killer", killer);
        return GUILocalizationRepo.GetLocalizedSysMsgByName("sys_Boss_Killed", param);
    }

    private void BroadcastBossKilled(int id, string killer)
    {
        SpecialBossJson info = SpecialBossRepo.GetInfoById(id);
        if (info == null)
            return;
        LevelJson level_info = LevelRepo.GetInfoById(info.location);
        NPCJson archetypeInfo = CombatNPCRepo.GetNPCById(info.archetypeid);
        string message = GetMonsterKilledMessage(level_info.localizedname, archetypeInfo.localizedname, killer);
        //HUD.Combat.SetTickerMessage(message);
        UIManager.ShowSystemMessage(message, true);
    }

    private void BroadcastMyDamage(int archetypeid, string damage)
    {
        NPCJson archetypeInfo = CombatNPCRepo.GetNPCById(archetypeid);
        Dictionary<string, string> param = new Dictionary<string, string>();
        param.Add("boss", archetypeInfo.localizedname);
        param.Add("damage", damage);
        string message = GUILocalizationRepo.GetLocalizedSysMsgByName("sys_Boss_MyDamage", param);
        UIManager.AddToChat((byte)MessageType.System, message);
    }

    private void BroadcastMyScore(string[] parameters_array)
    {
        NPCJson archetypeInfo = CombatNPCRepo.GetNPCById(int.Parse(parameters_array[0]));
        string message = "";
        Dictionary<string, string> param = new Dictionary<string, string>();
        param.Add("boss", archetypeInfo.localizedname);
        if (parameters_array.Length > 2)
        {
            param.Add("partyscore", parameters_array[1]);
            message = GUILocalizationRepo.GetLocalizedSysMsgByName("sys_Boss_PartyScore", param);
            for (int index = 2; index < parameters_array.Length; index++)
            {
                string memberscore = parameters_array[index];
                if (string.IsNullOrEmpty(memberscore))
                    continue;
                message += "\n" + memberscore;
            }
        }
        else
        {
            param.Add("myscore", parameters_array[1]);
            message = GUILocalizationRepo.GetLocalizedSysMsgByName("sys_Boss_MyScore", param);
        }
        UIManager.AddToChat((byte)MessageType.System, message);
    }

    private void BroadcastMonsterSpawn(int archetypeid, int levelid)
    {
        NPCJson archetypeInfo = CombatNPCRepo.GetNPCById(archetypeid);
        LevelJson level_info = LevelRepo.GetInfoById(levelid);
        Dictionary<string, string> param = new Dictionary<string, string>();
        param.Add("area", level_info.localizedname);
        param.Add("boss", archetypeInfo.localizedname);
        string message = GUILocalizationRepo.GetLocalizedSysMsgByName("sys_Boss_Spawn", param);
        //HUD.Combat.SetTickerMessage(message);
        UIManager.AddToChat((byte)MessageType.System, message);
    }

    private void BroadcastMonsterKilled(int archetypeid, int levelid, string killer)
    {
        NPCJson archetypeInfo = CombatNPCRepo.GetNPCById(archetypeid);
        LevelJson level_info = LevelRepo.GetInfoById(levelid);
        string message = GetMonsterKilledMessage(level_info.localizedname, archetypeInfo.localizedname, killer);
        //HUD.Combat.SetTickerMessage(message);
        UIManager.AddToChat((byte)MessageType.System, message);
    }

    private void BroadcastGMActivityConfigChanged(string configs)
    {
        OnGMActivityConfigChanged(configs);
    }

    private void OnGMActivityConfigChanged(string configs)
    {
        GMActivityConfig.CleanUp();
        string[] configs_array = configs.TrimEnd(';').Split(';');
        foreach (string entry in configs_array)
        {
            if (string.IsNullOrEmpty(entry))
                continue;
            string[] config = entry.Split('|');
            GMActivityType configType = (GMActivityType)(byte.Parse(config[0]));
            DateTime start = DateTime.ParseExact(config[1], "yyyy/MM/dd HH:mm", null);
            DateTime end = DateTime.ParseExact(config[2], "yyyy/MM/dd HH:mm", null);
            List<int> datalist = new List<int>();
            for (int index = 3; index < config.Length; index++)
                datalist.Add(int.Parse(config[index]));
            GMActivityConfig.AddConfig(configType, start, end, datalist);
        }
    }

    private void BroadcastActivityStart(byte notificationtype)
    {
        /*
        RealmJson realminfo = GameInfo.mRealmInfo;
        NotificationType notficiationType = (NotificationType)notificationtype;
        switch (notficiationType)
        {
            case NotificationType.LeiYinTa:
                GameUtils.SetActivityStatus(ActivityStatusBitIndex.LeiYinTa);
                break;
            case NotificationType.TianZiWar:
                GameUtils.SetActivityStatus(ActivityStatusBitIndex.TianZiZhan);
                break;
            case NotificationType.YiZuWar:
                GameUtils.SetActivityStatus(ActivityStatusBitIndex.YiZuZhan);
                break;
            case NotificationType.ResourceDouble:
                GameUtils.SetActivityStatus(ActivityStatusBitIndex.ResourceDouble);
                if (realminfo != null && realminfo.type == RealmType.ActivityResourceMap)
                    return;
                break;
            case NotificationType.YunBiao:
                GameUtils.SetActivityStatus(ActivityStatusBitIndex.YunBiao);
                if (realminfo != null && realminfo.type == RealmType.ActivityResourceMap)
                {
                    //UIManager.GetWidget(HUDWidgetType.InstanceObjective).GetComponent<UI_RealmObjective>().OnYunBiaoChanged();
                    GameObject yunbiaopath = GameInfo.gCombat.YunBiaoPath;
                    if (yunbiaopath != null)
                        yunbiaopath.SetActive(true);
                    string message = GUILocalizationRepo.GetLocalizedSysMsgByName("sys_YunBiaoStart", null);
                    UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("sys_YunBiaoStart", null), false);
                    return;
                }
                break;
        }
        //UIManager.SystemMsgManager.ShowSystemMessage .AddNotification(notficiationType, 0.0f);
        */
    }

    private void BroadcastActivityStatusStart(byte activityStatus)
    {
        /*ActivityStatusBitIndex status = (ActivityStatusBitIndex)activityStatus;
        GameUtils.SetActivityStatus(status);
        if (status == ActivityStatusBitIndex.TianZiShouChang)
        {
            if (WM.Instance.IsActivated(WinPanel.Nation))
                WM.Instance[WinPanel.Nation].GetComponent<UI_Nation>().RefreshTabNationHint();
        }*/
    }

    private void BroadcastActivityStatusEnd(byte activityStatus)
    {
        /*ActivityStatusBitIndex status = (ActivityStatusBitIndex)activityStatus;
        GameUtils.ClearActivityStatus(status);
        if (status == ActivityStatusBitIndex.LeiYinTa)
            Debug.Log("HUD.Combat.ActiveMessage.RemoveNotification(NotificationType.LeiYinTa);");
        else if (status == ActivityStatusBitIndex.TianZiShouChang)
        {
            if (WM.Instance.IsActivated(WinPanel.Nation))
                WM.Instance[WinPanel.Nation].GetComponent<UI_Nation>().RefreshTabNationHint();
        }
        else if (status == ActivityStatusBitIndex.YunBiao)
        {
            GameObject yunbiaopath = GameInfo.gCombat.YunBiaoPath;
            if (yunbiaopath != null)
                yunbiaopath.SetActive(false);
        }*/
    }

    private void BroadcastArenaRankUp(string playername, string rank, string opponentname)
    {
        Dictionary<string, string> param = new Dictionary<string, string>();
        param.Add("playername", playername);
        param.Add("rank", rank);
        param.Add("opponentname", opponentname);
        string message = GUILocalizationRepo.GetLocalizedSysMsgByName("sys_ArenaRankUp", param);
        //UIManager.GetWidget(HUDWidgetType.TickerTape).GetComponent<HUD_TickerTape>().SetAnnoucementText(message);
    }

    private void BroadcastAuctionBegin(int itemid, string endtime)
    {
        ItemBaseJson item = GameRepo.ItemFactory.GetItemById(itemid);
        DateTime endDT = DateTime.ParseExact(endtime, "yyyy/MM/dd HH:mm", CultureInfo.InvariantCulture);
        Dictionary<string, string> param = new Dictionary<string, string>();
        param.Add("item", item.localizedname);
        param.Add("time", string.Format("{0:D2}:{1:D2}", endDT.Hour, endDT.Minute));
        string message = GUILocalizationRepo.GetLocalizedString("auction_Begin", param);
        //UIManager.GetWidget(HUDWidgetType.TickerTape).GetComponent<HUD_TickerTape>().SetAnnoucementText(message);
        UIManager.AddToChat((byte)MessageType.System, message);

        // set alert
        GameUtils.mAuctionStatus = GameUtils.SetBit(GameUtils.mAuctionStatus, (int)AuctionStatusBit.AuctionOpen);
        GameInfo.gLocalPlayer.SetAuctionAlert();
    }

    private void BroadcastAuctionEnd(byte result, int itemId, string serverName, string bidder, int price, string biddersListStr)
    {      
        ItemBaseJson item = GameRepo.ItemFactory.GetItemById(itemId);      
        Dictionary<string, string> param = new Dictionary<string, string>();
        param.Add("item", item.localizedname);
        string message = "";

        if (result == 0)  // no bid
            message = GUILocalizationRepo.GetLocalizedString("auction_NoBid", param);
        else
        {
            param.Add("server", serverName);
            param.Add("name", bidder);
            param.Add("price", price.ToString("N0"));
            message = GUILocalizationRepo.GetLocalizedString("auction_Congratulations", param);
            GameUtils.mAuctionStatus = GameUtils.SetBit(GameUtils.mAuctionStatus, (int)AuctionStatusBit.NewRecord);
        }

        //if (result != 0)  // only for win bid
        //UIManager.GetWidget(HUDWidgetType.TickerTape).GetComponent<HUD_TickerTape>().SetAnnoucementText(message);
        UIManager.AddToChat((byte)MessageType.System, message);

        GameUtils.mAuctionStatus = GameUtils.UnsetBit(GameUtils.mAuctionStatus, (int)AuctionStatusBit.AuctionOpen);

        List<string> biddersList = JsonConvert.DeserializeObject<List<string>>(biddersListStr);
        if (biddersList != null && biddersList.Count > 0)
        {
            if (biddersList.Contains(GameInfo.gLocalPlayer.Name))
                GameUtils.mAuctionStatus = GameUtils.SetBit(GameUtils.mAuctionStatus, (int)AuctionStatusBit.CollectionAvailable);
        }

        // set alert
        GameInfo.gLocalPlayer.SetAuctionAlert();
    }

    private void BroadcastAuctionChanged(string parameters)
    {      
        PushNotificationJson sysnoti = PushNotificationRepo.GetPushNotificationByID(8);
        if (sysnoti != null)
        {
            DateTime beginDt = DateTime.ParseExact(parameters, "yyyy/MM/dd HH:mm", CultureInfo.InvariantCulture);
            int seconds = (int)(beginDt.Subtract(DateTime.Now).TotalSeconds);
        }
    }

    private void GMMessageChanged(string message)
    {
        List<GMMessageData> messageList = JsonConvertDefaultSetting.DeserializeObject<List<GMMessageData>> (message);
        TickerTapeSystem.Instance.RefreshGMMessage(messageList);
    }

    private void MessageBroadcaster(bool emergency, string message)
    {
        string _message = GUILocalizationRepo.GetLocalizedSysMsgByName(message);
        if (emergency)
            UIManager.ShowEventNotification(_message);
        else
            UIManager.ShowSystemMessage(_message, true);
    }
}
