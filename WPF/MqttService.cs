using System.Security.Cryptography.X509Certificates;
using Xamarin.Forms;

using XamarinApp.WPF;
using Logic;

[assembly: Dependency(typeof(MqttService))]
namespace XamarinApp.WPF {
  public class MqttService : IMqttService {
    public string GetSystem() {
      return "Wpf";
    }
    public bool Start() {
      return true;
    }
    public bool Stop() {
      return true;
    }
  }
}
