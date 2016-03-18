using NotificationsExtensions.Badges;
using System;
using wallabag.Common;
using wallabag.Data.Models;
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

            Data.Interfaces.IDataService dataService = new Data.Services.RuntimeDataService();

            await dataService.SyncOfflineTasksWithServerAsync();
            await dataService.DownloadItemsFromServerAsync();

            uint newItemsSinceLastOpening = (uint)(await dataService.GetItemsAsync(new FilterProperties
            {
                ItemType = FilterProperties.FilterPropertiesItemType.Unread,
                CreationDateFrom = AppSettings.LastOpeningDateTime,
                CreationDateTo = DateTime.Now
            })).Count;
            BadgeNumericNotificationContent badgeContent = new BadgeNumericNotificationContent(newItemsSinceLastOpening);
            BadgeUpdateManager.CreateBadgeUpdaterForApplication().Update(new BadgeNotification(badgeContent.GetXml()));

            _deferral.Complete();
        }

        private void TaskInstance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            System.Diagnostics.Debug.WriteLine("Cancel reason: " + reason.ToString());
        }
    }

}
