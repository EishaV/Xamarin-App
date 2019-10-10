using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Text;

using MqttJson;

namespace XamarinApp {
  public class HistoryItem {
    public DateTime Stamp { get; set; }

    public LsMqtt Mqtt { get; private set; }

    public string[] WeekDay { get; } = new string[] { "So", "Mo", "Di", "Mi", "Do", "Fr", "Sa" };

    public string[] StartTime { get; private set; }
    public string[] Duration { get; private set; }

    public HistoryItem(DateTime stamp, LsMqtt json) {
      Stamp = stamp;
      Mqtt = json;

      Schedule sc = Mqtt.Cfg.Schedule;

      StartTime = new string[7];
      Duration  = new string[7];
      for( int i = 0; i < 7; i++ ) {
        StartTime[i] = sc.Days[i][0].ToString();
        Duration[i] = sc.Days[i][1].ToString();
      }
    }
  }


  class HistoryModel : BaseView {
    public ObservableCollection<HistoryItem> History { get; } = App.Instance.History;

    private HistoryItem _Selected;
    public HistoryItem HistoryItem {
      get { return _Selected; }
      set { 
        _Selected = value;
        OnPropertyChanged(nameof(HistoryItem));
      }
    }

    public HistoryModel() {
      int x = History.Count;

    }
  }
}
