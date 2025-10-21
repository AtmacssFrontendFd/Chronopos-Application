using ChronoPos.Desktop.ViewModels;
using ChronoPos.Application.Logging;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ChronoPos.Domain.Entities;
using CommunityToolkit.Mvvm.Input;
using ChronoPos.Desktop.Services;
using System.ComponentModel;

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
        
        // Subscribe to CurrentView property changes to log ContentControl updates
        if (viewModel != null)
        {
            viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }
    }
    
    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "CurrentView" && sender is MainWindowViewModel vm)
        {
            AppLogger.LogInfo($"===== MainWindow: CurrentView property changed =====", filename: "reservation");
            AppLogger.LogInfo($"New CurrentView type: {vm.CurrentView?.GetType().FullName ?? "null"}", filename: "reservation");
            
            // Force layout update on the ContentControl
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    // Find the ContentControl that displays CurrentView
                    var contentControl = FindContentControl(this);
                    if (contentControl != null)
                    {
                        AppLogger.LogInfo($"ContentControl found - Content type: {contentControl.Content?.GetType().Name ?? "null"}", filename: "reservation");
                        AppLogger.LogInfo($"ContentControl ActualWidth: {contentControl.ActualWidth}, ActualHeight: {contentControl.ActualHeight}", filename: "reservation");
                        
                        contentControl.UpdateLayout();
                        contentControl.InvalidateMeasure();
                        contentControl.InvalidateArrange();
                        
                        AppLogger.LogInfo($"ContentControl after UpdateLayout - ActualWidth: {contentControl.ActualWidth}, ActualHeight: {contentControl.ActualHeight}", filename: "reservation");
                        
                        if (contentControl.Content is FrameworkElement element)
                        {
                            AppLogger.LogInfo($"Content element ActualWidth: {element.ActualWidth}, ActualHeight: {element.ActualHeight}", filename: "reservation");
                        }
                    }
                    else
                    {
                        AppLogger.LogWarning("ContentControl NOT found in MainWindow visual tree!", filename: "reservation");
                    }
                }
                catch (Exception ex)
                {
                    AppLogger.LogError("Error updating ContentControl layout", ex, filename: "reservation");
                }
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }
    }
    
    private ContentControl? FindContentControl(DependencyObject parent)
    {
        int childCount = System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < childCount; i++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
            if (child is ContentControl cc && cc.Content != null)
            {
                return cc;
            }
            var result = FindContentControl(child);
            if (result != null) return result;
        }
        return null;
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
