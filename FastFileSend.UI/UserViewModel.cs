using FastFileSend.Main;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace FastFileSend.UI
{
    public class UserViewModel
    {
        ApiServer ApiServer { get; set; }
        private UserViewModelUpdateStatus UserViewModelUpdateStatus { get; set;}

        public ObservableCollection<UserModel> List { get; private set; }
        public UserModel Selected { get; set; }

        public string ResolveName(int id)
        {
            if (ApiServer.Id == id)
            {
                return "Me";
            }

            UserModel target = List.FirstOrDefault(x => x.Id == id);

            if (target == null || string.IsNullOrEmpty(target.LocalName))
            {
                return id.ToString();
            }

            return target.LocalName;
        }

        public UserViewModel(ApiServer apiServer)
        {
            List = new ObservableCollection<UserModel> { };

            Load();

            ApiServer = apiServer;

            UserViewModelUpdateStatus = new UserViewModelUpdateStatus(ApiServer, this);

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
