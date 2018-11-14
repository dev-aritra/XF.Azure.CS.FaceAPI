using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace XF.Azure.CS.FaceAPI.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LandingPage : ContentPage
    {
        public LandingPage()
        {
            InitializeComponent();
        }

        private async void CaptureImage(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CaptureImagePage());
        }
    }
}