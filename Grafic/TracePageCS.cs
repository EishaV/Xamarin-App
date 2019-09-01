using Xamarin.Forms;

namespace XamarinApp
{
	public class TracePageCS : ContentPage
	{
		public TracePageCS ()
		{
			IconImageSource = "settings.png";
			Title = "Settings";
			Content = new StackLayout { 
				Children = {
					new Label {
						Text = "Settings go here",
						HorizontalOptions = LayoutOptions.Center,
						VerticalOptions = LayoutOptions.CenterAndExpand
					}
				}
			};
		}
	}
}
