using System.Windows.Controls;
using System.Windows.Input;

namespace ChronoPos.Desktop.Views
{
    /// <summary>
    /// Interaction logic for AddProductView.xaml
    /// </summary>
    public partial class AddProductView : UserControl
    {
        public AddProductView()
        {
            InitializeComponent();
        }

        private void BarcodeTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var viewModel = DataContext as ViewModels.AddProductViewModel;
                if (viewModel?.AddBarcodeCommand.CanExecute(null) == true)
                {
                    viewModel.AddBarcodeCommand.Execute(null);
                }
                e.Handled = true;
            }
        }
    }
}
