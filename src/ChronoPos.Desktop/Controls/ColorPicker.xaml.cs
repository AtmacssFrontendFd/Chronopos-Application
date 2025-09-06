using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ChronoPos.Desktop.Controls
{
    /// <summary>
    /// Interaction logic for ColorPicker.xaml
    /// </summary>
    public partial class ColorPicker : UserControl
    {
        public static readonly DependencyProperty SelectedColorProperty =
            DependencyProperty.Register("SelectedColor", typeof(string), typeof(ColorPicker),
                new FrameworkPropertyMetadata("#FFC107", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public string SelectedColor
        {
            get { return (string)GetValue(SelectedColorProperty); }
            set { SetValue(SelectedColorProperty, value); }
        }

        public ColorPicker()
        {
            InitializeComponent();
            DataContext = new ColorPickerViewModel(this);
        }
    }

    public partial class ColorPickerViewModel : ObservableObject
    {
        private readonly ColorPicker _colorPicker;

        public ColorPickerViewModel(ColorPicker colorPicker)
        {
            _colorPicker = colorPicker;
        }

        public string SelectedColor
        {
            get => _colorPicker.SelectedColor;
            set => _colorPicker.SelectedColor = value;
        }

        [RelayCommand]
        private void SelectColor(string colorValue)
        {
            if (!string.IsNullOrEmpty(colorValue))
            {
                SelectedColor = colorValue;
                OnPropertyChanged(nameof(SelectedColor));
            }
        }
    }
}
