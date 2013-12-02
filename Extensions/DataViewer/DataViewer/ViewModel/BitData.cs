using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Company.DataViewer.ViewModel
{
    public class BitData : INotifyPropertyChanged
    {
        private readonly Action _changed;
        private string _name;
        private bool _isChecked = true;

        public BitData(Action changed)
        {
            _changed = changed;
        }

        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                _isChecked = value;
                this.OnPropertyChanged("IsChecked");
                _changed();
            }
        }

        public string BitName
        {
            get { return _name; }
            set
            {
                _name = value;
                this.OnPropertyChanged("Name");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
