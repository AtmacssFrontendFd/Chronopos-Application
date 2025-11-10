using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ChronoPos.Application.Logging;
using ChronoPos.Desktop.ViewModels;

namespace ChronoPos.Desktop.Views;

/// <summary>
/// Interaction logic for BrandView.xaml
/// </summary>
public partial class BrandView : UserControl
{
    public BrandView()
    {
        try
        {
            AppLogger.Log("BrandView: Starting initialization...");
            
            // Log available resources
            var app = System.Windows.Application.Current;
            AppLogger.Log($"BrandView: Application is null: {app == null}");
            
            if (app != null)
            {
                AppLogger.Log($"BrandView: Application resources count: {app.Resources?.Count ?? 0}");
                
                // Check for specific resources
                var successButtonStyle = app.TryFindResource("SuccessButtonStyle");
                AppLogger.Log($"BrandView: SuccessButtonStyle found: {successButtonStyle != null}");
                
                var boolToVisConverter = app.TryFindResource("BoolToVisibilityConverter");
                AppLogger.Log($"BrandView: BoolToVisibilityConverter found: {boolToVisConverter != null}");
                
                var booleanToVisConverter = app.TryFindResource("BooleanToVisibilityConverter");
                AppLogger.Log($"BrandView: BooleanToVisibilityConverter found: {booleanToVisConverter != null}");
            }
            
            AppLogger.Log("BrandView: Calling InitializeComponent...");
            InitializeComponent();
            AppLogger.Log("BrandView: Initialization completed successfully");
            
            // Subscribe to DataContext changes
            this.DataContextChanged += BrandView_DataContextChanged;
            this.Loaded += BrandView_Loaded;
        }
        catch (Exception ex)
        {
            AppLogger.Log($"BrandView: ERROR during initialization: {ex.Message}");
            AppLogger.Log($"BrandView: Stack trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                AppLogger.Log($"BrandView: Inner exception: {ex.InnerException.Message}");
            }
            throw;
        }
    }

    private void BrandView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        // Unsubscribe from old ViewModel
        if (e.OldValue is BrandViewModel oldViewModel)
        {
            oldViewModel.PropertyChanged -= ViewModel_PropertyChanged;
        }

        // Subscribe to new ViewModel
        if (e.NewValue is BrandViewModel newViewModel)
        {
            newViewModel.PropertyChanged += ViewModel_PropertyChanged;
            // Update columns immediately
            UpdateColumnOrder();
        }
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(BrandViewModel.CurrentFlowDirection))
        {
            Dispatcher.BeginInvoke(new Action(() => UpdateColumnOrder()), System.Windows.Threading.DispatcherPriority.Loaded);
        }
    }

    private void BrandView_Loaded(object sender, RoutedEventArgs e)
    {
        // Update column order based on initial FlowDirection
        UpdateColumnOrder();
    }

    private void UpdateColumnOrder()
    {
        try
        {
            var dataGrid = BrandsDataGrid;
            
            if (dataGrid != null && dataGrid.Columns.Count > 0)
            {
                var isRtl = this.FlowDirection == FlowDirection.RightToLeft;
                var columnCount = dataGrid.Columns.Count;

                AppLogger.Log($"BrandView: Updating column order - RTL: {isRtl}, Column count: {columnCount}");

                // Reverse the DisplayIndex of all columns for RTL
                if (isRtl)
                {
                    // RTL: Reverse order
                    for (int i = 0; i < columnCount; i++)
                    {
                        dataGrid.Columns[i].DisplayIndex = columnCount - 1 - i;
                        AppLogger.Log($"BrandView: Column {i} DisplayIndex set to {columnCount - 1 - i}");
                    }
                }
                else
                {
                    // LTR: Normal order
                    for (int i = 0; i < columnCount; i++)
                    {
                        dataGrid.Columns[i].DisplayIndex = i;
                        AppLogger.Log($"BrandView: Column {i} DisplayIndex set to {i}");
                    }
                }
                
                AppLogger.Log($"BrandView: Updated column order for {(isRtl ? "RTL" : "LTR")} mode");
            }
            else
            {
                AppLogger.Log($"BrandView: DataGrid is null or has no columns");
            }
        }
        catch (Exception ex)
        {
            AppLogger.Log($"BrandView: Error updating column order: {ex.Message}");
        }
    }
}