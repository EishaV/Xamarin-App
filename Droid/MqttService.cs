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
using MqttJson;

[assembly: Dependency(typeof(MqttService))]
namespace XamarinApp.Droid {
  public static class Constants {
    public const int SERVICE_RUNNING_NOTIFICATION_ID = 10000;
    public const string SERVICE_STARTED_KEY = "has_service_been_started";

    public const string ACTION_START_SERVICE = "ServicesDemo3.action.START_SERVICE";
    public const string ACTION_STOP_SERVICE = "ServicesDemo3.action.STOP_SERVICE";
    public const string ACTION_MAIN_ACTIVITY = "ServicesDemo3.action.MAIN_ACTIVITY";
  }

  [BroadcastReceiver]
  public class RepeatingAlarm : BroadcastReceiver {
    public override void OnReceive(Context context, Intent intent) {
      //Every time the `RepeatingAlarm` is fired, set the next alarm
      //Intent intent = new Intent(context, typeof(RepeatingAlarm));
      PendingIntent source = PendingIntent.GetBroadcast(context, 0, intent, 0);
      AlarmManager am = (AlarmManager)context.GetSystemService(Context.AlarmService);
      LsMqtt mqtt = App.Web.GetLastState("Gustav");

      if( mqtt != null ) {
        mqtt.Dat.Firmware = "1.23";
        App.History.Add(new HistoryItem(DateTime.Now, mqtt));
        System.Diagnostics.Debug.WriteLine("Alarm {0} {1}", DateTime.Now, "ups");
      }
      am.SetAndAllowWhileIdle(AlarmType.ElapsedRealtimeWakeup, SystemClock.ElapsedRealtime() + 30 * 60 * 1000, source);
      //Toast.MakeText(context, "repeating_received and after 15s another alarm will be fired", ToastLength.Short).Show();
    }
  }

  public class MqttService : IMqttService {
    PendingIntent _penint = null;

    public string GetSystem() {
      App.Instance.UserModel.Testat = "Android";
      return "Droid";
    }
    public bool Start() {
      Context context = Android.App.Application.Context;
      AlarmManager am = (AlarmManager)context.GetSystemService(Context.AlarmService);
      NotificationManager nm = (NotificationManager)context.GetSystemService(Context.NotificationService);

      _penint = PendingIntent.GetBroadcast(context, 0, new Intent(context, typeof(RepeatingAlarm)), 0);
      am.SetAndAllowWhileIdle(AlarmType.ElapsedRealtimeWakeup, SystemClock.ElapsedRealtime() + 30 * 60 * 1000, _penint);

      var notIntent = new Intent(context, typeof(MainActivity));
      var contentIntent = PendingIntent.GetActivity(context, 0, notIntent, PendingIntentFlags.CancelCurrent);
      // Build the notification:
      var builder = new Notification.Builder(context, MainActivity.cn)
                    .SetAutoCancel(true) // Dismiss the notification from the notification area when the user clicks on it
                    .SetContentIntent(contentIntent) // Start up this activity when the user clicks the intent.
                    .SetContentTitle("title") // Set the title
                    //.SetNumber(count) // Display the count in the Content Info
                    .SetSmallIcon(Resource.Drawable.Icon_Notify) // This is the icon to display
                    .SetContentText("message start"); // the message to display.
                                              // Finally, publish the notification:
      nm.Notify(1000, builder.Build());
      return true;
    }
    public bool Stop() {
      Context context = Android.App.Application.Context;
      AlarmManager am = (AlarmManager)context.GetSystemService(Context.AlarmService);

      if( _penint != null ) am.Cancel(_penint);
      return true;
    }

  }
}

/*
    Intent startServiceIntent;
    Intent stopServiceIntent;

      //startServiceIntent = new Intent(Android.App.Application.Context, typeof(OreoService));
      //startServiceIntent.SetAction(Constants.ACTION_START_SERVICE);
      //Android.App.Application.Context.StartService(startServiceIntent);
      //Log.Info("TAG", "User requested that the service be started.");


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
*/
