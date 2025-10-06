using System.Windows;
using System.Windows.Controls;
using ChronoPos.Desktop.ViewModels;

namespace ChronoPos.Desktop.Views
{
    /// <summary>
    /// Interaction logic for AddGoodsReturnView.xaml
    /// </summary>
    public partial class AddGoodsReturnView : UserControl
    {
        public AddGoodsReturnView()
        {
            InitializeComponent();
            
            // Set default selected section
            SetSelectedSection("ReturnHeader");
        }

        /// <summary>
        /// Handle sidebar button clicks for navigation between sections
        /// </summary>
        private void SidebarButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string sectionName)
            {
                SetSelectedSection(sectionName);
            }
        }

        /// <summary>
        /// Set the selected section and update visibility
        /// </summary>
        private void SetSelectedSection(string sectionName)
        {
            // Hide all sections
            ReturnHeaderSection.Visibility = Visibility.Collapsed;
            ReturnItemsSection.Visibility = Visibility.Collapsed;
            SummarySection.Visibility = Visibility.Collapsed;

            // Clear all button selections
            ClearAllButtonSelections();

            // Show selected section and highlight button
            switch (sectionName)
            {
                case "ReturnHeader":
                    ReturnHeaderSection.Visibility = Visibility.Visible;
                    SetButtonSelected(ReturnHeaderButton, true);
                    break;
                
                case "ReturnItems":
                    ReturnItemsSection.Visibility = Visibility.Visible;
                    SetButtonSelected(ReturnItemsButton, true);
                    break;
                
                case "Summary":
                    SummarySection.Visibility = Visibility.Visible;
                    SetButtonSelected(SummaryButton, true);
                    break;
            }
        }

        /// <summary>
        /// Clear all button selections
        /// </summary>
        private void ClearAllButtonSelections()
        {
            SetButtonSelected(ReturnHeaderButton, false);
            SetButtonSelected(ReturnItemsButton, false);
            SetButtonSelected(SummaryButton, false);
        }

        /// <summary>
        /// Set button selected state
        /// </summary>
        private void SetButtonSelected(Button button, bool isSelected)
        {
            SetIsSelected(button, isSelected);
        }

        #region Attached Properties for Button Selection State

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.RegisterAttached(
                "IsSelected",
                typeof(bool),
                typeof(AddGoodsReturnView),
                new PropertyMetadata(false));

        public static bool GetIsSelected(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsSelectedProperty);
        }

        public static void SetIsSelected(DependencyObject obj, bool value)
        {
            obj.SetValue(IsSelectedProperty, value);
        }

        #endregion
    }
}