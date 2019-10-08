using System;
using System.Collections.Generic;
using System.Diagnostics;

using Xamarin.Forms;

namespace XamarinApp {
  public partial class TracePage : ContentPage {

    public TracePage() {
      InitializeComponent();

      BindingContext = App.Instance.TraceItems;
      //MessagingCenter.Subscribe<Application, TraceItem>(this, "Trace", TraceEvent);
    }

    private void trace_ItemSelected(object sender, SelectedItemChangedEventArgs e) {
      text.Text = (e.SelectedItem as TraceItem).Text;
    }
  }

}

