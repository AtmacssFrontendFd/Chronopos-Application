using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Desktop.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace ChronoPos.Desktop.ViewModels
{
    public partial class ProductModifierGroupSidePanelViewModel : ObservableObject
    {
        private readonly IProductModifierGroupService _groupService;
        private readonly IProductModifierGroupItemService _groupItemService;
        private readonly IProductModifierService _modifierService;
        private bool _isEditMode;
        private int _editingGroupId;

        // Events
        public event EventHandler? ModifierGroupSaved;
        public event EventHandler? CloseRequested;

        #region Observable Properties - Group Details

        [ObservableProperty]
        private string _formTitle = "Add New Modifier Group";

        [ObservableProperty]
        private int _selectedTabIndex = 0; // 0 = Group Details, 1 = Add Items

        [ObservableProperty]
        private string _groupName = string.Empty;

        [ObservableProperty]
        private string _description = string.Empty;

        [ObservableProperty]
        private string _selectedSelectionType = "Multiple";

        [ObservableProperty]
        private bool _isRequired = false;

        [ObservableProperty]
        private int _minSelections = 0;

        [ObservableProperty]
        private int? _maxSelections;

        [ObservableProperty]
        private string _selectedStatus = "Active";

        [ObservableProperty]
        private ObservableCollection<string> _selectionTypes = new();

        [ObservableProperty]
        private ObservableCollection<string> _statusOptions = new();

        #endregion

        #region Observable Properties - Group Items

        [ObservableProperty]
        private ObservableCollection<ProductModifierDto> _availableModifiers = new();

        [ObservableProperty]
        private ProductModifierDto? _selectedModifierToAdd;

        [ObservableProperty]
        private decimal _priceAdjustment = 0;

        [ObservableProperty]
        private int _sortOrder = 0;

        [ObservableProperty]
        private bool _defaultSelection = false;

        [ObservableProperty]
        private ObservableCollection<ProductModifierGroupItemDto> _groupItems = new();

        [ObservableProperty]
        private ProductModifierGroupItemDto? _selectedGroupItem;

        #endregion

        #region Observable Properties - UI State

        [ObservableProperty]
        private bool _isLoading = false;

        [ObservableProperty]
        private string _statusMessage = "Ready";

        [ObservableProperty]
        private bool _canSave = true;

        #endregion

        public ProductModifierGroupSidePanelViewModel(
            IProductModifierGroupService groupService,
            IProductModifierGroupItemService groupItemService,
            IProductModifierService modifierService)
        {
            _groupService = groupService;
            _groupItemService = groupItemService;
            _modifierService = modifierService;
            _isEditMode = false;
            _editingGroupId = 0;

            FileLogger.Log("🔧 ProductModifierGroupSidePanelViewModel constructor started (Add mode)");
            InitializeCollections();
            _ = LoadAvailableModifiersAsync();
        }

        public ProductModifierGroupSidePanelViewModel(
            IProductModifierGroupService groupService,
            IProductModifierGroupItemService groupItemService,
            IProductModifierService modifierService,
            ProductModifierGroupDto group)
        {
            _groupService = groupService;
            _groupItemService = groupItemService;
            _modifierService = modifierService;
            _isEditMode = true;
            _editingGroupId = group.Id;

            FileLogger.Log($"🔧 ProductModifierGroupSidePanelViewModel constructor started (Edit mode) - ID: {group.Id}");
            InitializeCollections();
            LoadGroupForEdit(group);
            _ = LoadAvailableModifiersAsync();
            _ = LoadGroupItemsAsync();
        }

        private void InitializeCollections()
        {
            SelectionTypes.Clear();
            SelectionTypes.Add("Single");
            SelectionTypes.Add("Multiple");

            StatusOptions.Clear();
            StatusOptions.Add("Active");
            StatusOptions.Add("Inactive");
        }

        private void LoadGroupForEdit(ProductModifierGroupDto group)
        {
            FormTitle = "Edit Modifier Group";
            GroupName = group.Name;
            Description = group.Description ?? string.Empty;
            SelectedSelectionType = group.SelectionType;
            IsRequired = group.Required;
            MinSelections = group.MinSelections;
            MaxSelections = group.MaxSelections;
            SelectedStatus = group.Status;
        }

        private async Task LoadAvailableModifiersAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Loading modifiers...";

                var modifiers = await _modifierService.GetAllAsync();

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    AvailableModifiers.Clear();
                    foreach (var modifier in modifiers.Where(m => m.Status == "Active"))
                    {
                        AvailableModifiers.Add(modifier);
                    }

                    if (AvailableModifiers.Any() && SelectedModifierToAdd == null)
                    {
                        SelectedModifierToAdd = AvailableModifiers.First();
                    }
                });

                StatusMessage = "Ready";
            }
            catch (Exception ex)
            {
                FileLogger.Log($"❌ Error loading modifiers: {ex.Message}");
                StatusMessage = $"Error loading modifiers: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadGroupItemsAsync()
        {
            if (!_isEditMode) return;

            try
            {
                IsLoading = true;
                StatusMessage = "Loading group items...";

                var items = await _groupItemService.GetByGroupIdAsync(_editingGroupId);

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    GroupItems.Clear();
                    foreach (var item in items)
                    {
                        GroupItems.Add(item);
                    }
                });

                StatusMessage = "Ready";
            }
            catch (Exception ex)
            {
                FileLogger.Log($"❌ Error loading group items: {ex.Message}");
                StatusMessage = $"Error loading group items: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task SaveGroup()
        {
            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(GroupName))
                {
                    StatusMessage = "Please enter group name";
                    return;
                }

                if (GroupName.Length > 100)
                {
                    StatusMessage = "Group name must be less than 100 characters";
                    return;
                }

                if (MinSelections < 0)
                {
                    StatusMessage = "Min selections must be 0 or greater";
                    return;
                }

                if (MaxSelections.HasValue && MaxSelections.Value < MinSelections)
                {
                    StatusMessage = "Max selections must be greater than or equal to min selections";
                    return;
                }

                IsLoading = true;
                CanSave = false;
                StatusMessage = _isEditMode ? "Updating group..." : "Creating group...";

                var dto = new CreateProductModifierGroupDto
                {
                    Name = GroupName.Trim(),
                    Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
                    SelectionType = SelectedSelectionType,
                    Required = IsRequired,
                    MinSelections = MinSelections,
                    MaxSelections = MaxSelections,
                    Status = SelectedStatus,
                    CreatedBy = 1 // TODO: Get from current user
                };

                if (_isEditMode)
                {
                    var updateDto = new UpdateProductModifierGroupDto
                    {
                        Id = _editingGroupId,
                        Name = dto.Name,
                        Description = dto.Description,
                        SelectionType = dto.SelectionType,
                        Required = dto.Required,
                        MinSelections = dto.MinSelections,
                        MaxSelections = dto.MaxSelections,
                        Status = dto.Status
                    };
                    await _groupService.UpdateAsync(updateDto);
                    FileLogger.Log($"✅ Updated modifier group: {dto.Name}");
                }
                else
                {
                    var createdGroup = await _groupService.CreateAsync(dto);
                    _editingGroupId = createdGroup.Id;
                    FileLogger.Log($"✅ Created modifier group: {dto.Name} with ID: {createdGroup.Id}");
                }

                StatusMessage = _isEditMode ? "Group updated successfully" : "Group created successfully";

                // Notify parent
                try
                {
                    ModifierGroupSaved?.Invoke(this, EventArgs.Empty);
                }
                catch (Exception eventEx)
                {
                    FileLogger.Log($"❌ Error in ModifierGroupSaved event: {eventEx.Message}");
                }

                // If new group, switch to items tab
                if (!_isEditMode)
                {
                    _isEditMode = true;
                    FormTitle = "Edit Modifier Group";
                    SelectedTabIndex = 1; // Switch to Add Items tab
                    await LoadGroupItemsAsync();
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error saving group: {ex.Message}";
                FileLogger.Log($"❌ Error saving group: {ex.Message}");
                FileLogger.Log($"❌ Stack trace: {ex.StackTrace}");
                MessageBox.Show($"Error saving group: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
                CanSave = true;
            }
        }

        [RelayCommand]
        private async Task AddItemToGroup()
        {
            try
            {
                if (!_isEditMode || _editingGroupId == 0)
                {
                    StatusMessage = "Please save the group first before adding items";
                    MessageBox.Show("Please save the group first before adding items", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (SelectedModifierToAdd == null)
                {
                    StatusMessage = "Please select a modifier to add";
                    return;
                }

                // Check if already exists
                if (GroupItems.Any(i => i.ModifierId == SelectedModifierToAdd.Id))
                {
                    StatusMessage = "This modifier is already in the group";
                    MessageBox.Show("This modifier is already in the group", "Duplicate", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                IsLoading = true;
                StatusMessage = "Adding modifier to group...";

                var itemDto = new CreateProductModifierGroupItemDto
                {
                    GroupId = _editingGroupId,
                    ModifierId = SelectedModifierToAdd.Id,
                    PriceAdjustment = PriceAdjustment,
                    SortOrder = SortOrder > 0 ? SortOrder : await GetNextSortOrderAsync(),
                    DefaultSelection = DefaultSelection,
                    Status = "Active"
                };

                await _groupItemService.CreateAsync(itemDto);
                FileLogger.Log($"✅ Added modifier {SelectedModifierToAdd.Name} to group");

                // Reload items
                await LoadGroupItemsAsync();

                // Reset form
                PriceAdjustment = 0;
                SortOrder = 0;
                DefaultSelection = false;
                StatusMessage = "Modifier added to group successfully";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error adding modifier: {ex.Message}";
                FileLogger.Log($"❌ Error adding modifier: {ex.Message}");
                MessageBox.Show($"Error adding modifier: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task RemoveItemFromGroup(ProductModifierGroupItemDto? item)
        {
            if (item == null) return;

            try
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to remove '{item.ModifierName}' from this group?",
                    "Confirm Remove",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    IsLoading = true;
                    StatusMessage = "Removing modifier from group...";

                    await _groupItemService.DeleteAsync(item.Id);
                    FileLogger.Log($"✅ Removed modifier {item.ModifierName} from group");

                    // Reload items
                    await LoadGroupItemsAsync();

                    StatusMessage = "Modifier removed from group successfully";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error removing modifier: {ex.Message}";
                FileLogger.Log($"❌ Error removing modifier: {ex.Message}");
                MessageBox.Show($"Error removing modifier: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task<int> GetNextSortOrderAsync()
        {
            try
            {
                var items = await _groupItemService.GetByGroupIdAsync(_editingGroupId);
                var maxSort = items.Any() ? items.Max(i => i.SortOrder) : 0;
                return maxSort + 1;
            }
            catch
            {
                return 1;
            }
        }

        [RelayCommand]
        private void Reset()
        {
            GroupName = string.Empty;
            Description = string.Empty;
            SelectedSelectionType = "Multiple";
            IsRequired = false;
            MinSelections = 0;
            MaxSelections = null;
            SelectedStatus = "Active";
            StatusMessage = "Form reset";
        }

        [RelayCommand]
        private void ResetItemForm()
        {
            PriceAdjustment = 0;
            SortOrder = 0;
            DefaultSelection = false;
            SelectedModifierToAdd = AvailableModifiers.FirstOrDefault();
            StatusMessage = "Item form reset";
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
    }
}
