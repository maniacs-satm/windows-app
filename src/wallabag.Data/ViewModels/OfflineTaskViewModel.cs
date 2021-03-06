﻿using PropertyChanged;
using System.Threading.Tasks;
using Template10.Mvvm;
using wallabag.Data.Interfaces;
using wallabag.Data.Models;

namespace wallabag.Data.ViewModels
{
    [ImplementPropertyChanged]
    public class OfflineTaskViewModel : ViewModelBase
    {
        private IDataService _dataService;
        public OfflineTask Model { get; set; }

        public string ActionLogo { get; set; }

        public OfflineTaskViewModel(IDataService dataService) { _dataService = dataService; }

        public Task InitializeAsync(OfflineTask Model)
        {
            this.Model = Model;

            switch (Model.Action)
            {
                case OfflineTask.OfflineTaskAction.MarkAsRead:
                    ActionLogo = "\uE001";
                    break;
                case OfflineTask.OfflineTaskAction.UnmarkAsRead:
                    ActionLogo = "\uE052";
                    break;
                case OfflineTask.OfflineTaskAction.MarkAsFavorite:
                    ActionLogo = "\uE006";
                    break;
                case OfflineTask.OfflineTaskAction.UnmarkAsFavorite:
                    ActionLogo = "\uE007";
                    break;
                case OfflineTask.OfflineTaskAction.AddTags:
                case OfflineTask.OfflineTaskAction.DeleteTag:
                    ActionLogo = "\uE1CB";
                    break;
                case OfflineTask.OfflineTaskAction.Delete:
                    ActionLogo = "\uE107";
                    break;
                case OfflineTask.OfflineTaskAction.AddItem:
                    ActionLogo = "\uE109";
                    break;
                default:
                    break;
            }

            return Task.CompletedTask;
        }
    }
}
