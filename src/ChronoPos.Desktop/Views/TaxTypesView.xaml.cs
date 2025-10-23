using System.Windows.Controls;
using System;
using ChronoPos.Desktop.Services;

namespace ChronoPos.Desktop.Views
{
    /// <summary>
    /// Interaction logic for TaxTypesView.xaml
    /// </summary>
    public partial class TaxTypesView : UserControl
    {
        public TaxTypesView()
        {
            try
            {
                FileLogger.LogSeparator("TaxTypesView Constructor Start");
                FileLogger.Log("TaxTypesView: Starting initialization...");
                
                InitializeComponent();
                
                FileLogger.Log("TaxTypesView: InitializeComponent completed successfully");
                FileLogger.LogSeparator("TaxTypesView Constructor End");
            }
            catch (Exception ex)
            {
                FileLogger.Log($"TaxTypesView: ERROR during initialization - {ex.Message}");
                FileLogger.Log($"TaxTypesView: Stack trace - {ex.StackTrace}");
                throw;
            }
        }
    }
}