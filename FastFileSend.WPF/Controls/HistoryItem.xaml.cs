using FastFileSend.Main.Enum;
using FastFileSend.Main.ViewModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace FastFileSend.WPF.Controls
{
    /// <summary>
    /// Interaction logic for HistoryItem.xaml
    /// </summary>
    public partial class HistoryItem : UserControl
    {
        DispatcherTimer DispatcherTimer { get; set; }
        HistoryModelStatus PreviousStatus = HistoryModelStatus.Ok;

        public HistoryItem()
        {
            InitializeComponent();

            DispatcherTimer = new DispatcherTimer();
            DispatcherTimer.Interval = TimeSpan.FromSeconds(1);
            DispatcherTimer.Tick += DispatcherTimer_Tick;
            DispatcherTimer.Start();
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            LabelTimeAgo.GetBindingExpression(ContentProperty).UpdateTarget();

            HistoryViewModel history = DataContext as HistoryViewModel;
            if (history.Status != HistoryModelStatus.Ok || history.Status != PreviousStatus)
            {
                LabelSubStatus.GetBindingExpression(ContentProperty).UpdateTarget();
                PreviousStatus = history.Status;
            }
        }
    }
}
