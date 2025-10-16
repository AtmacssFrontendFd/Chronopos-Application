using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using ChronoPos.Desktop.ViewModels;

namespace ChronoPos.Desktop.Views
{
    public partial class OnboardingWindow : Window
    {
        private readonly OnboardingViewModel _viewModel;


        private Controls.CameraPreviewControl? _cameraPreviewControl;
        private bool _cameraInitialized = false;

        public OnboardingWindow(OnboardingViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;

            _viewModel.OnboardingCompleted += OnOnboardingCompleted;

            Loaded += OnboardingWindow_Loaded;
            Unloaded += OnboardingWindow_Unloaded;
        }

        private void OnboardingWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _cameraPreviewControl = this.FindName("CameraPreview") as Controls.CameraPreviewControl;
            if (_cameraPreviewControl != null)
            {
                _cameraPreviewControl.QRCodeScanned += CameraPreviewControl_QRCodeScanned;
            }
        }

        private void OnboardingWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_cameraPreviewControl != null)
            {
                _cameraPreviewControl.QRCodeScanned -= CameraPreviewControl_QRCodeScanned;
                _cameraPreviewControl.StopCamera();
            }
        }

        private void CameraPreviewControl_QRCodeScanned(string qrData)
        {
            Dispatcher.Invoke(() =>
            {
                if (!string.IsNullOrWhiteSpace(qrData))
                {
                    _viewModel.QrCodeData = qrData;
                    _viewModel.ProcessScratchCardCommand.Execute(null);
                    StopCameraButton_Click(null, null);
                }
            });
        }

        private void OnOnboardingCompleted()
        {
            DialogResult = true;
            Close();
        }

        private void StartCameraButton_Click(object sender, RoutedEventArgs e)
        {
            if (_cameraPreviewControl != null && !_cameraPreviewControl.IsCameraRunning)
            {
                try
                {
                    _cameraPreviewControl.StartCamera();
                    _cameraInitialized = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Camera error: {ex.Message}", "Camera Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void StopCameraButton_Click(object? sender, RoutedEventArgs? e)
        {
            if (_cameraPreviewControl != null && _cameraPreviewControl.IsCameraRunning)
            {
                _cameraPreviewControl.StopCamera();
                _cameraInitialized = false;
            }
        }

        private void SubmitManualEntry_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ProcessScratchCardCommand.Execute(null);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to exit the setup wizard? ChronoPOS requires activation to continue.",
                "Exit Setup",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                System.Windows.Application.Current.Shutdown();
            }
        }

        private void StartStandalone_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _viewModel.StartStandaloneSetupCommand.Execute(null);
        }

        private void StartHostConnection_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _viewModel.StartHostConnectionCommand.Execute(null);
        }

        protected override void OnClosed(EventArgs e)
        {
            if (_cameraPreviewControl != null)
            {
                _cameraPreviewControl.QRCodeScanned -= CameraPreviewControl_QRCodeScanned;
                _cameraPreviewControl.StopCamera();
            }
            _viewModel.OnboardingCompleted -= OnOnboardingCompleted;
            base.OnClosed(e);
        }
    }
}
