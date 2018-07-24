using ExitGames.Concurrency.Fibers;
using ExitGames.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zealot.Common;

namespace Photon.LoadBalancing.GameServer
{
    public static class SystemSwitch
    {
        private static readonly PoolFiber executionFiber;
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        public static readonly int UpdateInterval = 5 * 60000;
        public static SystemSwitchData mSysSwitch = new SystemSwitchData(); //record close system

        static SystemSwitch()
        {
            executionFiber = GameApplication.Instance.executionFiber;
        }

        public static void Init(string from = "game")
        {
            var task = InitTask(from);
        }

        public static async Task InitTask(string from)
        {
            List<Dictionary<string, object>> listDB = await GameApplication.dbGM.SystemSwitch.GetAll();
            executionFiber.Enqueue(() =>
            {
                Dictionary<string, int> closeSwitch = new Dictionary<string, int>();
                foreach (var info in listDB)
                {
                    string name = (string)info["name"];
                    bool open = (bool)info["open"];
                    //record closeswitch name
                    if (open == false)
                        closeSwitch.Add(name, 1);
                }

                Dictionary<string, int> change = mSysSwitch.SetBroadcastChange(closeSwitch);
                string closeStr = "[" + from + "]" + "closeSwitch : ";
                log.Info(closeStr + mSysSwitch.GetSemicolonList());

                //need broadcast change
                if (from == "update" && change.Count > 0)
                {
                    string bStr = "[SystemSwitch:BroadcastChange] ";
                    string chStr = "";
                    foreach (KeyValuePair<string, int> item1 in change)
                        chStr += item1.Key + ";";
                    if (change.Count > 0)
                        chStr = chStr.TrimEnd(';');
                    log.Info(bStr + chStr);

                    //broadcast system switch change to client
                    GameApplication.Instance.BroadcastMessage(BroadcastMessageType.SystemSwitchChange, chStr);
                }
            });
        }

        public static void Update()
        {
            Init("update");
            executionFiber.Schedule(Update, UpdateInterval);
        }
    }
}