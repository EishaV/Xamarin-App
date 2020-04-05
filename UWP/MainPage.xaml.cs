using Windows.UI.Xaml.Controls;
using Windows.UI.ViewManagement;
using Windows.Foundation;

namespace XamarinApp.UWP {
  /// <summary>
  /// An empty page that can be used on its own or navigated to within a Frame.
  /// </summary>
  public sealed partial class MainPage {
    public MainPage() {
      this.InitializeComponent();
      this.LoadApplication(new XamarinApp.App());

      ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(300, 300));
    }
  }
}
