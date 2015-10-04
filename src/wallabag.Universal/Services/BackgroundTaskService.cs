using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace wallabag.Services
{
    class BackgroundTaskService
    {
        public static async Task<BackgroundTaskRegistration> RegisterSyncItemsBackgroundTask()
        {
            await BackgroundExecutionManager.RequestAccessAsync();

            var builder = new BackgroundTaskBuilder();

            builder.Name = "SyncItemsBackgroundTask";
            builder.TaskEntryPoint = "wallabag.Tasks.SyncItemsBackgroundTask";

            IBackgroundTrigger timeTrigger = new TimeTrigger(15, false); // TODO: Add option for selecting the time span
            builder.SetTrigger(timeTrigger);
            builder.AddCondition(new SystemCondition(SystemConditionType.InternetAvailable));

            BackgroundTaskRegistration task = builder.Register();

            return task;
        }

        public static void UnregisterSyncItemsBackgroundTask()
        {
            foreach (var cur in BackgroundTaskRegistration.AllTasks)
                if (cur.Value.Name == "SyncItemsBackgroundTask")
                    cur.Value.Unregister(true);
        }
    }
}
