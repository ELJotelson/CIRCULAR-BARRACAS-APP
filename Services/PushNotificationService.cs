#if ANDROID
using Plugin.Firebase.CloudMessaging;
#endif

namespace Circulacion_Barracas.Services;

public class PushNotificationService
{
    private readonly SupabaseService supabase;
    private readonly AuthService auth;

    public PushNotificationService(SupabaseService supabase, AuthService auth)
    {
        this.supabase = supabase;
        this.auth = auth;
    }

    public async Task RegistrarAsync()
    {
#if ANDROID
        try
        {
            var userId = auth.UsuarioActualId;

            if (userId is null)
                return;

            if (DeviceInfo.Platform == DevicePlatform.Android && DeviceInfo.Version.Major >= 13)
            {
                var estado = await Permissions.CheckStatusAsync<Permissions.PostNotifications>();

                if (estado != PermissionStatus.Granted)
                    estado = await Permissions.RequestAsync<Permissions.PostNotifications>();

                if (estado != PermissionStatus.Granted)
                    return;
            }

            await CrossFirebaseCloudMessaging.Current.CheckIfValidAsync();
            var token = await CrossFirebaseCloudMessaging.Current.GetTokenAsync();

            if (!string.IsNullOrWhiteSpace(token))
                await supabase.RegistrarPushTokenAsync(userId.Value, token, "android");
        }
        catch
        {
            // Sin push no se rompe nada más de la app; se ignora cualquier falla.
        }
#else
        await Task.CompletedTask;
#endif
    }
}
