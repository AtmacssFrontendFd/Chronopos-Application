using System;
using System.Windows;
using System.Windows.Controls;
using ChronoPos.Application.Logging;

namespace ChronoPos.Desktop.Views;

/// <summary>
/// Interaction logic for ProductGroupsView.xaml
/// </summary>
public partial class ProductGroupsView : UserControl
{
    public ProductGroupsView()
    {
        InitializeComponent();
        this.Loaded += ProductGroupsView_Loaded;
    }

    private void ProductGroupsView_Loaded(object sender, RoutedEventArgs e)
    {
        // Update column order based on initial FlowDirection
        UpdateColumnOrder();
        
        // Subscribe to FlowDirection property changes
        var dpd = System.ComponentModel.DependencyPropertyDescriptor.FromProperty(
            FlowDirectionProperty, typeof(UserControl));
        dpd?.AddValueChanged(this, (s, args) => 
        {
            Dispatcher.BeginInvoke(new Action(() => UpdateColumnOrder()), 
                System.Windows.Threading.DispatcherPriority.Loaded);
        });
    }

    private void UpdateColumnOrder()
    {
        try
        {
            var dataGrid = ProductGroupsDataGrid;
            
            if (dataGrid != null && dataGrid.Columns.Count > 0)
            {
                var isRtl = this.FlowDirection == FlowDirection.RightToLeft;
                var columnCount = dataGrid.Columns.Count;

                AppLogger.Log($"ProductGroupsView: Updating column order - RTL: {isRtl}, Column count: {columnCount}");

                // Reverse the DisplayIndex of all columns for RTL
                if (isRtl)
                {
                    // RTL: Reverse order
                    for (int i = 0; i < columnCount; i++)
                    {
                        dataGrid.Columns[i].DisplayIndex = columnCount - 1 - i;
                    }
                }
                else
                {
                    // LTR: Normal order
                    for (int i = 0; i < columnCount; i++)
                    {
                        dataGrid.Columns[i].DisplayIndex = i;
                    }
                }
                
                AppLogger.Log($"ProductGroupsView: Updated column order for {(isRtl ? "RTL" : "LTR")} mode");
            }
        }
        catch (Exception ex)
        {
            AppLogger.Log($"ProductGroupsView: Error updating column order: {ex.Message}");
        }
    }
}
