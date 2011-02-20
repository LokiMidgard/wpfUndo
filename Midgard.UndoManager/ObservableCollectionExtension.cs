using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Midgard.WPFUndoManager
{
    static class ObservableCollectionExtension
    {
        public static void Push<T>(this ObservableCollection<T> coll,T item )
        {
            coll.Insert(coll.Count, item);
        }

        public static T Pop<T>(this ObservableCollection<T> coll)
        {
            var item = coll[coll.Count - 1];
            coll.RemoveAt(coll.Count - 1);
            return item;
        }
    }
}
