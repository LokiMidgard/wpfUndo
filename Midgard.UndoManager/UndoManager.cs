﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Diagnostics.Contracts;

namespace Midgard.WPFUndoManager
{
    public class UndoManager
    {


        public ICommand Undo { get { return undoCommand; } }
        public ICommand Redo { get { return redoCommand; } }

        #region EssentialUndo
        private readonly ObservableCollection<Tuple<UndoCommand, object>> undoStack;
        private readonly ReadOnlyObservableCollection<Tuple<UndoCommand, object>> undoReadonly;
        private readonly ObservableCollection<Tuple<UndoCommand, object>> redoStack;
        private readonly ReadOnlyObservableCollection<Tuple<UndoCommand, object>> redoReadonly;
        private readonly UndoC undoCommand;
        private readonly RedoC redoCommand;

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

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(undoReadonly != null);
            Contract.Invariant(redoReadonly != null);
            Contract.Invariant(undoStack != null);
            Contract.Invariant(redoStack != null);
            Contract.Invariant(Undo != null);
            Contract.Invariant(Redo != null);
            Contract.Invariant(undoCommand != null);
            Contract.Invariant(redoCommand != null);
        }



        private void UndoFunc()
        {
            Contract.Requires(undoStack.Count > 0);
            var tupel = undoStack.Pop();
            tupel.Item1.UndoExecute(tupel.Item2);
            redoStack.Push(tupel);
            redoCommand.RaiseCanExecuteChange();
            undoCommand.RaiseCanExecuteChange();
        }

        private void RedoFunc()
        {
            Contract.Requires(redoStack.Count > 0);
            var tupel = redoStack.Pop();
            tupel.Item1.RedoExecute(tupel.Item2);
            undoStack.Push(tupel);
            redoCommand.RaiseCanExecuteChange();
            undoCommand.RaiseCanExecuteChange();
        }
        #endregion

        #region CommandUndo
        internal void RegisterCommandUsage(UndoCommand command, object parameter)
        {
            Contract.Requires(command != null);

            redoStack.Clear();
            redoCommand.RaiseCanExecuteChange();
            if (!command.CanBeUndone)
            {
                undoStack.Clear();
                undoCommand.RaiseCanExecuteChange();
                return;
            }
            undoStack.Push(new Tuple<UndoCommand, object>(command, parameter));
            undoCommand.RaiseCanExecuteChange();
        }
        #endregion

        #region Property


        internal readonly object[] emptyArray = new object[0];

        internal readonly Dictionary<Tuple<object, String>, object> oldValues;
        internal readonly HashSet<Tuple<Object, String, object>> notTrackChanges;

        public UndoManager(params INotifyPropertyChanged[] toObserve)
        {
            Contract.Requires(toObserve != null);

            undoStack = new ObservableCollection<Tuple<UndoCommand, object>>();
            undoReadonly = new ReadOnlyObservableCollection<Tuple<UndoCommand, object>>(undoStack);
            redoStack = new ObservableCollection<Tuple<UndoCommand, object>>();
            redoReadonly = new ReadOnlyObservableCollection<Tuple<UndoCommand, object>>(redoStack);
            undoCommand = new UndoC(this);
            redoCommand = new RedoC(this);
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
            Contract.Requires(sender != null);
            Contract.Requires(e.PropertyName != null);

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

                if (this.undoStack.Count > 0 && this.undoStack.Peek().Item1 is PropertyUndoCommand)
                {
                    var lastCommand = this.undoStack.Peek().Item1 as PropertyUndoCommand;
                    if (Object.ReferenceEquals(sender, lastCommand.Sender) && property.GetCustomAttributes(typeof(FusePropertyChangeAttribute), true).Length > 0)
                    {
                        var fuse = property.GetCustomAttributes(typeof(FusePropertyChangeAttribute), true)[0] as FusePropertyChangeAttribute;
                        if (fuse.CanFuse(lastCommand.OldValue, lastCommand.NewValue, newValue))
                        {
                            undoCommand = new PropertyUndoCommand(this, sender, property, lastCommand.OldValue, newValue);
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

            [ContractInvariantMethod]
            private void ObjectInvariant()
            {
                Contract.Invariant(manager != null);
            }


            public UndoC(UndoManager manager)
            {
                Contract.Requires(manager != null);
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
                if (!CanExecute(parameter))
                    throw new ArgumentException();
                Contract.Assume(manager.undoStack.Count > 0);

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

            [ContractInvariantMethod]
            private void ObjectInvariant()
            {
                Contract.Invariant(manager != null);
            }

            public RedoC(UndoManager manager)
            {
                Contract.Requires(manager != null);

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
                if (!CanExecute(parameter))
                    throw new ArgumentException();
                Contract.Assume(manager.redoStack.Count > 0);
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