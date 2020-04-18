using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FastFileSend.WPF.Controls
{
    /// <summary>
    /// Interaction logic for VerticalProgressBarImage.xaml
    /// </summary>
    public partial class CustomProgress : UserControl
    {
        public CustomProgress()
        {
            InitializeComponent();
            //UpdateImage();
        }

        public static readonly DependencyProperty ProgressProperty = DependencyProperty.Register("Progress", typeof(double), typeof(CustomProgress), new PropertyMetadata(OnPropertyChanged));
        public static readonly DependencyProperty UploadProperty = DependencyProperty.Register("Uploading", typeof(bool), typeof(CustomProgress), new PropertyMetadata(OnPropertyChanged));

        public double Progress
        {
            get => (double)GetValue(ProgressProperty);
            set => SetValue(ProgressProperty, value);
        }

        public bool Uploading
        {
            get => (bool)GetValue(UploadProperty);
            set => SetValue(UploadProperty, value);
        }

        static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as CustomProgress).UpdateImage();
        }

        void UpdateImage()
        {
            DrawProgress(Progress, Uploading);
        }

        void DrawProgress(double progress, bool uploading)
        {
            Bitmap backgroundImage = uploading ? ControlsResources.upload_empty : ControlsResources.download_empty;
            Bitmap foregroundImage = uploading ? ControlsResources.upload_full : ControlsResources.download_full;

            int squareSize = backgroundImage.Width;
            int progressImageSize = (int)(squareSize * progress);

            if (progress > 0 && progressImageSize > 0)
            {
                System.Drawing.Rectangle targetRect = new System.Drawing.Rectangle(0, 0, squareSize, progressImageSize);

                int heightOffset = 0;
                if (uploading)
                {
                    heightOffset = squareSize - progressImageSize;
                    targetRect.Y = heightOffset;
                    targetRect.Height = squareSize - heightOffset;
                    //heightOffset = (int)(squareSize * (1 - progress));
                    //targetRect = new System.Drawing.Rectangle(0, heightOffset, squareSize, squareSize - heightOffset);
                }

                Bitmap foregroundImagePart = foregroundImage.Clone(targetRect, foregroundImage.PixelFormat);

                using (Graphics graphics = Graphics.FromImage(backgroundImage))
                {
                    graphics.DrawImage(foregroundImagePart, 0, heightOffset);
                }
            }

            ProgressImage.Source = BitmapToImageSource(backgroundImage);
        }

        BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }
    }
}
