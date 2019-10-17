using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

using Xamarin.Forms;

using Logic;
using System;

namespace XamarinApp {
  public class TraceItem {
    public string Name { get; set; }
    public string Text { get; set; }

    public TraceItem(string name) { Name = name; }
    public TraceItem(string name, string text) { Name = name; Text = text; }
  }

  public class App : Application {
    string AppData;
    string CfgFile;

    WebClient _wc = null;

    public UserModel UserModel = null;

    private readonly ObservableCollection<TraceItem> _TraceItems = new ObservableCollection<TraceItem>();

    public ObservableCollection<TraceItem> TraceItems { get { return _TraceItems; } }

    public static ObservableCollection<HistoryItem> History { get; } = new ObservableCollection<HistoryItem>();

    public static App Instance {
      get { return Application.Current as App; }
    }

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

    public JsonConfig Config { get; private set; }

    static public void Err(String s) {
      Instance.MainPage.DisplayAlert("Error", s, "OK");
    }

    static public void Trace(String name, string text) {
      Instance.TraceItems.Add(new TraceItem(name, text));
      //MessagingCenter.Send(Application.Current, "Trace", new TraceItem(name, text));
    }

    public App() {
      AppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
      if( !Directory.Exists(AppData) ) Directory.CreateDirectory(AppData);
      CfgFile = Path.Combine(AppData, "Config.json");
      if( File.Exists(CfgFile) ) {
        DataContractJsonSerializer dcjs = new DataContractJsonSerializer(typeof(JsonConfig));
        FileStream fs = new FileStream(CfgFile, FileMode.Open);

        try {
          Config = (JsonConfig)dcjs.ReadObject(fs);
        } catch( Exception ex ) {
          Err(string.Format("Exception Config.json: '{0}'\r\n", ex.ToString()));
        }
        fs.Close();
      } else {
        Config = new JsonConfig();
        Config.Uuid = Guid.NewGuid().ToString();
      }

      Trace("App created", Config.ToString());

      MainPage = new XamarinApp.MainPage();
    }

    protected override void OnStart() {
      // Handle when your app starts
    }
    protected override void OnSleep() {
      //Logout();
      // Handle when your app sleeps
    }
    protected override void OnResume() {
      // Handle when your app resumes
    }

    public void Login(string uuid, string email, string pass) {
      WebClient wc = new WebClient("https://api.worxlandroid.com/api/v2/", "nCH3A0WvMYn66vGorjSrnGZ2YtjQWDiCvjg7jNxK");
      UI.SetLog(App.Trace);
      UI.SetErr(App.Err);

      if( wc.Login(email, pass, uuid) ) {
        Trace("Broker", wc.Broker);

        App.Web = wc;
        if( true && wc.States.Count > 0 ) {
          OnRecv(this, new MyEventArgs(wc.States[0]));
        }
        if( wc.Broker != null && wc.Cert != null && wc.Products != null && wc.Products.Count > 0 ) {
          LsProductItem pi = wc.Products[0];

          DependencyService.Get<IMqttService>().Start(wc.Broker, uuid, wc.Cert, pi.Topic.CmdIn, pi.Topic.CmdOut);
        }
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

