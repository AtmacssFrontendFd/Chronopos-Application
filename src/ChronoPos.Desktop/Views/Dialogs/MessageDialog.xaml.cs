using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ChronoPos.Desktop.Views.Dialogs;

public partial class MessageDialog : Window
{
    public enum MessageType
    {
        Info,
        Success,
        Warning,
        Error
    }

    public MessageDialog(string title, string message, MessageType type = MessageType.Info)
    {
        InitializeComponent();
        
        TitleText.Text = title;
        MessageText.Text = message;
        
        SetMessageType(type);
    }

    private void SetMessageType(MessageType type)
    {
        var iconPath = (Path)IconBorder.Child;
        
        switch (type)
        {
            case MessageType.Info:
                IconBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3B82F6"));
                iconPath.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3B82F6"));
                iconPath.Data = Geometry.Parse("M 20,15 L 20,35 M 20,40 L 20,44");
                OkButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3B82F6"));
                break;
                
            case MessageType.Success:
                IconBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981"));
                iconPath.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981"));
                iconPath.Data = Geometry.Parse("M 15,28 L 28,38 L 48,18");
                OkButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981"));
                break;
                
            case MessageType.Warning:
                IconBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F59E0B"));
                iconPath.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F59E0B"));
                iconPath.Data = Geometry.Parse("M 20,15 L 20,30 M 20,35 L 20,39");
                OkButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F59E0B"));
                break;
                
            case MessageType.Error:
                IconBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444"));
                iconPath.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444"));
                iconPath.Data = Geometry.Parse("M 18,18 L 42,42 M 42,18 L 18,42");
                OkButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444"));
                break;
        }
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
