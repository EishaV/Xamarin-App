using Xamarin.Forms;

using Logic;
using System;

namespace XamarinApp
{
	public class App : Application
	{
    WebClient _wc = null;
    AwsClient _ac = null;

    public event EventHandler<MyEventArgs> Recv;

    public static App Instance {
      get { return Application.Current as App; }
    }

    public static WebClient Web {
      get { return Instance?._wc; }
      set { if( Instance != null ) Instance._wc = value; }
    }

    public AwsClient Aws {
      get { return _ac; }
      set {
          _ac = value;
          _ac.Recv += OnRecv;
        }
      }

    private void OnRecv(object sender, MyEventArgs e) {
      Recv?.Invoke(sender, e);
    }

    public App ()	{
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

