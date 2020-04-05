#define PLUGIN

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.Net;

using MqttJson;

namespace Logic {
  #region Structs
  /*
  User auth {"id":000,"name":"...","email":"...","created_at":"...","updated_at":"...","city":null,"address":null,"zipcode":null,
              "country_id":276,"phone":null,"birth_date":null,"sex":null,"newsletter_subscription":null,"user_type":"customer",
              "api_token":"...","token_expiration":"...", "mqtt_client_id":"android-..."}
  */
  [DataContract]
  public struct LsOAuth {
    [DataMember(Name = "access_token")]
    public string Token;
    [DataMember(Name = "expires_in")]
    public int Expires;
    [DataMember(Name = "token_type")]
    public string Type;
  }
  [DataContract]
  public struct LsUser {
    [DataMember(Name = "mqtt_endpoint")] public string Endpoint;
  }
  [DataContract]
  public struct LsCertificate {
    [DataMember(Name = "pkcs12")]
    public string Pkcs12;
  }
  [DataContract]
  public struct LsMqttTopic {
    [DataMember(Name = "command_in")]
    public string CmdIn;
    [DataMember(Name = "command_out")]
    public string CmdOut;
  }
  [DataContract]
  public struct LsProductItem {
    [DataMember(Name = "name")]
    public string Name;
    [DataMember(Name = "serial_number")]
    public string SerialNo;
    [DataMember(Name = "mqtt_topics")]
    public LsMqttTopic Topic;
  }

  [DataContract]
  public struct LsJson {
    [DataMember(Name = "email")]    public string Email;
    [DataMember(Name = "pass")]     public string Password;
    [DataMember(Name = "uuid")]     public string Uuid;
    [DataMember(Name = "name")]     public string Name;
    [DataMember(Name = "broker")]   public string Broker;
    [DataMember(Name = "mac")]      public string MacAdr;
    [DataMember(Name = "blade")]    public int Blade;
    [DataMember(Name = "top")]      public bool Top;
    [DataMember(Name = "x")]        public int X;
    [DataMember(Name = "y")]        public int Y;
    [DataMember(Name = "w")]        public int W;
    [DataMember(Name = "h")]        public int H;
    [DataMember(Name = "plugins")]  public List<string> Plugins;

    public bool Equals(LsJson lsj) {
      bool b;

      b = Email == lsj.Email && Password == lsj.Password && Uuid == lsj.Uuid && Name == lsj.Name && Broker == lsj.Broker && MacAdr == lsj.MacAdr;
      b = b && Top == lsj.Top && X == lsj.X && Y == lsj.Y && W == lsj.W && H == lsj.H;
      if( b && Plugins != null && lsj.Plugins != null ) {
        b = Plugins.Count == lsj.Plugins.Count;
        for( int i = 0; b && i < Plugins.Count; i++ ) b = b && Plugins[i] == lsj.Plugins[i];
      }
      return b;
    }
  }

  [DataContract] public struct LsEstimatedTime {
    [DataMember(Name = "beg")] public float Beg;
    [DataMember(Name = "end")] public float End;
    [DataMember(Name = "vpm")] public float VoltPerMin;
  }

  [DataContract] public struct LsEstimatedTimes {
    [DataMember(Name = "home_0")] public LsEstimatedTime HomeOff;
    [DataMember(Name = "home_1")] public LsEstimatedTime HomeOn;
    [DataMember(Name = "mowing")] public LsEstimatedTime Mowing;
  }
  #endregion

  #region Activity-Log
  /*
  {
    "_id":"5d65fcd8241fa136e0551d1f",
    "timestamp":"2019-08-28 04:02:31",
    "product_item_id":12061,
    "payload":{
      "cfg":{"dt":"28/08/2019","tm":"06:02:23","mzv":[0,0,0,0,0,0,0,0,0,0],"mz":[0,0,0,0]},
      "dat":{"le":0,"ls":0,"fw":3.51,"lz":0,"lk":0,"bt":{"c":0,"m":1}}
    }
  }
  */
  [DataContract]
  public struct ActivityConfig {
    [DataMember(Name = "dt")] public string Date;
    [DataMember(Name = "tm")] public string Time;
    [DataMember(Name = "mz")] public int[] MultiZones; // [0-3] start point in meters
    [DataMember(Name = "mzv")] public int[] MultiZonePercs; // [0-9] ring list of start indizes
  }
  [DataContract]
  public struct ActivityBattery {
    [DataMember(Name = "c")] public ChargeCoge Charging;
    [DataMember(Name = "m")] public int Miss;
  }
  [DataContract]
  public struct ActivityData {
    [DataMember(Name = "le")] public ErrorCode LastError;
    [DataMember(Name = "ls")] public StatusCode LastState;
    [DataMember(Name = "fw")] public double Firmware;
    [DataMember(Name = "lz")] public int LastZone;
    [DataMember(Name = "lk")] public int Lock;
    [DataMember(Name = "bt")] public ActivityBattery Battery;
  }
  [DataContract]
  public struct ActivityPayload {
    [DataMember(Name = "cfg")] public ActivityConfig Cfg;
    [DataMember(Name = "dat")] public ActivityData Dat;
  }
  [DataContract]
  public struct Activity {
    [DataMember(Name = "_id")] public string ActId;
    [DataMember(Name = "timestamp")] public string Stamp;
    [DataMember(Name = "product_item_id")] public string MowId;
    [DataMember(Name = "payload")] public ActivityPayload Payload;
  }
  #endregion

  #region Weather
  /*
  {
    "coord":{"lon":12.4,"lat":50.52},
    "weather":[{"id":804,"main":"Clouds","description":"overcast clouds","icon":"04n"}],
    "base":"stations",
    "main":{"temp":4.29,"pressure":1005,"humidity":93,"temp_min":2.22,"temp_max":6},
    "visibility":10000,
    "wind":{"speed":5.7,"deg":100},
    "clouds":{"all":90},
    "dt":1574534869,
    "sys":{"type":1,"id":6836,"country":"DE","sunrise":1574491035,"sunset":1574522189},
    "timezone":3600,"id":2954602,"name":"Auerbach","cod":200}

    
    "weather":[
      {"id":701,"main":"Mist","description":"mist","icon":"50d"},
      {"id":741,"main":"Fog","description":"fog","icon":"50d"}
    ]
    "weather":[{"id":701,"main":"Mist","description":"mist","icon":"50n"}],
  */

  [DataContract]
  public struct WeatherArray {
    [DataMember(Name = "id")] public int Id;
    [DataMember(Name = "main")] public string Main;
    [DataMember(Name = "description")] public string Desc;
    [DataMember(Name = "icon")] public string Icon;
  }

  [DataContract]
  public struct WeatherMain {
    [DataMember(Name = "temp")] public float Temp;
    [DataMember(Name = "pressure")] public int Pressure;
    [DataMember(Name = "humidity")] public int Humidity;
  }

  [DataContract]
  public class Weather {
    [DataMember(Name = "weather")] public WeatherArray[] Array;
    [DataMember(Name = "main")] public WeatherMain Main;
    [DataMember(Name = "name")] public string Name;
  }
  #endregion

  public class WebClient {
  private string _webapi, _token;
  private System.Net.WebClient _client = new System.Net.WebClient();

  public List<LsProductItem> Products = new List<LsProductItem>();
  public List<LsMqtt> States = new List<LsMqtt>();

  public string Broker { get; private set; }
  public X509Certificate2 Cert { get; private set; }

  public WebClient(string webapi, string token) {
    _webapi = webapi; _token = token;
  }

  public bool Login(string mail, string pass, string uuid) {
    NameValueCollection nvc = new NameValueCollection();
    LsOAuth lsoa;
    string str;
    byte[] buf;

    #region Anmeldung
    nvc.Add("username", mail);
    nvc.Add("password", pass);
    nvc.Add("grant_type", "password");
    nvc.Add("client_id", "1");
    nvc.Add("client_secret", _token);
    nvc.Add("scope", "*");
    try {
      buf = _client.UploadValues(_webapi + "oauth/token", nvc);
      str = Encoding.UTF8.GetString(buf);
      Trace.TraceInformation("Oauth token => {0}", str);
      using( MemoryStream ms = new MemoryStream(buf) ) {
        DataContractJsonSerializer dcjs = new DataContractJsonSerializer(typeof(LsOAuth));

        lsoa = (LsOAuth)dcjs.ReadObject(ms);
        Trace.TraceInformation("Access token => {0}", lsoa.Token);
        _client.Headers["Authorization"] = string.Format("{0} {1}", lsoa.Type, lsoa.Token);
        Trace.TraceInformation("Token type => {0}", lsoa.Type);
        ms.Close();
      }
    } catch( Exception ex ) {
      Trace.TraceError("Login 1 {0}", ex.ToString());
      return false;
    }
    #endregion

    try {
      #region Benutzer
      buf = _client.DownloadData(_webapi + "users/me");
      str = Encoding.UTF8.GetString(buf);
      Trace.TraceInformation("User info => {0}", str);
      using( MemoryStream ms = new MemoryStream(buf) ) {
        DataContractJsonSerializer dcjs = new DataContractJsonSerializer(typeof(LsUser));
        LsUser ku = (LsUser)dcjs.ReadObject(ms);

        Broker = ku.Endpoint;
        Trace.TraceInformation("Broker => {0}", Broker);
        ms.Close();
      }
      #endregion

      #region Product items
      buf = _client.DownloadData(_webapi + "product-items");
      str = Encoding.UTF8.GetString(buf);
      Trace.TraceInformation("Product items => {0}", str);
      str = string.Empty;
      using( MemoryStream ms = new MemoryStream(buf) ) {
        DataContractJsonSerializer dcjs = new DataContractJsonSerializer(typeof(List<LsProductItem>));

        Products = (List<LsProductItem>)dcjs.ReadObject(ms);
        ms.Close();
      }
      #endregion

      #region Status
      foreach( LsProductItem pi in Products ) {
        buf = _client.DownloadData(_webapi + "product-items/" + pi.SerialNo + "/status");
        str = Encoding.UTF8.GetString(buf);
        Trace.TraceInformation("Status {0} => {1}", pi.Name, str);
        using( MemoryStream ms = new MemoryStream(buf) ) {
          DataContractJsonSerializer dcjs = new DataContractJsonSerializer(typeof(LsMqtt));

          States.Add((LsMqtt)dcjs.ReadObject(ms));
          ms.Close();
        }
      }
      #endregion

      #region Certificate
      buf = _client.DownloadData(_webapi + "users/certificate");
      str = Encoding.UTF8.GetString(buf);
      Trace.TraceInformation("AWS Certificate => {0}", str);
      using( MemoryStream ms = new MemoryStream(buf) ) {
        DataContractJsonSerializer dcjs = new DataContractJsonSerializer(typeof(LsCertificate));
        LsCertificate lsc = (LsCertificate)dcjs.ReadObject(ms);

        ms.Close();
        str = lsc.Pkcs12.Replace("\\/", "/");
        buf = Convert.FromBase64String(str);
        //Store.SaveBytes("AWS.p12", buf);
        Cert = new X509Certificate2(buf);
      }
      #endregion
    } catch( Exception ex ) {
      Trace.TraceError("Login 2 {0}", ex.ToString());
      return false;
    }

    buf = null;
    return true;
  }

  public LsMqtt GetLastState(string name) {
    LsMqtt mqtt = null;

    foreach( LsProductItem pi in Products ) {
      if( pi.Name == name ) {
        byte[] buf = _client.DownloadData(_webapi + "product-items/" + pi.SerialNo + "/status");

        using( MemoryStream ms = new MemoryStream(buf) ) {
          DataContractJsonSerializer dcjs = new DataContractJsonSerializer(typeof(LsMqtt));

          mqtt = (LsMqtt)dcjs.ReadObject(ms);
          ms.Close();
        }
      }
    }
    return mqtt;
  }

  public Weather GetWeather(string name) {
    Weather data = null;

    foreach( LsProductItem pi in Products ) {
      if( pi.Name == name ) {
        byte[] buf = _client.DownloadData(_webapi + "product-items/" + pi.SerialNo + "/weather/current");
        string str;

        str = Encoding.UTF8.GetString(buf);
        Trace.TraceInformation("Web weather => {0}", str);
        using( MemoryStream ms = new MemoryStream(buf) ) {
          DataContractJsonSerializer dcjs = new DataContractJsonSerializer(typeof(Weather));

          data = (Weather)dcjs.ReadObject(ms);
          ms.Close();
        }
      }
    }
    return data;
  }

  public List<Activity> GetActivities(string name) {
    List<Activity> ls = new List<Activity>();

    foreach( LsProductItem pi in Products ) {
      if( pi.Name == name ) {
        byte[] buf = _client.DownloadData(_webapi + "product-items/" + pi.SerialNo + "/activity-log");

        if( buf != null ) {
          MemoryStream ms = new MemoryStream(buf);
          DataContractJsonSerializer dcjs = new DataContractJsonSerializer(typeof(List<Activity>));

          ls = dcjs.ReadObject(ms) as List<Activity>;
          //foreach( Activity a in ls ) {
          //  ActivityPayload p = a.Payload;

          //  Trace.TraceInformation("{0}: {1} - {2} - {3} - {4}", a.Stamp, p.Dat.LastError, p.Dat.LastState, p.Dat.Battery.Charging, p.Dat.Battery.Miss);
          //}
          ms.Close();
        }
      }
    }
    return ls;
  }
}
}
