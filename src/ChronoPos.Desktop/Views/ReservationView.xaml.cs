using ChronoPos.Desktop.ViewModels;
using ChronoPos.Application.Logging;
using System.Windows.Controls;
using System;

namespace ChronoPos.Desktop.Views;

/// <summary>
/// Interaction logic for ReservationView.xaml
/// </summary>
public partial class ReservationView : UserControl
{
    public ReservationView()
    {
        AppLogger.LogInfo("===== ReservationView constructor called (parameterless) =====", filename: "reservation");
        
        AppLogger.LogInfo("Calling InitializeComponent()...", filename: "reservation");
        InitializeComponent();
        AppLogger.LogInfo("InitializeComponent() completed successfully", filename: "reservation");
        
        // Subscribe to Loaded event to track when view is actually rendered
        this.Loaded += ReservationView_Loaded;
        
        AppLogger.LogInfo("===== ReservationView constructor completed =====", filename: "reservation");
    }
    
    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);
        AppLogger.LogInfo("ReservationView OnInitialized event fired", filename: "reservation");
    }
    
    private void ReservationView_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        AppLogger.LogInfo("===== ReservationView Loaded event fired =====", filename: "reservation");
        AppLogger.LogInfo($"ActualWidth: {ActualWidth}, ActualHeight: {ActualHeight}", filename: "reservation");
        AppLogger.LogInfo($"Visibility: {Visibility}", filename: "reservation");
        AppLogger.LogInfo($"IsVisible: {IsVisible}", filename: "reservation");
        AppLogger.LogInfo($"DataContext is null: {DataContext == null}", filename: "reservation");
        
        if (DataContext is ReservationTimelineViewModel vm)
        {
            AppLogger.LogInfo($"ViewModel TableGridRows count: {vm.TableGridRows?.Count ?? 0}", filename: "reservation");
            AppLogger.LogInfo($"ViewModel HasTables: {vm.HasTables}", filename: "reservation");
            AppLogger.LogInfo($"ViewModel IsLoading: {vm.IsLoading}", filename: "reservation");
            AppLogger.LogInfo($"ViewModel StatusMessage: {vm.StatusMessage}", filename: "reservation");
        }
        else
        {
            AppLogger.LogWarning("DataContext is not ReservationTimelineViewModel!", filename: "reservation");
        }
        
        // Check the actual XAML elements to diagnose layout issue
        try
        {
            AppLogger.LogInfo("===== Checking XAML Element Tree =====", filename: "reservation");
            
            // Find the root Grid by name
            var rootGrid = this.Content as System.Windows.Controls.Grid;
            if (rootGrid != null)
            {
                AppLogger.LogInfo($"Root Grid found - ActualWidth: {rootGrid.ActualWidth}, ActualHeight: {rootGrid.ActualHeight}", filename: "reservation");
                AppLogger.LogInfo($"Root Grid RowDefinitions count: {rootGrid.RowDefinitions.Count}", filename: "reservation");
                AppLogger.LogInfo($"Root Grid Children count: {rootGrid.Children.Count}", filename: "reservation");
                
                for (int i = 0; i < rootGrid.Children.Count && i < 5; i++)
                {
                    var child = rootGrid.Children[i];
                    var gridRow = System.Windows.Controls.Grid.GetRow(child);
                    AppLogger.LogInfo($"Child {i}: Type={child.GetType().Name}, Grid.Row={gridRow}, Visibility={child.Visibility}, ActualHeight={(child as System.Windows.FrameworkElement)?.ActualHeight ?? 0}", filename: "reservation");
                }
            }
            else
            {
                AppLogger.LogWarning("Root Grid NOT found! Content type: " + (this.Content?.GetType().Name ?? "null"), filename: "reservation");
            }
        }
        catch (Exception ex)
        {
            AppLogger.LogError("Error checking XAML element tree", ex, filename: "reservation");
        }
    }
}
