using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Midgard.WPFUndoManager
{
    public class UndoManager
    {

        public ICommand Undo { get; private set; }
        public ICommand Redo { get; private set; }

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

        public ReadOnlyObservableCollection<Tuple<UndoCommand, object>> RedoList
        {
            get
            {
                return redoReadonly;
            }
        }




        private void UndoFunc()
        {
            var tupel = undoStack.Pop();
            tupel.Item1.UndoExecute(tupel.Item2);
            redoStack.Push(tupel);
            (Redo as RedoC).RaiseCanExecuteChange();
            (Undo as UndoC).RaiseCanExecuteChange();
        }

        private void RedoFunc()
        {
            var tupel = redoStack.Pop();
            tupel.Item1.RedoExecute(tupel.Item2);
            undoStack.Push(tupel);
            (Redo as RedoC).RaiseCanExecuteChange();
            (Undo as UndoC).RaiseCanExecuteChange();
        }
        #endregion

        #region CommandUndo
        internal void RegisterCommandUsage(UndoCommand command, object parameter)
        {
            redoStack.Clear();
            (Redo as RedoC).RaiseCanExecuteChange();
            if (!command.CanBeUndone)
            {
                undoStack.Clear();
                (Undo as UndoC).RaiseCanExecuteChange();
                return;
            }
            undoStack.Push(new Tuple<UndoCommand, object>(command, parameter));
            (Undo as UndoC).RaiseCanExecuteChange();
        }
        #endregion

        #region Property


        internal readonly object[] emptyArray = new object[0];

        internal readonly Dictionary<Tuple<object, String>, object> oldValues;
        internal readonly HashSet<Tuple<Object, String, object>> notTrackChanges;

        public UndoManager(params INotifyPropertyChanged[] toObserve)
            : base()
        {
            undoStack = new ObservableCollection<Tuple<UndoCommand, object>>();
            undoReadonly = new ReadOnlyObservableCollection<Tuple<UndoCommand, object>>(undoStack);
            redoStack = new ObservableCollection<Tuple<UndoCommand, object>>();
            redoReadonly = new ReadOnlyObservableCollection<Tuple<UndoCommand, object>>(redoStack);
            Undo = new UndoC(this);
            Redo = new RedoC(this);
            notTrackChanges = new HashSet<Tuple<object, string, object>>();
            oldValues = new Dictionary<Tuple<object, string>, object>();
            foreach (var item in toObserve)
            {
                bool valueAdded = false;
                foreach (var prop in item.GetType().GetProperties())
                {
                    //Damit der UndoMeschanissmus funktioniert, muss die Property sowohl lesbar als auch schreibbar sein.
                    if (prop.CanRead && prop.CanWrite && prop.GetCustomAttributes(typeof(IgnorUndoManagerAttribute), true).Length == 0)
                    {
                        oldValues[new Tuple<object, string>(item, prop.Name)] = prop.GetValue(item, emptyArray);
                        valueAdded = true;
                    }
                }
                if (valueAdded)
                    item.PropertyChanged += new PropertyChangedEventHandler(item_PropertyChanged);
            }
        }

        void item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var property = sender.GetType().GetProperty(e.PropertyName);
            if (property.GetCustomAttributes(typeof(IgnorUndoManagerAttribute), true).Length > 0)
                return;
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

                UndoCommand undoCommand = null;

                if (this.undoStack.Count != 0 && this.undoStack.Peek().Item1 is PropertyUndoCommand)
                {
                    var lastCommand = this.undoStack.Peek().Item1 as PropertyUndoCommand;
                    if (Object.ReferenceEquals(sender, lastCommand.Sender) && property.GetCustomAttributes(typeof(FusePropertyChangeAttribute), true).Length > 0)
                    {
                        var fuse = property.GetCustomAttributes(typeof(FusePropertyChangeAttribute), true)[0] as FusePropertyChangeAttribute;
                        if (fuse.CanFuse(oldValue, newValue))
                        {
                            undoCommand = new PropertyUndoCommand(this, sender, property, lastCommand.OldValue, fuse.Fuse(oldValue, newValue));
                            undoStack.Pop();
                        }
                    }
                }

                if (undoCommand == null)
                    undoCommand = new PropertyUndoCommand(this, sender, property, oldValue, newValue);
                RegisterCommandUsage(undoCommand, null);
            }
        }


        #endregion

        private class UndoC : ICommand
        {

            public UndoC(UndoManager manager)
            {
                this.manager = manager;
            }

            public bool CanExecute(object parameter)
            {
                return manager.UndoList.Count > 0;
            }

            public event EventHandler CanExecuteChanged;
            private UndoManager manager;

            public void Execute(object parameter)
            {
                manager.UndoFunc();
            }

            public void RaiseCanExecuteChange()
            {
                if (CanExecuteChanged != null)
                    CanExecuteChanged(this, EventArgs.Empty);
            }
        }

        private class RedoC : ICommand
        {

            public RedoC(UndoManager manager)
            {
                this.manager = manager;
            }

            public bool CanExecute(object parameter)
            {
                return manager.RedoList.Count > 0;
            }

            public event EventHandler CanExecuteChanged;
            private UndoManager manager;

            public void Execute(object parameter)
            {
                manager.RedoFunc();
            }

            public void RaiseCanExecuteChange()
            {
                if (CanExecuteChanged != null)
                    CanExecuteChanged(this, EventArgs.Empty);
            }
        }
    }
}
