using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ChronoPos.Desktop.Views.Controls
{
    /// <summary>
    /// Custom DatePicker control with theme-compatible styling and circular calendar icon
    /// </summary>
    public partial class CustomDatePicker : UserControl
    {
        public CustomDatePicker()
        {
            InitializeComponent();
            
            // Subscribe to mouse events for hover effects
            MainBorder.MouseEnter += MainBorder_MouseEnter;
            MainBorder.MouseLeave += MainBorder_MouseLeave;
        }

        #region Dependency Properties

        /// <summary>
        /// Selected date of the DatePicker
        /// </summary>
        public static readonly DependencyProperty SelectedDateProperty =
            DependencyProperty.Register(
                nameof(SelectedDate),
                typeof(DateTime?),
                typeof(CustomDatePicker),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnSelectedDateChanged));

        public DateTime? SelectedDate
        {
            get => (DateTime?)GetValue(SelectedDateProperty);
            set => SetValue(SelectedDateProperty, value);
        }

        private static void OnSelectedDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CustomDatePicker picker)
            {
                picker.InnerDatePicker.SelectedDate = e.NewValue as DateTime?;
            }
        }

        /// <summary>
        /// Placeholder text when no date is selected
        /// </summary>
        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.Register(
                nameof(Placeholder),
                typeof(string),
                typeof(CustomDatePicker),
                new PropertyMetadata("Select Date"));

        public string Placeholder
        {
            get => (string)GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }

        #endregion

        #region Events

        /// <summary>
        /// Event raised when selected date changes
        /// </summary>
        public event EventHandler<SelectionChangedEventArgs>? SelectedDateChanged;

        #endregion

        #region Event Handlers

        private void InnerDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is DatePicker datePicker)
            {
                SelectedDate = datePicker.SelectedDate;
                SelectedDateChanged?.Invoke(this, e);
            }
        }

        private void CalendarIcon_Click(object sender, MouseButtonEventArgs e)
        {
            // Open the calendar popup
            InnerDatePicker.IsDropDownOpen = true;
        }

        private void MainBorder_MouseEnter(object sender, MouseEventArgs e)
        {
            // Apply hover effect
            MainBorder.BorderBrush = (System.Windows.Media.Brush)FindResource("Primary");
            MainBorder.Background = (System.Windows.Media.Brush)FindResource("CardBackground");
        }

        private void MainBorder_MouseLeave(object sender, MouseEventArgs e)
        {
            // Remove hover effect
            MainBorder.BorderBrush = (System.Windows.Media.Brush)FindResource("BorderLight");
            MainBorder.Background = (System.Windows.Media.Brush)FindResource("SurfaceBackground");
        }

        #endregion
    }
}
