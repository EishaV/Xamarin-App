using System;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

using MqttJson;
using Logic;
using Xamarin.Forms;

namespace XamarinApp {
  [DataContract]
  public class JsonConfig {
    [DataMember(Name = "uuid")] public string Uuid;
    [DataMember(Name = "email")] public string Email;
    [DataMember(Name = "pass")] public string Password;
    [DataMember(Name = "name")] public string Name;
    [DataMember(Name = "broker")] public string Broker;
    [DataMember(Name = "cmdin")] public string CmdIn;
    [DataMember(Name = "cmdout")] public string CmdOut;
  }

  public class UserModel : BaseView {
    private string _uuid = "xxx-yyy-zzz";
    public string Uuid {
      get { return _uuid; }
      set { _uuid = value; OnPropertyChanged(nameof(Uuid)); }
    }
    private string _email;
    public string Email {
      get { return _email; }
      set { _email = value; OnPropertyChanged(nameof(Email)); }
    }
    private string _pass;
    public string Pass {
      get { return _pass; }
      set { _pass = value; OnPropertyChanged(nameof(Pass)); }
    }

    string _Testat;
    public string Testat {
      get { return _Testat; }
      set { _Testat = value; OnPropertyChanged(nameof(Testat)); }
    }

    public UserModel() {
      JsonConfig cfg = (Application.Current as App)?.Config;

      App.Instance.UserModel = this;

      if( cfg != null ) {
        Uuid = cfg.Uuid;
        Email = cfg.Email;
        Pass = cfg.Password;

        //edUsrName.Text = _settings.Name;
        //edUsrBroker.Text = _settings.Broker;
        //edUsrMac.Text = _settings.MacAdr;
      }
    }

    public void Login() {
      App.Instance.Login(Uuid, Email, Pass);
    }
  }
}