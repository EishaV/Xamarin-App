using Xamarin.Forms;

namespace XamarinApp
{
	public class MainPageCS : TabbedPage
	{
		public MainPageCS ()
		{
			Children.Add (new TodayPageCS ());
			Children.Add (new UserPageCS());
      Children.Add(new StatusPage());
			Children.Add (new TracePageCS());
		}
	}
}
