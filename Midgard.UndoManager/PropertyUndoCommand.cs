using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Midgard.WPFUndoManager
{
    class PropertyUndoCommand : UndoCommand
    {
       
        public PropertyUndoCommand(UndoManager manager, Object sender, PropertyInfo prop, object oldValue, object newValue)
            : base(manager, obj => ChangePropertyValue(manager, prop, sender, newValue), obj => ChangePropertyValue(manager, prop, sender, oldValue))
        {
            this.Sender = sender;
            this.NewValue = newValue;
            this.OldValue = oldValue;
            this.Property = prop;
        }

        private static void ChangePropertyValue(UndoManager manager, PropertyInfo prop, Object obj, object value)
        {
            var t = new Tuple<Object, String, object>(obj, prop.Name, value);
            manager.notTrackChanges.Add(t);
            prop.SetValue(obj, value, manager.emptyArray);
        }

        public object OldValue { get; private set; }

        public object NewValue { get; private set; }

        public object Sender { get; private set; }

        public PropertyInfo Property { get; private set; }
    }
}
