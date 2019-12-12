using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace FastFileSend.UI
{
    public class UserModel : INotifyPropertyChanged
    {
        private int id;
        private string localName;
        private bool online;

        public int Id
        {
            get { return id; }
            set { id = value; OnPropertyChanged(); }
        }
        public string LocalName
        {
            get { return localName; }
            set { localName = value; OnPropertyChanged(); }
        }

        public bool Online
        {
            get { return online; }
            set { online = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
