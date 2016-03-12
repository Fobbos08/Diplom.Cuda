using MetriCam;
using System;
using System.Collections.Generic;
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
//using System.Windows.Shapes;
using System.Windows.Forms;
using Common;

using System.Drawing;
using System.Windows.Media.Imaging;
using System.IO;

namespace Diplo.Cuda.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Timer _t;
        WebCam camera;
        public MainWindow()
        {
            InitializeComponent();
            _t = new Timer();
            _t.Interval = 30;
            _t.Tick += Tick;
            camera = new WebCam();
            camera.Connect();
            _t.Start();
        }

        private void Tick(object sender, EventArgs e)
        {
            
            var bitmap = camera.GetBitmap();
            //var img = BitmapToImageSource();
            var GPUTransferRes = Class1.GPUTransfer(GetArray(bitmap));

            var bitmap2 = GetBitmap(GPUTransferRes, bitmap.Width, bitmap.Height);

            // var img = BitmapToImageSource(GetBitmap(GPUTransferRes, bitmap.Width, bitmap.Height));
            var img = BitmapToImageSource(bitmap2);
            this.ImageBox.Source = img;
        }
        private Bitmap GetBitmap(byte[] rgbValues, int width, int height)
        {
            Bitmap img = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            Rectangle rect = new Rectangle(0, 0, img.Width, img.Height);
            System.Drawing.Imaging.BitmapData bmpData =
              img.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, img.PixelFormat);

            IntPtr ptr = bmpData.Scan0;
            int bytes = Math.Abs(bmpData.Stride) * img.Height;

            // Copy the RGB values into the array.
            //System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);

            // do something with the array

            // Copy the RGB values back to the bitmap
            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes);

            img.UnlockBits(bmpData);
            return img;
        }
        private byte[] GetArray(Bitmap img)
        {
            Rectangle rect = new Rectangle(0, 0, img.Width, img.Height);
            System.Drawing.Imaging.BitmapData bmpData =
              img.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, img.PixelFormat);

            IntPtr ptr = bmpData.Scan0;
            int bytes = Math.Abs(bmpData.Stride) * img.Height;
            byte[] rgbValues = new byte[bytes];

            // Copy the RGB values into the array.
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);

            // do something with the array

            // Copy the RGB values back to the bitmap
            //System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes);

            img.UnlockBits(bmpData);
            return rgbValues;
        }
        BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
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
