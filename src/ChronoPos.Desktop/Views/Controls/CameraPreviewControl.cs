using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using AForge.Video;
using AForge.Video.DirectShow;
using ZXing;
using ZXing.Windows.Compatibility;

namespace ChronoPos.Desktop.Views.Controls
{
    public class CameraPreviewControl : System.Windows.Controls.Image, IDisposable
    {
        private VideoCaptureDevice? _videoSource;
        private FilterInfoCollection? _videoDevices;
        private BarcodeReader _barcodeReader;
        public event Action<string>? QRCodeScanned;
        public bool IsCameraRunning => _videoSource != null && _videoSource.IsRunning;

        public CameraPreviewControl()
        {
            _barcodeReader = new BarcodeReader
            {
                AutoRotate = true,
                Options = new ZXing.Common.DecodingOptions { TryHarder = true }
            };
            Loaded += CameraPreviewControl_Loaded;
            Unloaded += CameraPreviewControl_Unloaded;
        }

        private void CameraPreviewControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Optionally auto-start camera
        }

        private void CameraPreviewControl_Unloaded(object sender, RoutedEventArgs e)
        {
            StopCamera();
        }

        public void StartCamera()
        {
            _videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (_videoDevices.Count == 0)
                throw new Exception("No camera devices found.");
            _videoSource = new VideoCaptureDevice(_videoDevices[0].MonikerString);
            _videoSource.NewFrame += VideoSource_NewFrame;
            _videoSource.Start();
        }

        public void StopCamera()
        {
            if (_videoSource != null)
            {
                _videoSource.NewFrame -= VideoSource_NewFrame;
                if (_videoSource.IsRunning)
                    _videoSource.SignalToStop();
                _videoSource = null;
            }
        }

        private void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                using (var bitmap = (Bitmap)eventArgs.Frame.Clone())
                {
                    var result = _barcodeReader.Decode(bitmap);
                    if (result != null)
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            QRCodeScanned?.Invoke(result.Text);
                        });
                    }
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        var bmp = bitmap.Clone() as Bitmap;
                        if (bmp != null)
                        {
                            var bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                                bmp.GetHbitmap(),
                                IntPtr.Zero,
                                Int32Rect.Empty,
                                BitmapSizeOptions.FromWidthAndHeight(bmp.Width, bmp.Height));
                            this.Source = bitmapSource;
                        }
                    });
                }
            }
            catch { }
        }

        public void Dispose()
        {
            StopCamera();
        }
    }
}
