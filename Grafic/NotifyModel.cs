using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

using MqttJson;

namespace XamarinApp {
  public class NotifyItem {
    public string Text { get; set; }
    public bool On { get; set; }
  }

  public class NotifyGroup : List<NotifyItem> {
    public string Title { get; set; }

    public NotifyGroup(string title) {
      Title = title;
    }
  }

  class NotifyModel : BaseModel {
    public ObservableCollection<NotifyGroup> Notify { get; } = App.Notify;

    public NotifyModel() {
      NotifyGroup ng;
      
      ng = new NotifyGroup("Errors");
      foreach( string s in Enum.GetNames(typeof(ErrorCode)) ) ng.Add(new NotifyItem() { Text=s });
      Notify.Add(ng);
      ng = new NotifyGroup("States");
      foreach( string s in Enum.GetNames(typeof(StatusCode)) ) ng.Add(new NotifyItem() { Text=s });
      Notify.Add(ng);
    }
  }
}
