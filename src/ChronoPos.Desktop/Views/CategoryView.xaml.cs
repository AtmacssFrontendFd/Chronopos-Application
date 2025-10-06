using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ChronoPos.Desktop.ViewModels;

namespace ChronoPos.Desktop.Views
{
    /// <summary>
    /// Interaction logic for CategoryView.xaml
    /// </summary>
    public partial class CategoryView : UserControl
    {
        public CategoryView()
        {
            InitializeComponent();
        }

        public CategoryView(CategoryViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }

        private void TreeView_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Find the parent ScrollViewer
            var scrollViewer = FindParent<ScrollViewer>((DependencyObject)sender);
            if (scrollViewer != null)
            {
                // Forward the mouse wheel event to the ScrollViewer
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta / 3.0);
                e.Handled = true;
            }
        }

        private static T? FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject? parentObject = VisualTreeHelper.GetParent(child);
            
            if (parentObject == null) return null;
            
            if (parentObject is T parent)
                return parent;
            
            return FindParent<T>(parentObject);
        }


    }
}