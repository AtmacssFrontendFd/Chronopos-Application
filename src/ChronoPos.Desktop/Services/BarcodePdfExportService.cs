using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ChronoPos.Application.DTOs;
using ChronoPos.Application.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ZXing;
using ZXing.Common;

namespace ChronoPos.Desktop.Services
{
    /// <summary>
    /// Service for exporting product barcodes to PDF format
    /// </summary>
    public class BarcodePdfExportService
    {
        public enum BarcodeFormat
        {
            CODE_128,
            EAN_13,
            QR_CODE
        }

        public enum PageOrientation
        {
            Portrait,
            Landscape
        }

        /// <summary>
        /// Export products with barcodes to PDF file
        /// </summary>
        public Task<string> ExportProductBarcodesToPdf(
            IEnumerable<ProductDto> products, 
            string filePath,
            BarcodeFormat barcodeFormat = BarcodeFormat.CODE_128,
            PageOrientation orientation = PageOrientation.Portrait)
        {
            return Task.Run(() =>
            {
                AppLogger.LogInfo("=== Starting PDF Barcode Export ===", null, "BarcodePdfExport");
                AppLogger.LogInfo($"Total products to export: {products.Count()}", null, "BarcodePdfExport");
                AppLogger.LogInfo($"Target file path: {filePath}", null, "BarcodePdfExport");
                AppLogger.LogInfo($"Barcode format: {barcodeFormat}", null, "BarcodePdfExport");
                AppLogger.LogInfo($"Page orientation: {orientation}", null, "BarcodePdfExport");

                try
                {
                    // Configure QuestPDF license (Community License)
                    QuestPDF.Settings.License = LicenseType.Community;

                    var document = Document.Create(container =>
                    {
                        container.Page(page =>
                        {
                            // Page setup
                            page.Margin(20);
                            page.Size(PageSizes.A4);
                            
                            if (orientation == PageOrientation.Landscape)
                            {
                                page.Size(PageSizes.A4.Landscape());
                            }

                            // Header
                            page.Header().Row(row =>
                            {
                                row.RelativeItem().Column(column =>
                                {
                                    column.Item().Text("ChronoPOS - Product Barcodes")
                                        .FontSize(16)
                                        .Bold()
                                        .FontColor(Colors.Blue.Darken2);
                                    
                                    column.Item().Text($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}")
                                        .FontSize(10)
                                        .FontColor(Colors.Grey.Darken1);
                                });
                            });

                            // Content - Barcode Grid
                            page.Content().PaddingVertical(10).Column(column =>
                            {
                                var itemsPerRow = orientation == PageOrientation.Portrait ? 3 : 4;
                                var productList = products.ToList();
                                
                                AppLogger.LogInfo($"Grid layout: {itemsPerRow} columns per row", null, "BarcodePdfExport");

                                // Create grid rows
                                for (int i = 0; i < productList.Count; i += itemsPerRow)
                                {
                                    var rowProducts = productList.Skip(i).Take(itemsPerRow).ToList();
                                    
                                    column.Item().Row(row =>
                                    {
                                        foreach (var product in rowProducts)
                                        {
                                            row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2)
                                                .Padding(8).Column(labelColumn =>
                                                {
                                                    // Product Name
                                                    var productName = product.Name?.Length > 30 
                                                        ? product.Name.Substring(0, 27) + "..." 
                                                        : product.Name ?? "Unknown";
                                                    
                                                    labelColumn.Item().Text(productName)
                                                        .FontSize(10)
                                                        .Bold()
                                                        .FontColor(Colors.Black);

                                                    // SKU/Barcode
                                                    labelColumn.Item().PaddingTop(3).Text($"SKU: {product.Barcode ?? "N/A"}")
                                                        .FontSize(8)
                                                        .FontColor(Colors.Grey.Darken1);

                                                    // Barcode Image
                                                    if (!string.IsNullOrEmpty(product.Barcode))
                                                    {
                                                        try
                                                        {
                                                            var barcodeImageBytes = GenerateBarcodeImage(product.Barcode, barcodeFormat);
                                                            if (barcodeImageBytes != null && barcodeImageBytes.Length > 0)
                                                            {
                                                                labelColumn.Item().PaddingTop(5).Image(barcodeImageBytes);
                                                                AppLogger.LogInfo($"Barcode added for product {product.Id}: {product.Barcode}", null, "BarcodePdfExport");
                                                            }
                                                            else
                                                            {
                                                                labelColumn.Item().PaddingTop(5).Text("[Barcode Error]")
                                                                    .FontSize(8)
                                                                    .FontColor(Colors.Red.Medium);
                                                                AppLogger.LogError($"Failed to generate barcode for product {product.Id}", null, null, "BarcodePdfExport");
                                                            }
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            labelColumn.Item().PaddingTop(5).Text("[Barcode Error]")
                                                                .FontSize(8)
                                                                .FontColor(Colors.Red.Medium);
                                                            AppLogger.LogError($"Exception generating barcode for product {product.Id}: {ex.Message}", ex, null, "BarcodePdfExport");
                                                        }
                                                    }
                                                    else
                                                    {
                                                        labelColumn.Item().PaddingTop(5).Text("[No Barcode]")
                                                            .FontSize(8)
                                                            .FontColor(Colors.Grey.Medium);
                                                    }
                                                });
                                        }
                                        
                                        // Fill empty cells if last row is incomplete
                                        var emptyCells = itemsPerRow - rowProducts.Count;
                                        for (int j = 0; j < emptyCells; j++)
                                        {
                                            row.RelativeItem();
                                        }
                                    });

                                    // Add spacing between rows
                                    column.Item().PaddingVertical(5);
                                }
                            });

                            // Footer
                            page.Footer().AlignCenter().Text(text =>
                            {
                                text.Span("Page ");
                                text.CurrentPageNumber();
                                text.Span(" of ");
                                text.TotalPages();
                            });
                        });
                    });

                    // Generate PDF
                    AppLogger.LogInfo($"Generating PDF to: {filePath}", null, "BarcodePdfExport");
                    document.GeneratePdf(filePath);
                    AppLogger.LogInfo("PDF generated successfully", null, "BarcodePdfExport");
                    AppLogger.LogInfo("=== PDF Barcode Export Completed ===", null, "BarcodePdfExport");

                    return filePath;
                }
                catch (Exception ex)
                {
                    AppLogger.LogError($"Failed to generate PDF: {ex.Message}", ex, null, "BarcodePdfExport");
                    throw;
                }
            });
        }

        /// <summary>
        /// Generate barcode image as byte array
        /// </summary>
        private byte[] GenerateBarcodeImage(string data, BarcodeFormat format)
        {
            try
            {
                AppLogger.LogInfo($"Generating barcode image for: {data}, Format: {format}", null, "BarcodePdfExport");

                // Map enum to ZXing format
                ZXing.BarcodeFormat zxingFormat = format switch
                {
                    BarcodeFormat.EAN_13 => ZXing.BarcodeFormat.EAN_13,
                    BarcodeFormat.QR_CODE => ZXing.BarcodeFormat.QR_CODE,
                    _ => ZXing.BarcodeFormat.CODE_128
                };

                var writer = new BarcodeWriterPixelData
                {
                    Format = zxingFormat,
                    Options = new EncodingOptions
                    {
                        Height = 80,
                        Width = 250,
                        Margin = 2,
                        PureBarcode = false
                    }
                };

                var pixelData = writer.Write(data);

                // Convert pixel data to bitmap
                using (var bitmap = new Bitmap(pixelData.Width, pixelData.Height, PixelFormat.Format32bppRgb))
                {
                    var bitmapData = bitmap.LockBits(
                        new Rectangle(0, 0, pixelData.Width, pixelData.Height),
                        ImageLockMode.WriteOnly,
                        PixelFormat.Format32bppRgb);

                    try
                    {
                        System.Runtime.InteropServices.Marshal.Copy(
                            pixelData.Pixels, 
                            0, 
                            bitmapData.Scan0, 
                            pixelData.Pixels.Length);
                    }
                    finally
                    {
                        bitmap.UnlockBits(bitmapData);
                    }

                    // Convert to byte array
                    using (var ms = new MemoryStream())
                    {
                        bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        var bytes = ms.ToArray();
                        AppLogger.LogInfo($"Barcode image generated successfully. Size: {bytes.Length} bytes", null, "BarcodePdfExport");
                        return bytes;
                    }
                }
            }
            catch (Exception ex)
            {
                AppLogger.LogError($"Error generating barcode image: {ex.Message}", ex, null, "BarcodePdfExport");
                return null;
            }
        }
    }
}
