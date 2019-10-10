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
using BackEnd;

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
        Debug.Print("Oauth token: {0}", str);
        UI.Trace("Oauth token", str);
        using( MemoryStream ms = new MemoryStream(buf) ) {
          DataContractJsonSerializer dcjs = new DataContractJsonSerializer(typeof(LsOAuth));

          lsoa = (LsOAuth)dcjs.ReadObject(ms);
          UI.Trace("Access token", lsoa.Token);
          _client.Headers["Authorization"] = string.Format("{0} {1}", lsoa.Type, lsoa.Token);
          UI.Trace("Token type", lsoa.Type);
          ms.Close();
        }
      } catch( Exception ex ) {
        UI.Err(ex.Message);
        UI.Trace("Login 1", ex.ToString());
        return false;
      }
      #endregion

      try {
        #region Benutzer
        buf = _client.DownloadData(_webapi + "users/me");
        str = Encoding.UTF8.GetString(buf);
        UI.Trace("User info", str);
        Debug.Print("User info: {0}", str);
        using( MemoryStream ms = new MemoryStream(buf) ) {
          DataContractJsonSerializer dcjs = new DataContractJsonSerializer(typeof(LsUser));
          LsUser ku = (LsUser)dcjs.ReadObject(ms);

          Broker = ku.Endpoint;
          ms.Close();
        }
        #endregion

        #region Product items
        buf = _client.DownloadData(_webapi + "product-items");
        str = Encoding.UTF8.GetString(buf);
        UI.Trace("Product items", str);
        Debug.Print("Product items: {0}", str);
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
          Debug.WriteLine("Status {0}: {1}", pi.Name, str);
          UI.Trace("Status " + pi.Name, str);
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
        UI.Trace("AWS Certificate", str);
        Debug.WriteLine("AWS Certificate: {0}", str);
        using( MemoryStream ms = new MemoryStream(buf) ) {
          DataContractJsonSerializer dcjs = new DataContractJsonSerializer(typeof(LsCertificate));
          LsCertificate lsc = (LsCertificate)dcjs.ReadObject(ms);

          ms.Close();
          str = lsc.Pkcs12.Replace("\\/", "/");
          buf = Convert.FromBase64String(str);
          Store.SaveBytes("AWS.p12", buf);
          Cert = new X509Certificate2(buf);
        }
        #endregion
      } catch( Exception ex ) {
        UI.Err(ex.Message);
        UI.Trace("Login 2", ex.ToString());
        return false;
      }

      buf = null;
      return true;
    }
  }
}
