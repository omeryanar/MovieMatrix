using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace MovieMatrix.View
{
    public partial class ImageGalleryView : UserControl
    {
        public ImageGalleryView()
        {
            InitializeComponent();
        }

        public void SetWindowSize(BitmapImage image)
        {
            image.DownloadCompleted += (s, e) =>
            {
                Window window = Window.GetWindow(this);

                double screenWidth = SystemParameters.PrimaryScreenWidth - 100;
                double screenHeight = SystemParameters.PrimaryScreenHeight - 100;

                double imageWidth = image.Width;
                double imageHeight = image.Height;

                double aspectRatio = imageWidth / imageHeight;
                double widthRate = imageWidth / screenWidth;
                double heightRate = imageHeight / screenHeight;

                if (widthRate <= 1 && heightRate <= 1)
                {
                    window.Width = imageWidth;
                    window.Height = imageHeight;
                }
                else if (widthRate >= 1 && heightRate <= 1)
                {
                    window.Width = screenWidth;
                    window.Height = window.Width / aspectRatio;
                }
                else if (widthRate <= 1 && heightRate >= 1)
                {
                    window.Height = screenHeight;
                    window.Width = window.Height * aspectRatio;
                }
                else if (widthRate >= 1 && heightRate >= 1)
                {
                    if (widthRate > heightRate)
                    {
                        window.Width = screenWidth;
                        window.Height = window.Width / aspectRatio;
                    }
                    else
                    {
                        window.Height = screenHeight;
                        window.Width = window.Height * aspectRatio;
                    }
                }

                window.Top = (screenHeight - window.Height + 100) / 2;
                window.Left = (screenWidth - window.Width + 100) / 2;
            };
        }
    }
}
