using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ChronoPos.Application.DTOs;
using ChronoPos.Desktop.Services;

namespace ChronoPos.Desktop.Views
{
    /// <summary>
    /// Interaction logic for AddProductView.xaml
    /// </summary>
    public partial class AddProductView : UserControl
    {
        private Button? _currentSelectedButton;
        private bool _isScrollingProgrammatically = false;
        private ScrollViewer? _scrollViewer;
        private StackPanel? _stackPanel;

        public AddProductView()
        {
            InitializeComponent();
            
            // Wait for the control to load before setting up scroll tracking
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Get references to named elements
            _scrollViewer = FindName("ContentScrollViewer") as ScrollViewer;
            _stackPanel = FindName("SectionsStackPanel") as StackPanel;
            
            if (_scrollViewer != null)
            {
                _scrollViewer.ScrollChanged += OnScrollChanged;
            }
            
            // Set initial active button
            var firstButton = FindName("ProductInfoButton") as Button;
            if (firstButton != null)
            {
                SetSelectedButton(firstButton);
            }
        }

        private void SidebarButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                var sectionName = button.Tag?.ToString();
                if (!string.IsNullOrEmpty(sectionName))
                {
                    ScrollToSection(sectionName);
                    SetSelectedButton(button);
                }
            }
        }

        private void ScrollToSection(string sectionName)
        {
            if (_scrollViewer == null || _stackPanel == null)
                return;

            var section = FindName($"{sectionName}Section") as Border;
            if (section == null)
                return;

            _isScrollingProgrammatically = true;

            try
            {
                // Get the vertical offset of the section relative to the StackPanel
                var transform = section.TransformToAncestor(_stackPanel);
                var sectionPosition = transform.Transform(new Point(0, 0));

                // Calculate target scroll position with some padding
                var targetOffset = sectionPosition.Y - 20;

                // Ensure target is within valid range
                targetOffset = Math.Max(0, Math.Min(targetOffset, _scrollViewer.ScrollableHeight));

                // Animate smooth scroll
                AnimateScroll(_scrollViewer.VerticalOffset, targetOffset);
            }
            catch
            {
                // Fallback: reset flag
                _isScrollingProgrammatically = false;
            }
        }

        private async void AnimateScroll(double fromOffset, double toOffset)
        {
            if (_scrollViewer == null)
                return;

            const int steps = 20;
            const int delayMs = 10;
            var delta = (toOffset - fromOffset) / steps;

            try
            {
                for (int i = 0; i < steps; i++)
                {
                    var newOffset = fromOffset + (delta * (i + 1));
                    _scrollViewer.ScrollToVerticalOffset(newOffset);
                    await Task.Delay(delayMs);
                }

                // Ensure we land exactly on target
                _scrollViewer.ScrollToVerticalOffset(toOffset);
            }
            finally
            {
                _isScrollingProgrammatically = false;
            }
        }

        private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // Don't update active button during programmatic scrolling
            if (_isScrollingProgrammatically)
                return;

            UpdateActiveButton();
        }

        private void UpdateActiveButton()
        {
            if (_scrollViewer == null || _stackPanel == null)
                return;

            var viewportTop = _scrollViewer.VerticalOffset;
            var viewportMiddle = viewportTop + (_scrollViewer.ViewportHeight / 3); // Use top third for activation

            Button? activeButton = null;
            double closestDistance = double.MaxValue;

            // Check all sections
            string[] sectionNames = { "ProductInfo", "TaxPricing", "Barcodes", "Pictures", "Attributes", "UnitPrices", "ProductBatches" };
            
            foreach (var sectionName in sectionNames)
            {
                var section = FindName($"{sectionName}Section") as Border;
                var button = FindName($"{sectionName}Button") as Button;
                
                if (section == null || button == null)
                    continue;

                try
                {
                    var transform = section.TransformToAncestor(_stackPanel);
                    var sectionPosition = transform.Transform(new Point(0, 0));
                    var sectionTop = sectionPosition.Y;
                    var distance = Math.Abs(sectionTop - viewportMiddle);

                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        activeButton = button;
                    }
                }
                catch
                {
                    // Skip if transform fails
                }
            }

            if (activeButton != null && activeButton != _currentSelectedButton)
            {
                SetSelectedButton(activeButton);
            }
        }

        private void SetSelectedButton(Button selectedButton)
        {
            // Reset all buttons to unselected state
            var buttonNames = new[] { "ProductInfoButton", "TaxPricingButton", "BarcodesButton", 
                                     "PicturesButton", "AttributesButton", "UnitPricesButton", "ProductBatchesButton" };
            
            foreach (var buttonName in buttonNames)
            {
                var button = FindName(buttonName) as Button;
                button?.SetValue(IsSelectedProperty, false);
            }

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

        private void ProductUnitUOM_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FileLogger.Log($"🔄 ProductUnitUOM_SelectionChanged triggered");
            
            if (sender is ComboBox comboBox && comboBox.DataContext is ProductUnitDto productUnit)
            {
                FileLogger.Log($"   📦 ProductUnit found - UnitId: {productUnit.UnitId}");
                
                if (this.DataContext is ViewModels.AddProductViewModel viewModel)
                {
                    FileLogger.Log($"   🎯 ViewModel found - Calling UpdateProductUnitPricingCommand");
                    // When UOM changes, recalculate cost and price of unit
                    viewModel.UpdateProductUnitPricingCommand?.Execute(productUnit);
                    viewModel.CalculateRemainingQuantityCommand?.Execute(null);
                    FileLogger.Log($"   ✅ Commands executed");
                }
                else
                {
                    FileLogger.Log($"   ❌ ViewModel not found");
                }
            }
            else
            {
                FileLogger.Log($"   ❌ ComboBox or ProductUnit not found");
            }
        }

        private void ProductUnitQuantity_LostFocus(object sender, RoutedEventArgs e)
        {
            FileLogger.Log($"🔢 ProductUnitQuantity_LostFocus triggered");
            
            if (sender is TextBox textBox && textBox.DataContext is ProductUnitDto productUnit)
            {
                FileLogger.Log($"   📦 ProductUnit found - QtyInUnit: {productUnit.QtyInUnit}");
                
                if (this.DataContext is ViewModels.AddProductViewModel viewModel)
                {
                    FileLogger.Log($"   🎯 ViewModel found - Calling CalculateRemainingQuantityCommand");
                    // When quantity changes, recalculate remaining quantity
                    viewModel.CalculateRemainingQuantityCommand?.Execute(null);
                    FileLogger.Log($"   ✅ Command executed");
                }
                else
                {
                    FileLogger.Log($"   ❌ ViewModel not found");
                }
            }
            else
            {
                FileLogger.Log($"   ❌ TextBox or ProductUnit not found");
            }
        }

    // Removed old TaxTypesListBox handlers after UI change to dropdown + chips
    }
}
