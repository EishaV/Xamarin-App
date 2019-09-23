using Xamarin.Forms;

namespace XamarinApp
{
	public class App : Application
	{
		public App ()
		{
			MainPage = new XamarinApp.MainPage ();
      //ViewModel vm = ViewModel.Instance;
		}

		protected override void OnStart ()
		{
			// Handle when your app starts
		}

		protected override void OnSleep ()
		{
      ViewModel.Instance.Logout();
			// Handle when your app sleeps
		}

		protected override void OnResume ()
		{
			// Handle when your app resumes
		}
	}
}

