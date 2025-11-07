using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Drawing;
using System.Drawing.Imaging;
using ChronoPos.Application.DTOs;
using ChronoPos.Application.Logging;
using ChronoPos.Application.Interfaces;
using ZXing;
using ZXing.Common;
using ClosedXML.Excel;

namespace ChronoPos.Desktop.Services
{
    /// <summary>
    /// Service for exporting product barcodes to Excel with generated barcode images
    /// </summary>
    public class BarcodeExportService : IBarcodeExportService
    {
        private readonly IActiveCurrencyService _activeCurrencyService;

        public BarcodeExportService(IActiveCurrencyService activeCurrencyService)
        {
            _activeCurrencyService = activeCurrencyService;
        }

        /// <summary>
        /// Exports products with barcodes to Excel file
        /// </summary>
        public Task<string> ExportProductBarcodesToExcel(IEnumerable<ProductDto> products, string filePath)
        {
            return Task.Run(() =>
            {
                AppLogger.LogInfo("=== Starting Barcode Export ===", null, "BarcodeExport");
                AppLogger.LogInfo($"Total products to export: {products.Count()}", null, "BarcodeExport");
                AppLogger.LogInfo($"Target file path: {filePath}", null, "BarcodeExport");
                
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Product Barcodes");

                // Set up headers
                worksheet.Cell(1, 1).Value = "Product ID";
                worksheet.Cell(1, 2).Value = "Barcode";
                worksheet.Cell(1, 3).Value = "Product Name";
                worksheet.Cell(1, 4).Value = "Category";
                worksheet.Cell(1, 5).Value = $"Price ({_activeCurrencyService.CurrencySymbol})";
                worksheet.Cell(1, 6).Value = "Stock";
                worksheet.Cell(1, 7).Value = "Barcode Image";

                // Style headers
                var headerRange = worksheet.Range(1, 1, 1, 7);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#4F46E5");
                headerRange.Style.Font.FontColor = XLColor.White;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                int row = 2;
                foreach (var product in products)
                {
                    // Add product data
                    worksheet.Cell(row, 1).Value = product.Id.ToString();
                    worksheet.Cell(row, 2).Value = product.Barcode ?? "N/A";
                    worksheet.Cell(row, 3).Value = product.Name ?? "Unknown";
                    worksheet.Cell(row, 4).Value = product.CategoryName ?? "Uncategorized";
                    worksheet.Cell(row, 5).Value = product.Price;
                    worksheet.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";
                    worksheet.Cell(row, 6).Value = product.StockQuantity;

                    // Generate and insert barcode image
                    if (!string.IsNullOrEmpty(product.Barcode))
                    {
                        AppLogger.LogInfo($"Processing barcode for product {product.Id}: {product.Barcode}", null, "BarcodeExport");
                        
                        try
                        {
                            var barcodeBitmap = GenerateBarcodeBitmap(product.Barcode);
                            if (barcodeBitmap != null)
                            {
                                AppLogger.LogInfo($"Barcode bitmap generated successfully. Size: {barcodeBitmap.Width}x{barcodeBitmap.Height}", null, "BarcodeExport");
                                
                                // Save barcode to temp file
                                var tempImagePath = Path.Combine(Path.GetTempPath(), $"barcode_{product.Id}_{Guid.NewGuid()}.png");
                                AppLogger.LogInfo($"Temp image path: {tempImagePath}", null, "BarcodeExport");
                                
                                using (barcodeBitmap)
                                {
                                    barcodeBitmap.Save(tempImagePath, ImageFormat.Png);
                                    AppLogger.LogInfo($"Barcode saved to temp file", null, "BarcodeExport");
                                }

                                // Verify file exists
                                if (File.Exists(tempImagePath))
                                {
                                    var fileInfo = new FileInfo(tempImagePath);
                                    AppLogger.LogInfo($"Temp file verified. Size: {fileInfo.Length} bytes", null, "BarcodeExport");
                                    
                                    // Insert image into Excel using stream to avoid file lock issues
                                    using (var stream = new FileStream(tempImagePath, FileMode.Open, FileAccess.Read))
                                    {
                                        var picture = worksheet.AddPicture(stream);
                                        AppLogger.LogInfo($"Picture added to worksheet", null, "BarcodeExport");
                                        
                                        // Position the image in column G (7)
                                        picture.MoveTo(worksheet.Cell(row, 7));
                                        AppLogger.LogInfo($"Picture moved to cell G{row}", null, "BarcodeExport");
                                        
                                        // Use actual size without scaling for best scannability
                                        // picture.Scale(1.0); // Full size for scanning
                                        AppLogger.LogInfo($"Picture placed at full size for scanning", null, "BarcodeExport");
                                    }
                                    
                                    // Note: Don't delete temp file yet, Excel needs it during save
                                }
                                else
                                {
                                    AppLogger.LogError($"Temp file not found after save: {tempImagePath}", null, null, "BarcodeExport");
                                    worksheet.Cell(row, 7).Value = "Image file not created";
                                }
                            }
                            else
                            {
                                AppLogger.LogError($"Barcode bitmap generation returned null for: {product.Barcode}", null, null, "BarcodeExport");
                                worksheet.Cell(row, 7).Value = "Barcode generation failed";
                            }
                        }
                        catch (Exception ex)
                        {
                            AppLogger.LogError($"Error processing barcode for product {product.Id}: {ex.Message}", ex, null, "BarcodeExport");
                            worksheet.Cell(row, 7).Value = $"Error: {ex.Message}";
                        }
                    }
                    else
                    {
                        AppLogger.LogInfo($"Product {product.Id} has no barcode", null, "BarcodeExport");
                        worksheet.Cell(row, 7).Value = "No Barcode";
                    }

                    // Set row height for barcode image - increased for better scanning
                    worksheet.Row(row).Height = 100; // Taller to fit larger barcode
                    
                    // Center align all cells vertically
                    var rowRange = worksheet.Range(row, 1, row, 7);
                    rowRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                    // Add borders
                    rowRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    rowRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    row++;
                    
                    // Add an empty row for spacing between products
                    worksheet.Row(row).Height = 20; // Small gap row
                    row++;
                }

                // Auto-fit columns
                worksheet.Column(1).Width = 12;
                worksheet.Column(2).Width = 22;
                worksheet.Column(3).Width = 30;
                worksheet.Column(4).Width = 20;
                worksheet.Column(5).Width = 15;
                worksheet.Column(6).Width = 10;
                worksheet.Column(7).Width = 45; // Much wider for full-size barcode image

                // Save workbook
                AppLogger.LogInfo($"Saving workbook to: {filePath}", null, "BarcodeExport");
                workbook.SaveAs(filePath);
                AppLogger.LogInfo($"Workbook saved successfully", null, "BarcodeExport");
                
                // Clean up temp barcode files
                try
                {
                    var tempPath = Path.GetTempPath();
                    var barcodeFiles = Directory.GetFiles(tempPath, "barcode_*.png");
                    AppLogger.LogInfo($"Cleaning up {barcodeFiles.Length} temp barcode files", null, "BarcodeExport");
                    foreach (var file in barcodeFiles)
                    {
                        try { File.Delete(file); } catch { }
                    }
                }
                catch (Exception ex)
                {
                    AppLogger.LogError($"Error cleaning temp files: {ex.Message}", ex, null, "BarcodeExport");
                }

                AppLogger.LogInfo("=== Barcode Export Completed ===", null, "BarcodeExport");
                return filePath;
            });
        }

        /// <summary>
        /// Generates barcode as System.Drawing.Bitmap for Excel embedding
        /// </summary>
        private System.Drawing.Bitmap? GenerateBarcodeBitmap(string barcodeText)
        {
            try
            {
                AppLogger.LogInfo($"Generating barcode for text: {barcodeText}", null, "BarcodeExport");
                
                // Use BarcodeWriter for System.Drawing.Bitmap
                var writer = new BarcodeWriterPixelData
                {
                    Format = BarcodeFormat.CODE_128,
                    Options = new EncodingOptions
                    {
                        Height = 150,  // Even taller for better scanning
                        Width = 500,   // Wider for better scanning
                        Margin = 20,   // More margin for cleaner barcode
                        PureBarcode = false
                    }
                };
                
                var pixelData = writer.Write(barcodeText);
                
                // Convert pixel data to Bitmap
                using (var bitmap = new System.Drawing.Bitmap(pixelData.Width, pixelData.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb))
                {
                    var bitmapData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, pixelData.Width, pixelData.Height),
                        System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                    try
                    {
                        System.Runtime.InteropServices.Marshal.Copy(pixelData.Pixels, 0, bitmapData.Scan0, pixelData.Pixels.Length);
                    }
                    finally
                    {
                        bitmap.UnlockBits(bitmapData);
                    }
                    
                    AppLogger.LogInfo($"Barcode bitmap generated: {bitmap != null}", null, "BarcodeExport");
                    return new System.Drawing.Bitmap(bitmap); // Return a copy to avoid dispose issues
                }
            }
            catch (Exception ex)
            {
                AppLogger.LogError($"Error generating barcode bitmap: {ex.Message}", ex, null, "BarcodeExport");
                return null;
            }
        }

        /// <summary>
        /// Exports products with barcodes to PDF (future enhancement)
        /// </summary>
        public async Task<string> ExportProductBarcodesToPdf(IEnumerable<ProductDto> products, string filePath)
        {
            // TODO: Implement PDF export using iTextSharp or similar
            await Task.CompletedTask;
            throw new NotImplementedException("PDF export will be implemented in future version");
        }
    }

    /// <summary>
    /// Interface for barcode export service
    /// </summary>
    public interface IBarcodeExportService
    {
        Task<string> ExportProductBarcodesToExcel(IEnumerable<ProductDto> products, string filePath);
        Task<string> ExportProductBarcodesToPdf(IEnumerable<ProductDto> products, string filePath);
    }
}
