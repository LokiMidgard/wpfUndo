using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace Midgard.WPFUndoManager
{
    public class UndoManager
    {

        #region EssentialUndo
        private readonly ObservableCollection<Tuple<UndoCommand, object>> undoStack;
        private readonly ReadOnlyObservableCollection<Tuple<UndoCommand, object>> undoReadonly;
        private readonly ObservableCollection<Tuple<UndoCommand, object>> redoStack;
        private readonly ReadOnlyObservableCollection<Tuple<UndoCommand, object>> redoReadonly;

        public ReadOnlyObservableCollection<Tuple<UndoCommand, object>> UndoList
        {
            get
            {
                return undoReadonly;
            }
        }

        public ReadOnlyObservableCollection<Tuple<UndoCommand, object>> RediList
        {
            get
            {
                return redoReadonly;
            }
        }


        public UndoManager()
        {
            undoStack = new ObservableCollection<Tuple<UndoCommand, object>>();
            undoReadonly = new ReadOnlyObservableCollection<Tuple<UndoCommand, object>>(undoStack);
            redoStack = new ObservableCollection<Tuple<UndoCommand, object>>();
            redoReadonly = new ReadOnlyObservableCollection<Tuple<UndoCommand, object>>(redoStack);
        }

        public void Undo()
        {
            var tupel = undoStack.Pop();
            tupel.Item1.UndoExecute(tupel.Item2);
            redoStack.Push(tupel);
        }

        public void Redo()
        {
            var tupel = redoStack.Pop();
            tupel.Item1.RedoExecute(tupel.Item2);
            undoStack.Push(tupel);
        }
        #endregion


        #region CommandUndo
        internal void RegisterCommandUsage(UndoCommand command, object parameter)
        {
            {
                redoStack.Clear();
                if (!command.CanBeUndone)
                {
                    undoStack.Clear();
                    return;
                }
            }
            undoStack.Push(new Tuple<UndoCommand, object>(command, parameter));

        }
        #endregion




        private readonly object[] emptyArray = new object[0];

        Dictionary<Tuple<object, String>, object> oldValues;
        HashSet<Tuple<Object, String, object>> notTrackChanges;

        public UndoManager(params INotifyPropertyChanged[] toObserve)
        {
            notTrackChanges = new HashSet<Tuple<object, string, object>>();
            oldValues = new Dictionary<Tuple<object, string>, object>();
            foreach (var item in toObserve)
            {
                item.PropertyChanged += new PropertyChangedEventHandler(item_PropertyChanged);
                foreach (var prop in item.GetType().GetProperties())
                {
                    //Damit der UndoMeschanissmus funktioniert, muss die Property sowohl lesbar als auch schreibbar sein.
                    if (prop.CanRead && prop.CanWrite)
                        oldValues[new Tuple<object, string>(item, prop.Name)] = prop.GetValue(item, emptyArray);
                }
            }
        }

        void item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var property = sender.GetType().GetProperty(e.PropertyName);
            var tupel = new Tuple<object, string>(sender, e.PropertyName);
            if (oldValues.ContainsKey(tupel) && property.GetCustomAttributes(typeof(IgnorUndoManagerAttribute), true).Length == 0)
            {
                var newValue = property.GetValue(sender, emptyArray);

                var notChangedTuple = new Tuple<Object, String, object>(sender, e.PropertyName, newValue);
                if (notTrackChanges.Contains(notChangedTuple))
                {
                    notTrackChanges.Remove(notChangedTuple);
                    return;
                }


                var oldValue = oldValues[tupel];
                oldValues[tupel] = newValue;

                var UndoCommand = new UndoCommand(this, obj => ChangePropertyValue(property, sender, newValue), obj => ChangePropertyValue(property, sender, oldValue));
                RegisterCommandUsage(UndoCommand, null);
            }
        }

        private void ChangePropertyValue(PropertyInfo prop, Object obj, object value)
        {
            var t = new Tuple<Object, String, object>(obj, prop.Name, value);
            notTrackChanges.Add(t);
            prop.SetValue(obj, value, emptyArray);
        }

    }
}
