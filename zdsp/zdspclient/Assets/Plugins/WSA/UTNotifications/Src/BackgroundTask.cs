#if WSA_PLUGIN

using Windows.ApplicationModel.Background;

namespace UTNotifications.WSA
{
    public sealed class BackgroundTask : IBackgroundTask
    {
    //public
        public static void Register()
        {
            BackgroundTaskBuilder builder = new BackgroundTaskBuilder();

            builder.TaskEntryPoint = typeof(BackgroundTask).FullName;
            builder.Name = builder.TaskEntryPoint;
            builder.SetTrigger(new MaintenanceTrigger(15, false));

            builder.Register();
        }

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            NotificationTools.Reschedule(false);
        }
    }  
}
#endif
