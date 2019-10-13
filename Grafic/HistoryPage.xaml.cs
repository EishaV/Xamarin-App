using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace XamarinApp {
  [XamlCompilation(XamlCompilationOptions.Compile)]
  public partial class HistoryPage : ContentPage {
    double ow = Double.NaN, oh = Double.NaN;

    public HistoryPage() {
      InitializeComponent();

      //BindingContext = new HistoryModel();
    }

    protected override void OnSizeAllocated(double width, double height) {
      base.OnSizeAllocated(width, height);

      if( ow != width || oh != height ) {
        ow = width; oh = height;
        if( width > 1.1 * height ) {
          Main.Orientation = StackOrientation.Horizontal;
        } else {
          Main.Orientation = StackOrientation.Vertical;
        }
      }
    }
  }
}