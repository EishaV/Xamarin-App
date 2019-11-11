using System;
using System.Collections.Generic;
using System.Diagnostics;

using Xamarin.Forms;

namespace XamarinApp {
  public partial class TracePage : ContentPage {

    public TracePage() {
      InitializeComponent();
    }

    protected override void OnSizeAllocated(double width, double height) {
      base.OnSizeAllocated(width, height);

      if( width > height ) Flex.Direction = FlexDirection.Row;
      else Flex.Direction = FlexDirection.Column;
      }

      //private void trace_ItemSelected(object sender, SelectedItemChangedEventArgs e) {
      //  text.Text = (e.SelectedItem as TraceItem).Text;
      //}
    }

  }

