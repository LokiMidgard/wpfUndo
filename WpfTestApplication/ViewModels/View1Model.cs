using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.ComponentModel;
using System.Windows;

namespace WpfTestApplication
{
	public class View1Model : INotifyPropertyChanged
	{
		public View1Model()
		{
			
		}


        private String name;

        public String Name
        {
            get { return name; }
            set { name = value; }
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