using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.Constants;
using ChronoPos.Desktop.Services;
using ChronoPos.Desktop.ViewModels;
using ChronoPos.Desktop.Views;
using ChronoPos.Desktop.Views.Dialogs;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;
using Microsoft.Win32;

namespace ChronoPos.Desktop.ViewModels
{
    public partial class ProductAttributeViewModel : ObservableObject
    {
        private readonly IProductAttributeService _attributeService;
        private readonly ICurrentUserService _currentUserService;
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

        [ObservableProperty]
        private bool canCreateProductAttribute = false;

        [ObservableProperty]
        private bool canEditProductAttribute = false;

        [ObservableProperty]
        private bool canDeleteProductAttribute = false;

        [ObservableProperty]
        private bool canImportProductAttribute = false;

        [ObservableProperty]
        private bool canExportProductAttribute = false;

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

        public ProductAttributeViewModel(
            IProductAttributeService attributeService, 
            ICurrentUserService currentUserService,
            Action? navigateBack = null)
        {
            _attributeService = attributeService ?? throw new ArgumentNullException(nameof(attributeService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _navigateBack = navigateBack;
            
            ChronoPos.Desktop.Services.FileLogger.Log("🔧 ProductAttributeViewModel constructor started");

            InitializePermissions();
            
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
                var dialog = new ConfirmationDialog(
                    "Delete Attribute Value",
                    $"Are you sure you want to delete the value '{attributeValue.Value}' from attribute '{attributeValue.AttributeName}'?\n\nThis action cannot be undone.",
                    ConfirmationDialog.DialogType.Danger,
                    "Delete",
                    "Cancel");

                var result = dialog.ShowDialog();
                if (result != true)
                    return;

                IsLoading = true;
                LoadingMessage = "Deleting attribute value...";
                
                await _attributeService.DeleteValueAsync(attributeValue.Id);
                
                StatusMessage = $"Deleted value: {attributeValue.Value}";
                ChronoPos.Desktop.Services.FileLogger.Log($"🗑️ Deleted attribute value: {attributeValue.Value}");
                
                await LoadAttributesAsync();
                
                var successDialog = new MessageDialog(
                    "Success",
                    $"Attribute value '{attributeValue.Value}' has been deleted successfully.",
                    MessageDialog.MessageType.Success);
                successDialog.ShowDialog();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error deleting attribute value: {ex.Message}";
                ChronoPos.Desktop.Services.FileLogger.Log($"❌ Error deleting attribute value: {ex.Message}");
                
                var errorDialog = new MessageDialog(
                    "Delete Error",
                    $"An error occurred while deleting the attribute value:\n\n{ex.Message}",
                    MessageDialog.MessageType.Error);
                errorDialog.ShowDialog();
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

        /// <summary>
        /// Command to export product attributes to CSV
        /// </summary>
        [RelayCommand]
        private async Task ExportAsync()
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                    FileName = $"ProductAttributes_{DateTime.Now:yyyyMMdd_HHmmss}.csv",
                    DefaultExt = ".csv"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    IsLoading = true;
                    StatusMessage = "Exporting product attribute values...";

                    var csv = new StringBuilder();
                    csv.AppendLine("AttributeName,ValueName,ValueArabic,Status");

                    foreach (var attr in AttributeValues)
                    {
                        csv.AppendLine($"\"{attr.AttributeName}\"," +
                                     $"\"{attr.Value}\"," +
                                     $"\"{attr.ValueAr ?? ""}\"," +
                                     $"{attr.Status}");
                    }

                    await File.WriteAllTextAsync(saveFileDialog.FileName, csv.ToString());
                    StatusMessage = $"Exported {AttributeValues.Count} attributes successfully";
                    
                    var successDialog = new MessageDialog(
                        "Export Successful",
                        $"Successfully exported {AttributeValues.Count} attribute values to:\n\n{saveFileDialog.FileName}",
                        MessageDialog.MessageType.Success);
                    successDialog.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error exporting attributes: {ex.Message}";
                
                var errorDialog = new MessageDialog(
                    "Export Error",
                    $"An error occurred while exporting attributes:\n\n{ex.Message}",
                    MessageDialog.MessageType.Error);
                errorDialog.ShowDialog();
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Command to import product attributes from CSV
        /// </summary>
        [RelayCommand]
        private async Task ImportAsync()
        {
            try
            {
                // Show custom import dialog
                var importDialog = new ImportDialog();
                var dialogResult = importDialog.ShowDialog();

                if (dialogResult != true || importDialog.SelectedAction == ImportDialog.ImportAction.None)
                    return;

                if (importDialog.SelectedAction == ImportDialog.ImportAction.DownloadTemplate)
                {
                    // Download Template
                    var saveFileDialog = new SaveFileDialog
                    {
                        Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                        FileName = "ProductAttributes_Template.csv",
                        DefaultExt = ".csv"
                    };

                    if (saveFileDialog.ShowDialog() == true)
                    {
                        var templateCsv = new StringBuilder();
                        // Header with all fields (required fields marked with *)
                        templateCsv.AppendLine("AttributeName*,ValueName*,ValueArabic,Status");
                        // Sample data showing all fields
                        templateCsv.AppendLine("\"Size\",\"Small\",\"صغير\",\"Active\"");
                        templateCsv.AppendLine("\"Size\",\"Medium\",\"متوسط\",\"Active\"");
                        templateCsv.AppendLine("\"Size\",\"Large\",\"كبير\",\"Active\"");
                        templateCsv.AppendLine("\"Color\",\"Red\",\"أحمر\",\"Active\"");
                        templateCsv.AppendLine("\"Color\",\"Blue\",\"أزرق\",\"Active\"");
                        templateCsv.AppendLine("\"Color\",\"Green\",\"أخضر\",\"Inactive\"");
                        templateCsv.AppendLine("\"Material\",\"Cotton\",\"قطن\",\"Active\"");
                        templateCsv.AppendLine("\"Material\",\"Polyester\",\"بوليستر\",\"Active\"");

                        await File.WriteAllTextAsync(saveFileDialog.FileName, templateCsv.ToString());
                        
                        var successDialog = new MessageDialog(
                            "Template Downloaded",
                            $"Template downloaded successfully to:\n\n{saveFileDialog.FileName}\n\n" +
                            "📝 TEMPLATE INSTRUCTIONS:\n\n" +
                            "REQUIRED FIELDS (marked with *):\n" +
                            "• AttributeName* - The attribute this value belongs to (e.g., Size, Color)\n" +
                            "  → If attribute doesn't exist, it will be created automatically\n" +
                            "• ValueName* - The specific value (e.g., Small, Red, Cotton)\n\n" +
                            "OPTIONAL FIELDS:\n" +
                            "• ValueArabic - Arabic translation of the value (leave empty if not needed)\n" +
                            "• Status - 'Active' or 'Inactive' (default: Active)\n\n" +
                            "⚠️ IMPORTANT:\n" +
                            "- Required fields cannot be empty\n" +
                            "- Import will fail for rows with missing required fields\n" +
                            "- You can add multiple values for the same attribute\n\n" +
                            "Please fill in your data and use the Import function to upload.",
                            MessageDialog.MessageType.Success);
                        successDialog.ShowDialog();
                    }
                    return;
                }

                // Upload File
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                    DefaultExt = ".csv"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    IsLoading = true;
                    StatusMessage = "Importing product attributes...";

                    var lines = await File.ReadAllLinesAsync(openFileDialog.FileName);
                    if (lines.Length <= 1)
                    {
                        var warningDialog = new MessageDialog(
                            "Import Error",
                            "The CSV file is empty or contains only headers.",
                            MessageDialog.MessageType.Warning);
                        warningDialog.ShowDialog();
                        return;
                    }

                    int successCount = 0;
                    int errorCount = 0;
                    var errors = new StringBuilder();

                    // Load existing attributes to find or create parent attributes
                    var existingAttributesList = await _attributeService.GetAllAttributesAsync();
                    var existingAttributes = existingAttributesList.ToDictionary(a => a.Name, a => a.Id, StringComparer.OrdinalIgnoreCase);

                    // Skip header row
                    for (int i = 1; i < lines.Length; i++)
                    {
                        try
                        {
                            var line = lines[i];
                            if (string.IsNullOrWhiteSpace(line)) continue;

                            var values = ParseCsvLine(line);
                            if (values.Length < 4)
                            {
                                errorCount++;
                                errors.AppendLine($"Line {i + 1}: Invalid format (expected 4 columns: AttributeName,ValueName,ValueArabic,Status)");
                                continue;
                            }

                            var attributeName = values[0].Trim('"').Trim();
                            var valueName = values[1].Trim('"').Trim();
                            var valueArabic = string.IsNullOrWhiteSpace(values[2].Trim('"')) ? null : values[2].Trim('"').Trim();
                            var status = string.IsNullOrWhiteSpace(values[3].Trim('"')) ? "Active" : values[3].Trim('"').Trim();

                            // Validate required fields
                            if (string.IsNullOrWhiteSpace(attributeName))
                            {
                                errorCount++;
                                errors.AppendLine($"Line {i + 1}: AttributeName is required and cannot be empty");
                                continue;
                            }

                            if (string.IsNullOrWhiteSpace(valueName))
                            {
                                errorCount++;
                                errors.AppendLine($"Line {i + 1}: ValueName is required and cannot be empty");
                                continue;
                            }

                            // Get or create the parent attribute
                            int attributeId;
                            if (!existingAttributes.TryGetValue(attributeName, out attributeId))
                            {
                                // Create the parent attribute if it doesn't exist
                                var newAttribute = new ProductAttributeDto
                                {
                                    Name = attributeName,
                                    NameAr = null,
                                    Type = "Text",
                                    IsRequired = false,
                                    Status = "Active",
                                    CreatedBy = 1 // TODO: Get from current user
                                };
                                await _attributeService.AddAttributeAsync(newAttribute);
                                
                                // Re-fetch to get the ID since AddAttributeAsync doesn't return the created object
                                var createdAttributes = await _attributeService.GetAllAttributesAsync();
                                var createdAttribute = createdAttributes.FirstOrDefault(a => a.Name.Equals(attributeName, StringComparison.OrdinalIgnoreCase));
                                if (createdAttribute != null)
                                {
                                    attributeId = createdAttribute.Id;
                                    existingAttributes[attributeName] = attributeId;
                                }
                                else
                                {
                                    throw new Exception($"Failed to create attribute '{attributeName}'");
                                }
                            }

                            // Create the attribute value
                            var valueDto = new ProductAttributeValueDto
                            {
                                AttributeId = attributeId,
                                Value = valueName,
                                ValueAr = valueArabic,
                                Status = status
                            };

                            await _attributeService.AddValueAsync(valueDto);
                            successCount++;
                        }
                        catch (Exception ex)
                        {
                            errorCount++;
                            var errorMessage = ex.Message;
                            
                            // Include inner exception details if available
                            if (ex.InnerException != null)
                            {
                                errorMessage += $" | Inner: {ex.InnerException.Message}";
                                
                                // Go deeper if there's another inner exception
                                if (ex.InnerException.InnerException != null)
                                {
                                    errorMessage += $" | Details: {ex.InnerException.InnerException.Message}";
                                }
                            }
                            
                            errors.AppendLine($"Line {i + 1}: {errorMessage}");
                        }
                    }

                    await LoadAttributesAsync();

                    var message = $"Import completed:\n\n✓ {successCount} attribute values imported successfully";
                    if (errorCount > 0)
                    {
                        message += $"\n✗ {errorCount} errors occurred\n\nErrors:\n{errors}";
                    }

                    var resultDialog = new MessageDialog(
                        "Import Complete",
                        message,
                        errorCount > 0 ? MessageDialog.MessageType.Warning : MessageDialog.MessageType.Success);
                    resultDialog.ShowDialog();
                    
                    StatusMessage = $"Import completed: {successCount} successful, {errorCount} errors";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error importing attributes: {ex.Message}";
                
                var errorDialog = new MessageDialog(
                    "Import Error",
                    $"An error occurred while importing attributes:\n\n{ex.Message}",
                    MessageDialog.MessageType.Error);
                errorDialog.ShowDialog();
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Parse CSV line handling quoted values
        /// </summary>
        private string[] ParseCsvLine(string line)
        {
            var values = new List<string>();
            var currentValue = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    inQuotes = !inQuotes;
                    currentValue.Append(c);
                }
                else if (c == ',' && !inQuotes)
                {
                    values.Add(currentValue.ToString());
                    currentValue.Clear();
                }
                else
                {
                    currentValue.Append(c);
                }
            }

            values.Add(currentValue.ToString());
            return values.ToArray();
        }

        private void InitializePermissions()
        {
            try
            {
                CanCreateProductAttribute = _currentUserService.HasPermission(ScreenNames.PRODUCT_ATTRIBUTES, TypeMatrix.CREATE);
                CanEditProductAttribute = _currentUserService.HasPermission(ScreenNames.PRODUCT_ATTRIBUTES, TypeMatrix.UPDATE);
                CanDeleteProductAttribute = _currentUserService.HasPermission(ScreenNames.PRODUCT_ATTRIBUTES, TypeMatrix.DELETE);
                CanImportProductAttribute = _currentUserService.HasPermission(ScreenNames.PRODUCT_ATTRIBUTES, TypeMatrix.IMPORT);
                CanExportProductAttribute = _currentUserService.HasPermission(ScreenNames.PRODUCT_ATTRIBUTES, TypeMatrix.EXPORT);
            }
            catch (Exception)
            {
                CanCreateProductAttribute = false;
                CanEditProductAttribute = false;
                CanDeleteProductAttribute = false;
                CanImportProductAttribute = false;
                CanExportProductAttribute = false;
            }
        }
    }
}