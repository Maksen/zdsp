#if WSA_PLUGIN

using System.Collections.Generic;
using System;
using Windows.ApplicationModel.Background;
using Windows.Data.Json;
using Windows.Networking.PushNotifications;

namespace UTNotifications.WSA
{
    public sealed class PushBackgroundTask : IBackgroundTask
    {
    //public
        public static void Register()
        {
            BackgroundTaskBuilder builder = new BackgroundTaskBuilder();

            builder.TaskEntryPoint = typeof(PushBackgroundTask).FullName;
            builder.Name = builder.TaskEntryPoint;
            builder.SetTrigger(new PushNotificationTrigger());

            builder.Register();
        }

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            RawNotification notification = (RawNotification)taskInstance.TriggerDetails;

            JsonObject json = JsonValue.Parse(notification.Content).GetObject();
            string title = UrlDecode(json.ContainsKey(TITLE) ? json[TITLE].GetString() : INVALID_FORMAT_MESSAGE);
            string text = UrlDecode(json.ContainsKey(TEXT) ? json[TEXT].GetString() : INVALID_FORMAT_MESSAGE);
            int id = NotificationTools.GetNextPushNotificationId();
            var userData = GetUserData(json);
            string notificationProfile = json.ContainsKey(NOTIFICATION_PROFILE) ? json[NOTIFICATION_PROFILE].GetString() : null;

            NotificationTools.PostLocalNotification(title, text, id, userData, notificationProfile);
        }

    //private
        private IDictionary<string, string> GetUserData(JsonObject json)
        {
            IDictionary<string, string> userData = null;

            foreach (var it in json)
            {
                if (it.Key != TITLE && it.Key != TEXT && it.Key != NOTIFICATION_PROFILE)
                {
                    if (userData == null)
                    {
                        userData = new Dictionary<string, string>();
                    }

                    userData.Add(it.Key, UrlDecode(it.Value.GetString()));
                }
            }

            return userData;
        }

        private static string UrlDecode(string str)
        {
            return Uri.UnescapeDataString(str.Replace('+', ' '));
        }

        private readonly string TITLE = "title";
        private readonly string TEXT = "text";
        private readonly string NOTIFICATION_PROFILE = "notification_profile";
        private readonly string INVALID_FORMAT_MESSAGE = "WRONG MESSAGE FORMAT! Push notification message must contain at least \"title\" and \"text\". F.e. see Assets/UTNotifications/DemoServer/src/DemoServer/PushNotificator.java";
    }
}
#endif
