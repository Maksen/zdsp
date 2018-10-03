using Kopio.JsonContracts;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Zealot.Common;
using Zealot.Common.RPC;
using Zealot.Common.Entities;
using Zealot.Client.Entities;
using Zealot.Repository;

#if ZEALOT_DEVELOPMENT
public static class ConsoleVariables
{
    public static bool ShowControlStatus = false;
    public static bool ShowItemID = false;
    public static float DashDuration = 0.4f;
}
#endif

public static class CommandUtils
{
    private static CommandManager mCmdManager = new CommandManager();
    private static readonly int cmdHistoryLimit = 20;
    private static Queue<string> commandHistory = new Queue<string>();

#if ZEALOT_DEVELOPMENT
    public static bool ExecuteCommand(string cmd)
    {
        commandHistory.Enqueue(cmd);
        if (commandHistory.Count > cmdHistoryLimit)
            commandHistory.Dequeue();

        if (cmd.Length > 0 && cmd[0] == '\\')
        {
            string[] cmdObjs = cmd.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string methodname = cmdObjs[0].ToLower();
            methodname = methodname.Substring(1);
            bool closeUI = false;
            List<string> param = new List<string>();
            int cmdObjsLen = cmdObjs.Length;
            for (int i = 1; i < cmdObjsLen; ++i)
            {
                if (cmdObjs[i] == "-c" || cmdObjs[i] == "-C")
                    closeUI = true;
                else
                    param.Add(cmdObjs[i]);
            }
            return mCmdManager.Execute(methodname, param.ToArray(), closeUI);
        }
        return false;
    }

    public static List<string> GetCommandsByStartStr(string str)
    {
        return mCmdManager.GetCommandsByStartStr(str);
    }

    public static string[] GetCommandsHistory()
    {
        return commandHistory.ToArray();
    }
#endif 
}

public class ConsoleCmdAttribute : Attribute
{
    public string mDesc;
    public string mDescExt;
    public ConsoleCmdAttribute(string desc, string descext = "")
    {
        mDesc = desc;
        mDescExt = descext;
    }
}

public class CommandManager
{
    public bool Execute(string methodname, string[] param, bool closeUI = false)
    {
        MethodInfo mi = this.GetType().GetMethod(methodname, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.IgnoreCase);
        if (mi != null)
        {
            ConsoleCmdAttribute[] cmdDescAttributes = (ConsoleCmdAttribute[])mi.GetCustomAttributes(typeof(ConsoleCmdAttribute), true);
            if (cmdDescAttributes.Length == 0)
                return false;

            mi.Invoke(this, new object[] { param });
            if (closeUI)
                UIManager.CloseWindow(WindowType.ConsoleCommand);

            return true;
        }
        return false;
    }

    #region Developer Commands

#if ZEALOT_DEVELOPMENT

    private void PrintToConsole(string info)
    {
        GameObject consoleCmdObj = UIManager.GetWindowGameObject(WindowType.ConsoleCommand);
        if (consoleCmdObj != null)
            consoleCmdObj.GetComponent<UI_ConsoleCommand>().AddToContent(info);
    }

    public List<string> GetCommandsByStartStr(string str)
    {
        List<string> res = new List<string>();
        str = str.ToLower();
        MethodInfo[] infos = this.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
        foreach (MethodInfo info in infos)
        {
            ConsoleCmdAttribute[] cmdDescAttributes = (ConsoleCmdAttribute[])info.GetCustomAttributes(typeof(ConsoleCmdAttribute), true);
            if (cmdDescAttributes.Length > 0)
            {
                if (info.Name.ToLower().StartsWith(str))
                {
                    res.Add(info.Name);
                }
            }
        }
        return res;
    }

    [ConsoleCmd("Shows a list of console commands available, which is what you are seeing now.")]
    public void Help(string[] param)
    {
        UI_ConsoleCommand cc = null;
        GameObject go = UIManager.GetWindowGameObject(WindowType.ConsoleCommand);
        if (go != null)
            cc = go.GetComponent<UI_ConsoleCommand>();
        if (cc == null)
            return;

        HashSet<string> mnames = new HashSet<string>();
        foreach (string mn in param)
        {
            mnames.Add(mn.ToLower());
        }
        MethodInfo[] infos = this.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
        if (mnames.Count <= 0)
        {
            foreach (MethodInfo info in infos)
            {
                ConsoleCmdAttribute[] cmdDescAttributes = (ConsoleCmdAttribute[])info.GetCustomAttributes(typeof(ConsoleCmdAttribute), true);
                foreach (ConsoleCmdAttribute cmdDesc in cmdDescAttributes)
                {
                    string description = info.Name + " > " + cmdDesc.mDesc;
                    cc.AddToContent(description);
                    break;
                }
            }
        }
        else
        {
            foreach (MethodInfo info in infos)
            {
                bool match = false;
                bool exactmatch = false;
                foreach (string mn in mnames)
                {
                    if (info.Name.ToLower().Contains(mn))
                    {
                        if (info.Name.ToLower() == mn)
                        {
                            exactmatch = true;
                        }
                        match = true;
                        break;
                    }
                }
                if (match)
                {
                    ConsoleCmdAttribute[] cmdDescAttributes = (ConsoleCmdAttribute[])info.GetCustomAttributes(typeof(ConsoleCmdAttribute), true);
                    foreach (ConsoleCmdAttribute cmdDesc in cmdDescAttributes)
                    {
                        string description = info.Name + " > " + cmdDesc.mDesc;
                        cc.AddToContent(description);
                        if (exactmatch)
                        {
                            if (cmdDesc.mDescExt != "")
                            {
                                cc.AddToContent(cmdDesc.mDescExt);
                            }
                        }
                        break;
                    }
                }
            }
        }
    }

    [ConsoleCmd("Clear chat log")]
    public void CLS(string[] param)
    {
        GameObject consoleCmdObj = UIManager.GetWindowGameObject(WindowType.ConsoleCommand);
        if (consoleCmdObj != null)
            consoleCmdObj.GetComponent<UI_ConsoleCommand>().ClearContent();
    }

    [ConsoleCmd("Revive")]
    public void Revive(string[] param)
    {
        //RPCFactory.CombatRPC.RespawnAtSafeZoneWithCost();
        RPCFactory.CombatRPC.RespawnOnSpot(true);
    }

    [ConsoleCmd("Parameters: itemId amount")]
    public void AddItem(string[] param)
    {
        if (param.Length > 1)
        {
            int itemId = 0, amount = 0;
            int.TryParse(param[0], out itemId);
            int.TryParse(param[1], out amount);
            if (itemId > 0 && amount != 0)
                RPCFactory.CombatRPC.AddItem(itemId, amount); // Check if itemId is 0 and negative amount is allowed
            else
                PrintToConsole("AddItem: invalid parameter");
        }
        else
        {
            PrintToConsole("Format: \\AddItem <itemid> <amount>");
        }
    }

    [ConsoleCmd("Parameters: itemIdStart itemIdEnd amount")]
    public void AddItemRange(string[] param)
    {
        if (param.Length > 2)
        {
            int itemIdStart = 0, itemIdEnd = 0, amount = 0;
            int.TryParse(param[0], out itemIdStart);
            int.TryParse(param[1], out itemIdEnd);
            int.TryParse(param[2], out amount);
            if (amount != 0)
            {
                for (int itemid = itemIdStart; itemid <= itemIdEnd; ++itemid)
                    RPCFactory.CombatRPC.AddItem(itemid, amount); // Check if itemId is 0 and negative amount is allowed
            }
            else
                PrintToConsole("AddItemRange: invalid parameter");
        }
        else
        {
            PrintToConsole("Format: \\AddManyItem <itemid> <itemid> <amount>");
        }
    }

    [ConsoleCmd("Parameters: slotId amount")]
    public void UseItem(string[] param)
    {
        if (param.Length > 1)
        {
            int slotId = 0, amount = 0;
            int.TryParse(param[0], out slotId);
            int.TryParse(param[1], out amount);
            if (amount > 0)
                RPCFactory.CombatRPC.UseItem(slotId, amount);
            else
                PrintToConsole("UseItem: invalid parameter");
        }
        else
            PrintToConsole("Format: \\UseItem <slotId> <amount>");
    }

    [ConsoleCmd("UnequipItem Parameters: slotId")]
    public void UnequipItem(string[] param)
    {
        if (param.Length == 1)
        {
            int slotId = 0;
            if (int.TryParse(param[0], out slotId) && GameInfo.gLocalPlayer.mEquipmentInvData.GetEquipmentBySlotId(slotId) != null)
            {
                RPCFactory.CombatRPC.UnequipItem(slotId, false);
                return;
            }
        }
        else
            PrintToConsole("Format: \\UnequipItem <slotId>");
    }

    [ConsoleCmd("ClearItemInventory")]
    public void ClearItemInventory(string[] param)
    {
        RPCFactory.NonCombatRPC.ConsoleClearItemInventory();
    }

    [ConsoleCmd("OpenItemSlot")]
    public void OpenItemSlot(string[] param)
    {
        if (param.Length > 0)
        {
            int numSlotsToUnlock = 0;
            int.TryParse(param[0], out numSlotsToUnlock);
            RPCFactory.CombatRPC.OpenNewSlot(numSlotsToUnlock);
        }
    }

    [ConsoleCmd("Get inventory item info by id", @"Example: \Inventory 1")]
    public void GetInventoryItemInfo(string[] param)
    {
        if (param.Length > 0)
        {
            int id;
            if (int.TryParse(param[0], out id) && id > 0)
                RPCFactory.NonCombatRPC.ConsoleGetInventoryItemInfo(id);
            else
                PrintToConsole("GetInventoryItemInfo: invalid id");
        }
        else
        {
            PrintToConsole("Format: \\GetInventoryItemInfo <id>");
        }
    }

    [ConsoleCmd("Parameters: CurrencyType amount",
@" Format: \addcurrency < CurrencyType > < amount > 
    Example: \addcurrency Gold 1000000000
    Can add:
    Money Gold LockGold GuildGold GuildContribution
    ")]
    public void AddCurrency(string[] param)
    {
        if (param.Length > 1)
        {
            try
            {
                var result = Enum.Parse(typeof(CurrencyType), param[0], true);
                int currency_type = (int)result;
                int amount;
                if (int.TryParse(param[1], out amount) && amount != 0)
                    RPCFactory.CombatRPC.AddCurrency(currency_type, amount);
            }
            catch
            {
                PrintToConsole("AddCurrency: invalid currency type.");
            }
        }
        else
        {
            PrintToConsole("Format: \\AddCurrency <CurrencyType> <Amount>");
        }
    }

    [ConsoleCmd("Parameters:amount",
@" Format: \TestCombatFormula  < amount > 
    ")]
    public void TestCombatFormula(string[] param)
    {
        if (param.Length > 0)
        {
            int amount;
            if (int.TryParse(param[0], out amount))
                RPCFactory.NonCombatRPC.ConsoleTestCombatFormula(amount);
        }
        else
        {
            PrintToConsole("Format: \\AddCurrency <CurrencyType> <Amount>");
        }
    }

    [ConsoleCmd("Parameters: msgid parameters")]
    public void BroadcastMessageId(string[] param)
    {
        if (param.Length > 0)
        {
            int msgid = -1;
            string s = "";
            if (param.Length == 1)
            {
                msgid = int.Parse(param[0]);
            }
            else if (param.Length == 2)
            {
                msgid = int.Parse(param[0]);
                s = param[1];
            }
            if (msgid != -1)
                RPCFactory.CombatRPC.BroadcastSysMsgToServer(msgid, s);
        }
        else
        {
            PrintToConsole("Format: \\BroadcastMessageId <msgid> <parameters>");
        }
    }

    [ConsoleCmd("Parameters: message")]
    public void BroadcastMessage(string[] param)
    {
        string s = "";
        foreach (string m in param)
        {
            s += m;
            s += " ";
        }
        RPCFactory.CombatRPC.BroadcastSysMsgToServer(-1, s);
    }

    [ConsoleCmd("Parameters: storetype productid")]
    public void VerifyReceipt(string[] param)
    {
        //int storetype = int.Parse(param[0]);
        //RPCFactory.CombatRPC.VerifyReceipt("", (byte)storetype, int.Parse(param[1]));

        //string productId = param[0].ToString();
        //RPCFactory.CombatRPC.VerifyReceipt("billingId");
    }

    [ConsoleCmd("Display Combat Stats of your player")]
    public void ShowCombatStats(string[] param)
    {
        RPCFactory.NonCombatRPC.RequestCombatStats();
        //RPCFactory.CombatRPC.CallFromClient(1, "test", 2.34f, new Vector3(7,8,9).ToRPCPosition()); //test
    }

    [ConsoleCmd("Display Local Skill Combat Stats")]
    public void ShowLocalSkillCombatStats(string[] param)
    {
        string msg = "";
        LocalSkillPassiveStats skillstats = GameInfo.gLocalPlayer.LocalSkillPassiveStats;
        msg += "HealthMax:" + skillstats.HealthMax;
        msg += "Attack:" + skillstats.Attack;
        msg += "Accuracy:" + skillstats.Accuracy;
        msg += "Critical:" + skillstats.Critical;
        msg += "CriticalDamage:" + skillstats.CriticalDamage;
        msg += "Evasion:" + skillstats.Evasion;
        msg += "Armor:" + skillstats.Armor;
        msg += "Cocritical:" + skillstats.CoCritical;
        //msg += "CocriticalDamage:" + skillstats.CoCriticalDamage;
        GameObject consoleCmdObj = UIManager.GetWindowGameObject(WindowType.ConsoleCommand);
        if (consoleCmdObj != null)
            consoleCmdObj.GetComponent<UI_ConsoleCommand>().AddToContent(msg);
    }

    [ConsoleCmd("Display Combat Stats of AIPlayer")]
    public void ShowArenaAICombatStats(string[] param)
    {
        RPCFactory.NonCombatRPC.RequestArenaAICombatStats();
    }

    [ConsoleCmd("Display Side effects on your player")]
    public void ShowSE(string[] param)
    {
        RPCFactory.NonCombatRPC.RequestSideEffectsInfo();
    }

    [ConsoleCmd("Parameters: targetSceneName",
        @"only works for world level, will teleport player to level respawn point. teleport by this command will ignore the level requirement check.
    Example: \TeleportToWorldMap lingyunfeng")]
    public void TeleportToWorldMap(string[] param)
    {
        if (param.Length == 1)
        {
            RPCFactory.NonCombatRPC.ConsoleTeleportToLevel(param[0]);
        }
        else
        {
            PrintToConsole("Format: \\TeleportToWorldMap <targetSceneName>");
        }
    }

    [ConsoleCmd("Show Control status e.g. stun, silence. Parameters: [0|1]")]
    public void ShowControlStatus(string[] param)
    {
        int flag;
        if (param.Length == 0)
        {
            PrintToConsole("Format: \\ShowControlStatus [0|1]");
            return;
        }
        bool valid = int.TryParse(param[0], out flag);
        if (valid)
            ConsoleVariables.ShowControlStatus = flag > 0 ? true : false;
    }

    [ConsoleCmd("Parameters: sideID")]
    public void AddSideEffect(string[] param)
    {
        int seID;
        if (param.Length == 0)
        {
            PrintToConsole("Format: \\AddSideEffect sideID");
            return;
        }
        int pid = -1; //the localplayer;
        if (param.Length == 2)
        {
            pid = GameInfo.gLocalPlayer.Bot.QueryForNonSpecificTarget(Zealot.Bot.BotController.MaxQueryRadius, true, null).GetPersistentID();
        }
        bool valid = int.TryParse(param[0], out seID);
        if (valid)
        {
            RPCFactory.NonCombatRPC.ConsoleAddSideEffect(seID, pid);
            Debug.Log("added side effect " + seID + " successfully");
        }
    }


    [ConsoleCmd("Parameters: effTypeID, dur")]
    public void TestSideEffect(string[] param)
    {
        int seTypeID;
        if (param.Length == 0)
        {
            PrintToConsole("Format: \\TestSideEffect effTypeID, dur");
            return;
        }
        int pid = -1;
        if (GameInfo.gSelectedEntity != null)
        {
            ActorGhost actor = GameInfo.gSelectedEntity as ActorGhost;
            if (actor != null)
            {
                pid = actor.GetPersistentID();
            }
        }
        bool valid = int.TryParse(param[0], out seTypeID);
        if (valid)
        {
            string others = "";
            if (param.Length > 1)
                others = param[1];
            RPCFactory.NonCombatRPC.ConsoleTestSideEffect(seTypeID, pid, others);
            EffectType setype = (EffectType)seTypeID;
            Debug.Log("Test side effect " + setype.ToString() + " and pid " + pid);
        }
    }

    [ConsoleCmd("Parameters: skillgpid, level, skill duration, mainseType, subseType, othermain, othersub")]
    public void TestSkillComboAndCast(string[] param)
    {
        if (param.Length < 5)
        {
            PrintToConsole("Format: \\TestSkillComboAndCast skillgpid  level  skillduration  mainseType  subseType  othermain  othersub");
            PrintToConsole("Format for othermain(othersub) :  dur,interval,max,parameter,stat1,stat2,isrelative,usereference,step,increase,ispersistendeath,ispersistentlogout");
            //PrintToConsole("E.g. :   \\TestSkillComboAndCast 1 2 300 902 10,0,1,0,0,0,false,true dummy_for_sub");
            return;
        }
        int mskillid = 0;
        int mainsetype = 0;
        int subsetype = 0;
        int lvl = 1;
        float skilldur = 1.0f;
        int.TryParse(param[0], out mskillid);
        int.TryParse(param[1], out lvl);
        float.TryParse(param[2], out skilldur);
        int.TryParse(param[3], out mainsetype);
        int.TryParse(param[4], out subsetype);

        string otherParamsForMain = "";
        if (param.Length > 5)
            otherParamsForMain = param[5];

        string otherParamsForSub = "";
        if (param.Length > 6)
            otherParamsForSub = param[6];
        if (mskillid == 0)
            return;
        SideEffectJson sejmain = CombatUtils.SetupTestSideEffect(mainsetype, false, otherParamsForMain);
        SideEffectJson sej = CombatUtils.SetupTestSideEffect(subsetype, true, otherParamsForSub);

        GameInfo.gLocalPlayer.TestComboSkill(mskillid, sejmain, sej, lvl, skilldur);
        RPCFactory.NonCombatRPC.ConsoleTestComboSkill(mskillid, lvl, skilldur, mainsetype, subsetype, otherParamsForMain, otherParamsForSub);
    }

    [ConsoleCmd("Parameters: sideID")]
    public void AddSideEffectToTarget(string[] param)
    {
        int seID;
        if (param.Length == 0)
        {
            PrintToConsole("Format: \\AddSideEffectToTarget sideID");
            return;
        }
        bool valid = int.TryParse(param[0], out seID);
        if (valid)
        {
            if (GameInfo.gSelectedEntity != null)
            {
                NetEntityGhost neGhost = GameInfo.gSelectedEntity as NetEntityGhost;
                if (neGhost != null)
                {
                    int pid = neGhost.GetPersistentID();
                    RPCFactory.NonCombatRPC.ConsoleAddSideEffect(seID, pid);
                    Debug.Log("Adding side effect " + seID + " to entity of pid " + pid);
                }
            }
            else
            {
                Debug.Log("Invalid selected entity");
            }
        }
    }

    [ConsoleCmd("teleport player to pos in level",
        @"Example: \TeleportToPos 100 0 100")]
    public void TeleportToPos(string[] param)
    {
        float x, y, z;
        if (param.Length == 3 && float.TryParse(param[0], out x) && float.TryParse(param[1], out y) && float.TryParse(param[2], out z))
        {
            Vector3 pos = new Vector3(x, y, z);
            RPCFactory.NonCombatRPC.ConsoleTeleportToPosInLevel(pos.ToRPCPosition());
        }
        else
        {
            PrintToConsole("Format: \\TeleportToPos <pos.x> <pos.y> <pos.z>");
        }
    }

    [ConsoleCmd("add experience to player",
        @"Example: \AddExperience 9999")]
    public void AddExperience(string[] param)
    {
        if (param.Length > 0)
        {
            int exp;
            if (int.TryParse(param[0], out exp) && exp > 0)
                RPCFactory.NonCombatRPC.ConsoleAddExperience(exp);
            else
                PrintToConsole("AddExperience: invalid experience value");
        }
        else
        {
            PrintToConsole("Format: \\AddExperience <val>");
        }
    }

    [ConsoleCmd("Add Stats Points", @"Example: \AddStatsPoint 999")]
    public void AddStatsPoint(string[] param)
    {
        if (param.Length > 0)
        {
            int amt;
            if (int.TryParse(param[0], out amt))
                RPCFactory.NonCombatRPC.ConsoleAddStatsPoint(amt);
        }
        else
        {
            PrintToConsole("Format: \\AddStatsPoint <val>");
        }
    }

    [ConsoleCmd("switch bot mode",
        @"Example: \SetBotMode 0")]
    public void SetBotMode(string[] param)
    {
        if (param.Length > 0)
        {
            int amt;
            if (int.TryParse(param[0], out amt) && amt >= 0 && amt < 4)
            {
                Zealot.Bot.BotController.BotMode mode = (Zealot.Bot.BotController.BotMode)amt;
                GameInfo.gLocalPlayer.Bot.StartBot(mode);
            }
            else
                PrintToConsole("SetBotMode 0 1 2 3 ");
        }
        else
        {
            PrintToConsole("Format: \\SetBotMode <val>");
        }
    }

    private Vector3 botDest = Vector3.zero;
    [ConsoleCmd("set bot destination",
        @"Example: \SetBotDestination **param")]
    public void SetBotDestination(string[] param)
    {
        if (param.Length == 0)
        {
            botDest = GameInfo.gLocalPlayer.Position;
            PrintToConsole("bot dest marked here.");
        }
        else
        {
            //botDest = Vector3.zero;
            //GameInfo.gLocalPlayer.Bot.SetBotMode(0);
            GameInfo.gLocalPlayer.PathFindToTarget(botDest, -1, 0f, true, true, null);
        }
    }

    [ConsoleCmd("TestDmgLabel")]
    public void TestDmgLabel(string[] param)
    {
        //while
        GameInfo.gDmgLabelPool.TestLoopPlay();
    }

    [ConsoleCmd("Add Combat Stats bonus")]
    public void AddCombatStats(string[] param)
    {
        if (param.Length == 2)
        {
            float amt;
            int type;
            if (float.TryParse(param[1], out amt) && int.TryParse(param[0], out type))
            { 
                RPCFactory.NonCombatRPC.ConsoleAddStats(type, amt);               
            }
        }
        
    }
    [ConsoleCmd("Add armor bonus")]
    public void AddArmor(string[] param)
    {
        if (param.Length > 0)
        {
            int amt;
            if (int.TryParse(param[0], out amt))
            {
                RPCFactory.NonCombatRPC.ConsoleAddStats((int)FieldName.ArmorBonus, amt);
            }
            else
                PrintToConsole("AddArmor: invalid armor value");
        }
        else
        {
            PrintToConsole("Format: \\AddArmor <val>");
        }
    }

    [ConsoleCmd("Add attack bonus")]
    public void AddAttack(string[] param)
    {
        if (param.Length > 0)
        {
            int amt;
            if (int.TryParse(param[0], out amt))
                RPCFactory.NonCombatRPC.ConsoleAddStats((int)FieldName.AttackBonus, amt);
            else
                PrintToConsole("AddAttack: invalid attack value");
        }
        else
        {
            PrintToConsole("Format: \\AddAttack <val>");
        }
    }

    [ConsoleCmd("Add accuracy bonus")]
    public void AddAccuracy(string[] param)
    {
        if (param.Length > 0)
        {
            int amt;
            if (int.TryParse(param[0], out amt))
                RPCFactory.NonCombatRPC.ConsoleAddStats(2, amt);
            else
                PrintToConsole("AddAccuracy: invalid accuracy value");
        }
        else
        {
            PrintToConsole("Format: \\AddAccuracy <val>");
        }
    }

    [ConsoleCmd("SetDmgPercent")]
    public void SetDmgPercent(string[] param)
    {
        if (param.Length > 0)
        {
            int amt;
            if (int.TryParse(param[0], out amt))
            {
                if (amt < 0 || amt > 100)
                {
                    PrintToConsole("SetDmgPercent: invalid value. [0-100]");
                }
                else
                    RPCFactory.NonCombatRPC.ConsoleSetDmgPercent(amt);
            }
            else
                PrintToConsole("SetDmgPercent: invalid value");
        }
        else
        {
            PrintToConsole("Format: \\SetDmgPercent <val>");
        }
    }

    [ConsoleCmd("End TrainingRealm")]
    public void EndTrainingRealm(string[] param)
    {
        GameInfo.gCombat.OnFinishedTraingingRealm();
        UIManager.CloseWindow(WindowType.ConsoleCommand);
    }

    [ConsoleCmd("FinishTutorial")]
    public void FinishTutorial(string[] param)
    {
        if (param.Length == 0)
        {
            RPCFactory.CombatRPC.TutorialStep((int)Trainingstep.Finished);
            UIManager.CloseWindow(WindowType.ConsoleCommand);
        }
        else
        {
            PrintToConsole("Format: \\FinishTutorial");
        }
    }

    [ConsoleCmd("Enter Activity Realm <realmid>")]
    public void ConsoleEnterActivity(string[] param)
    {
        if (param.Length == 1)
        {
            int realmid;
            if (int.TryParse(param[0], out realmid))
                RPCFactory.NonCombatRPC.ConsoleEnterActivityByRealmID(realmid);
        }
        else
            PrintToConsole("Format: \\ConsoleEnterActivity <realmid>");
    }

    [ConsoleCmd("Create Realm <realmid>")]
    public void ConsoleCreateRealm(string[] param)
    {
        if (param.Length == 1)
        {
            int realmid;
            if (int.TryParse(param[0], out realmid))
                RPCFactory.CombatRPC.CreateRealmByID(realmid, false, false);
        }
        else if (param.Length == 2)
        {
            int realmid;
            if (int.TryParse(param[0], out realmid))
            {
                bool logAI = false;
                bool.TryParse(param[1], out logAI);
                RPCFactory.CombatRPC.CreateRealmByID(realmid, logAI, false);
            }
        }
        else
            PrintToConsole("Format: \\ConsoleCreateRealm <realmid>");
    }

    [ConsoleCmd("Enter Dungeon Realm <realmid>")]
    public void ConsoleEnterDungeon(string[] param)
    {
        if (param.Length == 1)
        {
            int realmid;
            if (int.TryParse(param[0], out realmid))
                RPCFactory.CombatRPC.DungeonEnterRequest(realmid);
        }
        else
            PrintToConsole("Format: \\ConsoleEnterDungeon <realmid>");
    }

    [ConsoleCmd("Leave current realm.")]
    public void LeaveRealm(string[] param)
    {
        if (param.Length == 0)
            RPCFactory.CombatRPC.LeaveRealm();
        else
            PrintToConsole("Format: \\LeaveRealm");
    }

    [ConsoleCmd("CompleteRealm")]
    public void CompleteRealm(string[] param)
    {
        if (param.Length == 0)
        {
            RPCFactory.NonCombatRPC.ConsoleCompleteRealm();
            UIManager.CloseWindow(WindowType.ConsoleCommand);
        }
        else
            PrintToConsole("Format: \\CompleteRealm");
    }

    [ConsoleCmd("Get realm info")]
    public void GetAllRealmInfo(string[] param)
    {
        RPCFactory.NonCombatRPC.ConsoleGetAllRealmInfo();
    }

    [ConsoleCmd("ConsoleInspect")]
    public void ConsoleInspect(string[] param)
    {
        RPCFactory.NonCombatRPC.ConsoleInspect();
    }

    [ConsoleCmd("Go To Main Quest")]
    public void StartQuest(string[] param)
    {
        if (param.Length == 1)
        {
            int id;
            if (int.TryParse(param[0], out id))
                RPCFactory.NonCombatRPC.StartQuest(id, 99, 0);
        }
        else
        {
            PrintToConsole("Format: \\StartQuest <questid>");
        }
    }

    [ConsoleCmd("DeleteQuest")]
    public void DeleteQuest(string[] param)
    {
        if (param.Length == 1)
        {
            int id;
            if (int.TryParse(param[0], out id))
                RPCFactory.NonCombatRPC.DeleteQuest(id, "null");
        }
        else
        {
            PrintToConsole("Format: \\DeleteQuest <questid>");
        }
    }

    [ConsoleCmd("ResetQuest")]
    public void ResetQuest(string[] param)
    {
        if (param.Length == 1)
        {
            int id;
            if (int.TryParse(param[0], out id))
                RPCFactory.NonCombatRPC.ResetQuest(id);
        }
        else
        {
            PrintToConsole("Format: \\ResetQuest <questid>");
        }
    }

    [ConsoleCmd("Show Item Id")]
    public void ShowItemID(string[] param)
    {
        int flag;
        if (param.Length == 0)
        {
            PrintToConsole("Format: \\ShowItemID [0|1]");
            return;
        }
        bool valid = int.TryParse(param[0], out flag);
        if (valid)
            ConsoleVariables.ShowItemID = flag > 0 ? true : false;
    }

    [ConsoleCmd("Play Cutscene")]
    public void PlayCutscene(string[] param)
    {
        if (param.Length == 1)
        {
            GameInfo.gCombat.CutsceneManager.PlayQuestCutscene(param[0]);
        }
        else
            PrintToConsole("Format: \\PlayCutscene <cutscene name>");
    }

    [ConsoleCmd("Brand new day")]
    public void NewDay(string[] param)
    {
        if (param.Length == 0)
        {
            RPCFactory.NonCombatRPC.ConsoleNewDay();
            PrintToConsole("A brand new day!");
        }
        else
            PrintToConsole("Format: \\NewDay");
    }

    [ConsoleCmd("Server new day")]
    public void ServerNewDay(string[] param)
    {
        if (param.Length == 0)
        {
            RPCFactory.NonCombatRPC.ConsoleServerNewDay();
            PrintToConsole("Server new day!");
        }
        else
            PrintToConsole("Format: \\NewDay");
    }

    [ConsoleCmd("Set Shake Intensity")]
    public void SetShakeIntensity(string[] param)
    {
        if (param.Length == 1)
        {
            float res = 0;
            float.TryParse(param[0], out res);
            CombatUtils.SHAKE_INTENSITY = res;
        }
         
    }

    [ConsoleCmd("Inspect any player using input name")]
    public void InspectPlayer(string[] param)
    {
        if (param.Length == 1 && !string.IsNullOrEmpty(param[0]))
        {
            RPCFactory.CombatRPC.GetInspectPlayerInfo(param[0]);
        }
        else
        {
            PrintToConsole("Format: \\InspectPlayer <name>");
        }
    }

    [ConsoleCmd("AnimateInfo animname")]
    public void AnimateInfo(string[] param)
    {
        PlayerGhost player = GameInfo.gLocalPlayer;
        Animation anim = player.AnimObj.GetComponent<Animation>();
        if (anim != null)
        {
            foreach (AnimationState ac in anim)
            {
                Debug.LogFormat("statename: {0}, clip framerate {1}", ac.name, ac.clip.frameRate);
                if (ac.name == "blade_atk1")
                {
                    anim.Play(anim.clip.name);
                    Debug.Log("played");
                }
            }
            //int numofclips = anim.GetClipCount();
            anim.Rewind();
        }
    }
    [ConsoleCmd("AnimatePlayer animname")]
    public void AnimatePlayer(string[] param)
    {
        if (param.Length == 1)
        {
            PlayerGhost player = GameInfo.gLocalPlayer;
            player.PlaySEEffect(param[0]);
            UIManager.CloseDialog(WindowType.ConsoleCommand);
        }
        else if (param.Length == 2)
        {
            //Debug.Log(string.Format("param length {0} 1st {1} 2nd {2}", param.Length, param[0], param[1]));
            PlayerGhost player = GameInfo.gLocalPlayer;
            player.PlayEffect(param[0], param[1]);//the second is effect.
            UIManager.CloseDialog(WindowType.ConsoleCommand);
        }
        else
        {
            PrintToConsole("Format: \\AnimatePlayer animname skillgroupname");
        }
    }

    [ConsoleCmd("AnimateSpeed effectname speed")]
    public void AnimateSpeed(string[] param)
    {
        if (param.Length == 2)
        {
            PlayerGhost player = GameInfo.gLocalPlayer;
            float speed = 1.0f;
            float.TryParse(param[1], out speed);
            player.SetAnimSpeed(param[0], speed);
            player.PlayEffect(param[0]);
        }
        else
        {
            PrintToConsole("Format: \\AnimateSpeed effectname speed");
        }
    }

    [ConsoleCmd("DashDur durationinsec")]
    public void DashDur(string[] param)
    {
        if (param.Length == 1)
        {
            float duration;
            bool succeed = float.TryParse(param[0], out duration);
            if (succeed)
                ConsoleVariables.DashDuration = duration;
        }
        else
        {
            PrintToConsole("Format: \\DashDur durationinsec");
        }
    }


    [ConsoleCmd("perform a dash attack")]
    public void DashAttack(string[] param)
    {
        if (param.Length == 1)
        {
            float val;
            bool succeed = float.TryParse(param[0], out val);
            if (succeed)
                GameInfo.gLocalPlayer.DashAttack(val);
        }
        else if (param.Length == 2)
        {
            float val1, val2;
            bool succeed1 = float.TryParse(param[0], out val1);

            bool succeed2 = float.TryParse(param[1], out val2);
            if (succeed1 && succeed2)
                GameInfo.gLocalPlayer.DashAttack(val1, val2);
        }
    }

    private int id = 0;
    [ConsoleCmd("Test spawn")]
    public void TestSpawn(string[] param)
    {
        NetEntityGhost ghost = GameInfo.gLocalPlayer as NetEntityGhost;
        EffectEntityGhost newghost = ((ClientEntitySystem)GameInfo.gLocalPlayer.EntitySystem).SpawnClientEntity<EffectEntityGhost>();
        newghost.Init(1, ghost.Position, ghost.Forward, 3.0f);
        id = newghost.ID;
    }

    [ConsoleCmd("Test Despawn")]
    public void TestDespawn(string[] param)
    {
        ((ClientEntitySystem)GameInfo.gLocalPlayer.EntitySystem).RemoveEntityByID(id);
        id = 0;
    }

    [ConsoleCmd("TestStopEffect")]
    public void TestStopEffect(string[] param)
    {
        NetEntityGhost ghost = GameInfo.gLocalPlayer as NetEntityGhost;
        ghost.StopEffect("mag_basicattack02");
    }

    [ConsoleCmd("Show System Message. Parameters: [0|1] message")]
    public void ShowSystemMessage(string[] param)
    {
        if (param.Length >= 1)
        {
            int addToChatLog;
            bool succeed = int.TryParse(param[0], out addToChatLog);
            if (succeed)
            {
                string message = string.Join(" ", param, 1, param.Length - 1);
                UIManager.ShowSystemMessage(message, addToChatLog > 0 ? true : false);
            }
            else
                PrintToConsole("Format: \\ShowSystemMessage [0|1] message");
        }
        else
        {
            PrintToConsole("Format: \\ShowSystemMessage [0|1] message");
        }
    }

    [ConsoleCmd("Open UI Window. Parameters: WindowType")]
    public void OpenUIWindow(string[] param)
    {
        if (param.Length == 1)
        {
            WindowType windowtype = (WindowType)Enum.Parse(typeof(WindowType), param[0], true);
            if (windowtype != WindowType.None)
                UIManager.OpenWindow(windowtype);
        }
        else
        {
            PrintToConsole("Format: \\OpenUIWindow <WindowType>");
        }
    }

    [ConsoleCmd("FullHealPlayer")]
    public void FullHealPlayer(string[] param)
    {
        if (param.Length == 0)
        {
            RPCFactory.NonCombatRPC.ConsoleFullHealPlayer();
            UIManager.CloseWindow(WindowType.ConsoleCommand);
        }
        else
        {
            PrintToConsole("Format: \\FullHealPlayer");
        }
    }

    [ConsoleCmd("FullRecoverMana")]
    public void FullRecoverMana(string[] param)
    {
        if (param.Length == 0)
        {
            RPCFactory.NonCombatRPC.ConsoleFullRecoverMana();
            UIManager.CloseWindow(WindowType.ConsoleCommand);
        }
        else
        {
            PrintToConsole("Format: \\FullRecoverMana");
        }
    }

    [ConsoleCmd("Spawn Special Boss")]
    public void SpawnSpecialBoss(string[] param)
    {
        if (param.Length == 1)
            RPCFactory.NonCombatRPC.ConsoleSpawnSpecialBoss(param[0]);
        else
            PrintToConsole("Format: \\SpawnSpecialBoss name");
    }

    [ConsoleCmd("Create Guild. Parameters: guildname guildicon")]
    public void CreateGuild(string[] param)
    {
        if (param.Length == 2)
        {
            byte icon = 1;
            bool succeed = byte.TryParse(param[1], out icon);
            if (succeed)
                RPCFactory.CombatRPC.GuildAdd(param[0], icon);
        }
        else
        {
            PrintToConsole("Format: \\CreateGuild guildname guildicon");
        }
    }

    [ConsoleCmd("Leave Guild")]
    public void LeaveGuild(string[] param)
    {
        if (param.Length == 0)
        {
            PlayerGhost player = GameInfo.gLocalPlayer;
            RPCFactory.CombatRPC.GuildLeave(player.Name);
        }
        else if (param.Length == 1)  // kick player
        {
            RPCFactory.CombatRPC.GuildLeave(param[0]);
        }
        else
        {
            PrintToConsole("Format: \\LeaveGuild");
        }
    }

    [ConsoleCmd("Join Guild. Parameters: guildid")]
    public void JoinGuild(string[] param)
    {
        if (param.Length == 1)
        {
            int id = 0;
            bool succeed = int.TryParse(param[0], out id);
            if (succeed)
                RPCFactory.CombatRPC.GuildJoin(id);
        }
        else
        {
            PrintToConsole("Format: \\JoinGuild guildid");
        }
    }

    [ConsoleCmd("GuildList")]
    public void GuildList(string[] param)
    {
        if (param.Length == 0)
        {
            RPCFactory.NonCombatRPC.ConsoleGuildList();
        }
        else
        {
            PrintToConsole("Format: \\GuildList");
        }
    }

    [ConsoleCmd("GuildAddFavour")]
    public void GuildAddFavour(string[] param)
    {
        if (param.Length == 1)
        {
            int amt = int.Parse(param[0]);
            RPCFactory.NonCombatRPC.ConsoleGuildAddFavour(amt);
        }
        else
        {
            PrintToConsole("Format: \\GuildAddFavour <amt>");
        }
    }

    [ConsoleCmd("GuildSMBossLevel")]
    public void GuildSMBossLevel(string[] param)
    {
        if (param.Length == 1)
        {
            int level = int.Parse(param[0]);
            RPCFactory.NonCombatRPC.ConsoleGuildSMLevel(level);
        }
        else
        {
            PrintToConsole("Format: \\GuildSMBossLevel <level>");
        }
    }

    [ConsoleCmd("GiveRewardGrpMail")]
    public void GiveRewardGrpMail(string[] param)
    {
        if (param.Length == 1)
        {
            int value = 0;
            if (int.TryParse(param[0], out value))
                RPCFactory.NonCombatRPC.ConsoleAddRewardGroupMail(value);
        }
        else
        {
            PrintToConsole("Format: \\GiveRewardGrpMail");
        }
    }
    [ConsoleCmd("GiveRewardGrpBag")]
    public void GiveRewardGrpBag(string[] param)
    {
        if (param.Length == 1)
        {
            int value = 0;
            if (int.TryParse(param[0], out value))
                RPCFactory.NonCombatRPC.ConsoleAddRewardGroupBag(value);
        }
        else
        {
            PrintToConsole("Format: \\GiveRewardGrpBag");
        }
    }


    [ConsoleCmd("GiveRewardGrpBagSlotCheck")]
    public void GiveRewardGrp_BagSlotCheck(string[] param)
    {
        if (param.Length == 1)
        {
            int value = 0;
            if (int.TryParse(param[0], out value))
                RPCFactory.NonCombatRPC.ConsoleAddRewardGroupCheckBagSlot(value);
        }
        else
        {
            PrintToConsole("Format: \\GiveRewardGrpBagSlotCheck");
        }
    }
    [ConsoleCmd("GiveRewardGrpBagSlotCheckMail")]
    public void GiveRewardGrpBagSlotCheckMail(string[] param)
    {
        if (param.Length == 1)
        {
            int value = 0;
            if (int.TryParse(param[0], out value))
                RPCFactory.NonCombatRPC.ConsoleAddRewardGroupCheckBagMail(value);
        }
        else
        {
            PrintToConsole("Format: \\GiveRewardGrpBagSlotCheckMail");
        }
    }

    [ConsoleCmd("Set Achievement level. Parameters: level")]
    public void SetAchievementLevel(string[] param)
    {
        if (param.Length == 1)
        {
            int level;
            if (int.TryParse(param[0], out level))
                RPCFactory.NonCombatRPC.ConsoleSetAchievementLevel(level);
        }
        else
        {
            PrintToConsole("Format: \\SetAchievementLevel level");
        }
    }

    [ConsoleCmd("Get Collection. Parameters: objtype|reset|all [target]")]
    public void GetCollection(string[] param)
    {
        if (param.Length == 1)
        {
            if (string.Equals(param[0], "all", StringComparison.OrdinalIgnoreCase))
                RPCFactory.NonCombatRPC.ConsoleGetCollection("all", 0);
            else
            {
                if (string.Equals(param[0], "reset", StringComparison.OrdinalIgnoreCase))
                    GameInfo.gLocalPlayer.AchievementStats.ClearCollectionsDict();
                RPCFactory.NonCombatRPC.ConsoleGetCollection(param[0], 0);
            }
        }
        else if (param.Length == 2)
        {
            int target;
            if (int.TryParse(param[1], out target))
                RPCFactory.NonCombatRPC.ConsoleGetCollection(param[0], target);
        }
        else
        {
            PrintToConsole("Format: \\GetCollection objtype|reset|all [target]");
        }
    }

    [ConsoleCmd("Get Achievement. Parameters: objtype|reset [target] [count]|[max]")]
    public void GetAchievement(string[] param)
    {
        if (param.Length == 1)
        {
            if (string.Equals(param[0], "all", StringComparison.OrdinalIgnoreCase))
                RPCFactory.NonCombatRPC.ConsoleGetAchievement("all", "-1", -1, false);
            else
            {
                if (string.Equals(param[0], "reset", StringComparison.OrdinalIgnoreCase))
                    GameInfo.gLocalPlayer.AchievementStats.ClearAchievementDict();
                RPCFactory.NonCombatRPC.ConsoleGetAchievement(param[0], "-1", 1, true);
            }
        }
        else if (param.Length == 2)
        {
            RPCFactory.NonCombatRPC.ConsoleGetAchievement(param[0], param[1], 1, true);
        }
        else if (param.Length == 3)
        {
            int count;
            if (int.TryParse(param[2], out count))
                RPCFactory.NonCombatRPC.ConsoleGetAchievement(param[0], param[1], count, false);
            else if (string.Equals(param[2], "max", StringComparison.OrdinalIgnoreCase))
                RPCFactory.NonCombatRPC.ConsoleGetAchievement(param[0], param[1], -1, false);
        }
        else
        {
            PrintToConsole("Format: \\GetAchievement objtype|reset [target] [count]|[max]");
        }
    }

    [ConsoleCmd("Clear Achievement Rewards.")]
    public void ClearAchievementRewards(string[] param)
    {
        RPCFactory.NonCombatRPC.ConsoleClearAchievementRewards();
    }

    [ConsoleCmd("AddHero. Parameters: heroId")]
    public void AddHero(string[] param)
    {
        int id;
        if (param.Length == 1 && int.TryParse(param[0], out id))
        {
            RPCFactory.NonCombatRPC.ConsoleAddHero(id, true);
        }
        else
        {
            PrintToConsole("Format: \\AddHero heroId");
        }
    }

    [ConsoleCmd("RemoveHero. Parameters: heroId")]
    public void RemoveHero(string[] param)
    {
        int id;
        if (param.Length == 1 && int.TryParse(param[0], out id))
        {
            RPCFactory.NonCombatRPC.ConsoleRemoveHero(id);
        }
        else
        {
            PrintToConsole("Format: \\RemoveHero heroId (0 to remove all)");
        }
    }

    [ConsoleCmd("SummonHero. Parameters: heroId")]
    public void SummonHero(string[] param)
    {
        int id;
        if (param.Length == 1 && int.TryParse(param[0], out id))
        {
            RPCFactory.CombatRPC.SummonHero(id);
        }
        else
        {
            PrintToConsole("Format: \\SummonHero heroId (0 to unsummon)");
        }
    }

    [ConsoleCmd("ResetExplorations")]
    public void ResetExplorations(string[] param)
    {
        RPCFactory.NonCombatRPC.ConsoleResetExplorations();
    }

    [ConsoleCmd("KillPlayer")]
    public void KillPlayer(string[] param)
    {
        if (param.Length == 0)
        {
            RPCFactory.NonCombatRPC.ConsoleKillPlayer();
            UIManager.CloseWindow(WindowType.ConsoleCommand);
        }
        else
        {
            PrintToConsole("Format: \\KillPlayer");
        }
    }

    [ConsoleCmd("ItemMallTopUp")]
    public void ItemMallTopUp(string[] param)
    {
        if (param.Length == 1)
        {
            int amt = 0;
            int.TryParse(param[0], out amt);
            RPCFactory.NonCombatRPC.ConsoleItemMallTopUp(amt);
            UIManager.CloseWindow(WindowType.ConsoleCommand);
        }
        else
        {
            PrintToConsole("Format: \\SetItemMallTopUp amount");
        }
    }

    [ConsoleCmd("GetSkillIDBySideEff")]
    public void GetSkillIDBySideEff(string[] param)
    {
        if (param.Length == 1)
        {
            int id = 0;
            int.TryParse(param[0], out id);
            string self = "", target = "";
            if (id > 0)
            {
                //foreach (KeyValuePair<int, List<SideEffectJson>> entry in Zealot.Repository.SkillRepo.mSkillSideEffects)
                //{
                //    foreach (SideEffectJson sej in entry.Value)
                //    {
                //        if (sej.id == id)
                //        {
                //            str += entry.Key + "/";
                //            break;
                //        }
                //    }
                //}

                foreach (KeyValuePair<int, SkillSideEffect> entry in Zealot.Repository.SkillRepo.mSkillSideEffects) {
                    foreach (SideEffectJson sej in entry.Value.mSelf) {
                        if(sej.id == id) {
                            self += entry.Key + "/";
                            break;
                        }
                    }
                    foreach(SideEffectJson sej in entry.Value.mTarget) {
                        if(sej.id == id) {
                            target += entry.Key + "/";
                            break;
                        }
                    }
                }
            }
            PrintToConsole("Skill ID with this Sideeffect applied to self: " + self);
            PrintToConsole("Skill ID with this Sideeffect applied to target: " + target);
            Debug.Log("Skill ID with this Sideeffect applied to self: " + self);
            Debug.Log("Skill ID with this Sideeffect applied to target: " + target);
        }
        else
        {
            PrintToConsole("Format: \\GetSkillIDBySideEff parm");
        }
    }

    [ConsoleCmd("TestSkillCoolodwn")]
    public void TestSkillCoolodwn(string[] param)
    {
        if (param.Length == 1)
        {
            int idx = 0;
            int.TryParse(param[0], out idx);
            //0,1,2,3,4  => jobskill, rskill,gskill,bskill,flashskill
            UIManager.GetWidget(HUDWidgetType.SkillButtons).GetComponent<HUD_Skills>().ChangeCooldown(idx, 50);
            UIManager.CloseWindow(WindowType.ConsoleCommand);
        }
        else
        {
            PrintToConsole("Format: \\TestSkillCoolodwn parm");
        }
    }

    [ConsoleCmd("Parameters: mountid")]
    public void Mount(string[] param)
    {
        if (param.Length == 1)
        {
            int mountId;
            mountId = int.TryParse(param[0], out mountId) ? mountId : 0;
            if (mountId > 0)
                RPCFactory.CombatRPC.Mount(mountId);
            else
                PrintToConsole("Mount: invalid parameter");
        }
        else
        {
            PrintToConsole("Format: \\Mount <mount id>");
        }
    }

    [ConsoleCmd("Parameters: none")]
    public void UnMount(string[] param)
    {
        RPCFactory.CombatRPC.UnMount();
    }

    [ConsoleCmd("Parameters:teamid")]
    public void SetTeamID(string[] param)
    {
        if (param.Length == 1)
        {
            int teamid = int.Parse(param[0]);
            RPCFactory.CombatRPC.SetPlayerTeam(teamid);
        }
    }

    [ConsoleCmd("AddLotteryFreeTickets")]
    public void AddLotteryFreeTickets(string[] param)
    {
        if (param.Length == 2)
        {
            int lottery_id, count;
            lottery_id = int.TryParse(param[0], out lottery_id) ? lottery_id : 0;
            count = int.TryParse(param[1], out count) ? count : 0;
            if (lottery_id > 0 && count > 0)
                RPCFactory.NonCombatRPC.ConsoleAddLotteryFreeTickets(lottery_id, count);
            else
                PrintToConsole("AddLotteryFreeTickets: invalid parameter");
        }
        else
        {
            PrintToConsole("Format: \\AddLotteryFreeTickets free-ticket-counts");
        }
    }

    [ConsoleCmd("AddLotteryPoint")]
    public void AddLotteryPoint(string[] param)
    {
        if (param.Length == 2)
        {
            int lottery_id, point;
            lottery_id = int.TryParse(param[0], out lottery_id) ? lottery_id : 0;
            point = int.TryParse(param[1], out point) ? point : 0;
            if (lottery_id > 0 && point > 0)
                RPCFactory.NonCombatRPC.ConsoleAddLotteryPoint(lottery_id, point);
            else
                PrintToConsole("AddLotteryPoint: invalid parameter");
        }
        else
        {
            PrintToConsole("Format: \\AddLotteryPoint free-ticket-counts");
        }
    }

    [ConsoleCmd("RefreshLeaderBoard")]
    public void RefreshLeaderBoard(string[] param)
    {
        RPCFactory.NonCombatRPC.ConsoleRefreshLeaderBoard();
    }

    [ConsoleCmd("Reset PrizeGuarantee")]
    public void ResetPrizeGuarantee(string[] param)
    {
        RPCFactory.NonCombatRPC.ConsoleResetPrizeGuarantee();
    }

    [ConsoleCmd("DonateReset")]
    public void DonateReset(string[] param)
    {
        RPCFactory.NonCombatRPC.ConsoleDonateReset();
    }

    [ConsoleCmd("SetMaxEntity")]
    public void SetMaxEntity(string[] param)
    {
        if (param.Length == 1)
        {
            int count;
            if (int.TryParse(param[0], out count))
            {
                ((ClientEntitySystem)(GameInfo.gLocalPlayer.EntitySystem)).MAX_ENTITY = count;
            }
        }
        else
        {
            PrintToConsole("Format: \\SetMaxEntity count");
        }
    }

    [ConsoleCmd("DonateResetCount")]
    public void DonateResetCount(string[] param)
    {
        RPCFactory.NonCombatRPC.ConsoleResetDonateRemainingCount();
    }

    [ConsoleCmd("NotifyNewGMItem")]
    public void NotifyNewGMItem(string[] param)
    {
        RPCFactory.NonCombatRPC.ConsoleNotifyNewGMItem();
    }

    [ConsoleCmd("ShowMyPos")]
    public void ShowMyPos(string[] param)
    {
        string log = string.Format("ShowMyPos level = {0}, pos = {1}", ClientUtils.GetCurrentLevelName(), GameInfo.gLocalPlayer.Position.ToString());
        Debug.Log(log);
        PrintToConsole(log);
    }

    [ConsoleCmd("SetPos x,y,z")]
    public void SetPos(string[] param)
    {
        if (param.Length == 3)
        {
            float x;
            float y;
            float z;
            if (float.TryParse(param[0], out x) && float.TryParse(param[1], out y) && float.TryParse(param[2], out z))
                GameInfo.gLocalPlayer.Position = new Vector3(x, y, z);
        }
    }

    [ConsoleCmd("TestFeedbackEffect")]
    public void TestFeedbackEffect(string[] param)
    {
        if (param.Length == 1)
        {
            int amount = 0;
            int.TryParse(param[0], out amount);

            GameInfo.TestFeedbackIndex = amount;
        }
        if (param.Length == 2)
        {
            int amount = 0;
            int.TryParse(param[0], out amount);
            GameInfo.TestFeedbackIndex = amount;
            int.TryParse(param[1], out amount);
        }
    }

    [ConsoleCmd("AddActivePoints")]
    public void AddActivePoints(string[] param)
    {
        if (param.Length == 1)
        {
            int amount = 0;
            int.TryParse(param[0], out amount);

            RPCFactory.NonCombatRPC.ConsoleAddActivePoints(amount);
        }
        else
        {
            PrintToConsole("Format: \\AddActivePoints amount");
        }
    }

    [ConsoleCmd("FinishQERTask")]
    public void FinishQERTask(string[] param)
    {
        if (param.Length == 1)
        {
            int taskId = 0;
            int.TryParse(param[0], out taskId);

            RPCFactory.NonCombatRPC.ConsoleFinishQERTask(taskId);
        }
        else
        {
            PrintToConsole("Format: \\FinishQERTask taskId");
        }
    }

    [ConsoleCmd("FastForwardGuildQuest")]
    public void FastForwardGuildQuest(string[] param)
    {
        RPCFactory.NonCombatRPC.ClientGuildQuestOperation((int)GuildQuestOperation.Fastforwad, 0);
    }

    [ConsoleCmd("TickerTapeAnnoucement")]
    public void TickerTapeAnnoucement(string[] param)
    {
        if (param.Length == 1)
        {
            //string annoucement = param[0];
            //UIManager.GetWidget(HUDWidgetType.TickerTape).GetComponent<HUD_TickerTape>().SetAnnoucementText(annoucement);
        }
        else
        {
            PrintToConsole("Format: \\TickerTapeAnnoucement annoucementText");
        }
    }

    [ConsoleCmd("SocialAddfriend, add random friends if no name is specfied")]
    public void SocialAddfriend(string[] param)
    {
        if (param.Length <= 1)
        {
            string playerName = (param.Length == 1) ? param[0] : "";
            RPCFactory.NonCombatRPC.ConsoleSocialAddFriend(playerName);
        }
        else
        {
            PrintToConsole("Format: \\SocialAddfriend <name>");
        }
    }

    
    [ConsoleCmd("SetStoreRefreshTime")]
    public void SetStoreRefreshTime(string[] param)
    {
        if (param.Length > 1)
        {
            int storecat, time;
            storecat = int.TryParse(param[0], out storecat) ? storecat : 0;
            time = int.TryParse(param[1], out time) ? time : 0;
            RPCFactory.NonCombatRPC.ConsoleSetStoreRefreshTime(storecat, time);
        }
        else
        {
            PrintToConsole("Format: \\SetStoreRefreshTime <storeID> <HHMM(eg. 2359)>");
        }
    }

    [ConsoleCmd("ResetGoldJackpotRoll")]
    public void ResetGoldJackpotRoll(string[] param)
    {
        RPCFactory.NonCombatRPC.ConsoleResetGoldJackpotRoll();
    }

    [ConsoleCmd("ResetContLoginClaims")]
    public void ResetContLoginClaims(string[] param)
    {
        RPCFactory.NonCombatRPC.ConsoleResetContLoginClaims();
    }

    [ConsoleCmd("ActiveRandomBox")]
    public void ActiveRandomBox(string[] param)
    {
        bool active;
        if (bool.TryParse(param[0], out active))
            RPCFactory.CombatRPC.ActiveRandomBox(active);
    }

    [ConsoleCmd("SetEquipmentUpgradeLevel")]
    public void SetEquipmentLevel(string[] param)
    {
        if (param.Length == 2)
        {
            UIManager.CloseWindow(WindowType.ConsoleCommand);
            EquipmentSlot slotID = (EquipmentSlot)Enum.Parse(typeof(EquipmentSlot), param[0], true);
            int level = 0;
            if (int.TryParse(param[1], out level))
            {
                RPCFactory.NonCombatRPC.ConsoleSetEquipmentUpgradeLevel((int)slotID, level);
            }
        }
        else
        {
            PrintToConsole("Format: \\SetEquipmentUpgradeLevel <EquipName> <Level>");
        }
    }

    [ConsoleCmd("ConsoleResetArenaEntey")]
    public void ConsoleResetArenaEntey(string[] param)
    {
        RPCFactory.NonCombatRPC.ConsoleResetArenaEntey();
    }

    [ConsoleCmd("LowSettingUIToggle")]
    public void LowSettingUIToggle(string[] param)
    {
        GameInfo.mIsHighSetting = !GameInfo.mIsHighSetting;
        UIManager.UIHierarchy.SetVisibilityLowSettingObject(GameInfo.mIsHighSetting);
    }
    
    [ConsoleCmd("FlashDuration")]
    public void FlashDuration(string[] param)
    {
        if (param.Length == 1)
        {
            float amount = 0;
            float.TryParse(param[0], out amount);

            MonsterGhost.FlashDuration = amount;
        }
        else
        {
            PrintToConsole("Format: \\FlashDuration Duration");
        }
    }

    [ConsoleCmd("FlashMax")]
    public void FlashMax(string[] param) {
        if (param.Length == 1) {
            float amount = 0;
            float.TryParse(param[0], out amount);

            MonsterGhost.MaxFlashAmt = amount;
        }
        else {
            PrintToConsole("Format: \\FlashDuration Duration");
        }
    }

    [ConsoleCmd("AnimationDelay")]
    public void AnimationDelay(string[] param)
    {
        if(param.Length == 1)
        {
            float amount = 0;
            float.TryParse(param[0], out amount);
            Zealot.Client.Actions.BaseClientCastSkill.recovertime_mod = amount;
        }
            
    }

    [ConsoleCmd("TotalCrit")]
    public void TotalCrit(string[] param)
    {
        RPCFactory.NonCombatRPC.TotalCrit();
    }

    [ConsoleCmd("CritRate")]
    public void CritRate(string[] param)
    {
        if (param.Length == 1)
        {
            float amount = 0;
            float.TryParse(param[0], out amount);
            RPCFactory.NonCombatRPC.CritRate(amount);
        }     
    }

    #region server structure test.
    [ConsoleCmd("ConsoleDC")]
    public void ConsoleDC(string[] param)
    {
        PhotonNetwork.networkingPeer.Disconnect();
    }

    [ConsoleCmd("ConsoleRelogin")]
    public void ConsoleRelogin(string[] param)
    {
        if (GameInfo.DCReconnectingGameServer)
            PhotonNetwork.networkingPeer.ReconnectToGameServer();
    }

    [ConsoleCmd("ConsoleTransferServer")]
    public void ConsoleTransferServer(string[] param)
    {
        if (param.Length == 1)
        {
            int serverid;
            if (int.TryParse(param[0], out serverid))
            {
                UIManager.StartHourglass(10);
                RPCFactory.NonCombatRPC.ConsoleTransferServer(serverid);
                return;
            }
        }
        PrintToConsole("Format: \\ConsoleTransferServer <serverid>");
    }

    [ConsoleCmd("ConsoleDCFromCluster")]
    public void ConsoleDCFromCluster(string[] param)
    {
        RPCFactory.NonCombatRPC.ConsoleDCFromServer("cluster");
    }

    [ConsoleCmd("ConsoleDCFromMaster")]
    public void ConsoleDCFromMaster(string[] param)
    {
        RPCFactory.NonCombatRPC.ConsoleDCFromServer("master");
    }

    [ConsoleCmd("ConsoleKickMe")]
    public void ConsoleKickMe(string[] param)
    {
        RPCFactory.NonCombatRPC.ConsoleDCFromServer("game");
    }
    #endregion

    [ConsoleCmd("ConsoleSpawnPersonalMonster")]
    public void ConsoleSpawnPersonalMonster(string[] param)
    {
        if (param.Length == 2)
        {
            string archetype = param[0];
            int population;
            if (int.TryParse(param[1], out population))
            {
                RPCFactory.NonCombatRPC.ConsoleSpawnPersonalMonster(archetype, population);
                return;
            }
        }
        PrintToConsole("Format: \\ConsoleSpawnPersonalMonster <archetype:string> <population:int>");
    }

    [ConsoleCmd("RTSpeed")]
    public void RTSpeed(string[] param) {
        if(param.Length == 1) {
            float amount = 0;
            float.TryParse(param[0], out amount);
            RPCFactory.NonCombatRPC.ConsoleAddStats(2000, amount);
        }
    }

    [ConsoleCmd("BASpeed")]
    public void BASpeed(string[] param) {
        if(param.Length == 1) {
            float amount = 0;
            float.TryParse(param[0], out amount);
            RPCFactory.NonCombatRPC.ConsoleAddStats(1000, amount);
        }
    }

    [ConsoleCmd("SwapGender")]
    public void SwapGender(string[] param) {
        Gender gender = (Gender)GameInfo.gLocalPlayer.PlayerSynStats.Gender;
        gender = (gender == Gender.Male) ? Gender.Female : Gender.Male;

        RPCFactory.NonCombatRPC.ConsoleAddStats(3000, (int)gender);
    }

    [ConsoleCmd("SwapWeaponType")]
    public void SwapWeaponType(string[] param) {
        //GameInfo.gCombat.OnWeaponButton();
    }
    
    [ConsoleCmd("ConsoleReviveSelectedPlayer")]
    public void ConsoleReviveSelectedPlayer(string[] param)
    {
        BaseClientEntity entity = GameInfo.gSelectedEntity;
        ActorGhost actorghost = entity as ActorGhost;
        PlayerGhost playerghost = GameInfo.gLocalPlayer;

        if(actorghost == null)
            return;

        //Dictionary<int, IInventoryItem> itemList = playerghost.clientItemInvCtrl.itemInvData.GetItemsByItemId(1);
        //if(itemList.Count == 0)
        //{
        //    UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("reviveItem_NotEnoughItem"));
        //    return;
        //}

        //if(actorghost.IsAlive())
        //{
        //    UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("reviveItem_RequesteeNotDead"));
        //    return;
        //}

        RPCFactory.NonCombatRPC.StartReviveItemRequest(playerghost.Name, actorghost.Name, 1);
    }

    [ConsoleCmd("UpdateQuestProgress")]
    public void UpdateQuestProgress(string[] param)
    {
        if (param.Length == 1)
        {
            byte type = 0;
            if (byte.TryParse(param[0], out type) && type < 6)
                RPCFactory.NonCombatRPC.ConsoleUpdateQuestProgress(type);
        }
    }

    [ConsoleCmd("ConsoleChangeJob")]
    public void ConsoleChangeJob(string[] param)
    {
        if(param.Length == 1)
        {
            byte job = 0;
            if (byte.TryParse(param[0], out job) && job < 21)
                RPCFactory.NonCombatRPC.ConsoleChangeJob(job);
        }
    }

    [ConsoleCmd("ConsoleAddSkillPoint")]
    public void ConsoleAddSkillPoint(string[] param)
    {
        if(param.Length == 1)
        {
            int amt = 0;
            int.TryParse(param[0], out amt);

            RPCFactory.NonCombatRPC.ConsoleAddSkillPoint(amt);
        }
    }

    [ConsoleCmd("Logout")]
    public void Logout(string[] param)
    {
        PhotonNetwork.networkingPeer.Disconnect();
    }

    [ConsoleCmd("UpdateDonate")]
    public void UpdateDonate(string[] param)
    {
        if (param.Length == 1)
        {
            int type = 0;
            int.TryParse(param[0], out type);

            if (type == 6 || type == 12)
            {
                RPCFactory.NonCombatRPC.ConsoleUpdateDonate(type);
            }
        }
    }

    [ConsoleCmd("SendMail")]
    public void SendMail(string[] param)
    {
        if (param.Length == 1)
        {
            int mailid;
            if (!int.TryParse(param[0], out mailid))
            {
                PrintToConsole("SendMail: invalid mail id.");
                return;
            }
            RPCFactory.NonCombatRPC.ConsoleSendMail(mailid);
        }
        else
            PrintToConsole("Format: \\SendMail <Mail ID>");
    }

#endif
    #endregion
}