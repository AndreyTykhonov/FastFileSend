using FastFileSend.Main;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace FastFileSend.UI
{
    public class UserViewModel
    {
        private UserViewModelUpdateStatus UserViewModelUpdateStatus { get; set;}

        public ObservableCollection<UserModel> List { get; private set; }
        public UserModel Selected { get; set; }

        public UserViewModel(ApiServer apiServer)
        {
            List = new ObservableCollection<UserModel> { };

            Load();

            UserViewModelUpdateStatus = new UserViewModelUpdateStatus(apiServer, this);

            List.CollectionChanged += List_CollectionChanged;
        }

        private void List_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Save();
        }

        private void Save()
        {
            string json = JsonConvert.SerializeObject(List);
            File.WriteAllText(FilePathHelper.UsersConfig, json);
        }

        private void Load()
        {
            if (File.Exists(FilePathHelper.UsersConfig))
            {
                string json = File.ReadAllText(FilePathHelper.UsersConfig);
                List = JsonConvert.DeserializeObject<ObservableCollection<UserModel>>(json);
            }
        }
    }
}
