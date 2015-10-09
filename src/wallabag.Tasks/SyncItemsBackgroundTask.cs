using System;
using NotificationsExtensions.Badges;
using wallabag.Services;
using Windows.ApplicationModel.Background;
using Windows.UI.Notifications;

namespace wallabag.Tasks
{
    public sealed class SyncItemsBackgroundTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            taskInstance.Canceled += TaskInstance_Canceled;

            if (await DataService.SyncWithServerAsync())
            {
                BadgeNumericNotificationContent badgeContent =
                    new BadgeNumericNotificationContent(42); // Update this number with the number of new items since last opening
                TileUpdateManager.CreateTileUpdaterForApplication().Update(new TileNotification(badgeContent.GetXml()));
            }
        }

        private void TaskInstance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            System.Diagnostics.Debug.WriteLine("Cancel reason: " + reason.ToString());
        }
    }

}
