using System;
using System.Collections.Generic;
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
        private Dictionary<Border, Button> _sectionButtonMap;
        private bool _isScrolling = false;

        public AddGrnView()
        {
            InitializeComponent();
            
            _sectionButtonMap = new Dictionary<Border, Button>();

            // Set default selected section after controls are loaded
            Loaded += (s, e) =>
            {
                // Map sections to buttons after UI is loaded
                var grnHeaderSection = this.FindName("GrnHeaderSection") as Border;
                var grnItemsSection = this.FindName("GrnItemsSection") as Border;
                var summarySection = this.FindName("SummarySection") as Border;
                var grnHeaderButton = this.FindName("GrnHeaderButton") as Button;
                var grnItemsButton = this.FindName("GrnItemsButton") as Button;
                var summaryButton = this.FindName("SummaryButton") as Button;

                if (grnHeaderSection != null && grnHeaderButton != null)
                    _sectionButtonMap[grnHeaderSection] = grnHeaderButton;
                if (grnItemsSection != null && grnItemsButton != null)
                    _sectionButtonMap[grnItemsSection] = grnItemsButton;
                if (summarySection != null && summaryButton != null)
                    _sectionButtonMap[summarySection] = summaryButton;

                if (grnHeaderButton != null)
                    SetSelectedButton(grnHeaderButton);
            };
        }

        private void SidebarButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string sectionName)
            {
                ScrollToSection(sectionName);
            }
        }

        /// <summary>
        /// Scroll to a specific section smoothly
        /// </summary>
        private void ScrollToSection(string sectionName)
        {
            Border? targetSection = sectionName switch
            {
                "GrnHeader" => this.FindName("GrnHeaderSection") as Border,
                "GrnItems" => this.FindName("GrnItemsSection") as Border,
                "Summary" => this.FindName("SummarySection") as Border,
                _ => null
            };

            var scrollViewer = this.FindName("MainScrollViewer") as ScrollViewer;
            if (targetSection != null && scrollViewer != null)
            {
                _isScrolling = true;
                var transform = targetSection.TransformToAncestor(scrollViewer);
                var position = transform.Transform(new Point(0, 0));
                scrollViewer.ScrollToVerticalOffset(position.Y + scrollViewer.VerticalOffset - 20);
                
                // Re-enable scroll tracking after a short delay
                Dispatcher.InvokeAsync(async () =>
                {
                    await System.Threading.Tasks.Task.Delay(100);
                    _isScrolling = false;
                    UpdateActiveButton();
                });
            }
        }

        /// <summary>
        /// Handle scroll events to update active section button
        /// </summary>
        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (!_isScrolling)
            {
                UpdateActiveButton();
            }
        }

        /// <summary>
        /// Update which button is active based on scroll position
        /// </summary>
        private void UpdateActiveButton()
        {
            var scrollViewer = this.FindName("MainScrollViewer") as ScrollViewer;
            if (scrollViewer == null) return;

            var viewportTop = scrollViewer.VerticalOffset;
            var viewportMiddle = viewportTop + (scrollViewer.ViewportHeight / 3);

            Border? activeSection = null;
            double closestDistance = double.MaxValue;

            foreach (var section in _sectionButtonMap.Keys)
            {
                try
                {
                    var transform = section.TransformToAncestor(scrollViewer);
                    var position = transform.Transform(new Point(0, 0));
                    var sectionTop = position.Y + scrollViewer.VerticalOffset;
                    var distance = Math.Abs(sectionTop - viewportMiddle);

                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        activeSection = section;
                    }
                }
                catch { }
            }

            if (activeSection != null && _sectionButtonMap.TryGetValue(activeSection, out var button))
            {
                SetSelectedButton(button);
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