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
                uint newItemsSinceLastOpening = (uint)(await DataService.GetItemsAsync(new FilterProperties
                {
                    ItemType = FilterProperties.FilterPropertiesItemType.Unread,
                    CreationDateFrom = DataService.LastUserSyncDateTime
                })).Count;
                BadgeNumericNotificationContent badgeContent =
                    new BadgeNumericNotificationContent(newItemsSinceLastOpening); // Update this number with the number of new items since last opening
                BadgeUpdateManager.CreateBadgeUpdaterForApplication().Update(new BadgeNotification(badgeContent.GetXml()));
            }
        }

        private void TaskInstance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            System.Diagnostics.Debug.WriteLine("Cancel reason: " + reason.ToString());
        }
    }

}
