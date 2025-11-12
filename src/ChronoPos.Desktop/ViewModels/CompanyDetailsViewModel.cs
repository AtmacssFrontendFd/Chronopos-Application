using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.DTOs;
using System.Windows;
using SysApplication = System.Windows.Application;

namespace ChronoPos.Desktop.ViewModels;

/// <summary>
/// ViewModel for Company Details screen
/// </summary>
public partial class CompanyDetailsViewModel : ObservableObject
{
    private readonly ICompanyService _companyService;
    private int? _currentCompanyId;

    [ObservableProperty]
    private CompanyDto? _currentCompany;

    [ObservableProperty]
    private string _companyName = string.Empty;

    [ObservableProperty]
    private string? _licenseNumber;

    [ObservableProperty]
    private int _numberOfOwners = 1;

    [ObservableProperty]
    private string? _vatTrnNumber;

    [ObservableProperty]
    private string? _phoneNo;

    [ObservableProperty]
    private string? _emailOfBusiness;

    [ObservableProperty]
    private string? _website;

    [ObservableProperty]
    private string? _keyContactName;

    [ObservableProperty]
    private string? _keyContactMobNo;

    [ObservableProperty]
    private string? _keyContactEmail;

    [ObservableProperty]
    private string? _locationLatitude;

    [ObservableProperty]
    private string? _locationLongitude;

    [ObservableProperty]
    private string? _remarks;

    [ObservableProperty]
    private bool _status = true;

    [ObservableProperty]
    private string? _createdBy;

    [ObservableProperty]
    private DateTime _createdAt;

    [ObservableProperty]
    private string? _logoPath;

    [ObservableProperty]
    private FlowDirection _currentFlowDirection = FlowDirection.LeftToRight;

    [ObservableProperty]
    private bool _isLoading;

    public CompanyDetailsViewModel(ICompanyService companyService)
    {
        _companyService = companyService ?? throw new ArgumentNullException(nameof(companyService));
        _ = LoadCompanyDetailsAsync();
    }

    /// <summary>
    /// Load company details
    /// </summary>
    private async Task LoadCompanyDetailsAsync()
    {
        try
        {
            IsLoading = true;

            // Get the first company (assuming single company setup)
            var companies = await _companyService.GetActiveAsync();
            var company = companies.FirstOrDefault();

            if (company != null)
            {
                _currentCompanyId = company.Id;
                CurrentCompany = company;
                MapCompanyToProperties(company);
            }
        }
        catch (Exception ex)
        {
            SysApplication.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show($"Error loading company details: {ex.Message}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Map company DTO to properties
    /// </summary>
    private void MapCompanyToProperties(CompanyDto company)
    {
        CompanyName = company.CompanyName;
        LicenseNumber = company.LicenseNumber;
        NumberOfOwners = company.NumberOfOwners ?? 0;
        VatTrnNumber = company.VatTrnNumber;
        PhoneNo = company.PhoneNo;
        EmailOfBusiness = company.EmailOfBusiness;
        Website = company.Website;
        KeyContactName = company.KeyContactName;
        KeyContactMobNo = company.KeyContactMobNo;
        KeyContactEmail = company.KeyContactEmail;
        LocationLatitude = company.LocationLatitude?.ToString() ?? string.Empty;
        LocationLongitude = company.LocationLongitude?.ToString() ?? string.Empty;
        Remarks = company.Remarks;
        Status = company.Status;
        CreatedBy = company.CreatedBy;
        CreatedAt = company.CreatedAt;
        LogoPath = company.LogoPath;
    }

    /// <summary>
    /// Refresh company details
    /// </summary>
    [RelayCommand]
    private async Task RefreshCompanyDetailsAsync()
    {
        await LoadCompanyDetailsAsync();
    }

    /// <summary>
    /// Get status display text
    /// </summary>
    public string StatusText => Status ? "Active" : "Inactive";

    /// <summary>
    /// Get formatted created date
    /// </summary>
    public string CreatedAtText => CreatedAt.ToString("dd MMM yyyy");
}
