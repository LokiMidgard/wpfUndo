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

        private readonly StackList<Tuple<UndoCommand, object>> undoStack;
        private readonly ReadOnlyCollection<Tuple<UndoCommand, object>> undoReadonly;
        private readonly StackList<Tuple<UndoCommand, object>> redoStack;
        private readonly ReadOnlyCollection<Tuple<UndoCommand, object>> redoReadonly;

        public IList<Tuple<UndoCommand, object>> UndoList
        {
            get
            {
                return undoReadonly;
            }
        }

        public IList<Tuple<UndoCommand, object>> RediList
        {
            get
            {
                return redoReadonly;
            }
        }


        public UndoManager()
        {
            undoStack = new StackList<Tuple<UndoCommand, object>>();
            undoReadonly = new ReadOnlyCollection<Tuple<UndoCommand, object>>(undoStack);
            redoStack = new StackList<Tuple<UndoCommand, object>>();
            redoReadonly = new ReadOnlyCollection<Tuple<UndoCommand, object>>(redoStack);
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

        internal void RegisterCommandUsage(UndoCommand command, object parameter)
        {
            redoStack.Clear();
            if (!command.CanBeUndone)
            {
                undoStack.Clear();
                return;
            }


        }


    }
}
