using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

using BackEnd;

namespace Logic {
  public class AwsClient {
    private MqttClient _mqtt = null;

    private string _broker, _uuid;
    private X509Certificate2 _cert;
    private string _cmdIn;
    private string[] _cmdOut;
    private byte[] _cmdQos;
    private ushort _msgId = 0;
    private bool _msgPoll = false;

    public AwsClient(string broker, string uuid, X509Certificate2 cert, string cmdIn, string cmdOut) {
      _broker = broker; _uuid = "android-" + uuid; _cert = cert;
      _cmdIn = cmdIn; _cmdOut = new string[] { cmdOut };
    }

    public bool LoadCert() {
      const string aws = "AWS.p12";

      if( Store.Exist(aws) ) {
        _cert = new X509Certificate2(Store.LoadBytes(aws));
        return true;
      }
      return false;
    }
    public bool Start(bool first = true) {
      _cmdQos = new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE }; // | MqttMsgBase.QOS_LEVEL_GRANTED_FAILURE 

      try {
        _mqtt = new MqttClient(_broker, 8883, true, null, _cert, MqttSslProtocols.TLSv1_2);
        UI.Log(string.Format("Mqtt broker '{0}'", _broker));
      } catch(Exception ex) {
        if(first) UI.Err(ex.Message);
        UI.Log(ex.ToString(), 9);
        return false;
      }

      try {
        _mqtt.MqttMsgSubscribed += MqttMsgSubscribed;
        _mqtt.MqttMsgPublished += MqttMsgPublished;
        _mqtt.MqttMsgPublishReceived += MqttMsgPublishReceived;
        _mqtt.ConnectionClosed += ConnectionClosed;

        byte code = _mqtt.Connect(_uuid);
        UI.Log(string.Format("Mqtt connect '{0} ({1})'", code, _mqtt.IsConnected));

        _mqtt.Subscribe(_cmdOut, _cmdQos);
        UI.Log(string.Format("Mqtt subscribe init"));

        _msgId = _mqtt.Publish(_cmdIn, Encoding.ASCII.GetBytes("{}"));
        _msgPoll = true;
        UI.Log(string.Format("Mqtt publish send '{0}'", _msgId));
      } catch(Exception ex) {
        if(first) UI.Err(ex.Message);
        UI.Log(ex.ToString(), 9);
        return false;
      }

      return true;
    }
    public void Exit() {
      if(_mqtt != null && _mqtt.IsConnected) {
        _mqtt.ConnectionClosed -= ConnectionClosed;
        _mqtt.MqttMsgPublishReceived -= MqttMsgPublishReceived;
        _mqtt.MqttMsgPublished -= MqttMsgPublished;
        _mqtt.MqttMsgSubscribed -= MqttMsgSubscribed;
        _mqtt.Unsubscribe(_cmdOut);
        try { _mqtt.Disconnect(); } catch { }
        _mqtt = null;
      }
    }

    public bool Connected { get { return _mqtt != null && _mqtt.IsConnected; } }
    public bool Polling { get { return _msgPoll && _mqtt != null && _mqtt.IsConnected; } }
    public void Poll() {
      _msgId = _mqtt.Publish(_cmdIn, Encoding.UTF8.GetBytes("{}"));
      _msgPoll = true;
    }
    public void Publish(string s) {
      _msgId = _mqtt.Publish(_cmdIn, Encoding.UTF8.GetBytes(s));
    }
    private void MqttMsgSubscribed(object sender, MqttMsgSubscribedEventArgs e) {
      UI.Log(string.Format("Mqtt subscribe done '{0}'", e.MessageId));
    }
    private void MqttMsgPublished(object sender, MqttMsgPublishedEventArgs e) {
      UI.Log(string.Format("Mqtt published done '{0}' ({1})", e.MessageId, e.IsPublished));
    }
    private void MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e) {
      string Json = Encoding.UTF8.GetString(e.Message);

      Debug.WriteLine(Json);
      try {
        MemoryStream ms = new MemoryStream(e.Message);
        DataContractJsonSerializer dcjs = new DataContractJsonSerializer(typeof(LsMqtt));
        LsMqtt jm = (LsMqtt)dcjs.ReadObject(ms);

        _msgPoll = false;
        ms.Close();
        UI.Recv(jm);
      } catch(Exception ex) {
        string s;

        UI.Log(ex.Message);
        s = Encoding.UTF8.GetString(e.Message);
        UI.Log(s);
      }
    }
    private void ConnectionClosed(object sender, EventArgs e) {
      UI.Log("Mqtt connection closed", 9);
      for(int i = 0; i < 10; i++) {
        System.Threading.Thread.Sleep(10000);
        if(_mqtt.IsConnected) {
          UI.Log("Mqtt is connected"); break;
        } else if(Start(false)) {
          UI.Log("Mqtt reconnected"); break;
        } else UI.Log(string.Format("Mqtt reconnect {0} failed", i), 1);
      }
    }

  }
}
