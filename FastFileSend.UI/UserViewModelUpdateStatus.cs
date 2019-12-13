using FastFileSend.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace FastFileSend.UI
{
    class UserViewModelUpdateStatus
    {
        UserViewModel UserViewModel { get; set; }
        ApiServer ApiServer { get; set; }
        Timer TimerUserStatusCheck { get; set; }

        public UserViewModelUpdateStatus(ApiServer apiServer, UserViewModel userViewModel)
        {
            UserViewModel = userViewModel;
            ApiServer = apiServer;

            TimerUserStatusCheck = new Timer(15000);
            TimerUserStatusCheck.Elapsed += TimerUserStatusCheck_Elapsed;
            TimerUserStatusCheck.Start();

            TimerUserStatusCheck_Elapsed(this, null);
        }

        private async void TimerUserStatusCheck_Elapsed(object sender, ElapsedEventArgs e)
        {
            foreach (UserModel user in UserViewModel.List.ToArray())
            {
                DateTime lastOnline = await ApiServer.GetLastOnline(user.Id);
                
                user.Online = DateTime.UtcNow.AddSeconds(-30) < lastOnline;
            }
        }
    }
}
