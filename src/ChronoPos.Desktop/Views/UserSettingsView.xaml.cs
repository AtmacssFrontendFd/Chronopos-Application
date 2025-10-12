using System.Windows.Controls;

namespace ChronoPos.Desktop.Views;

/// <summary>
/// Interaction logic for UserSettingsView.xaml
/// </summary>
public partial class UserSettingsView : UserControl
{
    public UserSettingsView()
    {
        try
        {
            ChronoPos.Application.Logging.AppLogger.Log("=== UserSettingsView Constructor STARTED ===");
            ChronoPos.Application.Logging.AppLogger.Log("UserSettingsView: Calling InitializeComponent");
            
            InitializeComponent();
            
            ChronoPos.Application.Logging.AppLogger.Log("UserSettingsView: InitializeComponent completed");
            ChronoPos.Application.Logging.AppLogger.Log("=== UserSettingsView Constructor COMPLETED ===");
        }
        catch (Exception ex)
        {
            ChronoPos.Application.Logging.AppLogger.Log($"!!! UserSettingsView Constructor ERROR !!!: {ex.Message}");
            ChronoPos.Application.Logging.AppLogger.Log($"!!! Stack Trace !!!: {ex.StackTrace}");
            ChronoPos.Application.Logging.AppLogger.Log($"!!! Inner Exception !!!: {ex.InnerException?.Message ?? "None"}");
            throw;
        }
    }
}
