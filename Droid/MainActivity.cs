using System;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace XamarinApp.Droid
{
	[Activity (Label = "XamarinApp.Droid", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
	{
    public const string cn = "LandMiss";

    protected override void OnCreate (Bundle bundle)
		{
      NotificationChannel ch = new NotificationChannel(cn, cn+"_name", NotificationImportance.Default);
      NotificationManager nm = (NotificationManager)GetSystemService(NotificationService);

      base.OnCreate (bundle);
			global::Xamarin.Forms.Forms.Init (this, bundle);
      nm.CreateNotificationChannel(ch);
      LoadApplication(new App ());
		}
  }
}

