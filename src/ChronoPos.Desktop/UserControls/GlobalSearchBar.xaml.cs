using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ChronoPos.Desktop.ViewModels;

namespace ChronoPos.Desktop.UserControls;

/// <summary>
/// Interaction logic for GlobalSearchBar.xaml
/// </summary>
public partial class GlobalSearchBar : UserControl
{
    public GlobalSearchBar()
    {
        InitializeComponent();
    }

    private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox textBox && textBox.Text == textBox.Tag?.ToString())
        {
            textBox.Text = string.Empty;
        }
    }

    private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox textBox && string.IsNullOrWhiteSpace(textBox.Text))
        {
            textBox.Text = textBox.Tag?.ToString() ?? string.Empty;
        }
    }

    private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (DataContext is GlobalSearchBarViewModel viewModel)
        {
            switch (e.Key)
            {
                case Key.Down:
                    viewModel.SelectNextResult();
                    e.Handled = true;
                    break;
                case Key.Up:
                    viewModel.SelectPreviousResult();
                    e.Handled = true;
                    break;
                case Key.Enter:
                    viewModel.OpenSelectedResult();
                    e.Handled = true;
                    break;
                case Key.Escape:
                    viewModel.ClearSearch();
                    e.Handled = true;
                    break;
            }
        }
    }

    private void SearchResultItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is GlobalSearchBarViewModel viewModel && sender is ListBoxItem item)
        {
            viewModel.OpenSearchResult(item.DataContext);
        }
    }
}