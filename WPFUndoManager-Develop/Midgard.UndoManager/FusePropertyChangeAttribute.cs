using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Midgard.WPFUndoManager
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public abstract class FusePropertyChangeAttribute : Attribute
    {

        protected abstract bool FuseFunction(object originalValue, object firstChange, object seccondChange);

        /// <summary>
        /// A Function with the first Parameter the old Value of the Previus Change, the Seccond Parameter the new Value of the Previos Change and the last Parameter the new Value that shuld be assigned.
        /// This Function Returns true if those two changes can be merged.
        /// </summary>
        public Func<object, object, object, bool> CanFuse
        {
            get { return FuseFunction; }
        }

    }
}
