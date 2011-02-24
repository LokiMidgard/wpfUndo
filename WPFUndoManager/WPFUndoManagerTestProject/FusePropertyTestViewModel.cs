using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Midgard.WPFUndoManager;
using System.Diagnostics.Contracts;
using System.Diagnostics.CodeAnalysis;

namespace WPFUndoManagerTestProject
{
    public class FusePropertyTestViewModel : INotifyPropertyChanged
    {
        public FusePropertyTestViewModel()
        {
            UndoManager = new UndoManager(this);
        }

        public UndoManager UndoManager { get; private set; }

        private int fuseInt;

        [IntFuse]
        public int FuseInt
        {
            get { return fuseInt; }
            set { fuseInt = value; this.NotifyPropertyChanged("FuseInt"); }
        }

        private int noFuseInt;

        public int NoFuseInt
        {
            get { return noFuseInt; }
            set { noFuseInt = value; this.NotifyPropertyChanged("NoFuseInt"); }
        }




        class IntFuseAttribute : FusePropertyChangeAttribute
        {
            protected override bool FuseFunction(object originalValue, object firstChange, object seccondChange)
            {
                int s1 = (int)originalValue;
                int s2 = (int)firstChange;
                int s3 = (int)seccondChange;
                if (s1 == s2 + 1 && s2 == s3 + 1)
                    return true;
                if (s3 == s2 + 1 && s2 == s1 + 1)
                    return true;
                return false;
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