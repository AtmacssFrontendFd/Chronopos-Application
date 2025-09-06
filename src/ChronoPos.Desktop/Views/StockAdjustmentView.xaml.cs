using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using ChronoPos.Desktop.ViewModels;

namespace ChronoPos.Desktop.Views
{
    public partial class StockAdjustmentView : UserControl
    {
        public StockAdjustmentView()
        {
            InitializeComponent();
        }

        public void SetDataContext(System.IServiceProvider serviceProvider)
        {
            var viewModel = serviceProvider.GetRequiredService<StockAdjustmentViewModel>();
            DataContext = viewModel;
        }
    }
}