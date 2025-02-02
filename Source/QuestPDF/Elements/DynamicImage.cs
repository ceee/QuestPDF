﻿using System;
using System.Security;
using QuestPDF.Drawing;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SkiaSharp;

namespace QuestPDF.Elements
{
    public delegate byte[] GenerateDynamicImageDelegate(ImageSize size);
    
    internal class DynamicImage : Element
    {
        internal int? TargetDpi { get; set; }
        internal ImageCompressionQuality? CompressionQuality { get; set; }
        public GenerateDynamicImageDelegate? Source { get; set; }
        
        internal override SpacePlan Measure(Size availableSpace)
        {
            return availableSpace.IsNegative() 
                ? SpacePlan.Wrap() 
                : SpacePlan.FullRender(availableSpace);
        }

        internal override void Draw(Size availableSpace)
        {
            var targetResolution = GetTargetResolution(availableSpace, TargetDpi.Value);
            var imageData = Source?.Invoke(targetResolution);
            
            if (imageData == null)
                return;

            using var originalImage = SKImage.FromEncodedData(imageData);
            using var compressedImage = originalImage.CompressImage(CompressionQuality.Value);

            var targetImage = Helpers.Helpers.GetImageWithSmallerSize(originalImage, compressedImage);
            Canvas.DrawImage(targetImage, Position.Zero, availableSpace);
        }

        private static ImageSize GetTargetResolution(Size availableSize, int targetDpi)
        {
            var scalingFactor = targetDpi / (float)DocumentSettings.DefaultRasterDpi;

            return new ImageSize(
                (int)(availableSize.Width * scalingFactor),
                (int)(availableSize.Height * scalingFactor)
            );
        }
    }
}
