using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Midgard.WPFUndoManager
{
    /// <summary>
    /// This Attribute prevent the Monetoring of an Property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class IgnorUndoManagerAttribute : Attribute
    {

    }
}
