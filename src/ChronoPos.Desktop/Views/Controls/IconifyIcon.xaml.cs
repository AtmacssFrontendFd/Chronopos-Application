using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ChronoPos.Desktop.Views.Controls;

/// <summary>
/// A reusable icon control that displays Iconify icons using WPF Geometry
/// Automatically scales with zoom and theme settings
/// </summary>
public partial class IconifyIcon : UserControl
{
    #region Dependency Properties

    public static readonly DependencyProperty IconNameProperty =
        DependencyProperty.Register("IconName", typeof(string), typeof(IconifyIcon), 
            new PropertyMetadata("unknown", OnIconNameChanged));

    public static readonly DependencyProperty IconSizeProperty =
        DependencyProperty.Register("IconSize", typeof(double), typeof(IconifyIcon), 
            new PropertyMetadata(16.0));

    public static readonly DependencyProperty IconColorProperty =
        DependencyProperty.Register("IconColor", typeof(Brush), typeof(IconifyIcon), 
            new PropertyMetadata(null));

    #endregion

    #region Properties

    /// <summary>
    /// Name of the icon to display (e.g., "home", "refresh", "add")
    /// </summary>
    public string IconName
    {
        get => (string)GetValue(IconNameProperty);
        set => SetValue(IconNameProperty, value);
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
    /// Color brush for the icon
    /// </summary>
    public Brush IconColor
    {
        get => (Brush)GetValue(IconColorProperty);
        set => SetValue(IconColorProperty, value);
    }

    #endregion

    public IconifyIcon()
    {
        InitializeComponent();
    }

    private static void OnIconNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is IconifyIcon iconControl)
        {
            iconControl.UpdateIcon();
        }
    }

    private void UpdateIcon()
    {
        if (string.IsNullOrEmpty(IconName))
            return;

        // Get the geometry resource key
        var resourceKey = $"Icon{IconName.ToUpperInvariant()}Geometry";
        
        // Try to find the geometry in application resources
        if (System.Windows.Application.Current?.Resources.Contains(resourceKey) == true)
        {
            var geometry = System.Windows.Application.Current.Resources[resourceKey] as Geometry;
            if (geometry != null && IconPath != null)
            {
                IconPath.Data = geometry;
                
                // Set default color if none is specified
                if (IconColor == null)
                {
                    IconPath.Fill = (Brush)System.Windows.Application.Current.Resources["TextPrimary"];
                }
            }
        }
    }

    private void IconifyIcon_Loaded(object sender, RoutedEventArgs e)
    {
        UpdateIcon();
    }
}