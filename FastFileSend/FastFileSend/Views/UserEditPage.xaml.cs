using FastFileSend.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace FastFileSend.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class UserEditPage : ContentPage
    {
        UserModel UserModel { get; set; }

        public UserEditPage(UserModel userModel)
        {
            InitializeComponent();

            UserModel = userModel;
            BindingContext = UserModel;
        }

        private void ToolbarItem_Clicked(object sender, EventArgs e)
        {
            Navigation.PopModalAsync();
        }
    }
}