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
      base.OnSizeAllocated(width, height);

      if( Math.Abs(ow - width) > 50 || Math.Abs(oh - height) > 50 ) {
        ow = width; oh = height;
        if( width > 1.1 * height ) {
          FlexMain.Direction = FlexDirection.Row;
          StackOrient.Orientation = StackOrientation.Vertical;
          StackPitch.Orientation = StackRoll.Orientation = StackYaw.Orientation = StackOrientation.Horizontal;
          StackPitch.HorizontalOptions = StackRoll.HorizontalOptions = StackYaw.HorizontalOptions = LayoutOptions.StartAndExpand;
          ImgPitch.WidthRequest = ImgRoll.WidthRequest = ImgYaw.WidthRequest = -1;
          ImgPitch.HeightRequest = ImgRoll.HeightRequest = ImgYaw.HeightRequest = height / 6;
          StackStat.Orientation = StackOrientation.Vertical;
          StackBlade.Orientation = StackDist.Orientation = StackWork.Orientation = StackOrientation.Horizontal;
          StackBlade.HorizontalOptions = StackDist.HorizontalOptions = StackWork.HorizontalOptions = LayoutOptions.FillAndExpand;
        } else {
          FlexMain.Direction = FlexDirection.Column;
          StackOrient.Orientation = StackOrientation.Horizontal;
          StackPitch.Orientation = StackRoll.Orientation = StackYaw.Orientation = StackOrientation.Vertical;
          StackPitch.HorizontalOptions = StackRoll.HorizontalOptions = StackYaw.HorizontalOptions = LayoutOptions.CenterAndExpand;
          // sonst fehlen bei WPF beim ersten Afruf die Bilder ...
          ImgPitch.WidthRequest = ImgRoll.WidthRequest = ImgYaw.WidthRequest = 1.333 * height / 9.0;
          ImgPitch.HeightRequest = ImgRoll.HeightRequest = ImgYaw.HeightRequest = height / 9.0;
          StackStat.Orientation = StackOrientation.Horizontal;
          StackBlade.Orientation = StackDist.Orientation = StackWork.Orientation = StackOrientation.Vertical;
          StackBlade.HorizontalOptions = StackDist.HorizontalOptions = StackWork.HorizontalOptions = LayoutOptions.CenterAndExpand;
        }
        //System.Diagnostics.Debug.WriteLine("WxH {0:N0}x{1:N0} {2:N0}x{3:N0} {4:N0}x{5:N0}",
        //  width, height, FlexMain.Width, FlexMain.Height, FlexBack.Width, FlexBack.Height);
      }
    }
  }
}