using System;
using System.Collections.Generic;
using System.Text;
using PropertyChanged;
using Newtonsoft.Json;

namespace wallabag.DataModel
{
    [ImplementPropertyChanged]
    public class ItemViewModel
    {
        public Item Model { get; set; }

    }
}
