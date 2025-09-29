using ChronoPos.Desktop.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ChronoPos.Domain.Entities;
using CommunityToolkit.Mvvm.Input;
using ChronoPos.Desktop.Services;

namespace ChronoPos.Desktop.Views;

/// <summary>
/// Main window for the ChronoPos Desktop POS application
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        
        // Diagnose logging on startup
        LoggerDiagnostic.DiagnoseLogging();
    }

    private void GlobalSearchTextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        // The placeholder is now handled by the converter, no need for manual logic
    }

    private void GlobalSearchTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        // The placeholder is now handled by the converter, no need for manual logic
    }

    private void GlobalSearchTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            switch (e.Key)
            {
                case Key.Down:
                    viewModel.SelectNextGlobalSearchResult();
                    e.Handled = true;
                    break;
                case Key.Up:
                    viewModel.SelectPreviousGlobalSearchResult();
                    e.Handled = true;
                    break;
                case Key.Enter:
                    viewModel.OpenSelectedGlobalSearchResult();
                    e.Handled = true;
                    break;
                case Key.Escape:
                    viewModel.ClearGlobalSearch();
                    e.Handled = true;
                    break;
            }
        }
    }

    private void SearchResultItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel && sender is ListBoxItem item)
        {
            viewModel.OpenGlobalSearchResult(item.DataContext);
        }
    }

    private async void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel && 
            sender is ComboBox comboBox && 
            comboBox.SelectedItem is Language selectedLanguage)
        {
            if (viewModel.ChangeLanguageCommand is AsyncRelayCommand<Language> asyncCommand)
            {
                await asyncCommand.ExecuteAsync(selectedLanguage);
            }
        }
    }
}
