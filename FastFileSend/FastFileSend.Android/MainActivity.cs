using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Threading.Tasks;
using System.IO;
using Android.Support.V4.App;
using Android;
using Xamarin.Forms;
using Android.Content;
using Android.Support.V4.Content;

namespace FastFileSend.Droid
{
    [Activity(Label = "FastFileSend", Icon = "@mipmap/icon_main", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            Permission permission_storage = ContextCompat.CheckSelfPermission(this, Manifest.Permission.WriteExternalStorage);
            if (permission_storage != Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.ReadExternalStorage, Manifest.Permission.WriteExternalStorage }, 1000);
            }

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            Forms.Init(this, savedInstanceState);
            FormsMaterial.Init(this, savedInstanceState);

            LoadApplication(new App());
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            if (requestCode != 1000)
            {
                return;
            }

            if (grantResults[0] == Permission.Granted)
            {
                Restart();
            }
            else
            {
                Finish();
            }
        }

        private void Restart()
        {
            Intent intent = new Intent(ApplicationContext, typeof(MainActivity));
            // Schedule start after 1 second
            PendingIntent pi = PendingIntent.GetActivity(this, 0, intent, PendingIntentFlags.CancelCurrent);
            AlarmManager am = (AlarmManager)GetSystemService(Context.AlarmService);
            am.Set(AlarmType.Rtc, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + 500, pi);

            Finish();
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            var newExc = new Exception("TaskSchedulerOnUnobservedTaskException", e.Exception);
            LogUnhandledException(newExc);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var newExc = new Exception("CurrentDomainOnUnhandledException", e.ExceptionObject as Exception);
            LogUnhandledException(newExc);
        }

        internal static void LogUnhandledException(Exception exception)
        {
            try
            {
                const string errorFileName = "FFS.Fatal.log";
                var libraryPath = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, Android.OS.Environment.DirectoryDownloads);
                var errorFilePath = Path.Combine(libraryPath, errorFileName);
                Android.Util.Log.Debug("FFS", errorFilePath);
                var errorMessage = String.Format("Time: {0}\r\nError: Unhandled Exception\r\n{1}",
                DateTime.Now, exception.ToString());
                File.WriteAllText(errorFilePath, errorMessage);

                // Log to Android Device Logging.
                Android.Util.Log.Error("Crash Report", errorMessage);
            }
            catch
            {
                // just suppress any error logging exceptions
            }
        }
    }
}