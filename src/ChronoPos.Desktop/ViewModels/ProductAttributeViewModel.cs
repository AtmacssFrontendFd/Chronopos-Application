using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Desktop.Services;
using ChronoPos.Desktop.ViewModels;
using ChronoPos.Desktop.Views;
using System.Collections.ObjectModel;
using System.Windows;

namespace ChronoPos.Desktop.ViewModels
{
    public partial class ProductAttributeViewModel : ObservableObject
    {
        private readonly IProductAttributeService _attributeService;
        private readonly Action? _navigateBack;

        [ObservableProperty]
        private ObservableCollection<ProductAttributeValueDto> _attributeValues = new();

        [ObservableProperty]
        private ObservableCollection<ProductAttributeValueDto> _filteredAttributeValues = new();

        [ObservableProperty]
        private ProductAttributeValueDto? _selectedAttributeValue;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private bool _showActiveOnly = true;

        [ObservableProperty]
        private bool _isLoading = false;

        [ObservableProperty]
        private string _loadingMessage = "Loading...";

        [ObservableProperty]
        private string _statusMessage = "Ready";

        [ObservableProperty]
        private bool _isSidePanelVisible = false;

        [ObservableProperty]
        private object? _sidePanelContent;

        public string AttributeCountText 
        { 
            get 
            {
                try
                {
                    var result = $"{FilteredAttributeValues.Count} of {AttributeValues.Count} attribute values";
                    ChronoPos.Desktop.Services.FileLogger.Log($"🔄 AttributeCountText accessed: '{result}'");
                    return result;
                }
                catch (Exception ex)
                {
                    ChronoPos.Desktop.Services.FileLogger.Log($"❌ Error in AttributeCountText: {ex.Message}");
                    return "Error loading count";
                }
            }
        }
        
        public bool HasSearchText 
        { 
            get 
            {
                try
                {
                    var result = !string.IsNullOrWhiteSpace(SearchText);
                    ChronoPos.Desktop.Services.FileLogger.Log($"🔄 HasSearchText accessed: {result}");
                    return result;
                }
                catch (Exception ex)
                {
                    ChronoPos.Desktop.Services.FileLogger.Log($"❌ Error in HasSearchText: {ex.Message}");
                    return false;
                }
            }
        }
        
        public bool HasAttributeValues 
        { 
            get 
            {
                try
                {
                    var result = FilteredAttributeValues != null && FilteredAttributeValues.Count > 0;
                    ChronoPos.Desktop.Services.FileLogger.Log($"🔄 HasAttributeValues accessed: {result} (Count: {FilteredAttributeValues?.Count ?? 0})");
                    return result;
                }
                catch (Exception ex)
                {
                    ChronoPos.Desktop.Services.FileLogger.Log($"❌ Error in HasAttributeValues: {ex.Message}");
                    return false;
                }
            }
        }

        public ProductAttributeViewModel(IProductAttributeService attributeService, Action? navigateBack = null)
        {
            _attributeService = attributeService ?? throw new ArgumentNullException(nameof(attributeService));
            _navigateBack = navigateBack;
            
            ChronoPos.Desktop.Services.FileLogger.Log("🔧 ProductAttributeViewModel constructor started");

            // Initialize like DiscountViewModel - use Task.Run to avoid deadlocks
            _ = Task.Run(LoadAttributesAsync);
            
            ChronoPos.Desktop.Services.FileLogger.Log("🔧 ProductAttributeViewModel constructor completed");
        }

        partial void OnSearchTextChanged(string value)
        {
            try
            {
                ChronoPos.Desktop.Services.FileLogger.Log($"🔄 OnSearchTextChanged: '{value}'");
                FilterAttributes();
                ChronoPos.Desktop.Services.FileLogger.Log($"✅ OnSearchTextChanged completed");
            }
            catch (Exception ex)
            {
                ChronoPos.Desktop.Services.FileLogger.Log($"❌ Error in OnSearchTextChanged: {ex.Message}");
                ChronoPos.Desktop.Services.FileLogger.Log($"❌ OnSearchTextChanged stack trace: {ex.StackTrace}");
                throw;
            }
        }

        partial void OnShowActiveOnlyChanged(bool value)
        {
            try
            {
                ChronoPos.Desktop.Services.FileLogger.Log($"🔄 OnShowActiveOnlyChanged: {value}");
                FilterAttributes();
                ChronoPos.Desktop.Services.FileLogger.Log($"✅ OnShowActiveOnlyChanged completed");
            }
            catch (Exception ex)
            {
                ChronoPos.Desktop.Services.FileLogger.Log($"❌ Error in OnShowActiveOnlyChanged: {ex.Message}");
                ChronoPos.Desktop.Services.FileLogger.Log($"❌ OnShowActiveOnlyChanged stack trace: {ex.StackTrace}");
                throw;
            }
        }

        private void FilterAttributes()
        {
            try
            {
                ChronoPos.Desktop.Services.FileLogger.Log($"🔍 FilterAttributes started - SearchText: '{SearchText}', ShowActiveOnly: {ShowActiveOnly}");
                
                var filtered = AttributeValues.AsEnumerable();

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    filtered = filtered.Where(v => 
                        v.AttributeName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                        v.Value.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                        (!string.IsNullOrEmpty(v.AttributeNameAr) && v.AttributeNameAr.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(v.ValueAr) && v.ValueAr.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                        v.AttributeType.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
                }

                // Apply active filter
                if (ShowActiveOnly)
                {
                    filtered = filtered.Where(v => v.Status == "Active");
                }

                FilteredAttributeValues.Clear();
                foreach (var attributeValue in filtered)
                {
                    FilteredAttributeValues.Add(attributeValue);
                }

                OnPropertyChanged(nameof(AttributeCountText));
                OnPropertyChanged(nameof(HasAttributeValues));
                ChronoPos.Desktop.Services.FileLogger.Log($"🔍 FilterAttributes completed - {FilteredAttributeValues.Count} filtered from {AttributeValues.Count} total");
            }
            catch (Exception ex)
            {
                ChronoPos.Desktop.Services.FileLogger.Log($"❌ Error in FilterAttributes: {ex.Message}");
                ChronoPos.Desktop.Services.FileLogger.Log($"❌ FilterAttributes stack trace: {ex.StackTrace}");
            }
        }

        private async Task LoadAttributesAsync()
        {
            try
            {
                ChronoPos.Desktop.Services.FileLogger.Log("🔄 LoadAttributesAsync started");
                IsLoading = true;
                LoadingMessage = "Loading attribute values...";
                StatusMessage = "Loading...";

                ChronoPos.Desktop.Services.FileLogger.Log("🔄 Calling GetAllAttributeValuesAsync");
                var attributeValues = await _attributeService.GetAllAttributeValuesAsync();
                ChronoPos.Desktop.Services.FileLogger.Log($"🔄 Retrieved {attributeValues.Count} attribute values from service");
                
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    try
                    {
                        ChronoPos.Desktop.Services.FileLogger.Log("🔄 Clearing and populating AttributeValues collection");
                        AttributeValues.Clear();
                        foreach (var attributeValue in attributeValues)
                        {
                            AttributeValues.Add(attributeValue);
                        }
                        
                        ChronoPos.Desktop.Services.FileLogger.Log("🔄 Calling FilterAttributes");
                        FilterAttributes();
                        OnPropertyChanged(nameof(AttributeCountText));
                        OnPropertyChanged(nameof(HasAttributeValues));
                        ChronoPos.Desktop.Services.FileLogger.Log("🔄 UI update completed successfully");
                    }
                    catch (Exception dispatcherEx)
                    {
                        ChronoPos.Desktop.Services.FileLogger.Log($"❌ Error in UI thread: {dispatcherEx.Message}");
                        ChronoPos.Desktop.Services.FileLogger.Log($"❌ UI thread stack trace: {dispatcherEx.StackTrace}");
                        // Don't throw from dispatcher - just log
                    }
                });

                StatusMessage = "Ready";
                ChronoPos.Desktop.Services.FileLogger.Log($"✅ LoadAttributesAsync completed successfully - {attributeValues.Count} attribute values loaded");
            }
            catch (Exception ex)
            {
                ChronoPos.Desktop.Services.FileLogger.Log($"❌ Error in LoadAttributesAsync: {ex.Message}");
                ChronoPos.Desktop.Services.FileLogger.Log($"❌ LoadAttributesAsync stack trace: {ex.StackTrace}");
                StatusMessage = $"Error loading attribute values: {ex.Message}";
                
                // Safely clear collections on error
                try
                {
                    await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        AttributeValues.Clear();
                        FilteredAttributeValues.Clear();
                        OnPropertyChanged(nameof(HasAttributeValues));
                        OnPropertyChanged(nameof(AttributeCountText));
                    });
                    ChronoPos.Desktop.Services.FileLogger.Log("🔄 Cleared collections after error");
                }
                catch (Exception dispatcherEx)
                {
                    ChronoPos.Desktop.Services.FileLogger.Log($"❌ Error clearing collections: {dispatcherEx.Message}");
                }
            }
            finally
            {
                IsLoading = false;
                ChronoPos.Desktop.Services.FileLogger.Log("🔄 LoadAttributesAsync finally block - IsLoading set to false");
            }
        }

        [RelayCommand]
        private void AddAttribute()
        {
            try
            {
                var sidePanelViewModel = new ProductAttributeSidePanelViewModel(_attributeService);
                FileLogger.Log("🔧 Subscribing to sidePanelViewModel events");
                sidePanelViewModel.AttributeSaved += OnAttributeSaved;
                sidePanelViewModel.CloseRequested += OnSidePanelCloseRequested;
                FileLogger.Log("✅ Event subscriptions completed");
                
                // Create the view and set its DataContext
                var sidePanelView = new ProductAttributeSidePanelView();
                FileLogger.Log("🔧 Setting DataContext on sidePanelView");
                sidePanelView.DataContext = sidePanelViewModel;
                FileLogger.Log($"✅ DataContext set - Type: {sidePanelView.DataContext?.GetType().Name}");
                
                SidePanelContent = sidePanelView;
                IsSidePanelVisible = true;
                
                StatusMessage = "Add new product attribute value";
                ChronoPos.Desktop.Services.FileLogger.Log("📝 Opening add attribute side panel");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error opening add form: {ex.Message}";
                ChronoPos.Desktop.Services.FileLogger.Log($"❌ Error opening add attribute form: {ex.Message}");
            }
        }

        [RelayCommand]
        private void EditAttributeValue(ProductAttributeValueDto? attributeValue)
        {
            if (attributeValue == null) return;

            try
            {
                // Create side panel ViewModel with the attributeValue for editing
                var sidePanelViewModel = new ProductAttributeSidePanelViewModel(_attributeService, attributeValue);
                sidePanelViewModel.AttributeSaved += OnAttributeSaved;
                sidePanelViewModel.CloseRequested += OnSidePanelCloseRequested;
                
                // Create the view and set its DataContext
                var sidePanelView = new ProductAttributeSidePanelView();
                sidePanelView.DataContext = sidePanelViewModel;
                
                SidePanelContent = sidePanelView;
                IsSidePanelVisible = true;
                
                StatusMessage = $"Edit value: {attributeValue.Value}";
                ChronoPos.Desktop.Services.FileLogger.Log($"📝 Opening edit attribute value side panel for: {attributeValue.Value}");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error opening edit form: {ex.Message}";
                ChronoPos.Desktop.Services.FileLogger.Log($"❌ Error opening edit attribute value form: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task DeleteAttributeValue(ProductAttributeValueDto? attributeValue)
        {
            if (attributeValue == null) return;

            try
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to delete the value '{attributeValue.Value}' from attribute '{attributeValue.AttributeName}'?\n\nThis action cannot be undone.",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    IsLoading = true;
                    LoadingMessage = "Deleting attribute value...";
                    
                    await _attributeService.DeleteValueAsync(attributeValue.Id);
                    
                    StatusMessage = $"Deleted value: {attributeValue.Value}";
                    ChronoPos.Desktop.Services.FileLogger.Log($"🗑️ Deleted attribute value: {attributeValue.Value}");
                    
                    await LoadAttributesAsync();
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error deleting attribute value: {ex.Message}";
                ChronoPos.Desktop.Services.FileLogger.Log($"❌ Error deleting attribute value: {ex.Message}");
                MessageBox.Show($"Error deleting attribute value: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void ClearSearch()
        {
            SearchText = string.Empty;
        }

        [RelayCommand]
        private void ClearFilters()
        {
            SearchText = string.Empty;
            ShowActiveOnly = false;
        }

        [RelayCommand]
        private async Task RefreshData()
        {
            try
            {
                ChronoPos.Desktop.Services.FileLogger.Log("🔄 RefreshData command started");
                IsLoading = true;
                StatusMessage = "Refreshing attribute values data...";
                
                await LoadAttributesAsync();
                
                ChronoPos.Desktop.Services.FileLogger.Log("✅ RefreshData command completed");
            }
            catch (Exception ex)
            {
                ChronoPos.Desktop.Services.FileLogger.Log($"❌ Error in RefreshData: {ex.Message}");
                StatusMessage = $"Error refreshing data: {ex.Message}";
            }
        }

        [RelayCommand]
        private void Back()
        {
            try
            {
                FileLogger.Log("🔄 Back button clicked - attempting to navigate back");
                
                if (_navigateBack != null)
                {
                    _navigateBack.Invoke();
                    FileLogger.Log("✅ Navigation back completed successfully");
                }
                else
                {
                    StatusMessage = "Navigation back not configured.";
                    FileLogger.Log("⚠️ Navigation back not configured");
                }
            }
            catch (Exception ex)
            {
                FileLogger.Log($"❌ Error in Back command: {ex.Message}");
                StatusMessage = $"Error navigating back: {ex.Message}";
            }
        }

        private async void OnAttributeSaved(object? sender, EventArgs e)
        {
            try
            {
                await LoadAttributesAsync();
                StatusMessage = "Attribute value saved successfully";
                ChronoPos.Desktop.Services.FileLogger.Log("✅ Attribute saved event handled successfully");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error refreshing after save: {ex.Message}";
                ChronoPos.Desktop.Services.FileLogger.Log($"❌ Error in OnAttributeSaved: {ex.Message}");
            }
        }

        private void OnSidePanelCloseRequested(object? sender, EventArgs e)
        {
            try
            {
                FileLogger.Log("🔄 Side panel close requested - starting close process");
                
                // Ensure we're on UI thread
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    IsSidePanelVisible = false;
                    SidePanelContent = null;
                    StatusMessage = "Ready";
                });
                
                FileLogger.Log("✅ Side panel closed successfully");
            }
            catch (Exception ex)
            {
                FileLogger.Log($"❌ Error closing side panel: {ex.Message}");
                FileLogger.Log($"❌ Stack trace: {ex.StackTrace}");
                
                // Force close even if there's an error
                try
                {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        IsSidePanelVisible = false;
                        SidePanelContent = null;
                        StatusMessage = "Ready";
                    });
                }
                catch (Exception innerEx)
                {
                    FileLogger.Log($"❌ Error in fallback close operation: {innerEx.Message}");
                }
            }
        }
    }
}