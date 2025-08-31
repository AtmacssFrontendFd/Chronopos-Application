using CommunityToolkit.Mvvm.ComponentModel;

namespace ChronoPos.Desktop.ViewModels;

/// <summary>
/// ViewModel for customers management
/// </summary>
public partial class CustomersViewModel : ObservableObject
{
    [ObservableProperty]
    private string statusMessage = "Customers view loaded";

    public CustomersViewModel()
    {
        // Initialize customers view model
    }
}
