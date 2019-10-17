using System.Security.Cryptography.X509Certificates;

namespace XamarinApp {
  public interface IMqttService {
    string GetSystem();

    bool Start(string broker, string uuid, X509Certificate2 cert, string cmdIn, string cmdOut);
    bool Stop();
  }
}
