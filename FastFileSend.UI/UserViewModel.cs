using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace FastFileSend.UI
{
    public class UserViewModel
    {
        public ObservableCollection<UserModel> List { get; private set; }
        public UserModel Selected { get; set; }

        public UserViewModel()
        {
            List = new ObservableCollection<UserModel> { new UserModel { Id = 1000, Online = false, LocalName = "heH" }, new UserModel { Id = 2000, Online = true, LocalName = "jivoi" }  };
        }
    }
}
