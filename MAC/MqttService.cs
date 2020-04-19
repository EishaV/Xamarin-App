using Xamarin.Forms;

using XamarinApp.MAC;

[assembly: Dependency(typeof(MqttService))]
namespace XamarinApp.MAC {
  public class MqttService : IMqttService {
    public string GetSystem() {
      return "Mac";
    }
    public bool Start() {
      return true;
    }
    public bool Stop() {
      return true;
    }
  }
}
