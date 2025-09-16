using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ChronoPos.Desktop.Views.Controls;

/// <summary>
/// A circular back button control with customizable appearance
/// Displays a back arrow icon in a circular background
/// </summary>
public partial class CircularBackButton : UserControl
{
    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register("Command", typeof(ICommand), typeof(CircularBackButton), new PropertyMetadata(null));

    public static readonly DependencyProperty CommandParameterProperty =
        DependencyProperty.Register("CommandParameter", typeof(object), typeof(CircularBackButton), new PropertyMetadata(null));

    public static readonly DependencyProperty ButtonSizeProperty =
        DependencyProperty.Register("ButtonSize", typeof(double), typeof(CircularBackButton), new PropertyMetadata(40.0));

    public static readonly DependencyProperty ButtonBackgroundProperty =
        DependencyProperty.Register("ButtonBackground", typeof(Brush), typeof(CircularBackButton), 
            new PropertyMetadata(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#007ACC"))));

    public static readonly DependencyProperty ButtonForegroundProperty =
        DependencyProperty.Register("ButtonForeground", typeof(Brush), typeof(CircularBackButton), 
            new PropertyMetadata(new SolidColorBrush(Colors.White)));

    public static readonly DependencyProperty ButtonHoverBackgroundProperty =
        DependencyProperty.Register("ButtonHoverBackground", typeof(Brush), typeof(CircularBackButton), 
            new PropertyMetadata(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#005A9B"))));

    public static readonly DependencyProperty ButtonHoverForegroundProperty =
        DependencyProperty.Register("ButtonHoverForeground", typeof(Brush), typeof(CircularBackButton), 
            new PropertyMetadata(new SolidColorBrush(Colors.White)));

    public static readonly DependencyProperty ButtonPressedBackgroundProperty =
        DependencyProperty.Register("ButtonPressedBackground", typeof(Brush), typeof(CircularBackButton), 
            new PropertyMetadata(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#004578"))));

    public static readonly DependencyProperty ButtonPressedForegroundProperty =
        DependencyProperty.Register("ButtonPressedForeground", typeof(Brush), typeof(CircularBackButton), 
            new PropertyMetadata(new SolidColorBrush(Colors.White)));

    public static readonly DependencyProperty ButtonIconProperty =
        DependencyProperty.Register("ButtonIcon", typeof(string), typeof(CircularBackButton), new PropertyMetadata("<"));

    public static readonly DependencyProperty IconSizeProperty =
        DependencyProperty.Register("IconSize", typeof(double), typeof(CircularBackButton), new PropertyMetadata(16.0));

    public static readonly DependencyProperty IconWeightProperty =
        DependencyProperty.Register("IconWeight", typeof(FontWeight), typeof(CircularBackButton), new PropertyMetadata(FontWeights.Bold));

    public static readonly DependencyProperty ButtonCornerRadiusProperty =
        DependencyProperty.Register("ButtonCornerRadius", typeof(CornerRadius), typeof(CircularBackButton), 
            new PropertyMetadata(new CornerRadius(20)));

    public static readonly DependencyProperty ButtonTooltipProperty =
        DependencyProperty.Register("ButtonTooltip", typeof(string), typeof(CircularBackButton), new PropertyMetadata("Go Back"));

    public static readonly DependencyProperty ButtonBorderThicknessProperty =
        DependencyProperty.Register("ButtonBorderThickness", typeof(Thickness), typeof(CircularBackButton), 
            new PropertyMetadata(new Thickness(0)));

    public static readonly DependencyProperty ButtonBorderBrushProperty =
        DependencyProperty.Register("ButtonBorderBrush", typeof(Brush), typeof(CircularBackButton), 
            new PropertyMetadata(Brushes.Transparent));

    public CircularBackButton()
    {
        InitializeComponent();
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
    /// Size of the circular button (width and height)
    /// </summary>
    public double ButtonSize
    {
        get => (double)GetValue(ButtonSizeProperty);
        set => SetValue(ButtonSizeProperty, value);
    }

    /// <summary>
    /// Background brush for the button
    /// </summary>
    public Brush ButtonBackground
    {
        get => (Brush)GetValue(ButtonBackgroundProperty);
        set => SetValue(ButtonBackgroundProperty, value);
    }

    /// <summary>
    /// Foreground brush for the button icon
    /// </summary>
    public Brush ButtonForeground
    {
        get => (Brush)GetValue(ButtonForegroundProperty);
        set => SetValue(ButtonForegroundProperty, value);
    }

    /// <summary>
    /// Background brush when button is hovered
    /// </summary>
    public Brush ButtonHoverBackground
    {
        get => (Brush)GetValue(ButtonHoverBackgroundProperty);
        set => SetValue(ButtonHoverBackgroundProperty, value);
    }

    /// <summary>
    /// Foreground brush when button is hovered
    /// </summary>
    public Brush ButtonHoverForeground
    {
        get => (Brush)GetValue(ButtonHoverForegroundProperty);
        set => SetValue(ButtonHoverForegroundProperty, value);
    }

    /// <summary>
    /// Background brush when button is pressed
    /// </summary>
    public Brush ButtonPressedBackground
    {
        get => (Brush)GetValue(ButtonPressedBackgroundProperty);
        set => SetValue(ButtonPressedBackgroundProperty, value);
    }

    /// <summary>
    /// Foreground brush when button is pressed
    /// </summary>
    public Brush ButtonPressedForeground
    {
        get => (Brush)GetValue(ButtonPressedForegroundProperty);
        set => SetValue(ButtonPressedForegroundProperty, value);
    }

    /// <summary>
    /// Icon to display (default is "<" for back arrow)
    /// </summary>
    public string ButtonIcon
    {
        get => (string)GetValue(ButtonIconProperty);
        set => SetValue(ButtonIconProperty, value);
    }

    /// <summary>
    /// Size of the icon
    /// </summary>
    public double IconSize
    {
        get => (double)GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }

    /// <summary>
    /// Font weight of the icon
    /// </summary>
    public FontWeight IconWeight
    {
        get => (FontWeight)GetValue(IconWeightProperty);
        set => SetValue(IconWeightProperty, value);
    }

    /// <summary>
    /// Corner radius of the button (for circular shape)
    /// </summary>
    public CornerRadius ButtonCornerRadius
    {
        get => (CornerRadius)GetValue(ButtonCornerRadiusProperty);
        set => SetValue(ButtonCornerRadiusProperty, value);
    }

    /// <summary>
    /// Tooltip text for the button
    /// </summary>
    public string ButtonTooltip
    {
        get => (string)GetValue(ButtonTooltipProperty);
        set => SetValue(ButtonTooltipProperty, value);
    }

    /// <summary>
    /// Border thickness of the button
    /// </summary>
    public Thickness ButtonBorderThickness
    {
        get => (Thickness)GetValue(ButtonBorderThicknessProperty);
        set => SetValue(ButtonBorderThicknessProperty, value);
    }

    /// <summary>
    /// Border brush of the button
    /// </summary>
    public Brush ButtonBorderBrush
    {
        get => (Brush)GetValue(ButtonBorderBrushProperty);
        set => SetValue(ButtonBorderBrushProperty, value);
    }
}