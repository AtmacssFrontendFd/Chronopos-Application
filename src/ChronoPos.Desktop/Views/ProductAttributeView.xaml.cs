using ChronoPos.Desktop.ViewModels;
using ChronoPos.Desktop.Services;
using System.Windows.Controls;

namespace ChronoPos.Desktop.Views
{
    /// <summary>
    /// Interaction logic for ProductAttributeView.xaml
    /// </summary>
    public partial class ProductAttributeView : UserControl
    {
        public ProductAttributeView()
        {
            try
            {
                ChronoPos.Desktop.Services.FileLogger.Log("🔧 ProductAttributeView constructor started");
                InitializeComponent();
                ChronoPos.Desktop.Services.FileLogger.Log("✅ ProductAttributeView constructor completed successfully");
            }
            catch (Exception ex)
            {
                ChronoPos.Desktop.Services.FileLogger.Log($"❌ Error in ProductAttributeView constructor: {ex.Message}");
                ChronoPos.Desktop.Services.FileLogger.Log($"❌ ProductAttributeView constructor stack trace: {ex.StackTrace}");
                throw; // Re-throw to maintain original behavior
            }
        }
    }
}