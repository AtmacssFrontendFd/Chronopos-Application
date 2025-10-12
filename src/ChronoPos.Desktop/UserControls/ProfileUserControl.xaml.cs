using System.Windows.Controls;

namespace ChronoPos.Desktop.UserControls;

/// <summary>
/// Interaction logic for ProfileUserControl.xaml
/// </summary>
public partial class ProfileUserControl : UserControl
{
    public ProfileUserControl()
    {
        try
        {
            ChronoPos.Application.Logging.AppLogger.Log("=== ProfileUserControl Constructor STARTED ===");
            InitializeComponent();
            ChronoPos.Application.Logging.AppLogger.Log("=== ProfileUserControl Constructor COMPLETED ===");
        }
        catch (Exception ex)
        {
            ChronoPos.Application.Logging.AppLogger.Log($"!!! ProfileUserControl Constructor ERROR !!!: {ex.Message}");
            ChronoPos.Application.Logging.AppLogger.Log($"!!! Stack Trace !!!: {ex.StackTrace}");
            throw;
        }
    }
}
