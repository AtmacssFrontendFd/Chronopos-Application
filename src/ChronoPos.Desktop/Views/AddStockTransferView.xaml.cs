using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using ChronoPos.Desktop.ViewModels;

namespace ChronoPos.Desktop.Views
{
    /// <summary>
    /// Interaction logic for AddStockTransferView.xaml
    /// </summary>
    public partial class AddStockTransferView : UserControl
    {
        private readonly Dictionary<Border, Button> _sectionButtonMap;
        private bool _isScrolling = false;

        public AddStockTransferView()
        {
            InitializeComponent();
            
            // Map sections to buttons
            _sectionButtonMap = new Dictionary<Border, Button>
            {
                { TransferHeaderSection, TransferHeaderButton },
                { TransferItemsSection, TransferItemsButton },
                { SummarySection, SummaryButton }
            };

            // Set default selected section
            Loaded += (s, e) =>
            {
                SetButtonSelected(TransferHeaderButton, true);
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
                "TransferHeader" => TransferHeaderSection,
                "TransferItems" => TransferItemsSection,
                "Summary" => SummarySection,
                _ => null
            };

            if (targetSection != null && MainScrollViewer != null)
            {
                _isScrolling = true;
                var transform = targetSection.TransformToAncestor(MainScrollViewer);
                var position = transform.Transform(new Point(0, 0));
                MainScrollViewer.ScrollToVerticalOffset(position.Y + MainScrollViewer.VerticalOffset - 20);
                
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
            var scrollViewer = MainScrollViewer;
            if (scrollViewer == null) return;

            var viewportTop = scrollViewer.VerticalOffset;
            var viewportBottom = viewportTop + scrollViewer.ViewportHeight;
            var scrollableHeight = scrollViewer.ScrollableHeight;

            // Check if scrolled to the bottom (with small threshold)
            if (scrollableHeight > 0 && viewportTop >= scrollableHeight - 10)
            {
                // Activate the last section (Summary)
                ClearAllButtonSelections();
                SetButtonSelected(SummaryButton, true);
                return;
            }

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
                    
                    // Check if section is in viewport
                    var sectionBottom = sectionTop + section.ActualHeight;
                    
                    // If section is visible in viewport, calculate distance from top of viewport
                    if (sectionBottom > viewportTop && sectionTop < viewportBottom)
                    {
                        var distance = Math.Abs(sectionTop - viewportMiddle);

                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            activeSection = section;
                        }
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
            SetButtonSelected(TransferHeaderButton, false);
            SetButtonSelected(TransferItemsButton, false);
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
                typeof(AddStockTransferView),
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