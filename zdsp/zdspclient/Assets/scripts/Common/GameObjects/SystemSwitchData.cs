using System.Collections.Generic;

namespace Zealot.Common
{
    public class SysSwitchType
    {
        //special case need to be checked in some places
        public static string RareItem = "RareItem";
        public static string RandomBox = "RandomBox";
        public static string DialogTongbaoCostBuff = "DialogTongbaoCostBuff";
        public static string ItemMall = "ItemMall";
        public static string GuildActivity = "GuildActivity";
        public static string Wardrobe = "Wardrobe";

        //common case, relative to WindowType, in the client-side function OpenWindow or OpenDialog check

    }

    public class SystemSwitchData
    {
        //server, client order difference
        public Dictionary<string, int> closeSwitch;
        public SystemSwitchData()
        {
            closeSwitch = new Dictionary<string, int>();
        }

        //server side
        public Dictionary<string, int> SetBroadcastChange(Dictionary<string, int> newCloseSwitch)
        {
            Dictionary<string, int> change = new Dictionary<string, int>();
            foreach (KeyValuePair<string, int> ns in newCloseSwitch)
            {
                // add closeswitch key
                if (!closeSwitch.ContainsKey(ns.Key))
                    change.Add(ns.Key, 1);
            }
            foreach (KeyValuePair<string, int> cs in closeSwitch)
            {
                // remove closeswitch key
                if (!newCloseSwitch.ContainsKey(cs.Key))
                    change.Add(cs.Key, 1);
            }

            // replace all
            closeSwitch = newCloseSwitch;
            return change;
        }

        //clide side : login init
        public void ClientLoginInit(string str)
        {
            Dictionary<string, int> init = new Dictionary<string, int>();
            string[] strParam = str.Split(';');
            for (int i = 0; i < strParam.Length; i++)
            {
                string key = strParam[i];
                if (!init.ContainsKey(key))
                    init.Add(key, 1);
            }

            // login init
            closeSwitch = init;
        }

        //clide side : accept Broadcast change
        public void OnChange(string parameters)
        {
            string[] strParam = parameters.Split(';');
            for (int i = 0; i < strParam.Length; i++)
            {
                string key = strParam[i];
                if (closeSwitch.ContainsKey(key))
                {
                    //change to remove
                    closeSwitch.Remove(key);
                }
                else
                {
                    //change to add
                    closeSwitch.Add(key, 1);
                }
            }
        }

        //server side, clide side
        public string GetSemicolonList()
        {
            string closeStr = "";
            foreach (KeyValuePair<string, int> item in closeSwitch)
                closeStr += item.Key + ";";
            if (closeSwitch.Count > 0)
                closeStr = closeStr.TrimEnd(';');

            return closeStr;
        }

        //server side, clide side
        public bool IsOpen(string sysname)
        {
            if (closeSwitch.ContainsKey(sysname))
            {
                return false;
            }

            return true;
        }
    }
}
