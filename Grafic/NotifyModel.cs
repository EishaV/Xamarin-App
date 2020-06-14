using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

using MqttJson;

namespace XamarinApp {
  public class NotifyItem {
    public string Text { get; set; }
    public int Code { get; set; }
    public bool On { get; set; }
  }

  public class NotifyGroup : List<NotifyItem> {
    public string Title { get; }

    public string Tag { get; }

    public NotifyGroup(string tag) {
      Title = Resource.ResourceManager.GetString(tag);
      Tag = tag;
    }
  }

  class NotifyModel : BaseModel {
    public ObservableCollection<NotifyGroup> Notify { get; } = App.Notify;

    public NotifyModel() {
      string dir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
      string path = Path.Combine(dir, "Notify.lst");
      List<string> list = null;
      NotifyGroup ng;

      if( File.Exists(path) ) list = File.ReadAllLines(path).ToList();
      else list = new List<string>();
      ng = new NotifyGroup("Errors");
      foreach( ErrorCode ec in Enum.GetValues(typeof(ErrorCode)) ) {
        string id = "ERROR_" + ec.ToString();

        if( ec != ErrorCode.NONE ) ng.Add(new NotifyItem() { Text = Resource.ResourceManager.GetString(id), Code = (int)ec, On = list.Contains(id) });
      }
      Notify.Add(ng);
      ng = new NotifyGroup(Resource.ResourceManager.GetString("States"));
      foreach( StatusCode sc in Enum.GetValues(typeof(StatusCode)) ) {
        string id = "STATE_" + sc.ToString();

        ng.Add(new NotifyItem() { Text = Resource.ResourceManager.GetString(id), Code = (int)sc, On = list.Contains(id) });
      }
      Notify.Add(ng);
    }
  }
}
