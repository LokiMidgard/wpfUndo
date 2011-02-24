using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;

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
            Contract.Requires(coll.Count > 0);
            Contract.Ensures( coll.Count==Contract.OldValue(coll.Count) - 1 );
            var item = coll[coll.Count - 1];
            coll.RemoveAt(coll.Count - 1);
            return item;
        }

        [Pure]
        public static T Peek<T>(this ObservableCollection<T> coll)
        {
            Contract.Requires(coll.Count > 0);
            var item = coll[coll.Count - 1];
            return item;
        }
        
        
    }
}
