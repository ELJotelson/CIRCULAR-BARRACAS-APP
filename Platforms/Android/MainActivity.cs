using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Plugin.Firebase.CloudMessaging;

namespace Circulacion_Barracas
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            CrearCanalDeNotificaciones();
            FirebaseCloudMessagingImplementation.OnNewIntent(Intent);
        }

        protected override void OnNewIntent(Intent? intent)
        {
            base.OnNewIntent(intent);
            FirebaseCloudMessagingImplementation.OnNewIntent(intent);
        }

        private void CrearCanalDeNotificaciones()
        {
            var channelId = $"{PackageName}.general";
            var notificationManager = (NotificationManager)GetSystemService(NotificationService)!;
            var channel = new NotificationChannel(channelId, "General", NotificationImportance.Default);
            notificationManager.CreateNotificationChannel(channel);
            FirebaseCloudMessagingImplementation.ChannelId = channelId;
        }
    }
}
