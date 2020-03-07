using FastFileSend.Main.Models;
using System.ComponentModel;

namespace FastFileSend.Main.ViewModel
{
    /// <summary>
    /// Represent ViewModel of User.
    /// </summary>
    public class UserViewModel : UserModel, INotifyPropertyChanged
    {
        #pragma warning disable CS0067 // fody
        public event PropertyChangedEventHandler PropertyChanged;

        public override int Id { get; set; }
        public override bool Online { get; set; }
    }
}
