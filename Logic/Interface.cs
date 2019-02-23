using System;
using System.Collections.Generic;
using System.Text;

namespace Logic {
  public delegate void ErrDelegate(string msg);
  public delegate void LogDelegate(string log, int c = 0);
  public delegate void MqttDelegate(LsMqtt mj);

  public class UI {
    private static LogDelegate log = null;
    public static void SetLog(LogDelegate value) { log = value; }
    public static void Log(string text) { log?.Invoke(text); }
    public static void Log(string text, int c) { log?.Invoke(text, c); }

    private static ErrDelegate err = null;
    public static void SetErr(ErrDelegate value) { err = value; }
    public static void Err(string text) { err?.Invoke(text); }

    private static MqttDelegate recv = null;
    public static void SetRecv(MqttDelegate value) { recv = value; }
    public static void Recv(LsMqtt data) { recv?.Invoke(data); }
  }
}
