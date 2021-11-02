using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace CodeTime
{
    internal class ImageManager
    {
        public static Image CreateImage(string iconName)
        {
            // create Image
            Image image = new Image();
            image.Width = 16;
            image.Height = 16;

            BitmapImage bi = new BitmapImage();
            // BitmapImage.UriSource must be in a BeginInit/EndInit block.
            bi.BeginInit();
            bi.UriSource = new Uri(@"../Resources/" + iconName, UriKind.RelativeOrAbsolute);
            bi.EndInit();
            // Set the image source.
            image.Source = bi;

            return image;
        }
    }
}
