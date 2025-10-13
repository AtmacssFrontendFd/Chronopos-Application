using System;
using System.Collections.Generic;
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
        private Dictionary<Border, Button> _sectionButtonMap;
        private bool _isScrolling = false;

        public AddGoodsReturnView()
        {
            InitializeComponent();
            
            _sectionButtonMap = new Dictionary<Border, Button>();

            // Set default selected section after controls are loaded
            Loaded += (s, e) =>
            {
                // Map sections to buttons after UI is loaded
                var returnHeaderSection = this.FindName("ReturnHeaderSection") as Border;
                var returnItemsSection = this.FindName("ReturnItemsSection") as Border;
                var summarySection = this.FindName("SummarySection") as Border;
                var returnHeaderButton = this.FindName("ReturnHeaderButton") as Button;
                var returnItemsButton = this.FindName("ReturnItemsButton") as Button;
                var summaryButton = this.FindName("SummaryButton") as Button;

                if (returnHeaderSection != null && returnHeaderButton != null)
                    _sectionButtonMap[returnHeaderSection] = returnHeaderButton;
                if (returnItemsSection != null && returnItemsButton != null)
                    _sectionButtonMap[returnItemsSection] = returnItemsButton;
                if (summarySection != null && summaryButton != null)
                    _sectionButtonMap[summarySection] = summaryButton;

                if (returnHeaderButton != null)
                    SetButtonSelected(returnHeaderButton, true);
            };
        }

        /// <summary>
        /// Handle sidebar button clicks for navigation between sections
        /// </summary>
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
                "ReturnHeader" => this.FindName("ReturnHeaderSection") as Border,
                "ReturnItems" => this.FindName("ReturnItemsSection") as Border,
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
                ClearAllButtonSelections();
                SetButtonSelected(button, true);
            }
        }

        /// <summary>
        /// Clear all button selections
        /// </summary>
        private void ClearAllButtonSelections()
        {
            var returnHeaderButton = this.FindName("ReturnHeaderButton") as Button;
            var returnItemsButton = this.FindName("ReturnItemsButton") as Button;
            var summaryButton = this.FindName("SummaryButton") as Button;

            if (returnHeaderButton != null) SetButtonSelected(returnHeaderButton, false);
            if (returnItemsButton != null) SetButtonSelected(returnItemsButton, false);
            if (summaryButton != null) SetButtonSelected(summaryButton, false);
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