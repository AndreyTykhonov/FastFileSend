using FastFileSend.Main;
using FastFileSend.Main.Models;
using FastFileSend.Main.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace FastFileSend.Main.ViewModel
{
    /// <summary>
    /// Represents List of Users Model.
    /// </summary>
    public class UserListViewModel
    {
        Api ApiServer { get; set; }

        public ObservableCollection<UserViewModel> List { get; private set; }
        public UserViewModel Selected { get; set; }

        /// <summary>
        /// Returns user ID as friendly name.
        /// </summary>
        /// <param name="id">User Id.</param>
        /// <returns>Friendly name.</returns>
        public string ResolveName(int id)
        {
            if (ApiServer.AccountDetails.Id == id)
            {
                return "Me";
            }

            UserModel target = List.FirstOrDefault(x => x.Id == id);

            if (target == null || string.IsNullOrEmpty(target.LocalName))
            {
                return id.ToString(CultureInfo.InvariantCulture);
            }

            return target.LocalName;
        }

        private string UserConfigPath { get; set; }

        /// <summary>
        /// List of user models.
        /// </summary>
        /// <param name="apiServer">Authorized API server.</param>
        /// <param name="userConfigPath">Needs to auto save config updates.</param>
        public UserListViewModel(Api apiServer, string userConfigPath)
        {
            UserConfigPath = userConfigPath;

            List = new ObservableCollection<UserViewModel> { };

            Load();

            ApiServer = apiServer;;

            List.CollectionChanged += List_CollectionChanged;
            
            foreach (var item in List)
            {
                item.PropertyChanged += UserViewModel_PropertyChanged;
            }
        }

        /// <summary>
        /// Save users on add or remove.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void List_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (var item in e.NewItems)
                {
                    (item as UserViewModel).PropertyChanged += UserViewModel_PropertyChanged;
                }

            if (e.OldItems != null)
                foreach (var item in e.OldItems)
                {
                    (item as UserViewModel).PropertyChanged += UserViewModel_PropertyChanged;
                }

            Save();
        }

        /// <summary>
        /// Save users on data edit.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Save();
        }

        private void Save()
        {
            try
            {
                string json = JsonConvert.SerializeObject(List);
                File.WriteAllText(UserConfigPath, json);
            }
            catch (IOException)
            {
                // ok?
            }
        }

        private void Load()
        {
            if (File.Exists(UserConfigPath))
            {
                string json = File.ReadAllText(UserConfigPath);
                List = JsonConvert.DeserializeObject<ObservableCollection<UserViewModel>>(json);
            }
        }
    }
}
