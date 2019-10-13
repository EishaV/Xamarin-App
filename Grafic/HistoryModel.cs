using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Input;

using Xamarin.Forms;

using MqttJson;
using Logic;

namespace XamarinApp {
  public class HistoryItem {
    public DateTime Stamp { get; set; }

    public string Error { get; private set; }
    public string State { get; private set; }
    public string Text {
      get {
        return string.Format("{0} - {1} - {2}", Stamp, Error, State);
      }
    }

    public string[] WeekDay { get; } = new string[] { "So", "Mo", "Di", "Mi", "Do", "Fr", "Sa" };
    public string[] StartTime { get; private set; }
    public string[] Duration { get; private set; }

    public string CfgMain { get; private set; }
    public string CfgPlan { get; private set; }
    public string CfgZone { get; private set; }
    public string DatMain { get; private set; }
    public string DatAccu { get; private set; }
    public string DatDmp { get; private set; }
    public string DatStat { get; private set; }
    public bool Local { get; private set; }

    public HistoryItem(DateTime stamp, LsMqtt mqtt) {
      Schedule sc = mqtt.Cfg.Schedule;
      Battery bt = mqtt.Dat.Battery;
      Statistic st = mqtt.Dat.Statistic;

      Local = true;

      Stamp = stamp;
      Error = mqtt.Dat.Error.ToString();
      State = mqtt.Dat.State.ToString();

      CfgMain += string.Format("tm:\u00A0{0} ", mqtt.Cfg.Time);
      CfgMain += string.Format("dt:\u00A0{0} ", mqtt.Cfg.Date);
      CfgMain += string.Format("rd:\u00A0{0} ", mqtt.Cfg.RainDelay);

      CfgPlan += string.Format("m:\u00A0{0} ", sc.Mode);
      CfgPlan += string.Format("p:\u00A0{0} ", sc.Perc);
      CfgPlan += "d:\r\n";
      for( int i = 0; i < 7; i++ ) {
        CfgPlan += string.Format("{0}\u00A0{1}\u00A0{2}", sc.Days[i][0], sc.Days[i][1], sc.Days[i][2]);
        if( i < 6 ) CfgPlan += ", ";
      }

      CfgZone = string.Format("mz:\u00A0{0} ", string.Join(",", mqtt.Cfg.MultiZones));
      CfgZone += string.Format("mzv:\u00A0{0}", string.Join(",", mqtt.Cfg.MultiZonePercs));

      DatMain = string.Format("fw:\u00A0{0} ", mqtt.Dat.Firmware);
      DatMain += string.Format("ls:\u00A0{0} ", (int)mqtt.Dat.State);
      DatMain += string.Format("le:\u00A0{0} ", (int)mqtt.Dat.Error);
      DatMain += string.Format("lz:\u00A0{0} ", mqtt.Dat.LastZone);
      DatMain += string.Format("lk:\u00A0{0} ", mqtt.Dat.Lock);
      DatMain += string.Format("rsi:\u00A0{0}", mqtt.Dat.RecvSignal);

      DatAccu = string.Format("t:\u00A0{0} ", bt.Temp);
      DatAccu += string.Format("v:\u00A0{0} ", bt.Volt);
      DatAccu += string.Format("p:\u00A0{0} ", bt.Perc);
      DatAccu += string.Format("n:\u00A0{0} ", bt.Cycle);
      DatAccu += string.Format("c:\u00A0{0} ", (int)bt.Charging);
      DatAccu += string.Format("m:\u00A0{0}", bt.Mode);

      DatDmp = string.Format("dmp: {0}", string.Join("  ", mqtt.Dat.Orient));

      DatStat = string.Format("b:\u00A0{0} ", st.Blade);
      DatStat += string.Format("d:\u00A0{0} ", st.Distance);
      DatStat += string.Format("wt:\u00A0{0}", st.WorkTime);

      StartTime = new string[7];
      Duration  = new string[7];
      for( int i = 0; i < 7; i++ ) {
        StartTime[i] = sc.Days[i][0].ToString();
        Duration[i] = string.Format("{0} {1}", sc.Days[i][1], sc.Days[i][2]);
      }
    }
    public HistoryItem(Activity a) {
      ActivityConfig c = a.Payload.Cfg;
      ActivityData d = a.Payload.Dat;
      ActivityBattery bt = a.Payload.Dat.Battery;

      Local = false;

      Stamp = DateTime.Parse(a.Stamp);
      Error = d.LastError.ToString();
      State = d.LastState.ToString();

      CfgMain += string.Format("tm:\u00A0{0} ", c.Time);
      CfgMain += string.Format("dt:\u00A0{0} ", c.Date);

      CfgPlan = null;

      CfgZone = string.Format("mz:\u00A0{0} ", string.Join(",", c.MultiZones));
      CfgZone += string.Format("mzv:\u00A0{0}", string.Join(",", c.MultiZonePercs));

      DatMain = string.Format("fw:\u00A0{0} ", d.Firmware);
      DatMain += string.Format("ls:\u00A0{0} ", (int)d.LastState);
      DatMain += string.Format("le:\u00A0{0} ", (int)d.LastError);
      DatMain += string.Format("lz:\u00A0{0} ", d.LastZone);
      DatMain += string.Format("lk:\u00A0{0} ", d.Lock);

      DatAccu = string.Format("c:\u00A0{0} ", (int)bt.Charging);
      DatAccu += string.Format("m:\u00A0{0}", bt.Miss);

      DatDmp = null;

      DatStat = null;
    }
  }

  class HistoryModel : BaseView {
    public ObservableCollection<HistoryItem> History { get; private set; } = App.Instance.History;

    private HistoryItem _Selected;
    public HistoryItem HistoryItem {
      get { return _Selected; }
      set {
        _Selected = value;
        OnPropertyChanged(nameof(HistoryItem));
      }
    }
    public ICommand CmdServer { protected set; get; }
    public ICommand CmdLocal { protected set; get; }

    private void GetActLog() {
      ObservableCollection<HistoryItem> lh = new ObservableCollection<HistoryItem>();

      foreach( Activity a in App.Web.GetActivities(App.Web.Products[0].Name) ) lh.Add(new HistoryItem(a));
      History = lh;
      OnPropertyChanged(nameof(History));
      HistoryItem = History[0];
    }

    private void LocalHis() {
      History = App.Instance.History;
      OnPropertyChanged(nameof(History));
      if( History.Count > 0 ) HistoryItem = History[0];
    }

    public HistoryModel() {
      CmdServer = new Command(() => GetActLog());
      CmdLocal = new Command(() => LocalHis());
    }
  }
}
