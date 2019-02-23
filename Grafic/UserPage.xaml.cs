using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

using Xamarin.Forms;

using Plugin.DeviceInfo;
using Logic;

namespace XamarinApp {
  [DataContract]
  public struct LsJson {
    [DataMember(Name = "uuid")] public string Uuid;
    [DataMember(Name = "email")] public string Email;
    [DataMember(Name = "pass")] public string Password;
    [DataMember(Name = "name")] public string Name;
    [DataMember(Name = "broker")] public string Broker;
    [DataMember(Name = "mac")] public string MacAdr;
  }

  public partial class UserPage : ContentPage {
    string AppData;
    string CfgFile;
    LsJson Settings;

    public UserPage() {
      InitializeComponent();

      AppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
      if( !Directory.Exists(AppData) ) Directory.CreateDirectory(AppData);
      CfgFile = Path.Combine(AppData, "Config.json");
      if( File.Exists(CfgFile) ) {
        DataContractJsonSerializer dcjs = new DataContractJsonSerializer(typeof(LsJson));
        FileStream fs = new FileStream(CfgFile, FileMode.Open);

        try {
          Settings = (LsJson)dcjs.ReadObject(fs);

          uuid.Text = Settings.Uuid;
          email.Text = Settings.Email;
          pass.Text = Settings.Password;
          //edUsrName.Text = _settings.Name;
          //edUsrBroker.Text = _settings.Broker;
          //edUsrMac.Text = _settings.MacAdr;
        } catch( Exception ex ) {
          Err(string.Format("Exception Config.json: '{0}'\r\n", ex.ToString()));
        }
        fs.Close();
      } else {
        uuid.Text = Guid.NewGuid().ToString();
      }
    }

    void Log(String name, string text) {
      MessagingCenter.Send(Application.Current, "Trace", new TraceItem(name, text));
    }
    void Log(String s, int c = 0) {
      int pos = s.IndexOf(": ");

      if( pos == -1 ) MessagingCenter.Send(Application.Current, "Trace", new TraceItem(s));
      else MessagingCenter.Send(Application.Current, "Trace", new TraceItem(s.Substring(0, pos), s.Substring(pos+2)));
    }
    void Err(String s) {
      DisplayAlert("Error", s, "OK");
    }

    void OnLoginClicked(object sender, EventArgs args) {
      Log($"Email {email.Text}");
      WebClient wc = new WebClient("https://api.worxlandroid.com/api/v2/", "nCH3A0WvMYn66vGorjSrnGZ2YtjQWDiCvjg7jNxK");
      UI.SetLog(Log);
      UI.SetErr(Err);

      if( wc.Login(email.Text, pass.Text, uuid.Text) ) {
        Log($"Broker {wc.Broker}");
      }

      Settings.Uuid = uuid.Text;
      Settings.Email = email.Text;
      Settings.Password = pass.Text;

      DataContractJsonSerializer dcjs = new DataContractJsonSerializer(typeof(LsJson));
      FileStream fs = new FileStream(CfgFile, FileMode.Create);

      dcjs.WriteObject(fs, Settings);
      fs.Close();
    }
  }
}

