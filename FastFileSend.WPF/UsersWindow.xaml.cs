using FastFileSend.UI;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FastFileSend.WPF
{
    /// <summary>
    /// Interaction logic for UsersWindow.xaml
    /// </summary>
    public partial class UsersWindow : MetroWindow
    {
        UserViewModel UserViewModel { get; set; }

        public UsersWindow(UserViewModel userViewModel)
        {
            InitializeComponent();

            UserViewModel = userViewModel;

            ListViewUsers.SelectedItem = null;
            ListViewUsers.DataContext = userViewModel;
        }

        private void ListViewItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ListViewItem listViewItem = sender as ListViewItem;
            if (listViewItem != null)
            {
                ListViewUsers.SelectedItem = listViewItem.Content;
                Close();
            }
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            UserModel newUser = new UserModel();
            UserAddWindow userAddWindow = new UserAddWindow(newUser);
            userAddWindow.ShowDialog();

            if (UserViewModel.List.Any(x => x.Id == newUser.Id))
            {
                return;
            }

            UserViewModel.List.Add(newUser);
        }

        private void ButtonRemove_Click(object sender, RoutedEventArgs e)
        {
            if (UserViewModel.Selected == null)
            {
                return;
            }

            UserViewModel.List.Remove(UserViewModel.Selected);
        }

        private void ButtonRename_Click(object sender, RoutedEventArgs e)
        {
            UserAddWindow userAddWindow = new UserAddWindow(UserViewModel.Selected);
            userAddWindow.ShowDialog();
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            ListViewUsers.SelectedItem = null;
            Close();
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ListViewUsers.SelectedItem = null;
            Close();
        }
    }
}
