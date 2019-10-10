using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Input;

using Logic;
using MqttJson;
using Xamarin.Forms;

namespace XamarinApp {
  public class Mower {
    public String Name { get; set; }
    public String State { get; set; }
    public String Error { get; set; }
    public String Firmware { get; set; }
    public int Rrsi { get; set; }

    public Battery Accu { get; set; }

    public string Start { get; set; }
    public string Duration { get; set; }
    public float Pitch { get; set; }
    public float Roll { get; set; }
    public float Yaw { get; set; }

    public TimeSpan WorkTime { get; set; }
    public TimeSpan BladeTime { get; set; }
    public int Distance { get; set; }

    public DateTime Stamp { get; set; }
  }

  public class StatusModel : BaseView {
    public List<Mower> Mowers { get; private set; }
    public Mower Mower { get; private set; }

    public ICommand PollCommand { protected set; get; }

    public StatusModel() {
      PollCommand = new Command(() => App.Instance.Aws?.Poll());

      App.Instance.Recv += Recv;

      //Mowers = new List<Mower>();
      Mower = new Mower() {
        Name = "Miss Land",
        Error = "",
        State = "daheim",
        Rrsi = -00,
        Firmware = "0.12",
        Accu = new Battery { Temp = 38, Volt = 50.2F },
        Start = "15:00", Duration = "240"
      };

    }

    private void Recv(object sender, MyEventArgs e) {
      LsMqtt mqtt = e.Mqtt;
      string dts = string.Format("{0} {1}", mqtt.Cfg.Date, mqtt.Cfg.Time);

      //Invoke(new MqttDelegate(RecvInvoke));
      Mower.State = mqtt.Dat.State.ToString();
      Mower.Error = mqtt.Dat.Error.ToString();
      Mower.Rrsi = mqtt.Dat.RecvSignal;
      Mower.Firmware = mqtt.Dat.Firmware;
      Mower.Accu = mqtt.Dat.Battery;
      Mower.Pitch = mqtt.Dat.Orient[0];
      Mower.Roll = mqtt.Dat.Orient[1];
      Mower.Yaw = mqtt.Dat.Orient[2];
      Mower.WorkTime = TimeSpan.FromSeconds(mqtt.Dat.Statistic.WorkTime);
      Mower.BladeTime = TimeSpan.FromSeconds(mqtt.Dat.Statistic.Blade);
      Mower.Distance = mqtt.Dat.Statistic.Distance;
      Mower.Stamp = DateTime.ParseExact(dts, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
      OnPropertyChanged(nameof(Mower));
    }
  }
}