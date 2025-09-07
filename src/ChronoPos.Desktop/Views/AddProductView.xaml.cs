using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ChronoPos.Desktop.Views
{
    /// <summary>
    /// Interaction logic for AddProductView.xaml
    /// </summary>
    public partial class AddProductView : UserControl
    {
        private Button? _currentSelectedButton;

        public AddProductView()
        {
            InitializeComponent();
            
            // Set default selected section
            ShowSection("ProductInfo");
            SetSelectedButton(ProductInfoButton);
        }

        private void SidebarButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                var sectionName = button.Tag?.ToString();
                if (!string.IsNullOrEmpty(sectionName))
                {
                    ShowSection(sectionName);
                    SetSelectedButton(button);
                }
            }
        }

        private void ShowSection(string sectionName)
        {
            // Hide all sections first
            ProductInfoSection.Visibility = Visibility.Collapsed;
            TaxPricingSection.Visibility = Visibility.Collapsed;
            BarcodesSection.Visibility = Visibility.Collapsed;
            PicturesSection.Visibility = Visibility.Collapsed;
            AttributesSection.Visibility = Visibility.Collapsed;
            UnitPricesSection.Visibility = Visibility.Collapsed;

            // Show the selected section
            switch (sectionName)
            {
                case "ProductInfo":
                    ProductInfoSection.Visibility = Visibility.Visible;
                    break;
                case "TaxPricing":
                    TaxPricingSection.Visibility = Visibility.Visible;
                    break;
                case "Barcodes":
                    BarcodesSection.Visibility = Visibility.Visible;
                    break;
                case "Pictures":
                    PicturesSection.Visibility = Visibility.Visible;
                    break;
                case "Attributes":
                    AttributesSection.Visibility = Visibility.Visible;
                    break;
                case "UnitPrices":
                    UnitPricesSection.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void SetSelectedButton(Button selectedButton)
        {
            // Reset all buttons to unselected state
            ProductInfoButton.SetValue(IsSelectedProperty, false);
            TaxPricingButton.SetValue(IsSelectedProperty, false);
            BarcodesButton.SetValue(IsSelectedProperty, false);
            PicturesButton.SetValue(IsSelectedProperty, false);
            AttributesButton.SetValue(IsSelectedProperty, false);
            UnitPricesButton.SetValue(IsSelectedProperty, false);

            // Set the clicked button as selected
            selectedButton.SetValue(IsSelectedProperty, true);
            _currentSelectedButton = selectedButton;
        }

        // Dependency property for button selection state
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.RegisterAttached("IsSelected", typeof(bool), typeof(AddProductView),
                new PropertyMetadata(false));

        public static bool GetIsSelected(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsSelectedProperty);
        }

        public static void SetIsSelected(DependencyObject obj, bool value)
        {
            obj.SetValue(IsSelectedProperty, value);
        }

        private void BarcodeTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // Handle barcode entry on Enter key
                var textBox = sender as TextBox;
                if (textBox?.DataContext is ViewModels.AddProductViewModel viewModel)
                {
                    viewModel.AddBarcodeCommand?.Execute(null);
                }
            }
        }
    }
}
