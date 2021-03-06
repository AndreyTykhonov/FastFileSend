﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using FastFileSend.Main.Interfaces;
using Xamarin.Forms;

[assembly: Dependency(typeof(FastFileSend.Droid.PathResolver))]
namespace FastFileSend.Droid
{
    public class PathResolver : IPathResolver
    {
        //public string UsersConfig => Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "users.json");
        public string UsersConfig => Path.Combine(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).AbsolutePath, "users.json");

        //public string AccountConfig => Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "account.json");
        public string AccountConfig => Path.Combine(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).AbsolutePath, "account.json");

        public string Downloads => Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).AbsolutePath;

        public string Temp => Path.GetTempPath();
    }
}