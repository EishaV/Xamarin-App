using System.Security.Cryptography.X509Certificates;
using Xamarin.Forms;

using XamarinApp.UWP;

[assembly: Dependency(typeof(MqttService))]
namespace XamarinApp.UWP {
  public class MqttService : IMqttService {
    public string GetSystem() {
      return "Uwp";
    }
    public bool Start() {
      return true;
    }
    public bool Stop() {
      return true;
    }
  }
}
