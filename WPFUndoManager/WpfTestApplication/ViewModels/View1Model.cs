using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Midgard.WPFUndoManager;
using System.Collections.ObjectModel;

namespace WpfTestApplication
{
	public class View1Model : INotifyPropertyChanged
	{
		public View1Model()
		{
            UndoManager = new UndoManager(this);
            FirstList = new ObservableCollection<string>(new String[] {"1","2"});
            SeccondList = new ObservableCollection<string>(new String[] { "a", "b" });


            Action<object> fromOneToTwo = obj => { FirstList.Remove(obj as String); SeccondList.Add(obj as string); };
            Action<object> fromTwoToOne = obj => { SeccondList.Remove(obj as String); FirstList.Add(obj as string); };

            FromFirstToSeccond = new UndoCommand(UndoManager, fromOneToTwo, fromTwoToOne, obj => FirstList.Contains(obj as string));    
            FromSeccondToFirst = new UndoCommand(UndoManager, fromTwoToOne, fromOneToTwo, obj => SeccondList.Contains(obj as string));
        }

        public UndoManager UndoManager { get; private set; }

        private String name;

        public String Name
        {
            get { return name; }
            set { name = value; this.NotifyPropertyChanged("Name"); }
        }

        public ObservableCollection<String> FirstList { get; private set; }
        public ObservableCollection<String> SeccondList { get; private set; }

        public ICommand FromFirstToSeccond { get; private set; }
        public ICommand FromSeccondToFirst { get; private set; }

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