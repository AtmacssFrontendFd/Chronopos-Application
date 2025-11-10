using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ChronoPos.Application.DTOs;
using ChronoPos.Desktop.ViewModels;
using ChronoPos.Desktop.Services;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ChronoPos.Desktop.Services;

/// <summary>
/// Exchange receipt printer using QuestPDF for thermal (80mm) printers
/// Generates professional exchange receipts with direct printing (no preview)
/// </summary>
public class QuestPdfExchangePrinter
{
    private readonly IActiveCurrencyService _currency;
    
    public QuestPdfExchangePrinter(IActiveCurrencyService currency)
    {
        _currency = currency;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    /// <summary>
    /// Generate and auto-print exchange receipt to default printer
    /// </summary>
    public string GenerateAndPrintExchange(
        ExchangeTransactionDto exchange,
        List<ExchangeItemModel> returnItems,
        List<ExchangeItemModel> newItems,
        string invoiceNumber,
        string customerName,
        decimal totalReturnAmount,
        decimal totalNewAmount,
        decimal differenceToPay,
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
            string exchangeNo = $"E{exchange.Id:D4}";
            string fileName = $"Exchange-{exchangeNo}-{timestamp}.pdf";
            string filePath = Path.Combine(tempFolder, fileName);

            // Generate PDF
            var pdfBytes = GenerateExchangePdf(exchange, returnItems, newItems, invoiceNumber, customerName, 
                totalReturnAmount, totalNewAmount, differenceToPay, companyName, companyAddress, companyPhone, gstNo);
            File.WriteAllBytes(filePath, pdfBytes);

            // Auto-print using default PDF printer
            PrintPdfSilently(filePath);

            return filePath;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to generate and print exchange receipt: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Generate exchange PDF without printing
    /// </summary>
    public byte[] GenerateExchangePdf(
        ExchangeTransactionDto exchange,
        List<ExchangeItemModel> returnItems,
        List<ExchangeItemModel> newItems,
        string invoiceNumber,
        string customerName,
        decimal totalReturnAmount,
        decimal totalNewAmount,
        decimal differenceToPay,
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
                        header.Item().Text("EXCHANGE COPY INVOICE").SemiBold().FontSize(9).AlignCenter();
                        if (!string.IsNullOrWhiteSpace(companyAddress))
                            header.Item().Text(companyAddress).FontSize(7).AlignCenter();
                        if (!string.IsNullOrWhiteSpace(companyPhone))
                            header.Item().Text($"Tel: {companyPhone}").FontSize(7).AlignCenter();
                        if (!string.IsNullOrWhiteSpace(gstNo))
                            header.Item().Text($"GSTIN: {gstNo}").FontSize(7).AlignCenter();
                        header.Item().Text(new string('=', receiptCharWidth)).FontSize(8).AlignLeft();
                    });

                    // Exchange info
                    col.Item().Column(info =>
                    {
                        string Label(string l) => (l + ":").PadRight(12);
                        info.Item().Text(Label("Exchange No") + $"E{exchange.Id:D4}");
                        info.Item().Text(Label("Orig Invoice") + invoiceNumber);
                        info.Item().Text(Label("Date") + exchange.ExchangeTime.ToString("dd-MMM-yyyy HH:mm"));
                        if (!string.IsNullOrEmpty(customerName)) 
                            info.Item().Text(Label("Customer") + customerName);
                        info.Item().Text(new string('-', receiptCharWidth));
                    });

                    // RETURNED ITEMS SECTION
                    col.Item().Text("RETURNED ITEMS").SemiBold().AlignCenter();
                    col.Item().Text(new string('-', receiptCharWidth));
                    col.Item().Text(BuildItemHeader(descWidth, qtyWidth, rateWidth, amountWidth)).SemiBold();
                    col.Item().Text(new string('-', receiptCharWidth));

                    int serialNo = 1;
                    foreach (var item in returnItems)
                    {
                        foreach (var line in BuildReturnItemLines(item, serialNo, descWidth, qtyWidth, rateWidth, amountWidth))
                        {
                            col.Item().Text(line);
                        }
                        serialNo++;
                    }

                    col.Item().Text(new string('-', receiptCharWidth));
                    col.Item().Text(AlignTwoCols("Total Returned", _currency.FormatPrice(totalReturnAmount), receiptCharWidth)).SemiBold();
                    col.Item().Text(new string('=', receiptCharWidth));

                    // NEW ITEMS SECTION
                    col.Item().Text("NEW ITEMS").SemiBold().AlignCenter();
                    col.Item().Text(new string('-', receiptCharWidth));
                    col.Item().Text(BuildItemHeader(descWidth, qtyWidth, rateWidth, amountWidth)).SemiBold();
                    col.Item().Text(new string('-', receiptCharWidth));

                    serialNo = 1;
                    foreach (var item in newItems)
                    {
                        foreach (var line in BuildNewItemLines(item, serialNo, descWidth, qtyWidth, rateWidth, amountWidth))
                        {
                            col.Item().Text(line);
                        }
                        serialNo++;
                    }

                    col.Item().Text(new string('-', receiptCharWidth));
                    col.Item().Text(AlignTwoCols("Total New Items", _currency.FormatPrice(totalNewAmount), receiptCharWidth)).SemiBold();
                    col.Item().Text(new string('=', receiptCharWidth));

                    // EXCHANGE SUMMARY
                    col.Item().Column(summary =>
                    {
                        summary.Item().Text(AlignTwoCols("Returned Amount", _currency.FormatPrice(totalReturnAmount), receiptCharWidth));
                        summary.Item().Text(AlignTwoCols("New Items Amount", _currency.FormatPrice(totalNewAmount), receiptCharWidth));
                        summary.Item().Text(new string('=', receiptCharWidth));
                        
                        if (differenceToPay > 0)
                        {
                            summary.Item().Text(AlignTwoCols("AMOUNT TO PAY", _currency.FormatPrice(differenceToPay), receiptCharWidth)).SemiBold().FontSize(9);
                        }
                        else if (differenceToPay < 0)
                        {
                            summary.Item().Text(AlignTwoCols("REFUND DUE", _currency.FormatPrice(Math.Abs(differenceToPay)), receiptCharWidth)).SemiBold().FontSize(9);
                        }
                        else
                        {
                            summary.Item().Text("EVEN EXCHANGE - NO PAYMENT").SemiBold().FontSize(9).AlignCenter();
                        }
                        
                        summary.Item().Text(new string('=', receiptCharWidth));
                    });

                    // Footer
                    col.Item().Column(footer =>
                    {
                        footer.Item().Text("Thank you for your business!").AlignCenter();
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

    private IEnumerable<string> BuildReturnItemLines(ExchangeItemModel item, int serialNo, int descWidth, int qtyWidth, int rateWidth, int amountWidth)
    {
        var lines = new List<string>();
        var descLines = Wrap(item.ProductName, descWidth);
        string sn = FitRight(2, serialNo.ToString());
        string qty = FitRight(qtyWidth, item.ReturnQuantity.ToString("0.##"));
        string rate = FitRight(rateWidth, item.UnitPrice.ToString("0.00"));
        string amt = FitRight(amountWidth, (item.ReturnQuantity * item.UnitPrice).ToString("0.00"));

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

    private IEnumerable<string> BuildNewItemLines(ExchangeItemModel item, int serialNo, int descWidth, int qtyWidth, int rateWidth, int amountWidth)
    {
        var lines = new List<string>();
        var descLines = Wrap(item.ProductName, descWidth);
        string sn = FitRight(2, serialNo.ToString());
        string qty = FitRight(qtyWidth, item.Quantity.ToString("0.##"));
        string rate = FitRight(rateWidth, item.UnitPrice.ToString("0.00"));
        string amt = FitRight(amountWidth, (item.Quantity * item.UnitPrice).ToString("0.00"));

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
}
