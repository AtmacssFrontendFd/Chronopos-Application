using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ChronoPos.Desktop.Views.Controls;

/// <summary>
/// A generic sidebar navigation button with icon and text
/// Square layout with icon in circle on top and text below
/// </summary>
public partial class SidebarButton : UserControl
{
    #region Dependency Properties

    public static readonly DependencyProperty IconNameProperty =
        DependencyProperty.Register("IconName", typeof(string), typeof(SidebarButton), 
            new PropertyMetadata("home"));

    public static readonly DependencyProperty ButtonTextProperty =
        DependencyProperty.Register("ButtonText", typeof(string), typeof(SidebarButton), 
            new PropertyMetadata("Button"));

    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register("Command", typeof(ICommand), typeof(SidebarButton));

    public static readonly DependencyProperty CommandParameterProperty =
        DependencyProperty.Register("CommandParameter", typeof(object), typeof(SidebarButton));

    public static readonly DependencyProperty IsSelectedProperty =
        DependencyProperty.Register("IsSelected", typeof(bool), typeof(SidebarButton), 
            new PropertyMetadata(false));

    public static readonly DependencyProperty IconSizeProperty =
        DependencyProperty.Register("IconSize", typeof(double), typeof(SidebarButton), 
            new PropertyMetadata(20.0));

    public static readonly DependencyProperty CircleSizeProperty =
        DependencyProperty.Register("CircleSize", typeof(double), typeof(SidebarButton), 
            new PropertyMetadata(32.0));

    public static readonly DependencyProperty IsLogoutButtonProperty =
        DependencyProperty.Register("IsLogoutButton", typeof(bool), typeof(SidebarButton), 
            new PropertyMetadata(false));

    #endregion

    #region Properties

    /// <summary>
    /// Name of the Iconify icon to display
    /// </summary>
    public string IconName
    {
        get => (string)GetValue(IconNameProperty);
        set => SetValue(IconNameProperty, value);
    }

    /// <summary>
    /// Text to display below the icon
    /// </summary>
    public string ButtonText
    {
        get => (string)GetValue(ButtonTextProperty);
        set => SetValue(ButtonTextProperty, value);
    }

    /// <summary>
    /// Command to execute when button is clicked
    /// </summary>
    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    /// <summary>
    /// Parameter to pass to the command
    /// </summary>
    public object CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    /// <summary>
    /// Whether this button is currently selected/active
    /// </summary>
    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    /// <summary>
    /// Size of the icon in pixels
    /// </summary>
    public double IconSize
    {
        get => (double)GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }

    /// <summary>
    /// Size of the circle background for the icon
    /// </summary>
    public double CircleSize
    {
        get => (double)GetValue(CircleSizeProperty);
        set => SetValue(CircleSizeProperty, value);
    }

    /// <summary>
    /// Whether this is a logout button (uses Error color instead of Primary)
    /// </summary>
    public bool IsLogoutButton
    {
        get => (bool)GetValue(IsLogoutButtonProperty);
        set => SetValue(IsLogoutButtonProperty, value);
    }

    #endregion

    public SidebarButton()
    {
        InitializeComponent();
    }
}