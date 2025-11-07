using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.Constants;
using ChronoPos.Desktop.Services;
using ChronoPos.Desktop.Views;
using ChronoPos.Desktop.Views.Dialogs;
using System.Collections.ObjectModel;
using System.Windows;

namespace ChronoPos.Desktop.ViewModels
{
    public partial class ProductModifierViewModel : ObservableObject
    {
        private readonly IProductModifierService _modifierService;
        private readonly IProductModifierGroupService _modifierGroupService;
        private readonly IProductModifierGroupItemService _modifierGroupItemService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ITaxTypeService _taxTypeService;
        private readonly IActiveCurrencyService _activeCurrencyService;
        private readonly Action? _navigateBack;

        #region Observable Properties - Modifier Groups

        [ObservableProperty]
        private ObservableCollection<ProductModifierGroupDto> _modifierGroups = new();

        [ObservableProperty]
        private ObservableCollection<ProductModifierGroupDto> _filteredModifierGroups = new();

        [ObservableProperty]
        private ProductModifierGroupDto? _selectedModifierGroup;

        #endregion

        #region Observable Properties - Modifiers

        [ObservableProperty]
        private ObservableCollection<ProductModifierDto> _modifiers = new();

        [ObservableProperty]
        private ObservableCollection<ProductModifierDto> _filteredModifiers = new();

        [ObservableProperty]
        private ProductModifierDto? _selectedModifier;

        #endregion

        #region Observable Properties - UI State

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
        private int _selectedTabIndex = 0; // 0 = Groups, 1 = Modifiers

        [ObservableProperty]
        private bool _canCreateProductModifier = false;

        [ObservableProperty]
        private bool _canEditProductModifier = false;

        [ObservableProperty]
        private bool _canDeleteProductModifier = false;

        [ObservableProperty]
        private bool _canImportProductModifier = false;

        [ObservableProperty]
        private bool _canExportProductModifier = false;

        #endregion

        #region Computed Properties

        public int TotalModifierGroups => ModifierGroups?.Count ?? 0;
        public int TotalModifiers => Modifiers?.Count ?? 0;
        public bool HasModifierGroups => FilteredModifierGroups != null && FilteredModifierGroups.Count > 0;
        public bool HasModifiers => FilteredModifiers != null && FilteredModifiers.Count > 0;
        
        /// <summary>
        /// Gets the active currency symbol for dynamic table headers
        /// </summary>
        public string ActiveCurrencySymbol => _activeCurrencyService?.CurrencySymbol ?? "$";

        #endregion

        public ProductModifierViewModel(
            IProductModifierService modifierService,
            IProductModifierGroupService modifierGroupService,
            IProductModifierGroupItemService modifierGroupItemService,
            ICurrentUserService currentUserService,
            ITaxTypeService taxTypeService,
            IActiveCurrencyService activeCurrencyService,
            Action? navigateBack = null)
        {
            _modifierService = modifierService ?? throw new ArgumentNullException(nameof(modifierService));
            _modifierGroupService = modifierGroupService ?? throw new ArgumentNullException(nameof(modifierGroupService));
            _modifierGroupItemService = modifierGroupItemService ?? throw new ArgumentNullException(nameof(modifierGroupItemService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _taxTypeService = taxTypeService ?? throw new ArgumentNullException(nameof(taxTypeService));
            _activeCurrencyService = activeCurrencyService ?? throw new ArgumentNullException(nameof(activeCurrencyService));
            _navigateBack = navigateBack;

            FileLogger.Log("🔧 ProductModifierViewModel constructor started");

            InitializePermissions();
            _ = Task.Run(LoadDataAsync);

            FileLogger.Log("🔧 ProductModifierViewModel constructor completed");
        }

        #region Property Change Handlers

        partial void OnSearchTextChanged(string value)
        {
            FilterData();
        }

        partial void OnShowActiveOnlyChanged(bool value)
        {
            FilterData();
        }

        partial void OnSelectedTabIndexChanged(int value)
        {
            // When tab changes, close side panel
            IsSidePanelVisible = false;
            SidePanelContent = null;
            StatusMessage = "Ready";
            
            // Reload data for the selected tab
            if (value == 0)
            {
                // Modifier Groups tab selected
                _ = LoadModifierGroupsAsync();
            }
            else if (value == 1)
            {
                // Modifiers tab selected
                _ = LoadModifiersAsync();
            }
        }

        #endregion

        #region Data Loading

        private async Task LoadDataAsync()
        {
            try
            {
                FileLogger.Log("🔄 LoadDataAsync started");
                IsLoading = true;
                LoadingMessage = "Loading product modifiers...";
                StatusMessage = "Loading...";

                // Load both modifiers and groups
                await Task.WhenAll(
                    LoadModifierGroupsAsync(),
                    LoadModifiersAsync()
                );

                StatusMessage = "Ready";
                FileLogger.Log($"✅ LoadDataAsync completed - {ModifierGroups.Count} groups, {Modifiers.Count} modifiers loaded");
            }
            catch (Exception ex)
            {
                FileLogger.Log($"❌ Error in LoadDataAsync: {ex.Message}");
                StatusMessage = $"Error loading data: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadModifierGroupsAsync()
        {
            try
            {
                FileLogger.Log("🔄 LoadModifierGroupsAsync started");
                var groups = await _modifierGroupService.GetAllAsync();
                FileLogger.Log($"🔄 Retrieved {groups.Count()} modifier groups from service");

                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ModifierGroups.Clear();
                    foreach (var group in groups)
                    {
                        ModifierGroups.Add(group);
                    }
                    FilterData();
                    OnPropertyChanged(nameof(TotalModifierGroups));
                    OnPropertyChanged(nameof(HasModifierGroups));
                });

                FileLogger.Log($"✅ LoadModifierGroupsAsync completed - {groups.Count()} groups loaded");
            }
            catch (Exception ex)
            {
                FileLogger.Log($"❌ Error in LoadModifierGroupsAsync: {ex.Message}");
                throw;
            }
        }

        private async Task LoadModifiersAsync()
        {
            try
            {
                FileLogger.Log("🔄 LoadModifiersAsync started");
                var modifiers = await _modifierService.GetAllAsync();
                FileLogger.Log($"🔄 Retrieved {modifiers.Count()} modifiers from service");

                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Modifiers.Clear();
                    foreach (var modifier in modifiers)
                    {
                        Modifiers.Add(modifier);
                    }
                    FilterData();
                    OnPropertyChanged(nameof(TotalModifiers));
                    OnPropertyChanged(nameof(HasModifiers));
                });

                FileLogger.Log($"✅ LoadModifiersAsync completed - {modifiers.Count()} modifiers loaded");
            }
            catch (Exception ex)
            {
                FileLogger.Log($"❌ Error in LoadModifiersAsync: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region Filtering

        private void FilterData()
        {
            try
            {
                FileLogger.Log($"🔍 FilterData started - Tab: {SelectedTabIndex}, SearchText: '{SearchText}', ShowActiveOnly: {ShowActiveOnly}");

                if (SelectedTabIndex == 0)
                {
                    FilterModifierGroups();
                }
                else
                {
                    FilterModifiers();
                }
            }
            catch (Exception ex)
            {
                FileLogger.Log($"❌ Error in FilterData: {ex.Message}");
            }
        }

        private void FilterModifierGroups()
        {
            try
            {
                var filtered = ModifierGroups.AsEnumerable();

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    filtered = filtered.Where(g =>
                        g.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                        (!string.IsNullOrEmpty(g.Description) && g.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                        g.SelectionType.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
                }

                // Apply active filter
                if (ShowActiveOnly)
                {
                    filtered = filtered.Where(g => g.Status == "Active");
                }

                FilteredModifierGroups.Clear();
                foreach (var group in filtered)
                {
                    FilteredModifierGroups.Add(group);
                }

                OnPropertyChanged(nameof(HasModifierGroups));
                FileLogger.Log($"🔍 FilterModifierGroups completed - {FilteredModifierGroups.Count} filtered from {ModifierGroups.Count} total");
            }
            catch (Exception ex)
            {
                FileLogger.Log($"❌ Error in FilterModifierGroups: {ex.Message}");
            }
        }

        private void FilterModifiers()
        {
            try
            {
                var filtered = Modifiers.AsEnumerable();

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    filtered = filtered.Where(m =>
                        m.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                        (!string.IsNullOrEmpty(m.Description) && m.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(m.Sku) && m.Sku.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(m.Barcode) && m.Barcode.Contains(SearchText, StringComparison.OrdinalIgnoreCase)));
                }

                // Apply active filter
                if (ShowActiveOnly)
                {
                    filtered = filtered.Where(m => m.Status == "Active");
                }

                FilteredModifiers.Clear();
                foreach (var modifier in filtered)
                {
                    FilteredModifiers.Add(modifier);
                }

                OnPropertyChanged(nameof(HasModifiers));
                FileLogger.Log($"🔍 FilterModifiers completed - {FilteredModifiers.Count} filtered from {Modifiers.Count} total");
            }
            catch (Exception ex)
            {
                FileLogger.Log($"❌ Error in FilterModifiers: {ex.Message}");
            }
        }

        #endregion

        #region Commands - Modifier Groups

        [RelayCommand]
        private void AddModifierGroup()
        {
            try
            {
                var sidePanelViewModel = new ProductModifierGroupSidePanelViewModel(
                    _modifierGroupService, 
                    _modifierGroupItemService,
                    _modifierService);
                
                FileLogger.Log("🔧 Subscribing to sidePanelViewModel events");
                sidePanelViewModel.ModifierGroupSaved += OnModifierGroupSaved;
                sidePanelViewModel.CloseRequested += OnSidePanelCloseRequested;

                var sidePanelView = new ProductModifierGroupSidePanelView();
                sidePanelView.DataContext = sidePanelViewModel;

                SidePanelContent = sidePanelView;
                IsSidePanelVisible = true;
                StatusMessage = "Add new modifier group";
                FileLogger.Log("📝 Opening add modifier group side panel");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error opening add form: {ex.Message}";
                FileLogger.Log($"❌ Error opening add modifier group form: {ex.Message}");
            }
        }

        [RelayCommand]
        private void EditModifierGroup(ProductModifierGroupDto? modifierGroup)
        {
            if (modifierGroup == null) return;

            try
            {
                var sidePanelViewModel = new ProductModifierGroupSidePanelViewModel(
                    _modifierGroupService,
                    _modifierGroupItemService,
                    _modifierService,
                    modifierGroup);

                sidePanelViewModel.ModifierGroupSaved += OnModifierGroupSaved;
                sidePanelViewModel.CloseRequested += OnSidePanelCloseRequested;

                var sidePanelView = new ProductModifierGroupSidePanelView();
                sidePanelView.DataContext = sidePanelViewModel;

                SidePanelContent = sidePanelView;
                IsSidePanelVisible = true;
                StatusMessage = $"Edit group: {modifierGroup.Name}";
                FileLogger.Log($"📝 Opening edit modifier group side panel for: {modifierGroup.Name}");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error opening edit form: {ex.Message}";
                FileLogger.Log($"❌ Error opening edit modifier group form: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task DeleteModifierGroup(ProductModifierGroupDto? modifierGroup)
        {
            if (modifierGroup == null) return;

            try
            {
                var result = new ConfirmationDialog(
                    "Confirm Delete",
                    $"Are you sure you want to delete the modifier group '{modifierGroup.Name}'?\n\nThis will also delete all items in this group. This action cannot be undone.",
                    ConfirmationDialog.DialogType.Warning).ShowDialog();

                if (result == true)
                {
                    IsLoading = true;
                    LoadingMessage = "Deleting modifier group...";

                    await _modifierGroupService.DeleteAsync(modifierGroup.Id);

                    StatusMessage = $"Deleted group: {modifierGroup.Name}";
                    FileLogger.Log($"🗑️ Deleted modifier group: {modifierGroup.Name}");

                    await LoadModifierGroupsAsync();
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error deleting modifier group: {ex.Message}";
                FileLogger.Log($"❌ Error deleting modifier group: {ex.Message}");
                new MessageDialog("Error", $"Error deleting modifier group: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
            }
            finally
            {
                IsLoading = false;
            }
        }

        #endregion

        #region Commands - Modifiers

        [RelayCommand]
        private void AddModifier()
        {
            try
            {
                var sidePanelViewModel = new ProductModifierSidePanelViewModel(_modifierService, _taxTypeService);

                FileLogger.Log("🔧 Subscribing to sidePanelViewModel events");
                sidePanelViewModel.ModifierSaved += OnModifierSaved;
                sidePanelViewModel.CloseRequested += OnSidePanelCloseRequested;

                var sidePanelView = new ProductModifierSidePanelView();
                sidePanelView.DataContext = sidePanelViewModel;

                SidePanelContent = sidePanelView;
                IsSidePanelVisible = true;
                StatusMessage = "Add new modifier";
                FileLogger.Log("📝 Opening add modifier side panel");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error opening add form: {ex.Message}";
                FileLogger.Log($"❌ Error opening add modifier form: {ex.Message}");
            }
        }

        [RelayCommand]
        private void EditModifier(ProductModifierDto? modifier)
        {
            if (modifier == null) return;

            try
            {
                var sidePanelViewModel = new ProductModifierSidePanelViewModel(_modifierService, _taxTypeService, modifier);

                sidePanelViewModel.ModifierSaved += OnModifierSaved;
                sidePanelViewModel.CloseRequested += OnSidePanelCloseRequested;

                var sidePanelView = new ProductModifierSidePanelView();
                sidePanelView.DataContext = sidePanelViewModel;

                SidePanelContent = sidePanelView;
                IsSidePanelVisible = true;
                StatusMessage = $"Edit modifier: {modifier.Name}";
                FileLogger.Log($"📝 Opening edit modifier side panel for: {modifier.Name}");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error opening edit form: {ex.Message}";
                FileLogger.Log($"❌ Error opening edit modifier form: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task DeleteModifier(ProductModifierDto? modifier)
        {
            if (modifier == null) return;

            try
            {
                var result = new ConfirmationDialog(
                    "Confirm Delete",
                    $"Are you sure you want to delete the modifier '{modifier.Name}'?\n\nThis action cannot be undone.",
                    ConfirmationDialog.DialogType.Warning).ShowDialog();

                if (result == true)
                {
                    IsLoading = true;
                    LoadingMessage = "Deleting modifier...";

                    await _modifierService.DeleteAsync(modifier.Id);

                    StatusMessage = $"Deleted modifier: {modifier.Name}";
                    FileLogger.Log($"🗑️ Deleted modifier: {modifier.Name}");

                    await LoadModifiersAsync();
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error deleting modifier: {ex.Message}";
                FileLogger.Log($"❌ Error deleting modifier: {ex.Message}");
                new MessageDialog("Error", $"Error deleting modifier: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
            }
            finally
            {
                IsLoading = false;
            }
        }

        #endregion

        #region Commands - General

        [RelayCommand]
        private void FilterActive()
        {
            ShowActiveOnly = !ShowActiveOnly;
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
                FileLogger.Log("🔄 RefreshData command started");
                IsLoading = true;
                StatusMessage = "Refreshing data...";

                await LoadDataAsync();

                FileLogger.Log("✅ RefreshData command completed");
            }
            catch (Exception ex)
            {
                FileLogger.Log($"❌ Error in RefreshData: {ex.Message}");
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

        #endregion

        #region Event Handlers

        private async void OnModifierGroupSaved(object? sender, EventArgs e)
        {
            try
            {
                await LoadModifierGroupsAsync();
                StatusMessage = "Modifier group saved successfully";
                FileLogger.Log("✅ Modifier group saved event handled successfully");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error refreshing after save: {ex.Message}";
                FileLogger.Log($"❌ Error in OnModifierGroupSaved: {ex.Message}");
            }
        }

        private async void OnModifierSaved(object? sender, EventArgs e)
        {
            try
            {
                await LoadModifiersAsync();
                StatusMessage = "Modifier saved successfully";
                FileLogger.Log("✅ Modifier saved event handled successfully");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error refreshing after save: {ex.Message}";
                FileLogger.Log($"❌ Error in OnModifierSaved: {ex.Message}");
            }
        }

        private void OnSidePanelCloseRequested(object? sender, EventArgs e)
        {
            try
            {
                FileLogger.Log("🔄 Side panel close requested - starting close process");

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
            }
        }

        #endregion

        #region Permissions

        private void InitializePermissions()
        {
            try
            {
                // TODO: Add proper screen name constant when permission system is ready
                CanCreateProductModifier = true; // _currentUserService.HasPermission(ScreenNames.PRODUCT_MODIFIERS, TypeMatrix.CREATE);
                CanEditProductModifier = true; // _currentUserService.HasPermission(ScreenNames.PRODUCT_MODIFIERS, TypeMatrix.UPDATE);
                CanDeleteProductModifier = true; // _currentUserService.HasPermission(ScreenNames.PRODUCT_MODIFIERS, TypeMatrix.DELETE);
                CanImportProductModifier = true; // _currentUserService.HasPermission(ScreenNames.PRODUCT_MODIFIERS, TypeMatrix.IMPORT);
                CanExportProductModifier = true; // _currentUserService.HasPermission(ScreenNames.PRODUCT_MODIFIERS, TypeMatrix.EXPORT);
            }
            catch (Exception)
            {
                CanCreateProductModifier = false;
                CanEditProductModifier = false;
                CanDeleteProductModifier = false;
                CanImportProductModifier = false;
                CanExportProductModifier = false;
            }
        }

        #endregion
    }
}
