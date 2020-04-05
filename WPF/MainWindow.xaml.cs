using System;
using System.Windows.Controls;
using Xamarin.Forms;
using Xamarin.Forms.Platform.WPF;

namespace XamarinApp.WPF {
  public partial class MainWindow : FormsApplicationPage {
    public MainWindow() {
      InitializeComponent();

      Forms.Init();
      LoadApplication(new XamarinApp.App());
    }

    private bool topBarsRemoved = false;

    private void RemoveTopBars() {
      if( Template.FindName("PART_CommandsBar", this) is System.Windows.Controls.Grid cmdBar ) cmdBar.Height = 25;
      //  (commandBar.Parent as System.Windows.Controls.Grid)?.Children.Remove(commandBar);

      if( Template.FindName("PART_TopAppBar", this) is Xamarin.Forms.Platform.WPF.Controls.FormsAppBar topBar )
        (topBar.Parent as System.Windows.Controls.Grid)?.Children.Remove(topBar);

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