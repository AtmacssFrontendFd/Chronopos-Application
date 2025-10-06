using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using AForge.Video;
using AForge.Video.DirectShow;
using ZXing;
using ZXing.Common;
using ZXing.Windows.Compatibility;

namespace ChronoPos.Desktop.Views.Controls
{
    public class CameraPreviewControl : System.Windows.Controls.Image, IDisposable
    {
        private VideoCaptureDevice? _videoSource;
        private FilterInfoCollection? _videoDevices;
        private BarcodeReader _barcodeReader;
        private DateTime _lastScanTime = DateTime.MinValue;
        private string? _lastScannedData = null;
        
        public event Action<string>? QRCodeScanned;
        public bool IsCameraRunning => _videoSource != null && _videoSource.IsRunning;

        public CameraPreviewControl()
        {
            // Enhanced barcode reader configuration for better QR code detection
            _barcodeReader = new BarcodeReader
            {
                AutoRotate = true,
                Options = new DecodingOptions 
                { 
                    TryHarder = true,
                    PossibleFormats = new[] { BarcodeFormat.QR_CODE, BarcodeFormat.DATA_MATRIX },
                    TryInverted = true,
                    PureBarcode = false,
                    CharacterSet = "UTF-8"
                }
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
            
            // Set higher resolution for better QR code scanning
            if (_videoSource.VideoCapabilities.Length > 0)
            {
                var capability = _videoSource.VideoCapabilities[_videoSource.VideoCapabilities.Length - 1];
                _videoSource.VideoResolution = capability;
            }
            
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

        private Bitmap PreprocessImage(Bitmap original)
        {
            // Create a copy to work with
            var processed = new Bitmap(original.Width, original.Height);
            
            using (Graphics g = Graphics.FromImage(processed))
            {
                // High quality settings
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                
                // Draw the original
                g.DrawImage(original, 0, 0, original.Width, original.Height);
            }
            
            // Increase contrast for better QR code detection
            using (var attributes = new ImageAttributes())
            {
                var contrast = 1.3f;
                var brightness = 0.05f;
                
                float[][] colorMatrix = {
                    new float[] {contrast, 0, 0, 0, 0},
                    new float[] {0, contrast, 0, 0, 0},
                    new float[] {0, 0, contrast, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {brightness, brightness, brightness, 0, 1}
                };
                
                attributes.SetColorMatrix(new System.Drawing.Imaging.ColorMatrix(colorMatrix));
                
                using (Graphics g = Graphics.FromImage(processed))
                {
                    g.DrawImage(processed, 
                        new Rectangle(0, 0, processed.Width, processed.Height),
                        0, 0, processed.Width, processed.Height,
                        GraphicsUnit.Pixel, attributes);
                }
            }
            
            return processed;
        }

        private void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                using (var bitmap = (Bitmap)eventArgs.Frame.Clone())
                {
                    // Try multiple approaches for better detection
                    Result? result = null;
                    
                    // First attempt: Original image
                    result = _barcodeReader.Decode(bitmap);
                    
                    // Second attempt: Preprocessed image for better contrast
                    if (result == null)
                    {
                        using (var processed = PreprocessImage(bitmap))
                        {
                            result = _barcodeReader.Decode(processed);
                        }
                    }
                    
                    // Third attempt: Grayscale conversion
                    if (result == null)
                    {
                        using (var grayBitmap = ConvertToGrayscale(bitmap))
                        {
                            result = _barcodeReader.Decode(grayBitmap);
                        }
                    }
                    
                    if (result != null && !string.IsNullOrWhiteSpace(result.Text))
                    {
                        // Prevent duplicate scans within 2 seconds
                        if ((DateTime.Now - _lastScanTime).TotalSeconds > 2 || _lastScannedData != result.Text)
                        {
                            _lastScanTime = DateTime.Now;
                            _lastScannedData = result.Text;
                            
                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                QRCodeScanned?.Invoke(result.Text);
                            });
                        }
                    }
                    
                    // Update preview
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
                            bmp.Dispose();
                        }
                    });
                }
            }
            catch { }
        }

        private Bitmap ConvertToGrayscale(Bitmap original)
        {
            var grayscale = new Bitmap(original.Width, original.Height);
            
            using (Graphics g = Graphics.FromImage(grayscale))
            {
                var colorMatrix = new System.Drawing.Imaging.ColorMatrix(
                    new float[][]
                    {
                        new float[] {0.299f, 0.299f, 0.299f, 0, 0},
                        new float[] {0.587f, 0.587f, 0.587f, 0, 0},
                        new float[] {0.114f, 0.114f, 0.114f, 0, 0},
                        new float[] {0, 0, 0, 1, 0},
                        new float[] {0, 0, 0, 0, 1}
                    });
                
                using (var attributes = new ImageAttributes())
                {
                    attributes.SetColorMatrix(colorMatrix);
                    g.DrawImage(original,
                        new Rectangle(0, 0, original.Width, original.Height),
                        0, 0, original.Width, original.Height,
                        GraphicsUnit.Pixel, attributes);
                }
            }
            
            return grayscale;
        }

        public void Dispose()
        {
            StopCamera();
        }
    }
}
