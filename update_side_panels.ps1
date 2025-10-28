# Script to update all side panel controls with clean design
$files = @(
    "TaxTypeSidePanelControl.xaml",
    "PaymentTypeSidePanelControl.xaml",
    "PriceTypeSidePanelControl.xaml",
    "CategorySidePanelControl.xaml",
    "BrandSidePanelControl.xaml"
)

$basePath = "src\ChronoPos.Desktop\UserControls"

foreach ($file in $files) {
    $filePath = Join-Path $basePath $file
    if (Test-Path $filePath) {
        Write-Host "Updating $file..." -ForegroundColor Cyan
        
        $content = Get-Content $filePath -Raw
        
        # Update Input Field Style
        $content = $content -replace 'MinHeight.*Value="\{DynamicResource ButtonHeight\}"', 'Height" Value="40"'
        $content = $content -replace 'Padding.*Value="\{DynamicResource DefaultPaddingThickness\}"', 'Padding" Value="10,8"'
        $content = $content -replace '(<Style x:Key="InputFieldStyle"[^>]*>.*?<Setter Property="Background").*?Value="\{DynamicResource SurfaceBackground\}"', '$1 Value="White"'
        $content = $content -replace '(InputFieldStyle"[^>]*>.*?<Setter Property="Foreground").*?Value="\{DynamicResource TextPrimary\}"', '$1 Value="Black"'
        $content = $content -replace '(InputFieldStyle"[^>]*>.*?<Setter Property="BorderBrush").*?Value="\{DynamicResource BorderLight\}"', '$1 Value="Gray"'
        $content = $content -replace '(InputFieldStyle"[^>]*>.*?<Setter Property="FontSize").*?Value="\{DynamicResource FontSizeSmall\}"', '$1 Value="14"'
        
        # Update margins from 20 to 15
        $content = $content -replace 'Margin="0,0,0,20"', 'Margin="0,0,0,15"'
        
        # Update Section Header FontSize
        $content = $content -replace '(<Style x:Key="SectionHeaderStyle"[^>]*>.*?<Setter Property="FontSize").*?Value="\{DynamicResource FontSizeMedium\}"', '$1 Value="18"'
        
        # Save updated content
        Set-Content -Path $filePath -Value $content -NoNewline
        Write-Host "Updated $file successfully!" -ForegroundColor Green
    } else {
        Write-Host "File not found: $filePath" -ForegroundColor Red
    }
}

Write-Host "`nAll files updated!" -ForegroundColor Green
