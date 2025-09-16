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

        public static readonly DependencyProperty ChooseColorLabelProperty =
            DependencyProperty.Register("ChooseColorLabel", typeof(string), typeof(ColorPicker),
                new FrameworkPropertyMetadata("Choose Color:"));

        public static readonly DependencyProperty SelectedColorLabelProperty =
            DependencyProperty.Register("SelectedColorLabel", typeof(string), typeof(ColorPicker),
                new FrameworkPropertyMetadata("Selected: "));

        public string SelectedColor
        {
            get { return (string)GetValue(SelectedColorProperty); }
            set { SetValue(SelectedColorProperty, value); }
        }

        public string ChooseColorLabel
        {
            get { return (string)GetValue(ChooseColorLabelProperty); }
            set { SetValue(ChooseColorLabelProperty, value); }
        }

        public string SelectedColorLabel
        {
            get { return (string)GetValue(SelectedColorLabelProperty); }
            set { SetValue(SelectedColorLabelProperty, value); }
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

        public string ChooseColorLabel => _colorPicker.ChooseColorLabel;
        public string SelectedColorLabel => _colorPicker.SelectedColorLabel;

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
