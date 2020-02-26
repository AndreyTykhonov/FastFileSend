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
            (Application.Current as App).ModalPopping += UserSelectPage_ModalPopping;
        }

        private void UserSelectPage_ModalPopping(object sender, ModalPoppingEventArgs e)
        {
            if ((e.Modal as NavigationPage).CurrentPage != this)
            {
                return;
            }

            (Application.Current as App).ModalPopping -= UserSelectPage_ModalPopping;
            TaskCompletionSource.TrySetResult(null);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            ListViewUsers.BindingContext = Global.FastFileSendProgramXamarin.UserViewModel;
        }

        bool CanAddUser = true;

        private async void ToolbarItemAdd_Clicked(object sender, EventArgs e)
        {
            if (!CanAddUser)
            {
                return;
            }

            CanAddUser = false;

            UserModel newUser = new UserModel();
            UserEditPage userEditPage = new UserEditPage(newUser);
            userEditPage.Disappearing += UserEditPage_Disappearing;

            await Navigation.PushModalAsync(new NavigationPage(userEditPage) { Title = "Edit user" } );
        }

        private void UserEditPage_Disappearing(object sender, EventArgs e)
        {
            if ((sender as UserEditPage).OkPressed)
            {
                var userModel = (sender as UserEditPage).BindingContext;
                Global.FastFileSendProgramXamarin.UserViewModel.List.Add(userModel as UserModel);
            }

            CanAddUser = true;
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

            TaskCompletionSource.SetResult(Selected());
            Navigation.PopModalAsync();            
        }
    }
}