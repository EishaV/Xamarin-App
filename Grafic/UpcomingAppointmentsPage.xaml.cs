﻿using System;
using Xamarin.Forms;

namespace XamarinApp
{
	public partial class UpcomingAppointmentsPage : ContentPage
	{
		public UpcomingAppointmentsPage ()
		{
			InitializeComponent ();
		}

		async void OnBackButtonClicked (object sender, EventArgs e)
		{
			await Navigation.PopAsync ();
		}
	}
}

