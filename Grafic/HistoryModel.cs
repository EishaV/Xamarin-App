using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Text;

using MqttJson;

namespace XamarinApp {
  public class HistoryItem {
    public DateTime Stamp { get; set; }

    public LsMqtt Mqtt { get; private set; }

    public string Plan {
      get {
        string[] wd = new string[] { "So", "Mo", "Di", "Mi", "Do", "Fr", "Sa" };
        Schedule sc = Mqtt.Cfg.Schedule;
        string s = string.Empty;

        for( int i = 0; i < 7; i++ ) {
          s += string.Format("{0} {1} {2}", wd[i], sc.Days[i][0], sc.Days[i][1]);
        }
        return s;
      }
    }

    public HistoryItem(DateTime stamp, LsMqtt json) {
      Stamp = stamp;
      Mqtt = json;
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
