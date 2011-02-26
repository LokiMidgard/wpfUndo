using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Midgard.WPFUndoManager
{
    /// <summary>
    /// This Attribute allows to unite consecutiv Changes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public abstract class FusePropertyChangeAttribute : Attribute
    {

        /// <summary>
        /// Allows to uinite two Changes that were performed right after another.
        /// </summary>
        /// <param name="originalValue">The Value before the last Change</param>
        /// <param name="firstChange">The Value after the last Change</param>
        /// <param name="seccondChange">The Value after this Change</param>
        /// <returns>Returns true if the firstChange can be discarded. So using Undo returning to the Original Value instad to first Value.</returns>
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
