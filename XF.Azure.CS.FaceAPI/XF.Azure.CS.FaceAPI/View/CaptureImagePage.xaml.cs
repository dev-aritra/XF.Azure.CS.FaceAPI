using Acr.UserDialogs;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using Plugin.Media;
using Plugin.Media.Abstractions;
using SkiaSharp;
using System;
using System.Collections.Generic;
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

        private List<DetectedFaceExtended> detectedFaces;
        private SKBitmap Image;

        public CaptureImagePage()
        {
            InitializeComponent();
            InitOperation();
        }

        private async void InitOperation()
        {
            detectedFaces = new List<DetectedFaceExtended>();
            InitializeFaceClient();
            await CrossMedia.Current.Initialize();
            TakePictureAndAnalizeImage();
        }



        private string FindDetectedEmotion(Emotion emotion)
        {
            
            double max = 0;
            PropertyInfo prop = null;
            try
            {
                var emotionsValues = typeof(Emotion).GetProperties();
                foreach (var property in emotionsValues)
                {

                    var val = (double)property.GetValue(emotion);
                    
                    if (val > max)
                    {
                        max = val;
                        prop = property;
                    }
                }
            }
            catch (Exception ex)
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
                    PhotoSize = PhotoSize.Medium,
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
            Image = SKBitmap.Decode(image.GetStreamWithImageRotatedForExternalStorage());
            capturedImage.InvalidateSurface();
        }

        private async Task<bool> GetFaces(MediaFile image)
        {
            bool facesFound = false;
            try
            {

                var faceApiResponseList = await faceClient.Face.DetectWithStreamAsync(image.GetStream(), returnFaceAttributes: new List<FaceAttributeType> { { FaceAttributeType.Emotion } });
                if (detectedFaces.Count > 0)
                {
                    detectedFaces.Clear();
                }

                DetectedFaceExtended decFace = null;
                if(faceApiResponseList.Count>0)
                {
                    facesFound = true;
                    foreach (var face in faceApiResponseList)
                    {
                        decFace = new DetectedFaceExtended
                        {
                            FaceRectangle = face.FaceRectangle,

                        };
                        decFace.PredominantEmotion = FindDetectedEmotion(face.FaceAttributes.Emotion);
                        detectedFaces.Add(decFace);
                    }
                }
   
            }
            catch (Exception ex)
            {
                HideProgressDialog();
                UserDialogs.Instance.Toast("Could not detect any face");
                
            }
            return facesFound;
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
        


        

        private async void TakePictureAndAnalizeImage()
        {
            
            var capturedImg = await TakePicture();
            if (capturedImg != null)
            {
                ShowProgressDialog();
                SetImageInImageView(capturedImg);
                var facesFound = await GetFaces(capturedImg);
                if (facesFound)
                {
                    capturedImage.InvalidateSurface();
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
            ClearCanvas(info, canvas);
            if (Image != null)
            {
                var scale = Math.Min(info.Width / (float)Image.Width, info.Height / (float)Image.Height);

                var scaleHeight = scale * Image.Height;
                var scaleWidth = scale * Image.Width;

                var top = (info.Height - scaleHeight) / 2;
                var left = (info.Width - scaleWidth) / 2;

                canvas.DrawBitmap(Image, new SKRect(left, top, left + scaleWidth, top + scaleHeight));

                if (detectedFaces.Count > 0)
                {
                    foreach (var face in detectedFaces)
                    {
                        LabelPrediction(canvas, face.FaceRectangle, left, top, scale , face.PredominantEmotion);

                    }

                }
            }
        }



        static void LabelPrediction(SKCanvas canvas, FaceRectangle box, float left, float top, float scale, string emotion)
        {
            var scaledBoxLeft = left + (scale * box.Left);
            var scaledBoxWidth = scale * box.Width;
            var scaledBoxTop = top + (scale * box.Top);
            var scaledBoxHeight = scale * box.Height;


            DrawBox(canvas, scaledBoxLeft, scaledBoxTop, scaledBoxWidth, scaledBoxHeight);
            DrawText(canvas, emotion, scaledBoxLeft, scaledBoxTop, scaledBoxWidth, scaledBoxHeight);

        }

        static void DrawText(SKCanvas canvas, string tag, float startLeft, float startTop, float scaledBoxWidth, float scaledBoxHeight)
        {
            var textPaint = new SKPaint
            {
                IsAntialias = true,
                Color = SKColors.White,
                Style = SKPaintStyle.Fill,
                Typeface = SKTypeface.FromFamilyName("Arial")
            };

            var text = tag;

            var textWidth = textPaint.MeasureText(text);
            textPaint.TextSize = 0.9f * scaledBoxWidth * textPaint.TextSize / textWidth;

            var textBounds = new SKRect();
            textPaint.MeasureText(text, ref textBounds);

            var xText = (startLeft + (scaledBoxWidth / 2)) - textBounds.MidX;
            var yText = (startTop + (scaledBoxHeight / 2)) + textBounds.MidY;

            var paint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = new SKColor(0, 0, 0, 120)
            };

            var backgroundRect = textBounds;
            backgroundRect.Offset(xText, yText);
            backgroundRect.Inflate(10, 10);

            canvas.DrawRoundRect(backgroundRect, 5, 5, paint);

            canvas.DrawText(text,
                            xText,
                            yText,
                            textPaint);
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
                Color = SKColors.Red,
                StrokeWidth = 5,
                PathEffect = SKPathEffect.CreateDash(new[] { 20f, 20f }, 20f)
            };
            DrawBox(canvas, strokePaint, startLeft, startTop, scaledBoxWidth, scaledBoxHeight);

            var blurStrokePaint = new SKPaint
            {
                Color = SKColors.Red,
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