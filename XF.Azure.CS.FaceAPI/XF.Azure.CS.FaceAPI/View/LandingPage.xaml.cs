using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace XF.Azure.CS.FaceAPI.View
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class LandingPage : ContentPage
	{
		public LandingPage ()
		{
			InitializeComponent ();
		}

        private async void CaptureImage(object sender, EventArgs e)
        {
            
            await Navigation.PushAsync(new CaptureImagePage());
        }

        private async void SelectFromGallery(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SelectFromGalleryPage());
        }
    }
}