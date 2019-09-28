using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Windows.Input;

using Logic;
using MqttJson;
using Xamarin.Forms;

namespace XamarinApp {
  [DataContract]
  public class LsJson {
    [DataMember(Name = "uuid")] public string Uuid;
    [DataMember(Name = "email")] public string Email;
    [DataMember(Name = "pass")] public string Password;
    [DataMember(Name = "name")] public string Name;
    [DataMember(Name = "broker")] public string Broker;
    [DataMember(Name = "cmdin")] public string CmdIn;
    [DataMember(Name = "cmdout")] public string CmdOut;
  }

  public class BaseView : INotifyPropertyChanged {
    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName) {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }

  public class Mower {
    public String State { get; set; }
    public String Error { get; set; }
    public String Firmware { get; set; }
    public int Rrsi { get; set; }

    public Battery Accu { get; set; }

    public float Pitch { get; set; }
    public float Roll { get; set; }
    public float Yaw { get; set; }

    public TimeSpan WorkTime { get; set; }
    public TimeSpan BladeTime { get; set; }
    public int Distance { get; set; }
  }

  public class ViewModel : BaseView {
    static ViewModel _vm = new ViewModel();
    public static ViewModel Instance { get { return _vm; } }

    private string _uuid = "xxx-yyy-zzz";
    public string Uuid {
      get { return _uuid; }
      set { _uuid = value; OnPropertyChanged(nameof(Uuid)); }
    }
    private string _email = "email@roboter-forum.de";
    public string Email {
      get { return _email; }
      set { _email = value; OnPropertyChanged(nameof(Email)); }
    }
    private string _pass = "*****";
    public string Pass {
      get { return _pass; }
      set { _pass = value; OnPropertyChanged(nameof(Pass)); }
    }

    WebClient _wc = null;
    AwsClient _ac = null;

    public List<Mower> Mowers { get; private set; }
    public Mower Mower { get; private set; }
    public class TraceItem {
      public string Name { get; set; }
      public string Text { get; set; }

      public TraceItem(string name) { Name = name; }
      public TraceItem(string name, string text) { Name = name; Text = text; }
    }

    private ObservableCollection<TraceItem> _TraceItems = new ObservableCollection<TraceItem>();

    public ObservableCollection<TraceItem> TraceItems { get { return _TraceItems; } }

    public ICommand PollCommand { protected set; get; }

    string AppData;
    string CfgFile;

    public ViewModel() {
      PollCommand = new Command(() => _ac.Poll() );

      //Mowers = new List<Mower>();
      Mower = new Mower() {
        Error = "io",
        State = "daheim",
        Rrsi = -00,
        Firmware = "0.12",
        Accu = new Battery { Temp = 38, Volt = 50.2F }
      };

      TraceItems.Add(new TraceItem("Test", "irgenwas sollte da stehen"));

      AppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
      if( !Directory.Exists(AppData) ) Directory.CreateDirectory(AppData);
      CfgFile = Path.Combine(AppData, "Config.json");
      if( File.Exists(CfgFile) ) {
        DataContractJsonSerializer dcjs = new DataContractJsonSerializer(typeof(LsJson));
        FileStream fs = new FileStream(CfgFile, FileMode.Open);

        try {
          LsJson x = (LsJson)dcjs.ReadObject(fs);

          Uuid = x.Uuid;
          Email = x.Email;
          Pass = x.Password;

          //edUsrName.Text = _settings.Name;
          //edUsrBroker.Text = _settings.Broker;
          //edUsrMac.Text = _settings.MacAdr;
        } catch( Exception ex ) {
          Err(string.Format("Exception Config.json: '{0}'\r\n", ex.ToString()));
        }
        fs.Close();
      } else {
        Uuid = Guid.NewGuid().ToString();
      }
    }

    public void Log(String name, string text) {
      TraceItems.Add(new TraceItem(name, text));
      //MessagingCenter.Send(Application.Current, "Trace", new TraceItem(name, text));
    }
    public void Log(String s, int c = 0) {
      int pos = s.IndexOf(": ");

      if( pos == -1 ) TraceItems.Add(new TraceItem(s));
      else TraceItems.Add(new TraceItem(s.Substring(0, pos), s.Substring(pos+2)));
    }
    void Err(String s) {
      App.Current.MainPage.DisplayAlert("Error", s, "OK");
    }

    private void Recv(LsMqtt mqtt) {
      //Invoke(new MqttDelegate(RecvInvoke));
      Mower.State = mqtt.Dat.LastState.ToString();
      Mower.Error = mqtt.Dat.LastError.ToString();
      Mower.Rrsi = mqtt.Dat.RecvSignal;
      Mower.Firmware = mqtt.Dat.Firmware;
      Mower.Accu = mqtt.Dat.Battery;
      Mower.Pitch = mqtt.Dat.Orient[0];
      Mower.Roll = mqtt.Dat.Orient[1];
      Mower.Yaw = mqtt.Dat.Orient[2];
      Mower.WorkTime = TimeSpan.FromSeconds(mqtt.Dat.Statistic.WorkTime);
      Mower.BladeTime = TimeSpan.FromSeconds(mqtt.Dat.Statistic.Blade);
      Mower.Distance = mqtt.Dat.Statistic.Distance;
      OnPropertyChanged(nameof(Mower));
    }

    public void Login() {
      _wc = new WebClient("https://api.worxlandroid.com/api/v2/", "nCH3A0WvMYn66vGorjSrnGZ2YtjQWDiCvjg7jNxK");
      UI.SetLog(Log);
      UI.SetErr(Err);
      UI.SetRecv(Recv);

      if( _wc.Login(Email, Pass, Uuid) ) {
        Log($"Broker {_wc.Broker}");

        if( true && _wc.States.Count > 0 ) {
          Recv(_wc.States[0]);
        }
        if( false && _wc.Broker != null && _wc.Cert != null && _wc.Products != null && _wc.Products.Count > 0 ) {
          LsProductItem pi = _wc.Products[0];

          _ac = new AwsClient(_wc.Broker, Uuid, _wc.Cert, pi.Topic.CmdIn, pi.Topic.CmdOut);
          _ac.Start(true);
        }
      }

      LsJson x = new LsJson();

      x.Uuid = Uuid;
      x.Email = Email;
      x.Password = Pass;

      DataContractJsonSerializer dcjs = new DataContractJsonSerializer(typeof(LsJson));
      FileStream fs = new FileStream(CfgFile, FileMode.Create);

      dcjs.WriteObject(fs, x);
      fs.Close();
    }

    public void Logout() {
      if( _ac != null && _ac.Connected ) _ac.Exit();
    }
  }
}