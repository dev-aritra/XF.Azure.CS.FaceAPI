using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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

        private Stream capturedImageStream;
        public CaptureImagePage()
        {
            InitializeComponent();
            InitOperation();


        }

        private async void InitOperation()
        {
            InitializeFaceClient();
            var capturedImage = await InitCamera();
            if (capturedImage != null)
            {
                var emotions = await GetFaces(capturedImage);
                var detectedEmotion = FindDetectedEmotion(emotions);
            }
        }

        private string FindDetectedEmotion(List<Emotion> emotions)
        {
            float max = 0;
            PropertyInfo prop = null;
            try
            {
                var emotionsValues = typeof(Emotion).GetProperties();
                foreach (var property in emotionsValues)
                {
                    if ((double)property.GetValue(emotions[0]) > max)
                    {
                        max = (float)property.GetValue(emotions[0]);
                        prop = property;
                    }
                }
            }
            catch(Exception ex)
            {

            }
            
            return prop.PropertyType.ToString();
        }

        private async Task<MediaFile> InitCamera()
        {
            MediaFile image = null;
            await CrossMedia.Current.Initialize();
            if (CrossMedia.Current.IsCameraAvailable && CrossMedia.Current.IsTakePhotoSupported)
            {

                image = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
                {
                    RotateImage = false,
                    DefaultCamera = Plugin.Media.Abstractions.CameraDevice.Front,
                    Directory = "FaceAPI",
                    Name = "face.jpg"
                });
                if (image != null)
                {
                    //capturedImage.Source = ImageSource.FromStream(() =>
                    //{
                    //    var stream = file.GetStream();
                    //    imageStream = stream;
                    //    return stream;
                    //});
                    
                    capturedImage.Source = ImageSource.FromStream(() => {
                        return image.GetStream();
                    });
                }
                else
                {
                    await DisplayAlert("No Camera", ":( No camera available.", "OK");
                    
                }




            }
            else
            {
                

            }
            return image;

        }

        private async Task<List<Emotion>> GetFaces(MediaFile image)
        {
            List<Emotion> faces = null;
            try
            {
                
                var faceApiResponseList = await faceClient.Face.DetectWithStreamAsync(image.GetStream(), returnFaceAttributes: new List<FaceAttributeType> { { FaceAttributeType.Emotion } });
                faces = faceApiResponseList.Select(x => x.FaceAttributes.Emotion).ToList();
                
            }
            catch(Exception ex)
            {
                
            }
            return faces;
        }





        private void InitializeFaceClient()
        {
            credentials = new ApiKeyServiceClientCredentials("50c78f754e734425adca685ee646701b");
            faceClient = new FaceClient(credentials);
            faceClient.Endpoint = "https://centralindia.api.cognitive.microsoft.com/";
            faceOperations = new FaceOperations(faceClient);
        }
    }
}