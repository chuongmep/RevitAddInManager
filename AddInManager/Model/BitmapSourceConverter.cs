using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RevitAddinManager.Model;

public static class BitmapSourceConverter
{
    public enum ImageType
    {
        Small,
        Large
    }

    public static ImageSource ToImageSource(Bitmap bitmap, ImageType imageType)
    {
        switch (imageType)
        {
            case ImageType.Small:
                return ToImageSource(bitmap).Resize(16);

            case ImageType.Large:
                return ToImageSource(bitmap).Resize(32);

            default:
                throw new ArgumentOutOfRangeException(nameof(imageType), imageType, null);
        }
    }

    public static BitmapImage ToImageSource(Bitmap bitmap)
    {
        var ms = new MemoryStream();
        bitmap.Save(ms, ImageFormat.Png);
        var image = new BitmapImage();
        image.BeginInit();
        ms.Seek(0, SeekOrigin.Begin);
        image.StreamSource = ms;
        image.EndInit();
        return image;
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
        var rect = new Rect(0, 0, size, size);
        var drawingVisual = new DrawingVisual();
        using (var drawingContext = drawingVisual.RenderOpen())
        {
            drawingContext.DrawImage(source, rect);
        }
        var resizedImage = new RenderTargetBitmap((int)rect.Width, (int)rect.Height, 96, 96, PixelFormats.Default);
        resizedImage.Render(drawingVisual);

        return resizedImage;
    }
}