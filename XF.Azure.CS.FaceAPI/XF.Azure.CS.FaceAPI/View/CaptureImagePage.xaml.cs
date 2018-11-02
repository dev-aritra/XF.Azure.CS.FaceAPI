using Acr.UserDialogs;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using Plugin.Media;
using Plugin.Media.Abstractions;
using SkiaSharp;
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
        private string lastResult;
        private FaceRectangle faceRectangle;

        public CaptureImagePage()
        {
            InitializeComponent();
            InitOperation();
        }

        private async void InitOperation()
        {
            InitializeFaceClient();
            await CrossMedia.Current.Initialize();
            TakePictureAndAnalizeImage();
        }



        private string FindDetectedEmotion(List<Emotion> emotions)
        {
            lastResult = string.Empty;
            double max = 0;
            PropertyInfo prop = null;
            try
            {
                var emotionsValues = typeof(Emotion).GetProperties();
                foreach (var property in emotionsValues)
                {
                    
                    var val = (double)property.GetValue(emotions[0]);
                    lastResult += property.Name + ": " + val + "\n";
                    if ( val > max)
                    {
                        max = val;
                        prop = property;
                    }
                }
            }
            catch(Exception ex)
            {

            }
            
            return prop.Name.ToString();
        }

        private async Task<MediaFile> TakePicture()
        {
            MediaFile image = null;     
            if (CrossMedia.Current.IsCameraAvailable && CrossMedia.Current.IsTakePhotoSupported)
            {

                image = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
                {
                    
                    RotateImage = false,
                    DefaultCamera = Plugin.Media.Abstractions.CameraDevice.Front,
                    Directory = "FaceAPI",
                    Name = "face.jpg"
                });
            }
            else
            {
                await DisplayAlert("No Camera", ":( No camera available.", "OK");
            }
            return image;

        }

        private void SetImageInImageView(MediaFile image)
        {
            capturedImage.Source = ImageSource.FromStream(() => {
                return image.GetStream();
            });
        }

        private async Task<List<Emotion>> GetFaces(MediaFile image)
        {
            List<Emotion> faces = null;
            try
            {
                
                var faceApiResponseList = await faceClient.Face.DetectWithStreamAsync(image.GetStream(), returnFaceAttributes: new List<FaceAttributeType> { { FaceAttributeType.Emotion } });
                //faceRectangle = faceApiResponseList[0].FaceRectangle;
                //SkCanvasView.InvalidateSurface();
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

        private void OnClickTapped(object sender, EventArgs e)
        {
            
            TakePictureAndAnalizeImage();
            
        }
        private void OnDetailsTapped(object sender, EventArgs e)
        {
            UserDialogs.Instance.Alert(lastResult, "Raw", "Ok");
        }
        

        private void ResetResultLabel()
        {
            result.Text = "Detected emotion: ";
        }

        private async void TakePictureAndAnalizeImage()
        {
            ResetResultLabel();
            var capturedImage = await TakePicture();
            if (capturedImage != null)
            {
                ShowProgressDialog();
                SetImageInImageView(capturedImage);
                var emotions = await GetFaces(capturedImage);
                var detectedEmotion = FindDetectedEmotion(emotions);
                result.Text += detectedEmotion;
                HideProgressDialog();
            }
        }

        private void ShowProgressDialog()
        {

            UserDialogs.Instance.ShowLoading("Analysing", MaskType.Black);
        }
        private void HideProgressDialog()
        {

            UserDialogs.Instance.HideLoading();
        }

        

        private void SkCanvasView_PaintSurface(object sender, SkiaSharp.Views.Forms.SKPaintSurfaceEventArgs e)
        {
            SKImageInfo skImageInfo = e.Info;
            SKSurface skSurface = e.Surface;
            SKCanvas skCanvas = skSurface.Canvas;

            skCanvas.Clear(SKColors.Transparent);

            var skCanvasWidth = skImageInfo.Width;
            var skCanvasheight = skImageInfo.Height;

            // move canvas X,Y to center of screen
            skCanvas.Translate((float)skCanvasWidth / 2, (float)skCanvasheight / 2);
            // set the pixel scale of the canvas
            skCanvas.Scale(skCanvasWidth / 200f);
            
            Draw_Rectangle(skCanvas);
        }


        private void Draw_Rectangle(SKCanvas skCanvas)
        {
            try
            {
                SKPaint skPaint = new SKPaint()
                {
                    Style = SKPaintStyle.Stroke,
                    Color = SKColors.DeepPink,
                    StrokeWidth = 10,
                    IsAntialias = true,
                };

                SKRect skRectangle = new SKRect();
                skRectangle.Size = new SKSize(faceRectangle.Width, faceRectangle.Height);
                skRectangle.Location = new SKPoint(-100f / 2, -100f / 2);

                skCanvas.DrawRect(skRectangle, skPaint);
            }
            catch(Exception ex)
            {

            }
            // Draw Rectangle
            
        }





    }
}