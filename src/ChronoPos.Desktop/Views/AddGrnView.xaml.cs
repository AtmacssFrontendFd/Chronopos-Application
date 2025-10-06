using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ChronoPos.Desktop.ViewModels;

namespace ChronoPos.Desktop.Views
{
    /// <summary>
    /// Interaction logic for AddGrnView.xaml
    /// </summary>
    public partial class AddGrnView : UserControl
    {
        private Button? _currentSelectedButton;

        public AddGrnView()
        {
            InitializeComponent();
            
            // Set default selected section
            ShowSection("GrnHeader");
            SetSelectedButton(GrnHeaderButton);
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
            GrnHeaderSection.Visibility = Visibility.Collapsed;
            GrnItemsSection.Visibility = Visibility.Collapsed;
            SummarySection.Visibility = Visibility.Collapsed;

            // Show the selected section
            switch (sectionName)
            {
                case "GrnHeader":
                    GrnHeaderSection.Visibility = Visibility.Visible;
                    break;
                case "GrnItems":
                    GrnItemsSection.Visibility = Visibility.Visible;
                    break;
                case "Summary":
                    SummarySection.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void SetSelectedButton(Button selectedButton)
        {
            // Reset all buttons to unselected state
            GrnHeaderButton.SetValue(IsSelectedProperty, false);
            GrnItemsButton.SetValue(IsSelectedProperty, false);
            SummaryButton.SetValue(IsSelectedProperty, false);

            // Set the clicked button as selected
            selectedButton.SetValue(IsSelectedProperty, true);
            _currentSelectedButton = selectedButton;
        }

        // Dependency property for button selection state
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.RegisterAttached("IsSelected", typeof(bool), typeof(AddGrnView),
                new PropertyMetadata(false));

        public static bool GetIsSelected(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsSelectedProperty);
        }

        public static void SetIsSelected(DependencyObject obj, bool value)
        {
            obj.SetValue(IsSelectedProperty, value);
        }
    }
}