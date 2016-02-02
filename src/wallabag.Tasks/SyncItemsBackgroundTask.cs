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
            var _deferral = taskInstance.GetDeferral();
            taskInstance.Canceled += TaskInstance_Canceled;

            //await DataService.SyncOfflineTasksWithServerAsync();
            //await DataService.DownloadItemsFromServerAsync();

            //uint newItemsSinceLastOpening = (uint)(await DataService.GetItemsAsync(new FilterProperties
            //{
            //    ItemType = FilterProperties.FilterPropertiesItemType.Unread,
            //    CreationDateFrom = DataService.LastUserSyncDateTime,
            //    CreationDateTo = DateTime.Now
            //})).Count;
            //BadgeNumericNotificationContent badgeContent = new BadgeNumericNotificationContent(newItemsSinceLastOpening);
            //BadgeUpdateManager.CreateBadgeUpdaterForApplication().Update(new BadgeNotification(badgeContent.GetXml()));

            _deferral.Complete();
        }

        private void TaskInstance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            System.Diagnostics.Debug.WriteLine("Cancel reason: " + reason.ToString());
        }
    }

}
