# ChronoPos - Windows Desktop Point of Sale System

A modern Windows Desktop Point of Sale (POS) system built with .NET 9, WPF, Entity Framework Core, and clean architecture principles.

## Project Structure

```
ChronoPos/
├── src/
│   ├── ChronoPos.Domain/           # Core business entities and interfaces
│   ├── ChronoPos.Application/      # Application services and DTOs
│   ├── ChronoPos.Infrastructure/   # Data access and external services
│   └── ChronoPos.Desktop/         # WPF Desktop Application (Main UI)
├── tests/
│   ├── ChronoPos.Domain.Tests/     # Unit tests for domain layer
│   ├── ChronoPos.Application.Tests/# Unit tests for application layer
│   └── ChronoPos.Integration.Tests/# Integration tests
└── ChronoPos.sln
```

## Architecture

This project follows Clean Architecture principles with the following layers:

- **Domain Layer**: Contains business entities, value objects, and domain interfaces
- **Application Layer**: Contains application services, DTOs, and business logic
- **Infrastructure Layer**: Contains data access implementations, external services
- **Desktop Layer**: Contains WPF UI, ViewModels, and user interface logic

## Technologies

- .NET 9
- WPF (Windows Presentation Foundation)
- MVVM Pattern with CommunityToolkit.Mvvm
- Entity Framework Core
- SQL Server / LocalDB
- Dependency Injection with Microsoft.Extensions.Hosting
- xUnit for testing

## Getting Started

### Prerequisites

- .NET 9 SDK
- SQL Server or SQL Server LocalDB
- Windows 10/11
- Visual Studio 2022 (recommended) or VS Code

### Setup Instructions

1. **Clone the repository**

   ```powershell
   git clone <repository-url>
   cd chronopos
   ```

2. **Restore packages**

   ```powershell
   dotnet restore
   ```

3. **Create and run database migrations**

   ```powershell
   cd src\ChronoPos.Infrastructure
   dotnet ef migrations add InitialCreate --startup-project ..\ChronoPos.Desktop
   dotnet ef database update --startup-project ..\ChronoPos.Desktop
   ```

4. **Run the Desktop Application**
   ```powershell
   cd ..\..
   dotnet run --project src\ChronoPos.Desktop
   ```

### Running Tests

```powershell
# Run all tests
dotnet test

# Run specific test project
dotnet test tests\ChronoPos.Domain.Tests
```

## Features

### Core Entities

- **Products**: Manage inventory with categories, pricing, and stock levels
- **Categories**: Organize products into logical groups
- **Customers**: Store customer information and purchase history
- **Sales**: Process transactions with multiple payment methods
- **Sale Items**: Individual line items within a sale

### Desktop Application Interface

- **Dashboard** - Real-time business overview with statistics
- **Point of Sale** - Main transaction processing interface
- **Product Management** - Add, edit, and manage inventory
- **Customer Management** - Customer database and history
- **Sales History** - View and search past transactions
- **Reports** - Business analytics and insights
- **Settings** - Application configuration

### Technical Features

- **Offline Operation** - Works without internet connection
- **Real-time Updates** - Live clock and status information
- **Modern UI** - Clean, professional WPF interface
- **Keyboard Shortcuts** - Efficient operation for cashiers
- **Print Support** - Receipt and report printing
- **Data Export** - Export data to Excel/CSV formats

## Development Guidelines

### Coding Standards

- Use PascalCase for classes, methods, and properties
- Use camelCase for local variables and parameters
- Use meaningful and descriptive names
- Write XML documentation for public APIs
- Follow SOLID principles
- Use async/await for I/O operations
- Implement MVVM pattern consistently

### Project Organization

- **ViewModels** - Handle UI logic and data binding
- **Views** - XAML files for user interface
- **Services** - Business logic implementation
- **Models** - Data transfer objects and view models
- **Converters** - Value converters for data binding

## Deployment

### Building for Release

```powershell
dotnet publish src\ChronoPos.Desktop -c Release -r win-x64 --self-contained
```

### System Requirements

- Windows 10 version 1903 or later
- Windows 11 (recommended)
- .NET 9 Runtime (included in self-contained deployment)
- SQL Server Express LocalDB (included with .NET SDK)

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Ensure all tests pass
6. Submit a pull request

## License

This project is licensed under the MIT License - see the LICENSE file for details.
