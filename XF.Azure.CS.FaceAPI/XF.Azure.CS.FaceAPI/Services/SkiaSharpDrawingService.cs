﻿using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using SkiaSharp;
using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace XF.Azure.CS.FaceAPI.Services
{
    public class SkiaSharpDrawingService
    {
        public void ClearCanvas(SKImageInfo info, SKCanvas canvas)
        {
            var paint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = SKColors.White
            };

            canvas.DrawRect(info.Rect, paint);
        }

        public void DrawPrediction(SKCanvas canvas, FaceRectangle box, float left, float top, float scale, string emotion, bool showEmoji)
        {
            var scaledBoxLeft = left + (scale * box.Left);
            var scaledBoxWidth = scale * box.Width;
            var scaledBoxTop = top + (scale * box.Top);
            var scaledBoxHeight = scale * box.Height;
            if(showEmoji)
            {
                SKBitmap Image = GetEmojiBitmap(emotion);
                canvas.DrawBitmap(Image, new SKRect(scaledBoxLeft, scaledBoxTop, scaledBoxLeft + scaledBoxWidth, scaledBoxTop + scaledBoxHeight));
            }
            else
            {
                DrawBox(canvas, scaledBoxLeft, scaledBoxTop, scaledBoxWidth, scaledBoxHeight);
                DrawText(canvas, emotion, scaledBoxLeft, scaledBoxTop, scaledBoxWidth, scaledBoxHeight);
            }


        }


        public void DrawEmotiocon(SKImageInfo info, SKCanvas canvas, string emotion)
        {

            SKBitmap Image = GetEmojiBitmap(emotion);
            var scale = Math.Min(info.Width / (float)Image.Width, info.Height / (float)Image.Height);

            var scaleHeight = scale * Image.Height;
            var scaleWidth = scale * Image.Width;

            var top = (info.Height - scaleHeight) / 2;
            var left = (info.Width - scaleWidth) / 2;

            canvas.DrawBitmap(Image, new SKRect(left, top, left + scaleWidth, top + scaleHeight));
        }

        private SKBitmap GetEmojiBitmap(string emotion)
        {
            string resourceID = GetImageResourceId(emotion).ToString();
            Assembly assembly = GetType().GetTypeInfo().Assembly;
            SKBitmap resourceBitmap = null;
            using (Stream stream = assembly.GetManifestResourceStream(resourceID))
            {
                resourceBitmap = SKBitmap.Decode(stream);
            }
            return resourceBitmap;
        }

        private StringBuilder GetImageResourceId(string emotion)
        {
            StringBuilder resId = new StringBuilder("XF.Azure.CS.FaceAPI.Emojis.");
            switch (emotion)
            {
                case "Anger":
                    resId.Append("angry");
                    break;
                case "Contempt":
                    resId.Append("dislike");
                    break;
                case "Disgust":
                    resId.Append("disgust");
                    break;
                case "Fear":
                    resId.Append("fear");
                    break;
                case "Happiness":
                    resId.Append("happy");
                    break;
                case "Neutral":
                    resId.Append("neutral");
                    break;
                case "Sadness":
                    resId.Append("sad");
                    break;
                case "Surprise":
                    resId.Append("surprised");
                    break;

            }

            resId.Append(".png");
            return resId;
        }

        private SKPath CreateBoxPath(float startLeft, float startTop, float scaledBoxWidth, float scaledBoxHeight)
        {
            var path = new SKPath();
            path.MoveTo(startLeft, startTop);

            path.LineTo(startLeft + scaledBoxWidth, startTop);
            path.LineTo(startLeft + scaledBoxWidth, startTop + scaledBoxHeight);
            path.LineTo(startLeft, startTop + scaledBoxHeight);
            path.LineTo(startLeft, startTop);

            return path;
        }

        private void DrawBox(SKCanvas canvas, SKPaint paint, float startLeft, float startTop, float scaledBoxWidth, float scaledBoxHeight)
        {
            var path = CreateBoxPath(startLeft, startTop, scaledBoxWidth, scaledBoxHeight);
            canvas.DrawPath(path, paint);
        }

        private void DrawBox(SKCanvas canvas, float startLeft, float startTop, float scaledBoxWidth, float scaledBoxHeight)
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

        private void DrawText(SKCanvas canvas, string tag, float startLeft, float startTop, float scaledBoxWidth, float scaledBoxHeight)
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
            textPaint.TextSize = 0.5f * scaledBoxWidth * textPaint.TextSize / textWidth;

            var textBounds = new SKRect();
            textPaint.MeasureText(text, ref textBounds);

            var xText = startLeft;
            var yText = startTop + scaledBoxHeight;

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
    }
}