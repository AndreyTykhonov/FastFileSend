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

        public UserViewModel(ApiServer apiServer, HistoryViewModel historyViewModel)
        {
            List = new ObservableCollection<UserModel> { };

            Load();

            ApiServer = apiServer;

            UserViewModelUpdateStatus = new UserViewModelUpdateStatus(ApiServer, this);

            List.CollectionChanged += List_CollectionChanged;
            
            foreach (var item in List)
            {
                item.PropertyChanged += UserViewModel_PropertyChanged;
            }

            historyViewModel.List.CollectionChanged += HistoryViewList_CollectionChanged;
        }

        private void HistoryViewList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems == null)
            {
                return;
            }

            foreach (var item in e.NewItems)
            {
                HistoryModel historyModel = item as HistoryModel;
                TryAddUser(historyModel.Receiver);
                TryAddUser(historyModel.Sender);
            }
        }

        private void TryAddUser(int id)
        {
            if (id == ApiServer.Id)
            {
                return;
            }

            if (!List.Any(x => x.Id == id))
            {
                List.Add(new UserModel { Id = id, LocalName = id.ToString() });
            }
        }

        private void List_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (var item in e.NewItems)
                {
                    (item as UserModel).PropertyChanged += UserViewModel_PropertyChanged;
                }

            if (e.OldItems != null)
                foreach (var item in e.OldItems)
                {
                    (item as UserModel).PropertyChanged += UserViewModel_PropertyChanged;
                }

            Save();
        }

        private void UserViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
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
