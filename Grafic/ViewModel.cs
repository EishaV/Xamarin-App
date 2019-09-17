using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

using Logic;

namespace XamarinApp {
  [DataContract]
  public class LsJson {
    [DataMember(Name = "uuid")] public string Uuid;
    [DataMember(Name = "email")] public string Email;
    [DataMember(Name = "pass")] public string Password;
    [DataMember(Name = "name")] public string Name;
    [DataMember(Name = "broker")] public string Broker;
    [DataMember(Name = "mac")] public string MacAdr;
  }

  public class BaseView : INotifyPropertyChanged {
    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName) {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
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

    public class TraceItem {
      public string Name { get; set; }
      public string Text { get; set; }

      public TraceItem(string name) { Name = name; }
      public TraceItem(string name, string text) { Name = name; Text = text; }
    }

    private ObservableCollection<TraceItem> _TraceItems = new ObservableCollection<TraceItem>();

    public ObservableCollection<TraceItem> TraceItems { get { return _TraceItems; } }


    string AppData;
    string CfgFile;

    public ViewModel() {
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
      //DisplayAlert("Error", s, "OK");
    }

    public void Login() {
      WebClient wc = new WebClient("https://api.worxlandroid.com/api/v2/", "nCH3A0WvMYn66vGorjSrnGZ2YtjQWDiCvjg7jNxK");
      UI.SetLog(Log);
      UI.SetErr(Err);

      if( wc.Login(Email, Pass, Uuid) ) {
        Log($"Broker {wc.Broker}");

        if( wc.Broker != null && wc.Cert != null && wc.Products != null && wc.Products.Count > 0 ) {
          AwsClient aws = new AwsClient(wc.Broker, Uuid, wc.Cert, "DB510", wc.Products[0].MacAdr);

          aws.Start(true);

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
  }
}