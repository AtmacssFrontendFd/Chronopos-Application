using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ChronoPos.Application.DTOs;
using ChronoPos.Desktop.ViewModels;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ChronoPos.Desktop.Services;

/// <summary>
/// KOT (Kitchen Order Ticket) printer using QuestPDF for thermal (80mm) printers
/// Generates professional KOT receipts with direct printing (no preview)
/// </summary>
public class QuestPdfKotPrinter
{
    public QuestPdfKotPrinter()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    /// <summary>
    /// Generate and auto-print KOT to default printer
    /// </summary>
    public string GenerateAndPrintKot(
        List<CartItemModel> items,
        RestaurantTableDto? table,
        string? customerName = null,
        string? notes = null)
    {
        try
        {
            // Create temp directory if it doesn't exist
            string tempFolder = Path.Combine(Path.GetTempPath(), "ChronoPosReceipts");
            Directory.CreateDirectory(tempFolder);

            // Generate unique filename
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            string fileName = $"KOT-{timestamp}.pdf";
            string filePath = Path.Combine(tempFolder, fileName);

            // Generate PDF
            var pdfBytes = GenerateKotPdf(items, table, customerName, notes);
            File.WriteAllBytes(filePath, pdfBytes);

            // Auto-print using default PDF printer
            PrintPdfSilently(filePath);

            return filePath;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to generate and print KOT: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Generate KOT PDF without printing
    /// </summary>
    public byte[] GenerateKotPdf(
        List<CartItemModel> items,
        RestaurantTableDto? table,
        string? customerName = null,
        string? notes = null)
    {
        // Thermal receipt settings (80mm paper)
        int receiptCharWidth = 44;
        int descWidth = 26;
        int qtyWidth = 8;

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                // 80mm thermal paper = 226 points width at 72 DPI
                page.Size(226, 1440); 
                page.Margin(5);
                page.DefaultTextStyle(x => x.FontFamily("Courier New").FontSize(8));
                page.Content().Column(col =>
                {
                    col.Spacing(2);

                    // Header
                    col.Item().Column(header =>
                    {
                        header.Item().Text("KITCHEN ORDER TICKET").SemiBold().FontSize(10).AlignCenter();
                        header.Item().Text(new string('=', receiptCharWidth)).FontSize(8).AlignLeft();
                    });

                    // Order details
                    col.Item().Column(info =>
                    {
                        string Label(string l) => (l + ":").PadRight(12);
                        info.Item().Text(Label("Date/Time") + DateTime.Now.ToString("dd-MMM-yyyy HH:mm"));
                        if (table != null) info.Item().Text(Label("Table") + table.DisplayName);
                        if (!string.IsNullOrEmpty(customerName)) info.Item().Text(Label("Customer") + customerName);
                        info.Item().Text(new string('-', receiptCharWidth));
                    });

                    // Items header
                    col.Item().Text(BuildKotHeader(descWidth, qtyWidth)).SemiBold();
                    col.Item().Text(new string('-', receiptCharWidth));

                    // Items list with sequential numbering
                    int serialNo = 1;
                    foreach (var item in items)
                    {
                        foreach (var line in BuildKotItemLines(item, serialNo, descWidth, qtyWidth))
                        {
                            col.Item().Text(line);
                        }
                        serialNo++;
                    }

                    col.Item().Text(new string('-', receiptCharWidth));

                    // Notes section
                    if (!string.IsNullOrEmpty(notes))
                    {
                        col.Item().Column(notesSection =>
                        {
                            notesSection.Item().Text("NOTES:").SemiBold();
                            foreach (var line in Wrap(notes, receiptCharWidth))
                                notesSection.Item().Text(line);
                            notesSection.Item().Text(new string('-', receiptCharWidth));
                        });
                    }

                    // Footer
                    col.Item().Column(footer =>
                    {
                        footer.Item().Text(new string('=', receiptCharWidth));
                        footer.Item().Text($"Total Items: {items.Sum(i => i.Quantity):0.##}").AlignCenter().SemiBold();
                        footer.Item().Text(new string('=', receiptCharWidth));
                        footer.Item().Text("** KOT - FOR KITCHEN USE ONLY **").FontSize(6).AlignCenter();
                    });
                });
            });
        });

        using var stream = new MemoryStream();
        doc.GeneratePdf(stream);
        return stream.ToArray();
    }

    /// <summary>
    /// Print PDF file to default printer
    /// </summary>
    private void PrintPdfSilently(string pdfFilePath)
    {
        try
        {
            // Method 1: Try using SumatraPDF if available (best for thermal printers)
            if (TryPrintWithSumatraPDF(pdfFilePath))
            {
                return;
            }

            // Method 2: Try using Adobe Reader command line
            if (TryPrintWithAdobeReader(pdfFilePath))
            {
                return;
            }

            // Method 3: Fallback - Open PDF and let user print
            var processInfo = new ProcessStartInfo
            {
                FileName = pdfFilePath,
                UseShellExecute = true,
                Verb = "open"
            };

            Process.Start(processInfo);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to print PDF: {ex.Message}", ex);
        }
    }

    private bool TryPrintWithSumatraPDF(string pdfFilePath)
    {
        try
        {
            // Check common SumatraPDF installation paths
            string[] sumatraPaths = new[]
            {
                @"C:\Program Files\SumatraPDF\SumatraPDF.exe",
                @"C:\Program Files (x86)\SumatraPDF\SumatraPDF.exe",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"SumatraPDF\SumatraPDF.exe")
            };

            string? sumatraPath = sumatraPaths.FirstOrDefault(File.Exists);
            
            if (sumatraPath != null)
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = sumatraPath,
                    Arguments = $"-print-to-default \"{pdfFilePath}\"",
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false
                };

                var process = Process.Start(processInfo);
                process?.WaitForExit(5000);
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    private bool TryPrintWithAdobeReader(string pdfFilePath)
    {
        try
        {
            // Check common Adobe Reader installation paths
            string[] adobePaths = new[]
            {
                @"C:\Program Files\Adobe\Acrobat DC\Acrobat\Acrobat.exe",
                @"C:\Program Files (x86)\Adobe\Acrobat Reader DC\Reader\AcroRd32.exe",
                @"C:\Program Files\Adobe\Acrobat Reader DC\Reader\AcroRd32.exe"
            };

            string? adobePath = adobePaths.FirstOrDefault(File.Exists);
            
            if (adobePath != null)
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = adobePath,
                    Arguments = $"/t \"{pdfFilePath}\"",
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false
                };

                var process = Process.Start(processInfo);
                process?.WaitForExit(5000);
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    private string BuildKotHeader(int descWidth, int qtyWidth)
    {
        return FitRight(2, "SN") + " " + 
               FitLeft(descWidth, "ITEM DESCRIPTION") + " " + 
               FitRight(qtyWidth, "QTY");
    }

    private IEnumerable<string> BuildKotItemLines(CartItemModel item, int serialNo, int descWidth, int qtyWidth)
    {
        var lines = new List<string>();
        var descLines = Wrap(item.ProductName, descWidth);
        string sn = FitRight(2, serialNo.ToString());
        string qty = FitRight(qtyWidth, item.Quantity.ToString("0.##"));

        for (int i = 0; i < descLines.Count; i++)
        {
            if (i == 0)
            {
                // First line: SN + DESC + QTY
                lines.Add(sn + " " + 
                         FitLeft(descWidth, descLines[i]) + " " + 
                         qty);
            }
            else
            {
                // Continuation lines: only description
                lines.Add(FitRight(2, "") + " " + 
                         FitLeft(descWidth, descLines[i]) + " " + 
                         FitRight(qtyWidth, ""));
            }
        }
        return lines;
    }

    private string FitLeft(int width, string value)
    {
        if (value.Length > width) return value.Substring(0, width);
        return value.PadRight(width);
    }

    private string FitRight(int width, string value)
    {
        if (value.Length > width) return value.Substring(0, width);
        return value.PadLeft(width);
    }

    private List<string> Wrap(string text, int width)
    {
        var lines = new List<string>();
        if (string.IsNullOrWhiteSpace(text)) return lines;
        var words = text.Split(' ');
        var current = "";
        foreach (var w in words)
        {
            if ((current.Length + w.Length + 1) > width)
            {
                if (!string.IsNullOrEmpty(current)) lines.Add(current);
                current = w;
            }
            else
            {
                current = string.IsNullOrEmpty(current) ? w : current + " " + w;
            }
        }
        if (!string.IsNullOrEmpty(current)) lines.Add(current);
        return lines;
    }
}
