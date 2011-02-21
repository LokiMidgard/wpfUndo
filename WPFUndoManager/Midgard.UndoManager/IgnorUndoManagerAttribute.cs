using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Midgard.WPFUndoManager
{
    [AttributeUsage(AttributeTargets.Parameter, Inherited = true, AllowMultiple = false)]
    public sealed class IgnorUndoManagerAttribute : Attribute
    {

    }
}
