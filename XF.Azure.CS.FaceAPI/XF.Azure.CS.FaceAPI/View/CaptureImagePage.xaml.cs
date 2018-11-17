using Acr.UserDialogs;
using Plugin.Media;
using Plugin.Media.Abstractions;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XF.Azure.CS.FaceAPI.Model;
using XF.Azure.CS.FaceAPI.Services;

namespace XF.Azure.CS.FaceAPI.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CaptureImagePage : ContentPage
    {
        private Lazy<List<DetectedFaceExtended>> detectedFaces = new Lazy<List<DetectedFaceExtended>>();
        private FaceAPIService faceAPIService;
        private SKBitmap Image;
        private SkiaSharpDrawingService skiaDrawingService;
        private bool drawemoji = true;

        public CaptureImagePage()
        {
            InitializeComponent();
            InitOperation();
        }

        private void CapturedImage_PaintSurface(object sender, SkiaSharp.Views.Forms.SKPaintSurfaceEventArgs args)
        {
            var info = args.Info;
            var canvas = args.Surface.Canvas;
            skiaDrawingService.ClearCanvas(info, canvas);
            if (Image != null)
            {
                var scale = Math.Min(info.Width / (float)Image.Width, info.Height / (float)Image.Height);

                var scaleHeight = scale * Image.Height;
                var scaleWidth = scale * Image.Width;

                var top = (info.Height - scaleHeight) / 2;
                var left = (info.Width - scaleWidth) / 2;

                canvas.DrawBitmap(Image, new SKRect(left, top, left + scaleWidth, top + scaleHeight));

                if (detectedFaces.Value.Count > 0)
                {
                    foreach (var face in detectedFaces.Value)
                    {
                        skiaDrawingService.DrawPrediction(canvas, face.FaceRectangle, left, top, scale, face.PredominantEmotion, drawemoji);
                        //skiaDrawingService.DrawEmotiocon(info, canvas, face.PredominantEmotion);
                    }
                }
            }
        }

        private void HideProgressDialog()
        {
            UserDialogs.Instance.HideLoading();
        }

        private async void InitOperation()
        {
            faceAPIService = new FaceAPIService();
            skiaDrawingService = new SkiaSharpDrawingService();
            await CrossMedia.Current.Initialize();
            TakePictureAndAnalizeImage();
        }

        private void OnClickTapped(object sender, EventArgs e)
        {
            TakePictureAndAnalizeImage();
        }

        private void SetImageInImageView(MediaFile image)
        {
            Image = SKBitmap.Decode(image.GetStreamWithImageRotatedForExternalStorage());
            capturedImage.InvalidateSurface();
        }

        private void ShowProgressDialog()
        {
            UserDialogs.Instance.ShowLoading("Analysing", MaskType.Black);
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
        private async void TakePictureAndAnalizeImage()
        {
            var capturedImg = await TakePicture();
            if (detectedFaces.Value.Count > 0)
            {
                detectedFaces.Value.Clear();
            }
            if (capturedImg != null)
            {
                ShowProgressDialog();
                SetImageInImageView(capturedImg);
                try
                {
                    var foundFaces = await faceAPIService.GetFaces(capturedImg);
                    if (foundFaces != null && foundFaces.Count > 0)
                    {
                        
                        detectedFaces.Value.AddRange(foundFaces);
                        capturedImage.InvalidateSurface();
                    }
                    else
                    {
                        UserDialogs.Instance.Toast("Could not detect any face");
                    }

                    HideProgressDialog();
                }
                catch (Exception ex)
                {
                    HideProgressDialog();
                    UserDialogs.Instance.Toast("Could not detect any face");
                }
            }
        }

        private void Switch_Toggled(object sender, ToggledEventArgs e)
        {
            var mode= sender as Switch;
            if(mode.IsToggled)
            {
                drawemoji = true;
                displayMode.Text = "Emoji mode";


            }
            else
            {
                drawemoji = false;
                displayMode.Text = "Label mode";
            }

        }
    }
}