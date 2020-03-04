using FastFileSend.Main;
using FastFileSend.Main.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace FastFileSend.Main
{
    /// <summary>
    /// Updates users online status on timer.
    /// </summary>
    class UserListViewModelUpdater
    {
        UserListViewModel UserListViewModel { get; set; }
        Api ApiServer { get; set; }
        Timer TimerUserStatusCheck { get; set; }

        const double UserOnlineUpdateInterval = 15000;

        public UserListViewModelUpdater(Api apiServer, UserListViewModel userViewModel)
        {
            UserListViewModel = userViewModel;
            ApiServer = apiServer;

            TimerUserStatusCheck = new Timer(UserOnlineUpdateInterval);
            TimerUserStatusCheck.Elapsed += TimerUserStatusCheck_Elapsed;
            TimerUserStatusCheck.Start();

            TimerUserStatusCheck_Elapsed(this, null);
        }

        /// <summary>
        /// Updates all users last online.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void TimerUserStatusCheck_Elapsed(object sender, ElapsedEventArgs e)
        {
            foreach (UserViewModel user in UserListViewModel.List.ToArray())
            {
                DateTime lastOnline = await ApiServer.GetLastOnline(user.Id);
                
                user.Online = DateTime.UtcNow.AddSeconds(-30) < lastOnline;
            }
        }
    }
}
