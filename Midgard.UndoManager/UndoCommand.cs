using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Midgard.WPFUndoManager
{
   public class UndoCommand : ICommand
    {

        private readonly Predicate<object> canExecute;
        private readonly Action<object> execute;
        private readonly Action<object> unExecute;
        private readonly UndoManager manager;

        internal bool CanBeUndone { get { return unExecute != null; } }

        public UndoCommand(UndoManager manager, Action<object> execute, Action<object> unExecute = null,
                   Predicate<object> canExecute = null)
        {
            this.manager = manager;
            this.execute = execute;
            this.canExecute = canExecute;
            this.unExecute = unExecute;
        }

        internal void UndoExecute(object parameter)
        {
            unExecute(parameter);
        }

        internal void RedoExecute(object parameter)
        {
            execute(parameter);
        }


        #region ICommand Members

        public  bool CanExecute(object parameter)
        {
            if (canExecute == null)
                return true;

            return canExecute(parameter);
        }


        public event EventHandler CanExecuteChanged;

        public  void Execute(object parameter)
        {
            execute(parameter);
            manager.RegisterCommandUsage(this, parameter);
        }


        #endregion
    }
}
