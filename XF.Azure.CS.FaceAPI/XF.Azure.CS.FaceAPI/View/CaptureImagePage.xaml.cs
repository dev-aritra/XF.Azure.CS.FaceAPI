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

        private List<FaceRectangle> faceRectangles;
        private SKBitmap Image;

        public CaptureImagePage()
        {
            InitializeComponent();
            InitOperation();
        }

        private async void InitOperation()
        {
            faceRectangles= new List<FaceRectangle>();
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
            Image = null;
            MediaFile image = null;     
            if (CrossMedia.Current.IsCameraAvailable && CrossMedia.Current.IsTakePhotoSupported)
            {

                image = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
                {
                    PhotoSize=PhotoSize.Medium,
                    RotateImage = true,
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
            Image= SKBitmap.Decode(image.GetStreamWithImageRotatedForExternalStorage());
            capturedImage.InvalidateSurface();
        }

        private async Task<List<Emotion>> GetFaces(MediaFile image)
        {
            List<Emotion> faces = null;
            try
            {
                
                var faceApiResponseList = await faceClient.Face.DetectWithStreamAsync(image.GetStream(), returnFaceAttributes: new List<FaceAttributeType> { { FaceAttributeType.Emotion } });
                faces = faceApiResponseList.Select(x => x.FaceAttributes.Emotion).ToList();
                if(faceRectangles.Count>0)
                {
                    faceRectangles.Clear();
                }
                foreach(var face in faceApiResponseList)
                {
                    faceRectangles.Add(face.FaceRectangle);
                }
                
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
            var capturedImg = await TakePicture();
            if (capturedImg != null)
            {
                ShowProgressDialog();
                SetImageInImageView(capturedImg);
                var emotions = await GetFaces(capturedImg);
                if(emotions.Count>0)
                {
                    capturedImage.InvalidateSurface();
                    var detectedEmotion = FindDetectedEmotion(emotions);
                    result.Text += detectedEmotion;
                }
                
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

        private void CapturedImage_PaintSurface(object sender, SkiaSharp.Views.Forms.SKPaintSurfaceEventArgs args)
        {
            var info = args.Info;
            var canvas = args.Surface.Canvas;
            ClearCanvas(info,canvas);
            if (Image != null)
            {
                var scale = Math.Min((float)info.Width / (float)Image.Width, (float)info.Height / (float)Image.Height);

                var scaleHeight = scale * Image.Height;
                var scaleWidth = scale * Image.Width;

                var top = (info.Height - scaleHeight) / 2;
                var left = (info.Width - scaleWidth) / 2;

                canvas.DrawBitmap(Image, new SKRect(left, top, left + scaleWidth, top + scaleHeight));

                if(faceRectangles.Count>0)
                {
                    foreach(var face in faceRectangles)
                    {
                        LabelPrediction(canvas, face, left, top , scale);
                    }
                    
                }
            }
        }

       

        static void LabelPrediction(SKCanvas canvas, FaceRectangle box, float left, float top, float scale, bool addBox = true)
        {
            var scaledBoxLeft = left + (scale * (float)box.Left);
            var scaledBoxWidth = scale * (float)box.Width;
            var scaledBoxTop = top + (scale * (float)box.Top);
            var scaledBoxHeight = scale * (float)box.Height;

            if (addBox)
                DrawBox(canvas, scaledBoxLeft, scaledBoxTop, scaledBoxWidth, scaledBoxHeight);

            
        }

        static void DrawBox(SKCanvas canvas, SKPaint paint, float startLeft, float startTop, float scaledBoxWidth, float scaledBoxHeight)
        {
            var path = CreateBoxPath(startLeft, startTop, scaledBoxWidth, scaledBoxHeight);
            canvas.DrawPath(path, paint);
        }

        static SKPath CreateBoxPath(float startLeft, float startTop, float scaledBoxWidth, float scaledBoxHeight)
        {
            var path = new SKPath();
            path.MoveTo(startLeft, startTop);

            path.LineTo(startLeft + scaledBoxWidth, startTop);
            path.LineTo(startLeft + scaledBoxWidth, startTop + scaledBoxHeight);
            path.LineTo(startLeft, startTop + scaledBoxHeight);
            path.LineTo(startLeft, startTop);

            return path;
        }
        static void DrawBox(SKCanvas canvas, float startLeft, float startTop, float scaledBoxWidth, float scaledBoxHeight)
        {
            var strokePaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = SKColors.White,
                StrokeWidth = 5,
                PathEffect = SKPathEffect.CreateDash(new[] { 20f, 20f }, 20f)
            };
            DrawBox(canvas, strokePaint, startLeft, startTop, scaledBoxWidth, scaledBoxHeight);

            var blurStrokePaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 5,
                PathEffect = SKPathEffect.CreateDash(new[] { 20f, 20f }, 20f),
                IsAntialias = true,
                MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 0.57735f * 1.0f + 0.5f)
            };
            DrawBox(canvas, blurStrokePaint, startLeft, startTop, scaledBoxWidth, scaledBoxHeight);
        }

        static void ClearCanvas(SKImageInfo info, SKCanvas canvas)
        {
            var paint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = SKColors.White
            };

            canvas.DrawRect(info.Rect, paint);
        }
    }
}