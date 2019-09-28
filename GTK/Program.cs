using System;
using Xamarin.Forms;
using Xamarin.Forms.Platform.GTK;

using XamarinApp;

namespace GameOfLife.GTK
{
  class MainClass
  {
    [STAThread]
    public static void Main(string[] args) {
      Gtk.Application.Init();
      Forms.Init();

      var app = new App();
      var window = new FormsWindow();
      window.LoadApplication(app);
      window.SetApplicationTitle("Landroid App");
      window.Resize(400, 450);
      window.Show();

      Gtk.Application.Run();
    }
  }
}
