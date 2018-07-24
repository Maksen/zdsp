using System.Collections.Generic;
using Kopio.JsonContracts;
using System;
using System.Linq;
using Zealot.Common;

namespace Zealot.Repository
{
    public class PushNotificationRepo
    {
        static Dictionary<int, PushNotificationJson> mAllNotification;
        public static void Init(GameDBRepo gameData)
        {
            mAllNotification = new Dictionary<int, PushNotificationJson>();
            foreach (var noti in gameData.PushNotification)
            {
                mAllNotification.Add(noti.Value.notificationid, noti.Value);
            }

        }

        public static Dictionary<int,PushNotificationJson> GetScheduleNotification()
        {
            Dictionary<int, PushNotificationJson> result = new Dictionary<int, PushNotificationJson>();
            foreach (var noti in mAllNotification)
            {
                if (noti.Value.day != PushNotificationType.None)
                {
                    result.Add(noti.Value.notificationid, noti.Value);
                }
            }
            return result;
        }

        public static PushNotificationJson GetPushNotificationByID(int id)
        {
            PushNotificationJson result = null;
            mAllNotification.TryGetValue(id, out result);
            return result;
        }

        public static int GetSecond(PushNotificationJson PN)
        {
            var now = DateTime.Now;
            int leftTime = 0;

            if (PN.day != PushNotificationType.Everyday)
            {
                var dayOfWeek = (int)now.DayOfWeek;
                var daySecond = now.Hour * 3600 + now.Minute * 60 + now.Second;
                var weekSecond = dayOfWeek * 86400 + daySecond;

                var pnSeconds = (int)PN.day * 86400 + PN.hour * 3600 + PN.minute * 60;
                leftTime = pnSeconds - weekSecond;
                if (leftTime < 0)
                    leftTime += (86400 * 7);
            }
            else
            {
                int totalhour = 0;
                int totalmin = 0;
                if (now.Hour > PN.hour)//already pass, need to schedule for next day
                {
                    totalhour = 24 - now.Hour + PN.hour;
                    totalmin = 60 - now.Minute + PN.minute;
                    if (totalmin > 0)
                        totalhour--;
                }
                else
                {
                    totalhour = PN.hour - now.Hour;
                    totalmin = 60 - now.Minute + PN.minute;
                    if (totalmin > 0)
                        totalhour--;
                }

                leftTime += totalhour * 3600 + totalmin * 60;
            }
            return leftTime;
        }
    }
}
