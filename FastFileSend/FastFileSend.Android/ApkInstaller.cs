using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.Core.Content;
using Xamarin.Forms;

[assembly: Dependency(typeof(FastFileSend.Droid.ApkInstaller))]
namespace FastFileSend.Droid
{
    public class ApkInstaller : IApkInstaller
    {
        public void Launch(string path)
        {
            Intent intent = new Intent(Intent.ActionView);
            Android.Net.Uri fileUri = FileProvider.GetUriForFile(Android.App.Application.Context, "com.fastfilesend.fileprovider", new Java.IO.File(path));

            intent.PutExtra(Intent.ExtraNotUnknownSource, true);
            intent.SetDataAndType(fileUri, "application/vnd.android.package-archive");
            intent.SetFlags(ActivityFlags.NewTask);
            intent.AddFlags(ActivityFlags.GrantReadUriPermission);
            Android.App.Application.Context.StartActivity(intent);
        }
    }
}