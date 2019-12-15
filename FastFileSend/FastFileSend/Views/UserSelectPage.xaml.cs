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
    public partial class UserSelectPage : ContentPage
    {
        TaskCompletionSource<UserModel> TaskCompletionSource { get; set; }
        public UserSelectPage(TaskCompletionSource<UserModel> taskCompletionSource)
        {
            InitializeComponent();

            TaskCompletionSource = taskCompletionSource;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            ListViewUsers.BindingContext = Global.FastFileSendProgramXamarin.UserViewModel;
        }

        private async void ToolbarItemAdd_Clicked(object sender, EventArgs e)
        {
            UserModel newUser = new UserModel();
            UserEditPage userEditPage = new UserEditPage(newUser);
            userEditPage.Disappearing += UserEditPage_Disappearing;

            await Navigation.PushModalAsync(new NavigationPage(userEditPage) { Title = "Edit user" } );
        }

        private void UserEditPage_Disappearing(object sender, EventArgs e)
        {
            var userModel = (sender as UserEditPage).BindingContext;
            Global.FastFileSendProgramXamarin.UserViewModel.List.Add(userModel as UserModel);
        }

        UserModel Selected()
        {
            return (UserModel)ListViewUsers.SelectedItem;
        }

        private async void MenuItemEdit_Clicked(object sender, EventArgs e)
        {
            UserModel selected = (sender as MenuItem).CommandParameter as UserModel;

            UserEditPage userEditPage = new UserEditPage(selected);
            await Navigation.PushModalAsync(userEditPage);
        }
        private void MenuItemRemove_Clicked(object sender, EventArgs e)
        {
            UserModel selected = (sender as MenuItem).CommandParameter as UserModel;
            Global.FastFileSendProgramXamarin.UserViewModel.List.Remove(selected);
        }

        private void ToolbarItemSelect_Clicked(object sender, EventArgs e)
        {
            if (Selected() == null)
            {
                return;
            }

            Navigation.PopModalAsync();
            TaskCompletionSource.SetResult(Selected());
        }
    }
}