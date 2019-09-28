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

      if( width > height ) {
        StackFormat.Orientation = StackOrientation.Horizontal;
        RestLayout.Orientation = StackOrientation.Vertical;
      } else {
        StackFormat.Orientation = StackOrientation.Vertical;
        RestLayout.Orientation = StackOrientation.Horizontal;
      }
    }
  }
}