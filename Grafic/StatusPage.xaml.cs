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
    double ow = Double.NaN, oh = Double.NaN;

    public StatusPage() {
      InitializeComponent();

      BindingContext = new StatusModel();
    }

    protected override void OnSizeAllocated(double width, double height) {
      base.OnSizeAllocated(width, height);

      if( ow != width && oh != height ) {
        ow = width; oh = height;
        if( width > 1.1 * height ) {
          FlexMain.Direction = FlexDirection.Row;
          StackOrient.Orientation = StackOrientation.Vertical;
          StackPitch.Orientation = StackRoll.Orientation = StackYaw.Orientation = StackOrientation.Horizontal;
          ImgPitch.WidthRequest = ImgRoll.WidthRequest = ImgYaw.WidthRequest = -1;
          ImgPitch.HeightRequest = ImgRoll.HeightRequest = ImgYaw.HeightRequest = height / 6;
          StackStat.Orientation = StackOrientation.Vertical;
        } else {
          FlexMain.Direction = FlexDirection.Column;
          StackOrient.Orientation = StackOrientation.Horizontal;
          StackPitch.Orientation = StackRoll.Orientation = StackYaw.Orientation = StackOrientation.Vertical;
          // sonst fehlen bei WPF beim ersten Afruf die Bilder ...
          ImgPitch.WidthRequest = ImgRoll.WidthRequest = ImgYaw.WidthRequest = height / 9 * 200/150;
          ImgPitch.HeightRequest = ImgRoll.HeightRequest = ImgYaw.HeightRequest = height / 9;
          StackStat.Orientation = StackOrientation.Horizontal;
        }
        System.Diagnostics.Debug.WriteLine("WxH {0:N0}x{1:N0} {2:N0}x{3:N0} {4:N0}x{5:N0}",
          width, height, FlexMain.Width, FlexMain.Height, FlexBack.Width, FlexBack.Height);
      }
    }
  }
}