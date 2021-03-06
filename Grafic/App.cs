﻿using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text.RegularExpressions;
using System.Threading;

using Xamarin.Forms;

using MqttJson;
using Logic;

namespace XamarinApp {
  public class MyTraceListener : TraceListener {
    string LastWrite = null;

    public override void Write(string message) {
      LastWrite = message;
    }

    public override void WriteLine(string message) {
      string t, s;
      Regex r1 = new Regex(@"[^ ]+ ([^:]+):.*");
      Regex r2 = new Regex(@"(.*) => (.*)");
      Match m;

      t = DateTime.Now.ToString("HH:mm:ss ");
      if( LastWrite != null && (m = r1.Match(LastWrite)).Success ) t += m.Groups[1].Value;
      else t += "Unknown";
      t += ": ";
      m = r2.Match(message);
      if( m.Success ) {
        t += m.Groups[1].Value;
        s = m.Groups[2].Value;
      } else {
        t += message;
        s = string.Empty;
      }
      App.Instance.Trace.Add(new TraceItem(t, s));
      LastWrite = null;
    }
  }

  public class App : Application {
    public static ObservableCollection<NotifyGroup> Notify { get; } = new ObservableCollection<NotifyGroup>();
    public static ObservableCollection<HistoryItem> History { get; } = new ObservableCollection<HistoryItem>();

    public static App Instance { get { return Current as App; } }


    string AppData;
    string CfgFile;

    WebClient _wc = null;

    public UserModel UserModel = null;

    public ObservableCollection<TraceItem> Trace { get; } = new ObservableCollection<TraceItem>();

    public static WebClient Web {
      get { return Instance?._wc; }
      set { if( Instance != null ) Instance._wc = value; }
    }

    static AwsClient _ac = null;

    public static AwsClient Aws {
      get { return _ac; }
      set {
        _ac = value;
        _ac.Recv += OnRecv;
      }
    }

    public static event EventHandler<MyEventArgs> Recv;

    private static void OnRecv(object sender, MyEventArgs e) {
      Recv?.Invoke(sender, e);
      History.Add(new HistoryItem(DateTime.Now, e.Mqtt));
    }

    public static bool NotifyMsg(MqttJson.Data dat) {
      ObservableCollection<NotifyGroup> n = Notify;

      if( n != null ) {
        foreach( NotifyGroup ng in n ) {
          if( ng.Tag == "Errors" ) {
            foreach( NotifyItem ni in ng ) {
              if( ni.Code == (int)dat.Error ) return ni.On;
            }
          }
          if( ng.Tag == "States" ) {
            foreach( NotifyItem ni in ng ) {
              if( ni.Code == (int)dat.State ) return ni.On;
            }
          }
        }
      }
      return false;
    }

    public JsonConfig Config { get; private set; }

    public App() {
      System.Diagnostics.Trace.Listeners.Add(new MyTraceListener());

      AppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
      if( !Directory.Exists(AppData) ) Directory.CreateDirectory(AppData);
      CfgFile = Path.Combine(AppData, "Config.json");
      if( File.Exists(CfgFile) ) {
        DataContractJsonSerializer dcjs = new DataContractJsonSerializer(typeof(JsonConfig));
        FileStream fs = new FileStream(CfgFile, FileMode.Open);

        try {
          Config = (JsonConfig)dcjs.ReadObject(fs);
        } catch( Exception ex ) {
          System.Diagnostics.Trace.TraceError("Read Config => {0}", ex.ToString());
          //MainPage.DisplayAlert("Exception Config.json", ex.ToString(), ":-/");
        }
        fs.Close();
      } else {
        Config = new JsonConfig();
        Config.Uuid = Guid.NewGuid().ToString();
      }

      MainPage = new XamarinApp.MainPage();

      System.Diagnostics.Trace.TraceInformation("App created", Config.ToString());
    }

    protected override void OnStart() {
      // Handle when your app starts
    }
    protected override void OnSleep() {
      //Logout();
      // Handle when your app sleeps
    }
    protected override void OnResume() {
      DependencyService.Get<IMqttService>().Stop();
      // Handle when your app resumes
    }

    public void Login(string uuid, string email, string pass) {
      WebClient wc = new WebClient("https://api.worxlandroid.com/api/v2/", "nCH3A0WvMYn66vGorjSrnGZ2YtjQWDiCvjg7jNxK");

      if( wc.Login(email, pass, uuid) ) {
        App.Web = wc;
        if( true && wc.States.Count > 0 ) {
          OnRecv(this, new MyEventArgs(wc.States[0]));
        }
        if( wc.Broker != null && wc.Cert != null && wc.Products != null && wc.Products.Count > 0 ) {
          LsProductItem pi = wc.Products[0];
          AwsClient ac = new AwsClient(wc.Broker, uuid, wc.Cert, pi.Topic.CmdIn, pi.Topic.CmdOut);

          if( ac.Start() ) {
            XamarinApp.App.Aws = ac;
            DependencyService.Get<IMqttService>().Start();

            TabbedPage tp = MainPage as TabbedPage;

            tp.CurrentPage = tp.Children[1];
          } else MainPage.DisplayAlert("Alert", "Connect failed see Trace", ":-(");
        } else MainPage.DisplayAlert("Alert", "Login failed see Trace", ":-(");
      }

      JsonConfig x = new JsonConfig { Uuid = uuid, Email = email, Password = pass };
      DataContractJsonSerializer dcjs = new DataContractJsonSerializer(typeof(JsonConfig));
      FileStream fs = new FileStream(CfgFile, FileMode.Create);

      dcjs.WriteObject(fs, x);
      fs.Close();
    }

    public void Logout() {
      AwsClient ac = App.Aws;

      if( ac != null && ac.Connected ) ac.Exit();
    }

  }
}

