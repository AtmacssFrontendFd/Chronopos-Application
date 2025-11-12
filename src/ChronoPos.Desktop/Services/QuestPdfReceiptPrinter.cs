using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Printing;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;
using ChronoPos.Application.DTOs;
using ChronoPos.Desktop.ViewModels;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ChronoPos.Desktop.Services;

/// <summary>
/// Professional POS receipt printer using QuestPDF for thermal (80mm) and regular printers
/// Generates PDF receipts with pixel-perfect alignment and auto-print capability
/// </summary>
public class QuestPdfReceiptPrinter
{
    private readonly IActiveCurrencyService _currency;
    
    public QuestPdfReceiptPrinter(IActiveCurrencyService currency)
    {
        _currency = currency;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    
    /// <summary>
    /// Generate and auto-print receipt to default printer
    /// </summary>
    public string GenerateAndPrintReceipt(
        TransactionDto transaction,
        List<CartItemModel> items,
        CustomerDto? customer,
        RestaurantTableDto? table,
        decimal subtotal,
        decimal discount,
        decimal taxPercent,
        decimal taxAmount,
        decimal serviceCharge,
        decimal total,
        string companyName,
        string? companyAddress = null,
        string? companyPhone = null,
        string? gstNo = null)
    {
        try
        {
            // Generate PDF
            var pdfBytes = GenerateReceiptPdf(
                transaction, items, customer, table,
                subtotal, discount, taxPercent, taxAmount,
                serviceCharge, total, companyName,
                companyAddress, companyPhone, gstNo);

            // Save to temp folder with unique name
            string tempFolder = Path.Combine(Path.GetTempPath(), "ChronoPosReceipts");
            Directory.CreateDirectory(tempFolder);
            
            string fileName = $"Receipt-{transaction.InvoiceNumber ?? transaction.Id.ToString()}-{DateTime.Now:yyyyMMddHHmmss}.pdf";
            string filePath = Path.Combine(tempFolder, fileName);
            
            File.WriteAllBytes(filePath, pdfBytes);

            // Auto-print using default PDF printer
            PrintPdfSilently(filePath);

            return filePath;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to generate and print receipt: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Generate receipt PDF without printing
    /// </summary>
    public byte[] GenerateReceiptPdf(
        TransactionDto transaction,
        List<CartItemModel> items,
        CustomerDto? customer,
        RestaurantTableDto? table,
        decimal subtotal,
        decimal discount,
        decimal taxPercent,
        decimal taxAmount,
        decimal serviceCharge,
        decimal total,
        string companyName,
        string? companyAddress = null,
        string? companyPhone = null,
        string? gstNo = null)
    {
        // Thermal receipt settings (80mm paper)
        int receiptCharWidth = 44;
        int descWidth = 20 ;
        int qtyWidth = 3;
        int rateWidth = 5;
        int amountWidth = 7;

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                // 80mm thermal paper = 226 points width at 72 DPI, height auto-expands
                page.Size(226, 1440); // Width 226pt (~80mm), Height 1440pt (~20 inches max)
                page.Margin(5);
                page.DefaultTextStyle(x => x.FontFamily("Courier New").FontSize(8));
                page.Content().Column(col =>
                {
                    col.Spacing(2);

                    // Header
                    col.Item().Column(header =>
                    {
                        header.Item().Text(companyName.ToUpper()).SemiBold().FontSize(10).AlignCenter();
                        header.Item().Text("Original INVOICE").SemiBold().FontSize(9).AlignCenter();
                        if (!string.IsNullOrWhiteSpace(companyAddress))
                            header.Item().Text(companyAddress).FontSize(7).AlignCenter();
                        if (!string.IsNullOrWhiteSpace(companyPhone))
                            header.Item().Text($"Tel: {companyPhone}").FontSize(7).AlignCenter();
                        if (!string.IsNullOrWhiteSpace(gstNo))
                            header.Item().Text($"GSTIN: {gstNo}").FontSize(7).AlignCenter();
                        header.Item().Text(new string('=', receiptCharWidth)).FontSize(8).AlignLeft();
                    });

                    // Bill / customer details using strict label column
                    col.Item().Column(info =>
                    {
                        string Label(string l) => (l + ":").PadRight(10);
                        info.Item().Text(Label("Bill No") + (transaction.InvoiceNumber ?? transaction.Id.ToString()));
                        info.Item().Text(Label("Date") + transaction.SellingTime.ToString("dd-MMM-yyyy HH:mm"));
                        if (table != null) info.Item().Text(Label("Table") + table.DisplayName);
                        if (customer != null) info.Item().Text(Label("Customer") + customer.CustomerFullName);
                        info.Item().Text(new string('-', receiptCharWidth));
                    });

                    // Items header
                    col.Item().Text(BuildItemHeader(descWidth, qtyWidth, rateWidth, amountWidth)).SemiBold();
                    col.Item().Text(new string('-', receiptCharWidth));

                    // Items list with sequential numbering
                    int serialNo = 1;
                    foreach (var item in items)
                    {
                        foreach (var line in BuildItemLines(item, serialNo, descWidth, qtyWidth, rateWidth, amountWidth))
                        {
                            col.Item().Text(line);
                        }
                        serialNo++;
                    }

                    col.Item().Text(new string('-', receiptCharWidth));

                    // Totals table
                    col.Item().Column(totals =>
                    {
                        string Money(decimal v) => _currency.CurrencySymbol + " " + v.ToString("0.00");
                        totals.Item().Text(AlignTwoCols("Subtotal", Money(subtotal), receiptCharWidth));
                        if (discount > 0) totals.Item().Text(AlignTwoCols("Discount", "-" + Money(discount), receiptCharWidth));
                        if (taxAmount > 0)
                        {
                            var halfTax = taxAmount / 2;
                            totals.Item().Text(AlignTwoCols($"CGST @{(taxPercent/2):0.##}%", Money(halfTax), receiptCharWidth));
                            totals.Item().Text(AlignTwoCols($"SGST @{(taxPercent/2):0.##}%", Money(halfTax), receiptCharWidth));
                        }
                        if (serviceCharge > 0)
                            totals.Item().Text(AlignTwoCols("Service Charge", Money(serviceCharge), receiptCharWidth));

                        totals.Item().Text(new string('=', receiptCharWidth));
                        totals.Item().Text(AlignTwoCols("TOTAL", Money(total), receiptCharWidth)).SemiBold();
                        var rounded = Math.Round(total, 0);
                        if (Math.Abs(rounded - total) > 0.01m)
                        {
                            totals.Item().Text(AlignTwoCols("Rounding", Money(rounded - total), receiptCharWidth));
                            totals.Item().Text(AlignTwoCols("NET TOTAL", _currency.CurrencySymbol + " " + rounded.ToString("0"), receiptCharWidth)).SemiBold();
                        }
                        totals.Item().Text(new string('=', receiptCharWidth));
                        var amountWords = NumberToWords(Math.Round(total, 0));
                        foreach (var line in Wrap(amountWords + " Only", receiptCharWidth))
                            totals.Item().Text(line);
                    });

                    // Footer
                    col.Item().Column(footer =>
                    {
                        footer.Item().Text("Thank you for your visit!").AlignCenter();
                        footer.Item().Text("We hope to see you again soon").FontSize(7).AlignCenter();
                        footer.Item().Text("** Computer Generated Bill **").FontSize(6).AlignCenter();
                        footer.Item().Text("E.&O.E.").FontSize(6).AlignCenter();
                    });
                });
            });
        });

        using var stream = new MemoryStream();
        doc.GeneratePdf(stream);
        return stream.ToArray();
    }

    /// <summary>
    /// Print PDF file to default printer using WPF printing system
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
                // SumatraPDF has excellent command line printing support
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
            // Check common Adobe Reader paths
            string[] adobePaths = new[]
            {
                @"C:\Program Files\Adobe\Acrobat DC\Acrobat\Acrobat.exe",
                @"C:\Program Files (x86)\Adobe\Acrobat Reader DC\Reader\AcroRd32.exe",
                @"C:\Program Files\Adobe\Acrobat Reader DC\Reader\AcroRd32.exe"
            };

            string? adobePath = adobePaths.FirstOrDefault(File.Exists);
            
            if (adobePath != null)
            {
                // Adobe Reader command line: /t = print to default printer and close
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

    private string BuildItemHeader(int descWidth, int qtyWidth, int rateWidth, int amountWidth)
    {
        // Build header with exact spacing: SN(2) + space + DESC + space + QTY + space + RATE + space + AMOUNT
        return FitRight(2, "SN") + " " + 
               FitLeft(descWidth, "DESCRIPTION") + " " + 
               FitRight(qtyWidth, "QTY") + " " + 
               FitRight(rateWidth, "RATE") + " " + 
               FitRight(amountWidth, "AMOUNT");
    }

    private IEnumerable<string> BuildItemLines(CartItemModel item, int serialNo, int descWidth, int qtyWidth, int rateWidth, int amountWidth)
    {
        var lines = new List<string>();
        var descLines = Wrap(item.ProductName, descWidth);
        string sn = FitRight(2, serialNo.ToString());
        string qty = FitRight(qtyWidth, item.Quantity.ToString("0.##"));
        string rate = FitRight(rateWidth, item.UnitPrice.ToString("0.00"));
        string amt = FitRight(amountWidth, item.TotalPrice.ToString("0.00"));

        for (int i = 0; i < descLines.Count; i++)
        {
            if (i == 0)
            {
                // First line: SN + DESC + QTY + RATE + AMOUNT
                lines.Add(sn + " " + 
                         FitLeft(descWidth, descLines[i]) + " " + 
                         qty + " " + 
                         rate + " " + 
                         amt);
            }
            else
            {
                // Continuation lines: only description, rest empty
                lines.Add(FitRight(2, "") + " " + 
                         FitLeft(descWidth, descLines[i]) + " " + 
                         FitRight(qtyWidth, "") + " " + 
                         FitRight(rateWidth, "") + " " + 
                         FitRight(amountWidth, ""));
            }
        }
        return lines;
    }

    private string AlignTwoCols(string left, string right, int totalWidth)
    {
        // Calculate exact spacing needed
        int leftLen = left.Length;
        int rightLen = right.Length;
        int space = totalWidth - leftLen - rightLen;
        if (space < 1) space = 1;
        
        return left + new string(' ', space) + right;
    }

    private string AlignColumns(IEnumerable<string> cols)
    {
        // Simply join columns with single space - they're already sized
        return string.Join(" ", cols);
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

    private string NumberToWords(decimal number)
    {
        if (number == 0) return "Zero";
        long integer = (long)Math.Round(number, 0);
        string[] units = { "", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
        string[] tens = { "", "", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };
        string words = "";
        if (integer >= 1000)
        {
            words += NumberToWords(integer / 1000) + " Thousand ";
            integer %= 1000;
        }
        if (integer >= 100)
        {
            words += units[integer / 100] + " Hundred ";
            integer %= 100;
        }
        if (integer > 0)
        {
            if (integer < 20) words += units[integer];
            else
            {
                words += tens[integer / 10];
                if (integer % 10 > 0) words += " " + units[integer % 10];
            }
        }
        return words.Trim();
    }
}
