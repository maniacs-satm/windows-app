using Windows.ApplicationModel.Background;
using Windows.UI.Notifications;

namespace wallabag.Tasks
{
    public sealed class SyncItemsBackgroundTask : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            taskInstance.Canceled += TaskInstance_Canceled;
            // TODO: Update live tile
            // TODO: Sync items
        }

        private void TaskInstance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            System.Diagnostics.Debug.WriteLine("Cancel reason: " + reason.ToString());
        }
    }

}
