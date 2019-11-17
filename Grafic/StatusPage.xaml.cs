using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace XamarinApp {
  [XamlCompilation(XamlCompilationOptions.Compile)]
  public partial class StatusPage : ContentPage {
    double ow = 0, oh = 0;

    public StatusPage() {
      InitializeComponent();

      BindingContext = new StatusModel();
    }

    protected override void OnSizeAllocated(double width, double height) {
      bool wgth;

      base.OnSizeAllocated(width, height);

      if( Math.Abs(ow - width) > 50 || Math.Abs(oh - height) > 50 ) {
        ow = width; oh = height;
        if( wgth = (width > 1.1 * height) ) {
          FlexMain.Direction = FlexDirection.Row;
        } else {
          FlexMain.Direction = FlexDirection.Column;
        }
        GridOrientP.IsVisible = GridStatisticP.IsVisible = !wgth;
        GridOrientL.IsVisible = GridStatisticL.IsVisible = wgth;
        //System.Diagnostics.Debug.WriteLine("WxH {0:N0}x{1:N0} {2:N0}x{3:N0} {4:N0}x{5:N0}",
        //  width, height, FlexMain.Width, FlexMain.Height, FlexBack.Width, FlexBack.Height);
      }
    }
  }
}