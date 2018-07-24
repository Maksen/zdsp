using System.Text;
using Photon.LoadBalancing.GameServer;
using Zealot.Repository;
using Zealot.Common;

namespace Zealot.Server.Rules
{
    public static class RareItemNotificationRules
    {
        public static void CheckNotification(int itemid, string playername)
        {
            if (RareItemNotificationRepo.IsRareItem(itemid) && SystemSwitch.mSysSwitch.IsOpen(SysSwitchType.RareItem))
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(itemid);
                sb.Append(';');
                sb.Append(playername);
                GameApplication.Instance.BroadcastMessage(BroadcastMessageType.RareItemNotification, sb.ToString());
            }
        }
    }
}
