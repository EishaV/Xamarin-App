using System;
using System.Diagnostics;
using System.Text;

using Xamarin.Forms;

using Plugin.DeviceInfo;

namespace XamarinApp {
  public partial class UserPage : ContentPage {

    public UserPage() {
      InitializeComponent();
      BindingContext = new UserModel();
    }

    void OnLoginClicked(object sender, EventArgs args) {
      //TraceView.GetInstance().Log($"Email {email.Text}");

      (BindingContext as UserModel).Login();
    }

    void OnTestClicked(object sender, EventArgs args) {
      string sys = DependencyService.Get<IMqttService>().GetSystem();
      Test.Text = sys;
    }
  }
}

