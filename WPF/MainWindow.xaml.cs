using Xamarin.Forms;
using Xamarin.Forms.Platform.WPF;

namespace XamarinApp.WPF
{
  public partial class MainWindow : FormsApplicationPage
  {
    public MainWindow() {
      InitializeComponent();

      Forms.Init();
      LoadApplication(new XamarinApp.App());
    }
  }
}