using System;
using System.Collections.Generic;
using System.Text;
using PropertyChanged;
using wallabag.DataModel;

namespace wallabag.ViewModels
{
    [ImplementPropertyChanged]
    class SingleItemPageViewModel
    {
        public ItemViewModel CurrentItem { get; set; }
        public SingleItemPageViewModel(int Id)
        {

        }
    }
}
