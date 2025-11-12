using System.Windows;
using System.Windows.Media;

namespace ChronoPos.Desktop.Views.Dialogs;

public partial class ConfirmationDialog : Window
{
    public enum DialogType
    {
        Warning,
        Danger,
        Info,
        Success
    }

    public bool Result { get; private set; }

    public ConfirmationDialog(string title, string message, DialogType type = DialogType.Warning, string confirmText = "Confirm", string cancelText = "Cancel")
    {
        InitializeComponent();
        
        TitleText.Text = title;
        MessageText.Text = message;
        ConfirmButton.Content = confirmText;
        CancelButton.Content = cancelText;
        
        SetDialogType(type);
    }

    private void SetDialogType(DialogType type)
    {
        switch (type)
        {
            case DialogType.Warning:
                IconText.Text = "⚠";
                IconText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F59E0B"));
                IconBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FEF3C7"));
                ConfirmButton.Style = (Style)FindResource("ConfirmButtonStyle");
                break;
                
            case DialogType.Danger:
                IconText.Text = "✕";
                IconText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444"));
                IconBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FEE2E2"));
                ConfirmButton.Style = (Style)FindResource("DangerButtonStyle");
                break;
                
            case DialogType.Info:
                IconText.Text = "i";
                IconText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3B82F6"));
                IconBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DBEAFE"));
                ConfirmButton.Style = (Style)FindResource("ConfirmButtonStyle");
                break;
                
            case DialogType.Success:
                IconText.Text = "✓";
                IconText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981"));
                IconBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D1FAE5"));
                ConfirmButton.Style = (Style)FindResource("ConfirmButtonStyle");
                break;
        }
    }

    private void ConfirmButton_Click(object sender, RoutedEventArgs e)
    {
        Result = true;
        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        Result = false;
        DialogResult = false;
        Close();
    }
}
