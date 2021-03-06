﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

using MqttJson;

namespace Logic {
  public class MyEventArgs :EventArgs {
    public LsMqtt Mqtt { get; private set; }

    public MyEventArgs(LsMqtt mqtt) {
      Mqtt = mqtt;
    }
  }

  public class AwsClient {
    private MqttClient _mqtt = null;

    private string _broker, _uuid;
    private X509Certificate2 _cert;
    private string _cmdIn;
    private string[] _cmdOut;
    private byte[] _cmdQos;
    private ushort _msgId = 0;
    private bool _msgPoll = false;

    public event EventHandler<MyEventArgs> Recv;

    public AwsClient(string broker, string uuid, X509Certificate2 cert, string cmdIn, string cmdOut) {
      _broker = broker; _uuid = "android-" + uuid; _cert = cert;
      _cmdIn = cmdIn; _cmdOut = new string[] { cmdOut };
    }

    public bool Start() {
      _cmdQos = new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE }; // | MqttMsgBase.QOS_LEVEL_GRANTED_FAILURE 

      try {
        _mqtt = new MqttClient(_broker, 8883, true, null, _cert, MqttSslProtocols.TLSv1_2);
        Trace.TraceInformation("Mqtt broker => {0}", _broker);
      } catch( Exception ex ) {
        Trace.TraceError("Aws create failed {0}", ex.ToString());
        return false;
      }

      try {
        _mqtt.MqttMsgSubscribed += MqttMsgSubscribed;
        _mqtt.MqttMsgPublished += MqttMsgPublished;
        _mqtt.MqttMsgPublishReceived += MqttMsgPublishReceived;
        _mqtt.ConnectionClosed += ConnectionClosed;

        byte code = _mqtt.Connect(_uuid);
        Trace.TraceInformation("Mqtt connect => Code: {0} State: {1}", code, _mqtt.IsConnected);

        _mqtt.Subscribe(_cmdOut, _cmdQos);
        //UI.Trace("Mqtt subscribe init");

        _msgId = _mqtt.Publish(_cmdIn, Encoding.ASCII.GetBytes("{}"));
        _msgPoll = true;
        //UI.Trace(string.Format("Mqtt publish send '{0}'", _msgId));
      } catch( Exception ex ) {
        Trace.TraceError("Aws connect failed {0}", ex.ToString());
        return false;
      }

      return true;
    }
    public void Exit() {
      if( _mqtt != null && _mqtt.IsConnected ) {
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
      if( Connected ) {
        _msgId = _mqtt.Publish(_cmdIn, Encoding.UTF8.GetBytes("{}"));
        _msgPoll = true;
      }
    }
    public void Publish(string s) {
      _msgId = _mqtt.Publish(_cmdIn, Encoding.UTF8.GetBytes(s));
    }
    private void MqttMsgSubscribed(object sender, MqttMsgSubscribedEventArgs e) {
      Trace.TraceInformation("Mqtt subscribe done => MsgId: {0}", e.MessageId);
    }
    private void MqttMsgPublished(object sender, MqttMsgPublishedEventArgs e) {
      Trace.TraceInformation("Mqtt published done => MsgId: '{0}' IsPub: {1}", e.MessageId, e.IsPublished);
    }
    private void MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e) {
      string Json = Encoding.UTF8.GetString(e.Message);

      Trace.TraceInformation("Mqtt received => {0}", Json);
      try {
        MemoryStream ms = new MemoryStream(e.Message);
        DataContractJsonSerializer dcjs = new DataContractJsonSerializer(typeof(LsMqtt));
        LsMqtt jm = (LsMqtt)dcjs.ReadObject(ms);
        EventHandler<MyEventArgs> recv = Recv;

        _msgPoll = false;
        ms.Close();
        recv(this, new MyEventArgs(jm));
      } catch( Exception ex ) {
        string s;

        Trace.TraceError("Mqtt received eception => {0}", ex.ToString());
        s = Encoding.UTF8.GetString(e.Message);
        Trace.TraceError("Mqtt received message => {0}", s);
      }
    }
    private void ConnectionClosed(object sender, EventArgs e) {
      Trace.TraceInformation("Mqtt connection closed");
      return;
      for( int i = 0; i < 10; i++ ) {
        System.Threading.Thread.Sleep(10000);
        if( _mqtt.IsConnected ) {
          Trace.TraceInformation("Mqtt is connected");
          break;
        } else if( Start() ) {
          Trace.TraceInformation("Mqtt reconnected");
          break;
        } else {
          Trace.TraceError("Mqtt reconnect {0} failed", i);
        }
      }
    }

  }
}
