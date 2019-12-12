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
    /// Interaction logic for UserAddWindow.xaml
    /// </summary>
    public partial class UserAddWindow : MetroWindow
    {
        UserModel UserModel { get; set; }
        public UserAddWindow(UserModel userModel)
        {
            InitializeComponent();

            StackPanelContent.DataContext = userModel;

            UserModel = userModel;
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (string.IsNullOrEmpty(UserModel.LocalName) || UserModel.LocalName.Length < 5)
            {
                UserModel.LocalName = "CHANGE NAME!";
            }
        }
    }
}
