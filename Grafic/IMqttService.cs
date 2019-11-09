using System.Security.Cryptography.X509Certificates;

namespace XamarinApp {
  public interface IMqttService {
    string GetSystem();

    bool Start();
    bool Stop();
  }
}
