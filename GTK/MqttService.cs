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

    public bool Start(string broker, string uuid, X509Certificate2 cert, string cmdIn, string cmdOut) {
      AwsClient ac = new AwsClient(broker, uuid, cert, cmdIn, cmdOut);

      if( ac.Start(true) ) {
        XamarinApp.App.Aws = ac;
        return true;
      }
      return true;
    }
    public bool Stop() {
      return true;
    }
  }
}
