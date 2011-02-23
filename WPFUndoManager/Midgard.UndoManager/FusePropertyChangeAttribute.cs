using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Midgard.WPFUndoManager
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class FusePropertyChangeAttribute : Attribute
    {
        readonly Func<object, object, bool> canFuse;
        readonly Func<object, object, object> fuse;

        public FusePropertyChangeAttribute(Func<object,object,bool> canFuse,Func<object,object,object> fuse)
        {
            Contract.Requires(canFuse != null);
            Contract.Requires(fuse != null);
            this.canFuse = canFuse;
            this.fuse = fuse;
        }

        public Func<object, object, bool> CanFuse
        {
            get { return canFuse; }
        }

        public Func<object, object, object> Fuse { get { return fuse; } }

    }
}
