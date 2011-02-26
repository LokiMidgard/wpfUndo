using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Diagnostics.Contracts;

namespace Midgard.WPFUndoManager
{
   public class UndoCommand : ICommand
    {

        private readonly Predicate<object> canExecute;
        private readonly Action<object> execute;
        private readonly Action<object> unExecute;
        private readonly UndoManager manager;

        internal bool CanBeUndone { get { return unExecute != null; } }

       /// <summary>
       /// Creates an UndoCommand.
       /// </summary>
       /// <param name="manager">The UndoManager that manages this Command and can Undo it.</param>
       /// <param name="execute">The Action that shuld be perfomed when execute.</param>
       /// <param name="unExecute">The Action thet revert the changes of execute. If null this Command can't be undone.</param>
       /// <param name="canExecute">The Predicate that difines if an Execute can be performed.</param>
       /// <remarks>If this Command can't be undone, all previous commands on the undone Stack are deleted!</remarks>
        public UndoCommand(UndoManager manager, Action<object> execute, Action<object> unExecute = null,
                   Predicate<object> canExecute = null)
        {
            Contract.Requires(manager != null);
            Contract.Requires(execute != null);

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

        public void RaiseCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
            {
                CanExecuteChanged(this, EventArgs.Empty);
            }
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
