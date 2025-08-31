using CommunityToolkit.Mvvm.ComponentModel;

namespace ChronoPos.Desktop.ViewModels;

/// <summary>
/// ViewModel for sales management
/// </summary>
public partial class SalesViewModel : ObservableObject
{
    [ObservableProperty]
    private string statusMessage = "Sales view loaded";

    public SalesViewModel()
    {
        // Initialize sales view model
    }
}
