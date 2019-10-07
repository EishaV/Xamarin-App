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
    public StatusPage() {
      InitializeComponent();

      BindingContext = ViewModel.Instance;
    }

    protected override void OnSizeAllocated(double width, double height) {
      base.OnSizeAllocated(width, height);

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
        ImgPitch.HeightRequest = ImgRoll.HeightRequest = ImgYaw.HeightRequest = -1;
        ImgPitch.WidthRequest = ImgRoll.WidthRequest = ImgYaw.WidthRequest = width / 3.5;
        StackPitch.Orientation = StackRoll.Orientation = StackYaw.Orientation = StackOrientation.Vertical;
        StackStat.Orientation = StackOrientation.Horizontal;
      }
    }
  }
}