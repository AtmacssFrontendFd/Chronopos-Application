using System.Windows.Controls;
using System;
using ChronoPos.Desktop.Services;

namespace ChronoPos.Desktop.UserControls
{
    /// <summary>
    /// Interaction logic for TaxTypeSidePanelControl.xaml
    /// </summary>
    public partial class TaxTypeSidePanelControl : UserControl
    {
        public TaxTypeSidePanelControl()
        {
            try
            {
                FileLogger.LogSeparator("TaxTypeSidePanelControl Constructor Start");
                FileLogger.Log("TaxTypeSidePanelControl: Starting initialization...");
                
                InitializeComponent();
                
                FileLogger.Log("TaxTypeSidePanelControl: InitializeComponent completed successfully");
                FileLogger.LogSeparator("TaxTypeSidePanelControl Constructor End");
            }
            catch (Exception ex)
            {
                FileLogger.Log($"TaxTypeSidePanelControl: ERROR during initialization - {ex.Message}");
                FileLogger.Log($"TaxTypeSidePanelControl: Stack trace - {ex.StackTrace}");
                throw;
            }
        }
    }
}