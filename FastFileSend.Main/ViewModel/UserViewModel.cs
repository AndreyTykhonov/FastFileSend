using FastFileSend.Main.Models;
using System.ComponentModel;

namespace FastFileSend.Main.ViewModel
{
    /// <summary>
    /// Represent ViewModel of User.
    /// </summary>
    public class UserViewModel : UserModel, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
