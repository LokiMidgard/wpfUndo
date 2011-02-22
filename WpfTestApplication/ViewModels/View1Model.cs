using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Midgard.WPFUndoManager;

namespace WpfTestApplication
{
	public class View1Model : INotifyPropertyChanged
	{
		public View1Model()
		{
            Name = "muh";

            UndoManager = new UndoManager(this);
		}

        public UndoManager UndoManager { get; private set; }

        private String name;

        public String Name
        {
            get { return name; }
            set { name = value; this.NotifyPropertyChanged("Name"); }
        }

        public ICommand SetText
        {
            get
            {
                return new SetTextCommand(this);
            }
        }

        class SetTextCommand:ICommand
        {

            public SetTextCommand(View1Model parent)
            {
                this.parent = parent;
            }

            public bool CanExecute(object parameter)
            {
               return  true;
            }

            public event EventHandler CanExecuteChanged;
            private View1Model parent;

            public void Execute(object parameter)
            {
               parent.Name="test";
            }
        }
        


		#region INotifyPropertyChanged
		public event PropertyChangedEventHandler PropertyChanged;

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