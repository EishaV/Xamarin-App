using System;
using System.Collections.ObjectModel;
using System.Text;

using Xamarin.Forms;

namespace XamarinApp {
  public class TraceItem {
    public string Name { get; set; }
    public string Text { get; set; }

    public TraceItem(string name) { Name = name; }
    public TraceItem(string name, string text) { Name = name; Text = text; }
  }

  class TraceModel : BaseModel {
    public ObservableCollection<TraceItem> TraceList { get; private set; } = App.Instance.Trace;

    private TraceItem _Selected;
    public TraceItem TraceItem {
      get { return _Selected; }
      set {
        _Selected = value;
        OnPropertyChanged(nameof(TraceItem));
      }
    }
  }
}
