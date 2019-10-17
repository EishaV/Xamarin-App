using Android.App;
using Android.Content;
using Android.Util;
using Android.OS;
using Android.Widget;
using System;
using System.Security.Cryptography.X509Certificates;

using Xamarin.Forms;

using XamarinApp.Droid;
using Logic;
using Android;

[assembly: Dependency(typeof(MqttService))]
namespace XamarinApp.Droid {
  public static class Constants {
    public const int SERVICE_RUNNING_NOTIFICATION_ID = 10000;
    public const string SERVICE_STARTED_KEY = "has_service_been_started";

    public const string ACTION_START_SERVICE = "ServicesDemo3.action.START_SERVICE";
    public const string ACTION_STOP_SERVICE = "ServicesDemo3.action.STOP_SERVICE";
    public const string ACTION_MAIN_ACTIVITY = "ServicesDemo3.action.MAIN_ACTIVITY";
  }

  public static class Util {
    public static string Broker;
    public static string Uuid;
    public static X509Certificate2 Cert;
    public static string CmdIn, CmdOut;
  }

  [Service]
  public class OreoService : Service {
    static readonly string TAG = typeof(OreoService).FullName;
    const string cn = "LandMiss";
    int idx = 0;
    AwsClient ac = null;

    bool isStarted;

    public override void OnCreate() {
      base.OnCreate();
      Log.Info(TAG, "OnCreate: the service is initializing.");
    }

    private void OnRecv(object sender, MyEventArgs e) {
      Intent intent = new Intent(this, typeof(MainActivity));

      // Create a PendingIntent; we're only using one PendingIntent (ID = 0):
      const int pendingIntentId = 0;
      PendingIntent pendingIntent =
          PendingIntent.GetActivity(this, pendingIntentId, intent, PendingIntentFlags.OneShot);

      Notification.Builder builder = new Notification.Builder(this, cn)
          .SetContentIntent(pendingIntent)
          .SetContentTitle("Mqtt Recv")
          .SetContentText(e.Mqtt.Cfg.Date + " " + e.Mqtt.Cfg.Time)
          .SetSmallIcon(Resource.Drawable.IcMediaPause);

      // Build the notification:
      Notification notification = builder.Build();

      // Get the notification manager:
      NotificationManager notificationManager =
          GetSystemService(Context.NotificationService) as NotificationManager;

      // Publish the notification:
      notificationManager.Notify(idx++, notification);

      App.History.Add(new HistoryItem(DateTime.Now, e.Mqtt));
    }

    public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId) {
      if( intent.Action.Equals(Constants.ACTION_START_SERVICE) ) {
        if( isStarted ) {
          Log.Info(TAG, "OnStartCommand: The service is already running.");
        } else {
          Log.Info(TAG, "OnStartCommand: The service is starting.");
          ac = new AwsClient(Util.Broker, Util.Uuid, Util.Cert, Util.CmdIn, Util.CmdOut);

          if( ac.Start(true) ) {
            ac.Recv += OnRecv;
            App.Aws = ac;
          }

          RegisterForegroundService();
          isStarted = true;
        }
      } else if( intent.Action.Equals(Constants.ACTION_STOP_SERVICE) ) {
        Log.Info(TAG, "OnStartCommand: The service is stopping.");
        StopForeground(true);
        StopSelf();
        isStarted = false;
      }

      // This tells Android not to restart the service if it is killed to reclaim resources.
      return StartCommandResult.Sticky;
    }

    public override IBinder OnBind(Intent intent) {
      // Return null because this is a pure started service. A hybrid service would return a binder that would
      // allow access to the GetFormattedStamp() method.
      return null;
    }

    public override void OnDestroy() {
      // We need to shut things down.
      Log.Info(TAG, "OnDestroy: The started service is shutting down.");

      // Remove the notification from the status bar.
      var notificationManager = (NotificationManager)GetSystemService(NotificationService);
      notificationManager.Cancel(Constants.SERVICE_RUNNING_NOTIFICATION_ID);

      isStarted = false;
      base.OnDestroy();
    }

    void RegisterForegroundService() {
      NotificationChannel ch = new NotificationChannel(cn, cn+"_name", NotificationImportance.Default);
      NotificationManager nm = (NotificationManager)GetSystemService(NotificationService);

      nm.CreateNotificationChannel(ch);


      //     NotificationCompat.Builder builder = new NotificationCompat.Builder(this, NOTIFICATION_CHANNEL_ID)
      //                      .setContentIntent(broadcastIntent)
      //                      .setPriority(PRIORITY_HIGH)
      //                      .setCategory(Notification.CATEGORY_SERVICE);


      Notification builder = new Notification.Builder(this, cn)
        .SetContentTitle("Miss Land")
        .SetContentText("notification_text")
        .SetSmallIcon(Resource.Drawable.IcMediaPause)
        .SetContentIntent(BuildIntentToShowMainActivity())
        .SetOngoing(true)
        .AddAction(BuildStopServiceAction())
        .Build();

      // Enlist this instance of the service as a foreground service
      StartForeground(Constants.SERVICE_RUNNING_NOTIFICATION_ID, builder);
    }

    /// <summary>
    /// Builds a PendingIntent that will display the main activity of the app. This is used when the 
    /// user taps on the notification; it will take them to the main activity of the app.
    /// </summary>
    /// <returns>The content intent.</returns>
    PendingIntent BuildIntentToShowMainActivity() {
      var notificationIntent = new Intent(this, typeof(MainActivity));
      notificationIntent.SetAction(Constants.ACTION_MAIN_ACTIVITY);
      notificationIntent.SetFlags(ActivityFlags.SingleTop | ActivityFlags.ClearTask);
      notificationIntent.PutExtra(Constants.SERVICE_STARTED_KEY, true);

      var pendingIntent = PendingIntent.GetActivity(this, 0, notificationIntent, PendingIntentFlags.UpdateCurrent);
      return pendingIntent;
    }

    /// <summary>
    /// Builds the Notification.Action that will allow the user to stop the service via the
    /// notification in the status bar
    /// </summary>
    /// <returns>The stop service action.</returns>
    Notification.Action BuildStopServiceAction() {
      var stopServiceIntent = new Intent(this, GetType());
      stopServiceIntent.SetAction(Constants.ACTION_STOP_SERVICE);
      var stopServicePendingIntent = PendingIntent.GetService(this, 0, stopServiceIntent, 0);

      var action = new Notification.Action(Android.Resource.Drawable.IcMediaPause,
                              "stop_service", stopServicePendingIntent);
      return action;
    }
  }

  public class MqttService : IMqttService {
    Intent startServiceIntent;
    Intent stopServiceIntent;
    public string GetSystem() {
      App.Instance.UserModel.Testat = "Android";
      return "Droid";
    }
    public bool Start(string broker, string uuid, X509Certificate2 cert, string cmdIn, string cmdOut) {
      Util.Broker = broker;
      Util.Uuid = uuid;
      Util.Cert = cert;
      Util.CmdIn = cmdIn;
      Util.CmdOut = cmdOut;

      startServiceIntent = new Intent(Android.App.Application.Context, typeof(OreoService));
      startServiceIntent.SetAction(Constants.ACTION_START_SERVICE);
      Android.App.Application.Context.StartService(startServiceIntent);
      Log.Info("TAG", "User requested that the service be started.");

      return true;
    }
    public bool Stop() {
      return true;
    }

  }
}
