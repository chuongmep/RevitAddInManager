using System;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AddInManager.Model
{
    public static class BitmapSourceConverter
    {
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        /// <summary>
        /// Convert a bitmap image to bit map source
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static BitmapSource ConvertFromImage(Bitmap image)
        {
            IntPtr hBitmap = image.GetHbitmap();

            try
            {
                var bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap,
                    IntPtr.Zero,
                    System.Windows.Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());

                return bs;

            }
            finally
            {
                DeleteObject(hBitmap);
            }

        }
        /// <summary>
        /// Convert icon to bit map source
        /// </summary>
        /// <param name="icon"></param>
        /// <returns></returns>
        public static BitmapSource ConvertFromIcon(Icon icon)
        {

            try
            {
                var bs = Imaging
                    .CreateBitmapSourceFromHIcon(icon.Handle,
                                                 new Int32Rect(0, 0, icon.Width, icon.Height),
                                                 BitmapSizeOptions.FromWidthAndHeight(icon.Width, icon.Height));
                return bs;
            }
            finally
            {
                DeleteObject(icon.Handle);
            }
        }

        /// <summary>
        /// Resize ImageResource
        /// </summary>
        /// <param name="imageSource"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static ImageSource Resize(this ImageSource imageSource, int size)
        {
            return Thumbnail(imageSource, size);
        }

        private static ImageSource Thumbnail(ImageSource source, int size)
        {
            Rect rect = new Rect(0, 0, size, size);
            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                drawingContext.DrawImage(source, rect);
            }
            RenderTargetBitmap resizedImage = new RenderTargetBitmap((int)rect.Width, (int)rect.Height, 96, 96, PixelFormats.Default);
            resizedImage.Render(drawingVisual);

            return resizedImage;
        }
    }
}