using System.Windows;
using System.Windows.Controls;

namespace ChronoPos.Desktop.UserControls;

/// <summary>
/// Interaction logic for UserSidePanelControl.xaml
/// </summary>
public partial class UserSidePanelControl : UserControl
{
    public UserSidePanelControl()
    {
        InitializeComponent();
        Loaded += UserSidePanelControl_Loaded;
    }

    private void UserSidePanelControl_Loaded(object sender, RoutedEventArgs e)
    {
        // Clear password fields when control loads
        if (DataContext is ViewModels.UserSidePanelViewModel viewModel)
        {
            PasswordBox.Password = viewModel.Password ?? string.Empty;
            ConfirmPasswordBox.Password = viewModel.ConfirmPassword ?? string.Empty;
        }
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is ViewModels.UserSidePanelViewModel viewModel)
        {
            viewModel.Password = ((PasswordBox)sender).Password;
        }
    }

    private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is ViewModels.UserSidePanelViewModel viewModel)
        {
            viewModel.ConfirmPassword = ((PasswordBox)sender).Password;
        }
    }
}
