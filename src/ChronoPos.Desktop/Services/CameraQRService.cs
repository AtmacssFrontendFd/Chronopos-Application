using System;
using System.Drawing;
using System.Threading.Tasks;
using AForge.Video;
using AForge.Video.DirectShow;
using ZXing;
using ZXing.Windows.Compatibility;

namespace ChronoPos.Desktop.Services
{
    public class CameraQRService : ICameraService
    {
        private VideoCaptureDevice? _videoSource;
        private FilterInfoCollection? _videoDevices;
        private BarcodeReader _barcodeReader = new BarcodeReader
        {
            AutoRotate = true,
            Options = new ZXing.Common.DecodingOptions { TryHarder = true }
        };
        private TaskCompletionSource<string?>? _qrResultTcs;
        private bool _isCameraRunning = false;		public bool IsCameraAvailable()
		{
			_videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
			return _videoDevices.Count > 0;
		}

		public void StartCamera()
		{
			if (_isCameraRunning) return;
			_videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
			if (_videoDevices.Count == 0)
				throw new Exception("No camera devices found.");
			_videoSource = new VideoCaptureDevice(_videoDevices[0].MonikerString);
			_videoSource.NewFrame += VideoSource_NewFrame;
			_videoSource.Start();
			_isCameraRunning = true;
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
			_isCameraRunning = false;
		}

		public async Task<string?> ScanQRCodeAsync()
		{
			_qrResultTcs = new TaskCompletionSource<string?>();
			StartCamera();
			var result = await _qrResultTcs.Task;
			StopCamera();
			return result;
		}

		private void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
		{
			if (_qrResultTcs == null || _qrResultTcs.Task.IsCompleted)
				return;
			try
			{
				using (var bitmap = (Bitmap)eventArgs.Frame.Clone())
				{
					var result = _barcodeReader.Decode(bitmap);
					if (result != null)
					{
						_qrResultTcs.TrySetResult(result.Text);
					}
				}
			}
			catch { }
		}
	}
}
