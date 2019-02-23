using System;
using Xamarin.Forms;

namespace XamarinApp {
  public class UserPageCS : ContentPage {

    public UserPageCS() {
    }


    async void OnUpcomingAppointmentsButtonClicked(object sender, EventArgs e) {
      await Navigation.PushAsync(new UpcomingAppointmentsPage());
    }
  }
}
