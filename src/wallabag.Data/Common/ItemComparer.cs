using System;
using System.Collections.Generic;
using wallabag.Models;
using wallabag.ViewModels;

namespace wallabag.Common
{
    public class ItemComparer : IEqualityComparer<Item>
    {
        public int GetHashCode(Item i)
        {
            if (i == null)
            {
                return 0;
            }
            return i.Id;
        }

        public bool Equals(Item x1, Item x2)
        {
            if (x1.Id == x2.Id)
            {
                return true;
            }
            if (ReferenceEquals(x1, null) ||
                ReferenceEquals(x2, null))
            {
                return false;
            }
            return x1.Id == x2.Id;
        }
    }
    public class ItemByDateTimeComparer : IComparer<ItemViewModel>
    {
        public int Compare(ItemViewModel x, ItemViewModel y)
        {
            TimeSpan duration = x.Model.CreationDate.Subtract(y.Model.CreationDate);
            return (int)duration.TotalSeconds;
        }
    }
}
