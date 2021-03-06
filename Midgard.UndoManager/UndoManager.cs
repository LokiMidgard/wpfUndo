﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Diagnostics.Contracts;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Collections;

namespace Midgard.WPFUndoManager
{
    public class UndoManager
    {


        /// <summary>
        /// Expose the Undo Command
        /// </summary>
        public ICommand Undo { get { return undoCommand; } }

        /// <summary>
        /// Expose the Redo Command
        /// </summary>
        public ICommand Redo { get { return redoCommand; } }

        /// <summary>
        /// Expose a List of the last done Actions
        /// </summary>
        public ReadOnlyObservableCollection<Tuple<UndoCommand, object>> UndoList
        {
            get
            {
                return undoReadonly;
            }
        }

        /// <summary>
        /// Expose a List of the last undone Actions
        /// </summary>
        public ReadOnlyObservableCollection<Tuple<UndoCommand, object>> RedoList
        {
            get
            {
                return redoReadonly;
            }
        }

        private readonly ObservableCollection<Tuple<UndoCommand, object>> undoStack;
        private readonly ReadOnlyObservableCollection<Tuple<UndoCommand, object>> undoReadonly;
        private readonly ObservableCollection<Tuple<UndoCommand, object>> redoStack;
        private readonly ReadOnlyObservableCollection<Tuple<UndoCommand, object>> redoReadonly;
        private readonly UndoC undoCommand;
        private readonly RedoC redoCommand;
        private readonly ISet<BlockChanges> blockChangesSet;

        internal readonly object[] emptyArray = new object[0];

        internal readonly Dictionary<Tuple<object, String>, object> oldValues;
        internal readonly Dictionary<Tuple<object, String>, INotifyCollectionChanged> colleciontsListento;


        /// <summary>
        /// Creates an Instance of UndoManager
        /// </summary>
        /// <param name="toObserve">All Object's that shuld be Monitored.</param>
        public UndoManager(params INotifyPropertyChanged[] toObserve)
        {
            Contract.Requires(toObserve != null);

            undoStack = new ObservableCollection<Tuple<UndoCommand, object>>();
            undoReadonly = new ReadOnlyObservableCollection<Tuple<UndoCommand, object>>(undoStack);
            redoStack = new ObservableCollection<Tuple<UndoCommand, object>>();
            redoReadonly = new ReadOnlyObservableCollection<Tuple<UndoCommand, object>>(redoStack);
            blockChangesSet = new HashSet<BlockChanges>();
            undoCommand = new UndoC(this);
            redoCommand = new RedoC(this);
            colleciontsListento = new Dictionary<Tuple<object, String>, INotifyCollectionChanged>();
            oldValues = new Dictionary<Tuple<object, string>, object>();
            foreach (var item in toObserve)
            {
                bool valueAdded = false;
                foreach (var prop in item.GetType().GetProperties())
                {
                    if (prop.GetCustomAttributes(typeof(IgnorUndoManagerAttribute), true).Length == 0)
                    {
                        //Damit der UndoMeschanissmus funktioniert, muss die Property sowohl lesbar als auch schreibbar sein.
                        if (prop.CanRead && prop.CanWrite)
                        {
                            oldValues[new Tuple<object, string>(item, prop.Name)] = prop.GetValue(item, emptyArray);
                            valueAdded = true;
                        }
                        var collectionChanged = prop.GetValue(item, new object[0]) as INotifyCollectionChanged;
                        if (collectionChanged != null)
                        {
                            collectionChanged.CollectionChanged += collectionChanged_CollectionChanged;

                            this.colleciontsListento.Add(new Tuple<object, String>(item, prop.Name), collectionChanged);
                        }
                    }
                }
                if (valueAdded)
                    item.PropertyChanged += new PropertyChangedEventHandler(observed_PropertyChanged);
            }
        }

        /// <summary>
        /// Delets the History off all Commands managed by this UndoManager.
        /// </summary>
        public void ClearUndoHistory()
        {
            redoStack.Clear();
            undoStack.Clear();
        }

        void collectionChanged_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {

            if (IsList(sender))
            {
                RegisterIListChanges(sender, e);
            }
            else if (IsCollection(sender))
            {
                RegisterICollectionChanges(sender, e);
            }
            else
            {
                Debug.Assert(true, "The Class that implements INotifyCollectionChanged does not Implement ICollection<T>, IList or IList<T>");
            }
        }


        void observed_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Contract.Requires(sender != null);
            Contract.Requires(e.PropertyName != null);

            var property = sender.GetType().GetProperty(e.PropertyName);
            if (property.GetCustomAttributes(typeof(IgnorUndoManagerAttribute), true).Length > 0)
                return;
            var tupel = new Tuple<object, string>(sender, e.PropertyName);

            if (colleciontsListento.ContainsKey(tupel))
            {
                colleciontsListento[tupel].CollectionChanged -= collectionChanged_CollectionChanged;
            }
            var collectionChanged = property.GetValue(sender, new object[0]) as INotifyCollectionChanged;
            if (collectionChanged != null)
            {
                collectionChanged.CollectionChanged += collectionChanged_CollectionChanged;

                this.colleciontsListento.Add(new Tuple<object, String>(sender, property.Name), collectionChanged);
            }

            if (oldValues.ContainsKey(tupel) && property.GetCustomAttributes(typeof(IgnorUndoManagerAttribute), true).Length == 0)
            {
                var newValue = property.GetValue(sender, emptyArray);

                var oldValue = oldValues[tupel];
                oldValues[tupel] = newValue;

                var undoCommand = new PropertyUndoCommand(this, sender, property, oldValue, newValue);
                RegisterCommandUsage(undoCommand, null);
            }
        }

        /// <summary>
        /// Block the registration of all changes untill the IDisposable is disposed.
        /// </summary>
        /// <returns>The Disposable to dispose.</returns>
        public IDisposable SuspendRegisteringChanges()
        {
            var blocker = new BlockChanges((sender) => blockChangesSet.Remove(sender));
            blockChangesSet.Add(blocker);
            return blocker;
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
            Contract.Invariant(colleciontsListento != null);
            Contract.Invariant(blockChangesSet != null);
        }

        private void RegisterICollectionChanges(object sender, NotifyCollectionChangedEventArgs e)
        {
            var add = sender.GetType().GetMethod("Add");
            var remove = sender.GetType().GetMethod("Remove");
            switch (e.Action)
            {

                case NotifyCollectionChangedAction.Add:
                    var commandAdd = new UndoCommand(this, param =>
                    {
                        using (this.SuspendRegisteringChanges())
                        {
                            foreach (var item in e.NewItems)
                            {
                                add.Invoke(sender, new object[] { item });
                            }
                        }
                    }
                    , param =>
                    {
                        using (this.SuspendRegisteringChanges())
                        {
                            foreach (var item in e.NewItems)
                            {
                                remove.Invoke(sender, new Object[] { item });
                            }
                        }
                    });
                    RegisterCommandUsage(commandAdd, null);
                    break;
                case NotifyCollectionChangedAction.Move:
                    throw new NotSupportedException("You can only Move object in IList or IList<T> not Collection<T>");
                case NotifyCollectionChangedAction.Remove:
                    var commandRemove = new UndoCommand(this, param =>
                    {
                        using (this.SuspendRegisteringChanges())
                        {
                            foreach (var item in e.OldItems)
                            {
                                remove.Invoke(sender, new Object[] { item });
                            }
                        }
                    }
                    , param =>
                    {
                        using (this.SuspendRegisteringChanges())
                        {
                            foreach (var item in e.OldItems)
                            {
                                add.Invoke(sender, new object[] { item });
                            }
                        }
                    }
                    );
                    RegisterCommandUsage(commandRemove, null);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    var commandReplace = new UndoCommand(this, param =>
                    {
                        using (this.SuspendRegisteringChanges())
                        {
                            foreach (var item in e.OldItems)
                            {
                                remove.Invoke(sender, new object[] { item });
                            }
                            foreach (var item in e.NewItems)
                            {
                                add.Invoke(sender, new object[] { item });
                            }

                        }
                    }
                        , param =>
                        {
                            using (this.SuspendRegisteringChanges())
                            {
                                foreach (var item in e.NewItems)
                                {
                                    remove.Invoke(sender, new object[] { item });
                                }
                                foreach (var item in e.OldItems)
                                {
                                    add.Invoke(sender, new object[] { item });
                                }
                            }
                        });
                    RegisterCommandUsage(commandReplace, null);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    //TODO save changelog to be able to rekonstrukt Reset.
                    ClearUndoHistory();
                    break;
                default:
                    Debug.Assert(true, "No default shuld exist");
                    break;
            }

        }

        private void RegisterIListChanges(object sender, NotifyCollectionChangedEventArgs e)
        {
            var indexer = sender.GetType().GetProperty("Item");
            var insert = sender.GetType().GetMethod("Insert");
            var removeAt = sender.GetType().GetMethod("RemoveAt");
            switch (e.Action)
            {

                case NotifyCollectionChangedAction.Add:
                    var commandAdd = new UndoCommand(this, param =>
                    {
                        using (this.SuspendRegisteringChanges())
                        {
                            for (int i = e.NewItems.Count - 1; i >= 0; i--)
                            {
                                insert.Invoke(sender, new object[] { e.NewStartingIndex, e.NewItems[i] });
                            }
                        }
                    }
                    , param =>
                    {
                        using (this.SuspendRegisteringChanges())
                        {
                            for (int i = 0; i < e.NewItems.Count; i++)
                            {
                                Debug.Assert(indexer.GetValue(sender, new object[] { e.NewStartingIndex }) == e.NewItems[i]);
                                removeAt.Invoke(sender, new Object[] { e.NewStartingIndex });
                            }
                        }
                    });
                    RegisterCommandUsage(commandAdd, null);
                    break;
                case NotifyCollectionChangedAction.Move:
                    var commandMove = new UndoCommand(this, param =>
                    {
                        using (this.SuspendRegisteringChanges())
                        {
                            for (int i = 0; i < e.OldItems.Count; i++)
                            {
                                Debug.Assert(indexer.GetValue(sender, new object[] { e.OldStartingIndex }) == e.OldItems[i]);
                                removeAt.Invoke(sender, new Object[] { e.OldStartingIndex });
                            }
                            for (int i = e.NewItems.Count - 1; i >= 0; i--)
                            {
                                insert.Invoke(sender, new object[] { e.NewStartingIndex, e.NewItems[i] });
                            }
                        }
                    }
                    , param =>
                    {
                        using (this.SuspendRegisteringChanges())
                        {
                            for (int i = 0; i < e.NewItems.Count; i++)
                            {
                                Debug.Assert(indexer.GetValue(sender, new object[] { e.NewStartingIndex }) == e.NewItems[i]);
                                removeAt.Invoke(sender, new Object[] { e.NewStartingIndex });
                            }
                            for (int i = e.NewItems.Count - 1; i >= 0; i--)
                            {
                                insert.Invoke(sender, new object[] { e.OldStartingIndex, e.OldItems[i] });
                            }
                        }
                    }
                    );
                    RegisterCommandUsage(commandMove, null);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    var commandRemove = new UndoCommand(this, param =>
                    {
                        using (this.SuspendRegisteringChanges())
                        {
                            for (int i = 0; i < e.OldItems.Count; i++)
                            {
                                Debug.Assert(indexer.GetValue(sender, new object[] { e.OldStartingIndex }) == e.OldItems[i]);
                                removeAt.Invoke(sender, new Object[] { e.OldStartingIndex });
                            }
                        }
                    }
                    , param =>
                    {
                        using (this.SuspendRegisteringChanges())
                        {
                            for (int i = e.OldItems.Count - 1; i >= 0; i--)
                            {
                                insert.Invoke(sender, new object[] { e.OldStartingIndex, e.OldItems[i] });
                            }
                        }
                    }
                    );
                    RegisterCommandUsage(commandRemove, null);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    var commandReplace = new UndoCommand(this, param =>
                    {
                        using (this.SuspendRegisteringChanges())
                        {
                            for (int i = e.NewItems.Count - 1; i >= 0; i--)
                            {
                                indexer.SetValue(sender, e.NewItems[i], new object[] { i + e.NewStartingIndex });
                            }
                        }
                    }
                        , param =>
                        {
                            using (this.SuspendRegisteringChanges())
                            {
                                for (int i = 0; i < e.NewItems.Count; i++)
                                {
                                    indexer.SetValue(sender, e.OldItems[i], new object[] { i + e.NewStartingIndex });
                                }
                            }
                        });
                    RegisterCommandUsage(commandReplace, null);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    //TODO save changelog to be able to rekonstrukt Reset.
                    ClearUndoHistory();
                    break;
                default:
                    Debug.Assert(true, "No default shuld exist");
                    break;
            }
        }

        private static bool IsList(object sender)
        {
            if (sender is IList)
                return true;
            Queue<Type> queue = new Queue<Type>(sender.GetType().GetInterfaces());
            while (queue.Count != 0)
            {
                var inter = queue.Dequeue();
                foreach (var item in inter.GetInterfaces())
                    queue.Enqueue(item);

                if (!inter.IsGenericType)
                    continue;
                Contract.Assume(typeof(IList<object>).IsGenericType);
                var listGenType = typeof(IList<object>).GetGenericTypeDefinition();

                var gentType = inter.GetGenericTypeDefinition();
                if (listGenType == gentType)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool IsCollection(object sender)
        {
            Queue<Type> queue = new Queue<Type>(sender.GetType().GetInterfaces());
            while (queue.Count != 0)
            {
                var inter = queue.Dequeue();
                foreach (var item in inter.GetInterfaces())
                    queue.Enqueue(item);

                if (!inter.IsGenericType)
                    continue;
                Contract.Assume(typeof(ICollection<object>).IsGenericType);
                var listGenType = typeof(ICollection<object>).GetGenericTypeDefinition();

                var gentType = inter.GetGenericTypeDefinition();
                if (listGenType == gentType)
                {
                    return true;
                }
            }
            return false;
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

        internal void RegisterCommandUsage(UndoCommand command, object parameter)
        {
            Contract.Requires(command != null);

            if (blockChangesSet.Count != 0)
                return;

            redoStack.Clear();
            redoCommand.RaiseCanExecuteChange();
            if (!command.CanBeUndone)
            {
                undoStack.Clear();
                undoCommand.RaiseCanExecuteChange();
                return;
            }

            UndoCommand editedCommand = null;
            if (this.undoStack.Count > 0 && this.undoStack.Peek().Item1 is PropertyUndoCommand && command is PropertyUndoCommand)
            {
                var lastCommand = this.undoStack.Peek().Item1 as PropertyUndoCommand;
                var newCommand = command as PropertyUndoCommand;
                if (Object.ReferenceEquals(newCommand.Sender, lastCommand.Sender) && lastCommand.NewValue == newCommand.OldValue && newCommand.Property.Equals(lastCommand.Property) && newCommand.Property.GetCustomAttributes(typeof(FusePropertyChangeAttribute), true).Length > 0)
                {
                    var fuse = newCommand.Property.GetCustomAttributes(typeof(FusePropertyChangeAttribute), true)[0] as FusePropertyChangeAttribute;
                    if (fuse.CanFuse(lastCommand.OldValue, lastCommand.NewValue, newCommand.NewValue))
                    {
                        editedCommand = new PropertyUndoCommand(this, newCommand.Sender, newCommand.Property, lastCommand.OldValue, newCommand.NewValue);
                        undoStack.Pop();
                    }
                }
            }

            undoStack.Push(new Tuple<UndoCommand, object>(editedCommand ?? command, parameter));
            undoCommand.RaiseCanExecuteChange();
        }




        public class BlockChanges : IDisposable
        {
            Action<BlockChanges> onDispose;
            public BlockChanges(Action<BlockChanges> onDispose)
            {
                IsDisposed = false;
                this.onDispose = onDispose;
            }

            public bool IsDisposed { get; private set; }

            public void Dispose()
            {
                IsDisposed = true;
                onDispose(this);
            }
        }


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
