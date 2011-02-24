using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Midgard.WPFUndoManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace WPFUndoManagerTestProject
{
    public class IgnoreUndoManagerAttributeTestViewModel : INotifyPropertyChanged
    {
        public IgnoreUndoManagerAttributeTestViewModel()
        {
            UndoManager = new UndoManager(this);
        }
        private string surName;

        public UndoManager UndoManager { get; private set; }

        private String firstName;

        public String FirstName
        {
            get { return firstName; }
            set { firstName = value; this.NotifyPropertyChanged("FirstName"); }
        }

        [IgnorUndoManager]
        public String NotRegistertName
        {
            get { return surName; }
            set { surName = value; this.NotifyPropertyChanged("NotRegistertName"); }
        }

        public ICommand SetText
        {
            get
            {
                return new SetTextCommand(this);
            }
        }

        class SetTextCommand : ICommand
        {

            public SetTextCommand(IgnoreUndoManagerAttributeTestViewModel parent)
            {
                this.parent = parent;
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public event EventHandler CanExecuteChanged;
            private IgnoreUndoManagerAttributeTestViewModel parent;

            public void Execute(object parameter)
            {
                parent.FirstName = "FirstName is This";
            }
        }



        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            ThrowIfPropertyNotExist(info);
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        private void ThrowIfPropertyNotExist(String name)
        {
            if (this.GetType().GetProperty(name) == null)
                throw new ArgumentException("Property does not Exist");
        }
        #endregion
    }
}