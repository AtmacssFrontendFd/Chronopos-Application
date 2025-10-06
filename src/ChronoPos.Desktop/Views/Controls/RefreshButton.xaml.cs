using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ChronoPos.Desktop.Views.Controls;

/// <summary>
/// Reusable button control similar to refresh button - for buttons like Save, Delete, etc.
/// Follows the same pattern as ManagementModuleButton with full settings integration
/// </summary>
public partial class RefreshButton : UserControl
{
    public static readonly DependencyProperty ButtonTextProperty =
        DependencyProperty.Register("ButtonText", typeof(string), typeof(RefreshButton), new PropertyMetadata("Refresh"));

    public static readonly DependencyProperty ButtonIconProperty =
        DependencyProperty.Register("ButtonIcon", typeof(string), typeof(RefreshButton), new PropertyMetadata("🔄"));

    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register("Command", typeof(ICommand), typeof(RefreshButton), new PropertyMetadata(null));

    public static readonly DependencyProperty CommandParameterProperty =
        DependencyProperty.Register("CommandParameter", typeof(object), typeof(RefreshButton), new PropertyMetadata(null));

    public static readonly DependencyProperty ButtonBackgroundProperty =
        DependencyProperty.Register("ButtonBackground", typeof(object), typeof(RefreshButton), new PropertyMetadata(null));

    public static readonly DependencyProperty ButtonForegroundProperty =
        DependencyProperty.Register("ButtonForeground", typeof(object), typeof(RefreshButton), new PropertyMetadata(null));

    public static readonly DependencyProperty MinButtonWidthProperty =
        DependencyProperty.Register("MinButtonWidth", typeof(double), typeof(RefreshButton), new PropertyMetadata(120.0));

    public static readonly DependencyProperty ButtonHeightProperty =
        DependencyProperty.Register("ButtonHeight", typeof(double), typeof(RefreshButton), new PropertyMetadata(40.0));

    public static readonly DependencyProperty ButtonWidthProperty =
        DependencyProperty.Register("ButtonWidth", typeof(double), typeof(RefreshButton), new PropertyMetadata(double.NaN));

    public RefreshButton()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Text to display on the button (e.g., "🔄 Refresh", "💾 Save", "🗑️ Delete")
    /// </summary>
    public string ButtonText
    {
        get => (string)GetValue(ButtonTextProperty);
        set => SetValue(ButtonTextProperty, value);
    }

    /// <summary>
    /// Icon to display on the button (e.g., "🔄", "💾", "🗑️")
    /// </summary>
    public string ButtonIcon
    {
        get => (string)GetValue(ButtonIconProperty);
        set => SetValue(ButtonIconProperty, value);
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
    /// Background brush for the button
    /// </summary>
    public object ButtonBackground
    {
        get => GetValue(ButtonBackgroundProperty);
        set => SetValue(ButtonBackgroundProperty, value);
    }

    /// <summary>
    /// Foreground brush for the button text
    /// </summary>
    public object ButtonForeground
    {
        get => GetValue(ButtonForegroundProperty);
        set => SetValue(ButtonForegroundProperty, value);
    }

    /// <summary>
    /// Minimum width of the button
    /// </summary>
    public double MinButtonWidth
    {
        get => (double)GetValue(MinButtonWidthProperty);
        set => SetValue(MinButtonWidthProperty, value);
    }

    /// <summary>
    /// Height of the button
    /// </summary>
    public double ButtonHeight
    {
        get => (double)GetValue(ButtonHeightProperty);
        set => SetValue(ButtonHeightProperty, value);
    }

    /// <summary>
    /// Width of the button (if not set, uses MinButtonWidth)
    /// </summary>
    public double ButtonWidth
    {
        get => (double)GetValue(ButtonWidthProperty);
        set => SetValue(ButtonWidthProperty, value);
    }
}
