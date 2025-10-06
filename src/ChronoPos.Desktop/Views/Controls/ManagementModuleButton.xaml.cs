using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ChronoPos.Desktop.Views.Controls;

/// <summary>
/// Interaction logic for ManagementModuleButton.xaml
/// A reusable button component for management modules with customizable colors and content
/// </summary>
public partial class ManagementModuleButton : UserControl
{
    #region Dependency Properties

    public static readonly DependencyProperty ModuleTitleProperty =
        DependencyProperty.Register(nameof(ModuleTitle), typeof(string), typeof(ManagementModuleButton), 
            new PropertyMetadata("Module"));

    public static readonly DependencyProperty ItemCountProperty =
        DependencyProperty.Register(nameof(ItemCount), typeof(int), typeof(ManagementModuleButton), 
            new PropertyMetadata(0));

    public static readonly DependencyProperty ItemCountLabelProperty =
        DependencyProperty.Register(nameof(ItemCountLabel), typeof(string), typeof(ManagementModuleButton), 
            new PropertyMetadata("Items"));

    public static readonly DependencyProperty ModuleTypeProperty =
        DependencyProperty.Register(nameof(ModuleType), typeof(string), typeof(ManagementModuleButton), 
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(ManagementModuleButton));

    public static readonly DependencyProperty IconBackgroundProperty =
        DependencyProperty.Register(nameof(IconBackground), typeof(Brush), typeof(ManagementModuleButton), 
            new PropertyMetadata(new SolidColorBrush(Colors.Gold)));

    public static readonly DependencyProperty ButtonBackgroundProperty =
        DependencyProperty.Register(nameof(ButtonBackground), typeof(Brush), typeof(ManagementModuleButton), 
            new PropertyMetadata(new SolidColorBrush(Colors.White)));

    public static readonly DependencyProperty IsSelectedProperty =
        DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(ManagementModuleButton), 
            new PropertyMetadata(false));

    #endregion

    #region Properties

    /// <summary>
    /// The title/name of the management module
    /// </summary>
    public string ModuleTitle
    {
        get => (string)GetValue(ModuleTitleProperty);
        set => SetValue(ModuleTitleProperty, value);
    }

    /// <summary>
    /// The number of items in this module
    /// </summary>
    public int ItemCount
    {
        get => (int)GetValue(ItemCountProperty);
        set => SetValue(ItemCountProperty, value);
    }

    /// <summary>
    /// The label for item count (e.g., "Items", "Products", "Customers")
    /// </summary>
    public string ItemCountLabel
    {
        get => (string)GetValue(ItemCountLabelProperty);
        set => SetValue(ItemCountLabelProperty, value);
    }

    /// <summary>
    /// The type/identifier of the module for command parameter
    /// </summary>
    public string ModuleType
    {
        get => (string)GetValue(ModuleTypeProperty);
        set => SetValue(ModuleTypeProperty, value);
    }

    /// <summary>
    /// The command to execute when the button is clicked
    /// </summary>
    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    /// <summary>
    /// The background color for the icon container
    /// </summary>
    public Brush IconBackground
    {
        get => (Brush)GetValue(IconBackgroundProperty);
        set => SetValue(IconBackgroundProperty, value);
    }

    /// <summary>
    /// The background color for the entire button
    /// </summary>
    public Brush ButtonBackground
    {
        get => (Brush)GetValue(ButtonBackgroundProperty);
        set => SetValue(ButtonBackgroundProperty, value);
    }

    /// <summary>
    /// Whether this module button is currently selected
    /// </summary>
    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    #endregion

    public ManagementModuleButton()
    {
        InitializeComponent();
    }
}
