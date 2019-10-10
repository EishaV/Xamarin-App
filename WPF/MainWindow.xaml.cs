using System;
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

    private bool topBarsRemoved = false;

    private void RemoveTopBars() {
      //System.Windows.Controls.Grid commandBar = this.Template.FindName("PART_CommandsBar", this) as System.Windows.Controls.Grid;

      //if( commandBar != null )
      //  (commandBar.Parent as System.Windows.Controls.Grid)?.Children.Remove(commandBar);

      var topAppBar = this.Template.FindName("PART_TopAppBar", this) as Xamarin.Forms.Platform.WPF.Controls.FormsAppBar; // as WpfLightToolkit.Controls.LightAppBar;
      if( topAppBar != null )
        (topAppBar.Parent as System.Windows.Controls.Grid)?.Children.Remove(topAppBar);

      topBarsRemoved = true;
    }

    protected override void OnActivated(EventArgs e) {
      base.OnActivated(e);
      if( !topBarsRemoved ) RemoveTopBars();
    }

    protected override void OnClosed(EventArgs e) {
      XamarinApp.App.Instance.Logout();
      base.OnClosed(e);
    }
  }
}