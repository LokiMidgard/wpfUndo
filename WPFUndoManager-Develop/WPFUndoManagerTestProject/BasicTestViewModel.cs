using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Midgard.WPFUndoManager;
using System.Collections.ObjectModel;

namespace WPFUndoManagerTestProject
{
    public class BasicTestViewModel : INotifyPropertyChanged
    {
        public BasicTestViewModel()
        {
            StringCollection = new ObservableCollection<string>();
            this.NotifyPropertyChanged("StringCollection");
            UndoManager = new UndoManager(this);
        }

        void StringCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Console.WriteLine("Funzt in Viewmodel");
        }

        public UndoManager UndoManager { get; private set; }

        private String firstName;

        public String FirstName
        {
            get { return firstName; }
            set { firstName = value; this.NotifyPropertyChanged("FirstName"); }
        }

        public String SurName
        {
            get { return surName; }
            set { surName = value; this.NotifyPropertyChanged("SurName"); }
        }

        public ObservableCollection<String> StringCollection { get; private set; }

        public ICommand SetText
        {
            get
            {
                return new SetTextCommand(this);
            }
        }

        class SetTextCommand : ICommand
        {

            public SetTextCommand(BasicTestViewModel parent)
            {
                this.parent = parent;
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public event EventHandler CanExecuteChanged;
            private BasicTestViewModel parent;

            public void Execute(object parameter)
            {
                parent.FirstName = "FirstName is This";
            }
        }



        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private string surName;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        #endregion
    }
}