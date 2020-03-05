using FastFileSend.Main;
using FastFileSend.Main.ViewModel;
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

namespace FastFileSend.WPF.Pages
{
    /// <summary>
    /// Interaction logic for UserAddWindow.xaml
    /// </summary>
    public partial class UserAddWindow : MetroWindow
    {
        UserViewModel UserModel { get; set; }
        public UserAddWindow(UserViewModel userModel)
        {
            InitializeComponent();

            UserModel = userModel;
            StackPanelContent.DataContext = UserModel;
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //
        }
    }
}
