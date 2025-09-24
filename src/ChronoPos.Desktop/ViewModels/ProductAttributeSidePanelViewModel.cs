using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Desktop.Services;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Windows;

namespace ChronoPos.Desktop.ViewModels
{
    public partial class ProductAttributeSidePanelViewModel : ObservableValidator
    {
        private readonly IProductAttributeService _attributeService;
        private readonly bool _isEditMode;
        private readonly int _editingAttributeId;
        
        // Additional fields for value editing
        private int _editingValueId = 0;
        private bool _isEditingValue = false;
        private int _attributeIdToSelect = 0;

        // Events
        public event EventHandler? AttributeSaved;
        public event EventHandler? CloseRequested;

        #region Observable Properties

        [ObservableProperty]
        private string _formTitle = "Add New Attribute";

        [ObservableProperty]
        private int _selectedTabIndex = 0;

        // Attribute Form
        [ObservableProperty]
        [Required(ErrorMessage = "Attribute name is required")]
        [StringLength(100, ErrorMessage = "Name must be less than 100 characters")]
        private string _attributeName = string.Empty;

        [ObservableProperty]
        [StringLength(100, ErrorMessage = "Arabic name must be less than 100 characters")]
        private string _attributeNameAr = string.Empty;

        [ObservableProperty]
        private bool _isRequired = false;

        [ObservableProperty]
        private string _selectedAttributeType = "Custom";

        [ObservableProperty]
        private string _selectedAttributeStatus = "Active";

        // Attribute Value Form
        [ObservableProperty]
        private ProductAttributeDto? _selectedAttributeForValue;

        [ObservableProperty]
        [StringLength(100, ErrorMessage = "Value must be less than 100 characters")]
        private string _attributeValue = string.Empty;

        [ObservableProperty]
        [StringLength(100, ErrorMessage = "Arabic value must be less than 100 characters")]
        private string _attributeValueAr = string.Empty;

        [ObservableProperty]
        private string _selectedValueStatus = "Active";

        // Collections
        [ObservableProperty]
        private ObservableCollection<string> _attributeTypes = new();

        [ObservableProperty]
        private ObservableCollection<string> _statusOptions = new();

        [ObservableProperty]
        private ObservableCollection<ProductAttributeDto> _availableAttributes = new();

        // UI States
        [ObservableProperty]
        private bool _isLoading = false;

        [ObservableProperty]
        private string _statusMessage = "Ready";

        [ObservableProperty]
        private bool _canSaveAttribute = true;

        [ObservableProperty]
        private bool _canSaveValue = true;

        #endregion

        public ProductAttributeSidePanelViewModel(IProductAttributeService attributeService)
        {
            _attributeService = attributeService;
            _isEditMode = false;
            _editingAttributeId = 0;

            FileLogger.Log("🔧 ProductAttributeSidePanelViewModel constructor started");
            
            InitializeCollections();
            _ = LoadAvailableAttributesAsync();
            
            FileLogger.Log("🔧 ProductAttributeSidePanelViewModel constructor completed - CancelCommand should be available");
        }

        public ProductAttributeSidePanelViewModel(IProductAttributeService attributeService, ProductAttributeDto attribute)
        {
            _attributeService = attributeService;
            _isEditMode = true;
            _editingAttributeId = attribute.Id;

            InitializeCollections();
            LoadAttributeForEdit(attribute);
            _ = LoadAvailableAttributesAsync();
        }

        // Constructor for editing attribute values
        public ProductAttributeSidePanelViewModel(IProductAttributeService attributeService, ProductAttributeValueDto attributeValue)
        {
            _attributeService = attributeService;
            _isEditMode = false; // Not editing the attribute itself
            _editingAttributeId = 0;

            InitializeCollections();
            LoadAttributeValueForEdit(attributeValue);
            _ = LoadAvailableAttributesAsync();
        }

        private void InitializeCollections()
        {
            AttributeTypes.Clear();
            AttributeTypes.Add("Color");
            AttributeTypes.Add("Size");
            AttributeTypes.Add("Material");
            AttributeTypes.Add("Custom");

            StatusOptions.Clear();
            StatusOptions.Add("Active");
            StatusOptions.Add("Inactive");
        }

        private void LoadAttributeForEdit(ProductAttributeDto attribute)
        {
            FormTitle = "Edit Attribute";
            AttributeName = attribute.Name;
            AttributeNameAr = attribute.NameAr ?? string.Empty;
            IsRequired = attribute.IsRequired;
            SelectedAttributeType = attribute.Type;
            SelectedAttributeStatus = attribute.Status;
        }

        private void LoadAttributeValueForEdit(ProductAttributeValueDto attributeValue)
        {
            FormTitle = "Edit Attribute Value";
            
            // Switch to the Value tab
            SelectedTabIndex = 1; // Assuming 1 is the Value tab index
            
            // Set the value fields
            AttributeValue = attributeValue.Value;
            AttributeValueAr = attributeValue.ValueAr ?? string.Empty;
            SelectedValueStatus = attributeValue.Status;
            
            // Store the attribute ID to select later when attributes are loaded
            _attributeIdToSelect = attributeValue.AttributeId;
            
            // Store the value ID for updating
            _editingValueId = attributeValue.Id;
            _isEditingValue = true;
        }

        private async Task LoadAvailableAttributesAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Loading attributes...";
                
                var attributes = await _attributeService.GetAllAttributesAsync();
                
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    AvailableAttributes.Clear();
                    foreach (var attr in attributes.Where(a => a.Status == "Active"))
                    {
                        AvailableAttributes.Add(attr);
                    }
                    
                    // If we have an attribute ID to select (for editing values), find and select it
                    if (_attributeIdToSelect > 0)
                    {
                        var attributeToSelect = AvailableAttributes.FirstOrDefault(a => a.Id == _attributeIdToSelect);
                        if (attributeToSelect != null)
                        {
                            SelectedAttributeForValue = attributeToSelect;
                        }
                    }
                    // Set default selection for value form if no specific attribute to select
                    else if (AvailableAttributes.Any() && SelectedAttributeForValue == null)
                    {
                        SelectedAttributeForValue = AvailableAttributes.First();
                    }
                    
                    StatusMessage = "Ready";
                });
            }
            catch (Exception ex)
            {
                FileLogger.Log($"❌ Error loading attributes: {ex.Message}");
                StatusMessage = $"Error loading attributes: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task SaveAttribute()
        {
            try
            {
                // Validate only attribute-specific fields
                if (string.IsNullOrWhiteSpace(AttributeName))
                {
                    StatusMessage = "Please enter attribute name";
                    return;
                }

                if (AttributeName.Length > 100)
                {
                    StatusMessage = "Attribute name must be less than 100 characters";
                    return;
                }

                if (!string.IsNullOrWhiteSpace(AttributeNameAr) && AttributeNameAr.Length > 100)
                {
                    StatusMessage = "Arabic name must be less than 100 characters";
                    return;
                }

                IsLoading = true;
                CanSaveAttribute = false;
                StatusMessage = _isEditMode ? "Updating attribute..." : "Creating attribute...";

                var dto = new ProductAttributeDto
                {
                    Id = _isEditMode ? _editingAttributeId : 0,
                    Name = AttributeName.Trim(),
                    NameAr = string.IsNullOrWhiteSpace(AttributeNameAr) ? null : AttributeNameAr.Trim(),
                    IsRequired = IsRequired,
                    Type = SelectedAttributeType,
                    Status = SelectedAttributeStatus,
                    CreatedBy = 1, // TODO: Get from current user
                    CreatedAt = _isEditMode ? DateTime.UtcNow : DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                if (_isEditMode)
                {
                    await _attributeService.UpdateAttributeAsync(dto);
                    FileLogger.Log($"✅ Updated attribute: {dto.Name}");
                }
                else
                {
                    await _attributeService.AddAttributeAsync(dto);
                    FileLogger.Log($"✅ Created attribute: {dto.Name}");
                }

                StatusMessage = _isEditMode ? "Attribute updated successfully" : "Attribute created successfully";
                
                // Notify parent safely
                try
                {
                    AttributeSaved?.Invoke(this, EventArgs.Empty);
                }
                catch (Exception eventEx)
                {
                    FileLogger.Log($"❌ Error in AttributeSaved event: {eventEx.Message}");
                }
                
                // Close panel if not edit mode
                if (!_isEditMode)
                {
                    try
                    {
                        CloseRequested?.Invoke(this, EventArgs.Empty);
                    }
                    catch (Exception eventEx)
                    {
                        FileLogger.Log($"❌ Error in CloseRequested event: {eventEx.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error saving attribute: {ex.Message}";
                FileLogger.Log($"❌ Error saving attribute: {ex.Message}");
                FileLogger.Log($"❌ Stack trace: {ex.StackTrace}");
                
                // Don't show MessageBox to avoid potential threading issues
                // Just log the error and update status
            }
            finally
            {
                IsLoading = false;
                CanSaveAttribute = true;
            }
        }

        [RelayCommand]
        private async Task SaveAttributeValue()
        {
            try
            {
                if (SelectedAttributeForValue == null)
                {
                    StatusMessage = "Please select an attribute";
                    return;
                }

                if (string.IsNullOrWhiteSpace(AttributeValue))
                {
                    StatusMessage = "Please enter a value";
                    return;
                }

                IsLoading = true;
                CanSaveValue = false;
                
                if (_isEditingValue)
                {
                    StatusMessage = "Updating attribute value...";
                    
                    var dto = new ProductAttributeValueDto
                    {
                        Id = _editingValueId,
                        AttributeId = SelectedAttributeForValue.Id,
                        Value = AttributeValue.Trim(),
                        ValueAr = string.IsNullOrWhiteSpace(AttributeValueAr) ? null : AttributeValueAr.Trim(),
                        Status = SelectedValueStatus
                    };

                    await _attributeService.UpdateValueAsync(dto);
                    ChronoPos.Desktop.Services.FileLogger.Log($"✅ Updated attribute value: {dto.Value} for {SelectedAttributeForValue.Name}");

                    StatusMessage = "Attribute value updated successfully";
                }
                else
                {
                    StatusMessage = "Creating attribute value...";

                    var dto = new ProductAttributeValueDto
                    {
                        AttributeId = SelectedAttributeForValue.Id,
                        Value = AttributeValue.Trim(),
                        ValueAr = string.IsNullOrWhiteSpace(AttributeValueAr) ? null : AttributeValueAr.Trim(),
                        Status = SelectedValueStatus,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _attributeService.AddValueAsync(dto);
                    ChronoPos.Desktop.Services.FileLogger.Log($"✅ Created attribute value: {dto.Value} for {SelectedAttributeForValue.Name}");

                    StatusMessage = "Attribute value created successfully";
                }
                
                // Clear form
                AttributeValue = string.Empty;
                AttributeValueAr = string.Empty;
                SelectedValueStatus = "Active";
                
                // Reset editing state
                _isEditingValue = false;
                _editingValueId = 0;
                
                // Notify parent
                AttributeSaved?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error saving value: {ex.Message}";
                FileLogger.Log($"❌ Error saving attribute value: {ex.Message}");
                FileLogger.Log($"❌ Stack trace: {ex.StackTrace}");
                
                // Don't show MessageBox to avoid potential threading issues
                // Just log the error and update status
            }
            finally
            {
                IsLoading = false;
                CanSaveValue = true;
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            try
            {
                FileLogger.Log("🔄 CANCEL COMMAND EXECUTED - Cancel button clicked - attempting to close side panel");
                
                // Reset any ongoing loading state
                IsLoading = false;
                StatusMessage = "Cancelled";
                
                FileLogger.Log($"🔄 CloseRequested event has {CloseRequested?.GetInvocationList().Length ?? 0} subscribers");
                
                // Trigger close event
                CloseRequested?.Invoke(this, EventArgs.Empty);
                
                FileLogger.Log("✅ Close request event invoked successfully");
            }
            catch (Exception ex)
            {
                FileLogger.Log($"❌ Error in Cancel command: {ex.Message}");
                FileLogger.Log($"❌ Stack trace: {ex.StackTrace}");
                
                // Force the close event even if there's an error
                try
                {
                    CloseRequested?.Invoke(this, EventArgs.Empty);
                }
                catch (Exception innerEx)
                {
                    FileLogger.Log($"❌ Error in fallback close request: {innerEx.Message}");
                }
            }
        }

        [RelayCommand]
        private void Reset()
        {
            if (_isEditMode) return;

            AttributeName = string.Empty;
            AttributeNameAr = string.Empty;
            IsRequired = false;
            SelectedAttributeType = "Custom";
            SelectedAttributeStatus = "Active";
            StatusMessage = "Form reset";
            
            ClearErrors();
        }

        [RelayCommand] 
        private void ResetValueForm()
        {
            AttributeValue = string.Empty;
            AttributeValueAr = string.Empty;
            SelectedValueStatus = "Active";
            if (AvailableAttributes.Any())
            {
                SelectedAttributeForValue = AvailableAttributes.First();
            }
            StatusMessage = "Value form reset";
        }
    }
}