using FastFileSend.Main.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace FastFileSend.Views
{
    public partial class UserEditPage : ContentPage
    {
        UserViewModel UserModel { get; set; }
        public bool OkPressed { get; set; } = false;

        public UserEditPage(UserViewModel userModel)
        {
            InitializeComponent();

            UserModel = userModel;
            BindingContext = UserModel;
        }

        private void ToolbarItem_Clicked(object sender, EventArgs e)
        {
            OkPressed = true;
            Navigation.PopModalAsync();
        }
    }
}