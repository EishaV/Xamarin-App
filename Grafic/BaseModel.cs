using System.ComponentModel;

namespace XamarinApp {
  public class BaseView : INotifyPropertyChanged {
    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName) {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}