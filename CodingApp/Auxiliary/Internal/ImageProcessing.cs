using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CodingApp.Auxiliary.Internal {
    public static class ImageProcessing {
        public static byte[] BitmapSourceToPixelData(BitmapSource bitmap) {
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            int stride = width * (bitmap.Format.BitsPerPixel / 8);

            var pixels = new byte[height * stride];

            bitmap.CopyPixels(pixels, stride, 0);
            return pixels;
        }

        public static BitmapSource PixelDataToBitmapSource(byte[] pixels, BitmapSource bitmapInfo) {
            int width = bitmapInfo.PixelWidth;
            int height = bitmapInfo.PixelHeight;
            double dpiX = bitmapInfo.DpiX;
            double dpiY = bitmapInfo.DpiY;
            int stride = width * (bitmapInfo.Format.BitsPerPixel / 8);
            PixelFormat pixelFormat = bitmapInfo.Format;

            WriteableBitmap bitmap = new WriteableBitmap(width, height, dpiX, dpiY, pixelFormat, null);
            try {
                bitmap.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
            }
            catch (ArgumentException) {
                bitmap.WritePixels(new Int32Rect(0, 0, width, height), pixels, width * (bitmapInfo.Format.BitsPerPixel + 7 / 8), 0);
            }

            return bitmap;
        }
    }
}
