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
    public partial class UserSelectPage : ContentPage
    {
        TaskCompletionSource<UserViewModel> TaskCompletionSource { get; set; }
        public UserSelectPage(TaskCompletionSource<UserViewModel> taskCompletionSource)
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

            ListViewUsers.BindingContext = App.FastFileSendApp;
        }

        bool CanAddUser = true;

        private async void ToolbarItemAdd_Clicked(object sender, EventArgs e)
        {
            if (!CanAddUser)
            {
                return;
            }

            CanAddUser = false;

            UserViewModel newUser = new UserViewModel();
            UserEditPage userEditPage = new UserEditPage(newUser);
            userEditPage.Disappearing += UserEditPage_Disappearing;

            await Navigation.PushModalAsync(new NavigationPage(userEditPage) { Title = "Edit user" } );
        }

        private void UserEditPage_Disappearing(object sender, EventArgs e)
        {
            if ((sender as UserEditPage).OkPressed)
            {
                var userModel = (sender as UserEditPage).BindingContext;
                App.FastFileSendApp.UserListViewModel.List.Add(userModel as UserViewModel);
            }

            CanAddUser = true;
        }

        UserViewModel Selected()
        {
            return (UserViewModel)ListViewUsers.SelectedItem;
        }

        private async void MenuItemEdit_Clicked(object sender, EventArgs e)
        {
            UserViewModel selected = (sender as MenuItem).CommandParameter as UserViewModel;

            UserEditPage userEditPage = new UserEditPage(selected);
            await Navigation.PushModalAsync(userEditPage);
        }
        private void MenuItemRemove_Clicked(object sender, EventArgs e)
        {
            UserViewModel selected = (sender as MenuItem).CommandParameter as UserViewModel;
            App.FastFileSendApp.UserListViewModel.List.Remove(selected);
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