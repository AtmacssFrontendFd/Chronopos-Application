using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ChronoPos.Application.DTOs;
using ChronoPos.Desktop.Services;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ChronoPos.Desktop.Services;

/// <summary>
/// Refund receipt printer using QuestPDF for thermal (80mm) printers
/// Generates professional refund receipts with direct printing (no preview)
/// </summary>
public class QuestPdfRefundPrinter
{
    private readonly IActiveCurrencyService _currency;
    
    public QuestPdfRefundPrinter(IActiveCurrencyService currency)
    {
        _currency = currency;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    /// <summary>
    /// Generate and auto-print refund receipt to default printer
    /// </summary>
    public string GenerateAndPrintRefund(
        RefundTransactionDto refund,
        string companyName,
        string? companyAddress = null,
        string? companyPhone = null,
        string? gstNo = null)
    {
        try
        {
            // Create temp directory if it doesn't exist
            string tempFolder = Path.Combine(Path.GetTempPath(), "ChronoPosReceipts");
            Directory.CreateDirectory(tempFolder);

            // Generate unique filename
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            string refundNo = $"R{refund.Id:D4}";
            string fileName = $"Refund-{refundNo}-{timestamp}.pdf";
            string filePath = Path.Combine(tempFolder, fileName);

            // Generate PDF
            var pdfBytes = GenerateRefundPdf(refund, companyName, companyAddress, companyPhone, gstNo);
            File.WriteAllBytes(filePath, pdfBytes);

            // Auto-print using default PDF printer
            PrintPdfSilently(filePath);

            return filePath;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to generate and print refund receipt: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Generate refund PDF without printing
    /// </summary>
    public byte[] GenerateRefundPdf(
        RefundTransactionDto refund,
        string companyName,
        string? companyAddress = null,
        string? companyPhone = null,
        string? gstNo = null)
    {
        // Thermal receipt settings (80mm paper)
        int receiptCharWidth = 44;
        int descWidth = 20;
        int qtyWidth = 3;
        int rateWidth = 5;
        int amountWidth = 7;

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
                        header.Item().Text(companyName.ToUpper()).SemiBold().FontSize(10).AlignCenter();
                        header.Item().Text("REFUND COPY INVOICE").SemiBold().FontSize(9).AlignCenter();
                        if (!string.IsNullOrWhiteSpace(companyAddress))
                            header.Item().Text(companyAddress).FontSize(7).AlignCenter();
                        if (!string.IsNullOrWhiteSpace(companyPhone))
                            header.Item().Text($"Tel: {companyPhone}").FontSize(7).AlignCenter();
                        if (!string.IsNullOrWhiteSpace(gstNo))
                            header.Item().Text($"GSTIN: {gstNo}").FontSize(7).AlignCenter();
                        header.Item().Text(new string('=', receiptCharWidth)).FontSize(8).AlignLeft();
                    });

                    // Refund info
                    col.Item().Column(info =>
                    {
                        string Label(string l) => (l + ":").PadRight(12);
                        info.Item().Text(Label("Refund No") + $"R{refund.Id:D4}");
                        info.Item().Text(Label("Orig Invoice") + $"#{refund.SellingTransactionId}");
                        info.Item().Text(Label("Date") + refund.RefundTime.ToString("dd-MMM-yyyy HH:mm"));
                        if (!string.IsNullOrEmpty(refund.CustomerName)) 
                            info.Item().Text(Label("Customer") + refund.CustomerName);
                        info.Item().Text(new string('-', receiptCharWidth));
                    });

                    // Items header
                    col.Item().Text(BuildItemHeader(descWidth, qtyWidth, rateWidth, amountWidth)).SemiBold();
                    col.Item().Text(new string('-', receiptCharWidth));

                    // Items list with sequential numbering
                    int serialNo = 1;
                    foreach (var product in refund.RefundProducts)
                    {
                        foreach (var line in BuildItemLines(product, serialNo, descWidth, qtyWidth, rateWidth, amountWidth))
                        {
                            col.Item().Text(line);
                        }
                        serialNo++;
                    }

                    col.Item().Text(new string('-', receiptCharWidth));

                    // Totals
                    col.Item().Column(totals =>
                    {
                        string Money(decimal v) => _currency.CurrencySymbol + " " + v.ToString("0.00");
                        
                        decimal subtotal = refund.RefundProducts.Sum(p => p.TotalAmount);
                        totals.Item().Text(AlignTwoCols("Subtotal", Money(subtotal), receiptCharWidth));
                        
                        if (refund.TotalVat > 0)
                        {
                            var halfTax = refund.TotalVat / 2;
                            totals.Item().Text(AlignTwoCols($"CGST", Money(halfTax), receiptCharWidth));
                            totals.Item().Text(AlignTwoCols($"SGST", Money(halfTax), receiptCharWidth));
                        }

                        totals.Item().Text(new string('=', receiptCharWidth));
                        totals.Item().Text(AlignTwoCols("REFUND TOTAL", Money(refund.TotalAmount), receiptCharWidth)).SemiBold();
                        totals.Item().Text(new string('=', receiptCharWidth));
                        
                        var amountWords = NumberToWords(Math.Round(refund.TotalAmount, 0));
                        foreach (var line in Wrap(amountWords + " Only", receiptCharWidth))
                            totals.Item().Text(line);
                    });

                    // Footer
                    col.Item().Column(footer =>
                    {
                        footer.Item().Text("Amount refunded as per above").AlignCenter();
                        footer.Item().Text("Thank you!").AlignCenter().FontSize(7);
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

    private void PrintPdfSilently(string pdfFilePath)
    {
        try
        {
            if (TryPrintWithSumatraPDF(pdfFilePath)) return;
            if (TryPrintWithAdobeReader(pdfFilePath)) return;

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

    private string BuildItemHeader(int descWidth, int qtyWidth, int rateWidth, int amountWidth)
    {
        return FitRight(2, "SN") + " " + 
               FitLeft(descWidth, "DESCRIPTION") + " " + 
               FitRight(qtyWidth, "QTY") + " " + 
               FitRight(rateWidth, "RATE") + " " + 
               FitRight(amountWidth, "AMOUNT");
    }

    private IEnumerable<string> BuildItemLines(RefundTransactionProductDto product, int serialNo, int descWidth, int qtyWidth, int rateWidth, int amountWidth)
    {
        var lines = new List<string>();
        var descLines = Wrap(product.ProductName ?? "Unknown", descWidth);
        string sn = FitRight(2, serialNo.ToString());
        string qty = FitRight(qtyWidth, product.TotalQuantityReturned.ToString("0.##"));
        decimal unitPrice = product.TotalQuantityReturned > 0 ? product.TotalAmount / product.TotalQuantityReturned : 0;
        string rate = FitRight(rateWidth, unitPrice.ToString("0.00"));
        string amt = FitRight(amountWidth, product.TotalAmount.ToString("0.00"));

        for (int i = 0; i < descLines.Count; i++)
        {
            if (i == 0)
            {
                lines.Add(sn + " " + 
                         FitLeft(descWidth, descLines[i]) + " " + 
                         qty + " " + 
                         rate + " " + 
                         amt);
            }
            else
            {
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
        int leftLen = left.Length;
        int rightLen = right.Length;
        int space = totalWidth - leftLen - rightLen;
        if (space < 1) space = 1;
        
        return left + new string(' ', space) + right;
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
