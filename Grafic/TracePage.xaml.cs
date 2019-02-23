using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

using Xamarin.Forms;

namespace XamarinApp {
  public partial class TracePage : ContentPage {
    ObservableCollection<TraceItem> traceItems = new ObservableCollection<TraceItem>();

    public TracePage() {
      InitializeComponent();
      trace.ItemsSource = traceItems;
      traceItems.Add(new TraceItem("Test", "irgenwas sollte da stehen"));
      MessagingCenter.Subscribe<Application, TraceItem>(this, "Trace", TraceEvent);
    }

    public void TraceEvent(object sender, TraceItem ti) {
      traceItems.Add(ti);
      Debug.WriteLine($"TraceRecv -> Name {ti.Name} Text{ti.Text}");
    }

    private void trace_ItemSelected(object sender, SelectedItemChangedEventArgs e) {
      text.Text = (e.SelectedItem as TraceItem).Text;
    }
  }
  public class TraceItem {
    public string Name { get; set; }
    public string Text { get; set; }

    public TraceItem(string name) { Name = name; }
    public TraceItem(string name, string text) { Name = name; Text = text; }
  }
}

