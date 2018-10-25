using Microsoft.Azure.CognitiveServices.Vision.Face;
using Plugin.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace XF.Azure.CS.FaceAPI.View
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class CaptureImagePage : ContentPage
	{
        private ApiKeyServiceClientCredentials credentials;
        private FaceClient faceClient;
        private FaceOperations faceOperations;
		public CaptureImagePage ()
		{
			InitializeComponent ();
            InitializeFaceClient();
            var capturedImage = InitCamera();
            

		}
        private async Task<Stream> InitCamera()
        {
            await CrossMedia.Current.Initialize();
            if (CrossMedia.Current.IsCameraAvailable && CrossMedia.Current.IsTakePhotoSupported)
            {
                var file = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
                {
                    RotateImage = false,
                    DefaultCamera = Plugin.Media.Abstractions.CameraDevice.Front,
                    Directory = "FaceAPI",
                    Name = "face.jpg"
                });

                if (file == null)
                    return null;

                capturedImage.Source = ImageSource.FromStream(() =>
                {
                    var stream = file.GetStream();
                    return stream;
                });
                return file.GetStream();
                
                

            }
            else
            {
                await DisplayAlert("No Camera", ":( No camera available.", "OK");
                return null;
            }

            
        }

        private async Task<dynamic> GetFaces(Stream image)
        {
            var faces = faceOperations.DetectWithStreamAsync(image);
           
            

            return null;
        }

        private void InitializeFaceClient()
        {
            credentials = new ApiKeyServiceClientCredentials("50c78f754e734425adca685ee646701b");
            faceClient = new FaceClient(credentials);
            faceClient.Endpoint = "https://centralindia.api.cognitive.microsoft.com/face/v1.0";
            faceOperations = new FaceOperations(faceClient);
        }
    }
}