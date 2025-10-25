using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Desktop.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace ChronoPos.Desktop.ViewModels
{
    public partial class ProductModifierSidePanelViewModel : ObservableObject
    {
        private readonly IProductModifierService _modifierService;
        private readonly ITaxTypeService? _taxTypeService;
        private readonly bool _isEditMode;
        private readonly int _editingModifierId;

        // Events
        public event EventHandler? ModifierSaved;
        public event EventHandler? CloseRequested;

        #region Observable Properties

        [ObservableProperty]
        private string _formTitle = "Add New Modifier";

        [ObservableProperty]
        private string _modifierName = string.Empty;

        [ObservableProperty]
        private string _description = string.Empty;

        [ObservableProperty]
        private decimal _price = 0;

        [ObservableProperty]
        private decimal _cost = 0;

        [ObservableProperty]
        private string _sku = string.Empty;

        [ObservableProperty]
        private string _barcode = string.Empty;

        [ObservableProperty]
        private int? _selectedTaxTypeId;

        [ObservableProperty]
        private TaxTypeDto? _selectedTaxType;

        [ObservableProperty]
        private string _selectedStatus = "Active";

        [ObservableProperty]
        private ObservableCollection<TaxTypeDto> _taxTypes = new();

        [ObservableProperty]
        private ObservableCollection<string> _statusOptions = new();

        [ObservableProperty]
        private bool _isLoading = false;

        [ObservableProperty]
        private string _statusMessage = "Ready";

        [ObservableProperty]
        private bool _canSave = true;

        #endregion

        public ProductModifierSidePanelViewModel(IProductModifierService modifierService, ITaxTypeService taxTypeService)
        {
            _modifierService = modifierService;
            _taxTypeService = taxTypeService;
            _isEditMode = false;
            _editingModifierId = 0;

            FileLogger.Log("🔧 ProductModifierSidePanelViewModel constructor started (Add mode)");
            InitializeCollections();
            _ = LoadTaxTypesAsync();
        }

        public ProductModifierSidePanelViewModel(IProductModifierService modifierService, ITaxTypeService taxTypeService, ProductModifierDto modifier)
        {
            _modifierService = modifierService;
            _taxTypeService = taxTypeService;
            _isEditMode = true;
            _editingModifierId = modifier.Id;

            FileLogger.Log($"🔧 ProductModifierSidePanelViewModel constructor started (Edit mode) - ID: {modifier.Id}");
            InitializeCollections();
            LoadModifierForEdit(modifier);
            _ = LoadTaxTypesAsync();
        }

        private void InitializeCollections()
        {
            StatusOptions.Clear();
            StatusOptions.Add("Active");
            StatusOptions.Add("Inactive");
        }

        private void LoadModifierForEdit(ProductModifierDto modifier)
        {
            FormTitle = "Edit Modifier";
            ModifierName = modifier.Name;
            Description = modifier.Description ?? string.Empty;
            Price = modifier.Price;
            Cost = modifier.Cost;
            Sku = modifier.Sku ?? string.Empty;
            Barcode = modifier.Barcode ?? string.Empty;
            SelectedTaxTypeId = modifier.TaxTypeId;
            SelectedStatus = modifier.Status;
        }

        private async Task LoadTaxTypesAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Loading tax types...";

                if (_taxTypeService != null)
                {
                    var taxTypes = await _taxTypeService.GetAllTaxTypesAsync();

                    System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                    {
                        TaxTypes.Clear();
                        foreach (var taxType in taxTypes)
                        {
                            TaxTypes.Add(taxType);
                        }

                        // Set selected tax type if editing
                        if (_isEditMode && SelectedTaxTypeId.HasValue)
                        {
                            SelectedTaxType = TaxTypes.FirstOrDefault(t => t.Id == SelectedTaxTypeId.Value);
                        }
                    });

                    FileLogger.Log($"✅ Loaded {taxTypes.Count()} tax types");
                }
                else
                {
                    FileLogger.Log("⚠️ TaxTypeService is null");
                }

                StatusMessage = "Ready";
            }
            catch (Exception ex)
            {
                FileLogger.Log($"❌ Error loading tax types: {ex.Message}");
                StatusMessage = $"Error loading tax types: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task SaveModifier()
        {
            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(ModifierName))
                {
                    StatusMessage = "Please enter modifier name";
                    return;
                }

                if (ModifierName.Length > 100)
                {
                    StatusMessage = "Modifier name must be less than 100 characters";
                    return;
                }

                if (Price < 0)
                {
                    StatusMessage = "Price must be 0 or greater";
                    return;
                }

                if (Cost < 0)
                {
                    StatusMessage = "Cost must be 0 or greater";
                    return;
                }

                IsLoading = true;
                CanSave = false;
                StatusMessage = _isEditMode ? "Updating modifier..." : "Creating modifier...";

                var dto = new CreateProductModifierDto
                {
                    Name = ModifierName.Trim(),
                    Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
                    Price = Price,
                    Cost = Cost,
                    Sku = string.IsNullOrWhiteSpace(Sku) ? null : Sku.Trim(),
                    Barcode = string.IsNullOrWhiteSpace(Barcode) ? null : Barcode.Trim(),
                    TaxTypeId = SelectedTaxType?.Id,
                    Status = SelectedStatus,
                    CreatedBy = 1 // TODO: Get from current user
                };

                if (_isEditMode)
                {
                    var updateDto = new UpdateProductModifierDto
                    {
                        Id = _editingModifierId,
                        Name = dto.Name,
                        Description = dto.Description,
                        Price = dto.Price,
                        Cost = dto.Cost,
                        Sku = dto.Sku,
                        Barcode = dto.Barcode,
                        TaxTypeId = dto.TaxTypeId,
                        Status = dto.Status
                    };
                    await _modifierService.UpdateAsync(updateDto);
                    FileLogger.Log($"✅ Updated modifier: {dto.Name}");
                }
                else
                {
                    await _modifierService.CreateAsync(dto);
                    FileLogger.Log($"✅ Created modifier: {dto.Name}");
                }

                StatusMessage = _isEditMode ? "Modifier updated successfully" : "Modifier created successfully";

                // Notify parent
                try
                {
                    ModifierSaved?.Invoke(this, EventArgs.Empty);
                }
                catch (Exception eventEx)
                {
                    FileLogger.Log($"❌ Error in ModifierSaved event: {eventEx.Message}");
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
            catch (InvalidOperationException ex)
            {
                // Handle validation errors (SKU/Barcode duplicates)
                StatusMessage = ex.Message;
                FileLogger.Log($"⚠️ Validation error: {ex.Message}");
                MessageBox.Show(ex.Message, "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error saving modifier: {ex.Message}";
                FileLogger.Log($"❌ Error saving modifier: {ex.Message}");
                FileLogger.Log($"❌ Stack trace: {ex.StackTrace}");
                MessageBox.Show($"Error saving modifier: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
                CanSave = true;
            }
        }

        [RelayCommand]
        private void Reset()
        {
            ModifierName = string.Empty;
            Description = string.Empty;
            Price = 0;
            Cost = 0;
            Sku = string.Empty;
            Barcode = string.Empty;
            SelectedTaxType = null;
            SelectedStatus = "Active";
            StatusMessage = "Form reset";
        }

        [RelayCommand]
        private void Cancel()
        {
            try
            {
                FileLogger.Log("🔄 Cancel button clicked - closing side panel");
                CloseRequested?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                FileLogger.Log($"❌ Error in Cancel: {ex.Message}");
            }
        }

        [RelayCommand]
        private void GenerateSku()
        {
            try
            {
                // Generate a unique SKU based on modifier name or timestamp
                var prefix = !string.IsNullOrWhiteSpace(ModifierName) && ModifierName.Length >= 3
                    ? ModifierName.Substring(0, 3).ToUpper()
                    : "MOD";
                
                var timestamp = DateTime.Now.ToString("yyMMddHHmmss");
                Sku = $"{prefix}-{timestamp}";
                
                StatusMessage = "SKU generated successfully";
                FileLogger.Log($"✅ Generated SKU: {Sku}");
            }
            catch (Exception ex)
            {
                FileLogger.Log($"❌ Error generating SKU: {ex.Message}");
                StatusMessage = $"Error generating SKU: {ex.Message}";
            }
        }

        [RelayCommand]
        private void GenerateBarcode()
        {
            try
            {
                // Generate unique barcode using multiple formats for variety
                var random = new Random(Guid.NewGuid().GetHashCode());
                var barcodeTypes = new string[] { "EAN13", "CODE128", "TIMESTAMP" };
                var selectedType = barcodeTypes[random.Next(barcodeTypes.Length)];

                Barcode = selectedType switch
                {
                    "EAN13" => GenerateEAN13Barcode(),
                    "CODE128" => GenerateCode128Barcode(),
                    "TIMESTAMP" => GenerateTimestampBarcode(),
                    _ => GenerateTimestampBarcode()
                };
                
                StatusMessage = $"Barcode generated successfully ({selectedType})";
                FileLogger.Log($"✅ Generated {selectedType} barcode: {Barcode}");
            }
            catch (Exception ex)
            {
                FileLogger.Log($"❌ Error generating barcode: {ex.Message}");
                StatusMessage = $"Error generating barcode: {ex.Message}";
            }
        }

        private string GenerateEAN13Barcode()
        {
            // Generate EAN-13 format: 13 digits with timestamp for uniqueness
            var random = new Random(Guid.NewGuid().GetHashCode());
            var digits = new int[12]; // First 12 digits, 13th is check digit
            
            // Start with "2" for internal use (200-299 range is for store internal use)
            digits[0] = 2;
            
            // Add timestamp component for uniqueness (6 digits from current ticks)
            var timestamp = DateTime.Now.Ticks % 1000000;
            var timestampStr = timestamp.ToString("D6");
            for (int i = 0; i < 6; i++)
            {
                digits[i + 1] = int.Parse(timestampStr[i].ToString());
            }
            
            // Fill remaining 5 digits randomly
            for (int i = 7; i < 12; i++)
            {
                digits[i] = random.Next(0, 10);
            }
            
            // Calculate EAN-13 check digit
            int sum = 0;
            for (int i = 0; i < 12; i++)
            {
                sum += digits[i] * (i % 2 == 0 ? 1 : 3);
            }
            int checkDigit = (10 - (sum % 10)) % 10;
            
            return string.Join("", digits) + checkDigit;
        }

        private string GenerateCode128Barcode()
        {
            // Generate CODE-128 format: Alphanumeric with timestamp
            var random = new Random(Guid.NewGuid().GetHashCode());
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var timestamp = DateTime.Now.ToString("yyMMddHHmmss");
            
            var barcode = new System.Text.StringBuilder("MOD");
            
            // Add some random characters
            for (int i = 0; i < 4; i++)
            {
                barcode.Append(chars[random.Next(chars.Length)]);
            }
            
            // Add timestamp for uniqueness
            barcode.Append(timestamp);
            
            return barcode.ToString();
        }

        private string GenerateTimestampBarcode()
        {
            // Generate timestamp-based barcode with random suffix
            var random = new Random(Guid.NewGuid().GetHashCode());
            var timestamp = DateTime.Now.ToString("yyMMddHHmmssfff");
            var suffix = random.Next(1000, 9999);
            return $"M{timestamp}{suffix}";
        }
    }
}
