using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ChronoPos.Desktop.Views.Controls
{
    public partial class SettingsModuleButton : UserControl
    {
        public static readonly DependencyProperty ModuleTitleProperty =
            DependencyProperty.Register(nameof(ModuleTitle), typeof(string), typeof(SettingsModuleButton), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty ModuleDescriptionProperty =
            DependencyProperty.Register(nameof(ModuleDescription), typeof(string), typeof(SettingsModuleButton), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty ModuleTypeProperty =
            DependencyProperty.Register(nameof(ModuleType), typeof(string), typeof(SettingsModuleButton), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(SettingsModuleButton), new PropertyMetadata(null));

        public static readonly DependencyProperty IconBackgroundProperty =
            DependencyProperty.Register(nameof(IconBackground), typeof(Brush), typeof(SettingsModuleButton), new PropertyMetadata(new SolidColorBrush(Colors.Blue)));

        public static readonly DependencyProperty ButtonBackgroundProperty =
            DependencyProperty.Register(nameof(ButtonBackground), typeof(Brush), typeof(SettingsModuleButton), new PropertyMetadata(new SolidColorBrush(Colors.White)));

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(SettingsModuleButton), new PropertyMetadata(false));

        public string ModuleTitle
        {
            get { return (string)GetValue(ModuleTitleProperty); }
            set { SetValue(ModuleTitleProperty, value); }
        }

        public string ModuleDescription
        {
            get { return (string)GetValue(ModuleDescriptionProperty); }
            set { SetValue(ModuleDescriptionProperty, value); }
        }

        public string ModuleType
        {
            get { return (string)GetValue(ModuleTypeProperty); }
            set { SetValue(ModuleTypeProperty, value); }
        }

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public Brush IconBackground
        {
            get { return (Brush)GetValue(IconBackgroundProperty); }
            set { SetValue(IconBackgroundProperty, value); }
        }

        public Brush ButtonBackground
        {
            get { return (Brush)GetValue(ButtonBackgroundProperty); }
            set { SetValue(ButtonBackgroundProperty, value); }
        }

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public SettingsModuleButton()
        {
            InitializeComponent();
        }
    }
}
