using System;
using System.Diagnostics;
using System.Text;

using Xamarin.Forms;

using Plugin.DeviceInfo;

namespace XamarinApp {
  public partial class UserPage : ContentPage {

    public UserPage() {
      InitializeComponent();

      BindingContext = ViewModel.Instance;
    }

    void OnLoginClicked(object sender, EventArgs args) {
      //TraceView.GetInstance().Log($"Email {email.Text}");

      ViewModel.Instance.Login();
    }
  }
}

