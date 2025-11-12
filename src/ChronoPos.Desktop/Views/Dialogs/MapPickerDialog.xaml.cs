using System.Windows;

namespace ChronoPos.Desktop.Views.Dialogs;

public partial class MapPickerDialog : Window
{
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public bool LocationSelected { get; private set; }

    public MapPickerDialog(decimal? currentLatitude = null, decimal? currentLongitude = null)
    {
        InitializeComponent();
        DataContext = this;

        // Set initial values if provided
        if (currentLatitude.HasValue)
            Latitude = currentLatitude.Value;
        if (currentLongitude.HasValue)
            Longitude = currentLongitude.Value;

        // Update textboxes
        LatitudeTextBox.Text = Latitude?.ToString("F6") ?? string.Empty;
        LongitudeTextBox.Text = Longitude?.ToString("F6") ?? string.Empty;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        LocationSelected = false;
        DialogResult = false;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        LocationSelected = false;
        DialogResult = false;
        Close();
    }

    private void ConfirmButton_Click(object sender, RoutedEventArgs e)
    {
        // Parse the coordinates from textboxes
        if (decimal.TryParse(LatitudeTextBox.Text, out decimal lat) &&
            decimal.TryParse(LongitudeTextBox.Text, out decimal lng))
        {
            // Validate coordinates
            if (lat >= -90 && lat <= 90 && lng >= -180 && lng <= 180)
            {
                Latitude = lat;
                Longitude = lng;
                LocationSelected = true;
                DialogResult = true;
                Close();
            }
            else
            {
                new MessageDialog(
                    "Invalid Input",
                    "Invalid coordinates!\n\nLatitude must be between -90 and 90\nLongitude must be between -180 and 180",
                    MessageDialog.MessageType.Warning).ShowDialog();
            }
        }
        else
        {
            new MessageDialog(
                "Invalid Input",
                "Please enter valid numeric coordinates.",
                MessageDialog.MessageType.Warning).ShowDialog();
        }
    }

    private void SetDubaiLocation_Click(object sender, RoutedEventArgs e)
    {
        LatitudeTextBox.Text = "25.2048";
        LongitudeTextBox.Text = "55.2708";
    }

    private void SetAbuDhabiLocation_Click(object sender, RoutedEventArgs e)
    {
        LatitudeTextBox.Text = "24.4539";
        LongitudeTextBox.Text = "54.3773";
    }
}
