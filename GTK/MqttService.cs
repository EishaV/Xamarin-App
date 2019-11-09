using System.Security.Cryptography.X509Certificates;
using Xamarin.Forms;

using XamarinApp.GTK;

using Logic;

[assembly: Dependency(typeof(MqttService))]
namespace XamarinApp.GTK {
  public class MqttService : IMqttService {
    public string GetSystem() {
      return "Gtk";
    }

    public bool Start() {
      return true;
    }
    public bool Stop() {
      return true;
    }
  }
}
