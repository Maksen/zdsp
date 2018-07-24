using Photon.LoadBalancing.GameServer;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zealot.Common;

namespace Zealot.Server.Rules
{
    public static class GMActivityRules
    {
        public static string mGMActivityConfigString = "";

        public static void Init()
        {
            int server = GameApplication.Instance.GetMyServerId();
            List<Dictionary<string, object>> configs = GameApplication.dbGM.GMActivity.GetConfigs(server);
            RefreshActivityConfig(configs);
        }

        public static async Task UpdateConfigs()
        {
            int server = GameApplication.Instance.GetMyServerId();
            List<Dictionary<string, object>> configs = await GameApplication.dbGM.GMActivity.GetConfigsAsync(server);
            GameApplication.Instance.executionFiber.Enqueue(() =>
            {
                RefreshActivityConfig(configs);
                GameApplication.Instance.BroadcastMessage(BroadcastMessageType.GMActivityConfigChanged, mGMActivityConfigString);
            });
        }

        public static void RefreshActivityConfig(List<Dictionary<string, object>> configs)
        {
            GMActivityConfig.CleanUp();
            foreach (var entry in configs)
            {
                if (!Enum.IsDefined(typeof(GMActivityType), (byte)entry["Type"]))
                    continue;
                string parameters = (string)entry["Parameters"];
                List<int> datalist = new List<int>();
                GMActivityType type = (GMActivityType)entry["Type"];
                string[] parameters_array = parameters.Split(';');
                for (int index = 0; index < parameters_array.Length; index++)
                    datalist.Add(int.Parse(parameters_array[index]));
                GMActivityConfig.AddConfig(type, (DateTime)entry["StartDT"], (DateTime)entry["EndDT"], datalist);
            }
            mGMActivityConfigString = GMActivityConfig.ToString();
        }

        public static string GetConfig()
        {
            return mGMActivityConfigString;
        }
    }
}
