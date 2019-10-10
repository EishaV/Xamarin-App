using System;
using System.Collections.Generic;
using System.Text;

using MqttJson;

namespace Logic {
  public delegate void ErrorDelegate(string msg);
  public delegate void TraceDelegate(string cat, string txt);
  public delegate void MqttDelegate(LsMqtt mj);

  public class UI {
    private static TraceDelegate log = null;
    public static void SetLog(TraceDelegate value) { log = value; }
    public static void Trace(string cat) { log?.Invoke(cat, null); }
    public static void Trace(string cat, string txt) { log?.Invoke(cat, txt); }

    private static ErrorDelegate err = null;
    public static void SetErr(ErrorDelegate value) { err = value; }
    public static void Err(string text) { err?.Invoke(text); }
  }
}
