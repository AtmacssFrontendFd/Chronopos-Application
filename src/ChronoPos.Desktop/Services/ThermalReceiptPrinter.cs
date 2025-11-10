using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using ChronoPos.Application.DTOs;
using ChronoPos.Desktop.ViewModels;

namespace ChronoPos.Desktop.Services;

/// <summary>
/// Service for printing thermal receipts (80mm width - 336 dots)
/// Professional restaurant bill format with proper alignment
/// </summary>
public class ThermalReceiptPrinter
{
    private const int RECEIPT_WIDTH = 48; // Increased by 20% for better width
    private const int DESCRIPTION_WIDTH = 24; // Width for item descriptions
    private const int QTY_WIDTH = 3; // Width for quantity
    private const int RATE_WIDTH = 7; // Width for rate
    private const int AMOUNT_WIDTH = 8; // Width for amount
    private readonly IActiveCurrencyService _activeCurrencyService;

    public ThermalReceiptPrinter(IActiveCurrencyService activeCurrencyService)
    {
        _activeCurrencyService = activeCurrencyService;
    }

    /// <summary>
    /// Print sales receipt in thermal format
    /// </summary>
    public void PrintSalesReceipt(
        TransactionDto transaction,
        List<CartItemModel> cartItems,
        CustomerDto? customer,
        RestaurantTableDto? table,
        decimal subtotal,
        decimal discount,
        decimal taxPercentage,
        decimal taxAmount,
        decimal serviceCharge,
        decimal total,
        string? companyName = null,
        string? companyAddress = null,
        string? companyPhone = null,
        string? gstNo = null)
    {
        try
        {
            var printDialog = new PrintDialog();

            // Create print document with minimal margins for thermal paper
            var document = new FlowDocument
            {
                PagePadding = new Thickness(2), // Reduced padding
                FontFamily = new FontFamily("Courier New"),
                FontSize = 8, // Reduced font size for compact layout
                PageWidth = 336,
                ColumnWidth = 336
            };

            // Add content
            AddHeader(document, companyName, companyAddress, companyPhone, gstNo);
            AddBillInfo(document, transaction, customer, table);
            AddItemsSection(document, cartItems);
            AddTotalsSection(document, subtotal, discount, taxPercentage, taxAmount, serviceCharge, total, customer);
            AddFooter(document);

            // Print
            var paginator = ((IDocumentPaginatorSource)document).DocumentPaginator;
            printDialog.PrintDocument(paginator, $"Receipt-{transaction.InvoiceNumber}");
        }
        catch (Exception ex)
        {
            throw new Exception($"Error printing receipt: {ex.Message}", ex);
        }
    }

    private void AddHeader(FlowDocument document, string? companyName, string? address, string? phone, string? gstNo)
    {
        var headerPara = new Paragraph
        {
            TextAlignment = TextAlignment.Left, // left-align for consistent vertical edges
            Margin = new Thickness(0, 0, 0, 1), // Reduced margin
            LineHeight = 1.0 // Reduced line height
        };

        // Company name
        if (!string.IsNullOrEmpty(companyName))
        {
            headerPara.Inlines.Add(new Run(FillLine(companyName.ToUpper(), 10, FontWeights.Bold)) { FontSize = 10, FontWeight = FontWeights.Bold });
        }
        headerPara.Inlines.Add(new Run(FillLine("TAX INVOICE", 9, FontWeights.Bold)) { FontSize = 9, FontWeight = FontWeights.Bold });

        // Address
        if (!string.IsNullOrEmpty(address))
        {
            var addressLines = WrapText(address, RECEIPT_WIDTH - 4);
            foreach (var line in addressLines)
            {
                headerPara.Inlines.Add(new Run(FillLine(line, 7)) { FontSize = 7 });
            }
        }

        // Phone
        if (!string.IsNullOrEmpty(phone))
        {
            headerPara.Inlines.Add(new Run(FillLine($"Tel: {phone}", 7)) { FontSize = 7 });
        }

        // GST Number
        if (!string.IsNullOrEmpty(gstNo))
        {
            headerPara.Inlines.Add(new Run(FillLine($"GSTIN: {gstNo}", 7)) { FontSize = 7 });
        }

        // Divider - increased width by 20%
        headerPara.Inlines.Add(new Run(new string('=', RECEIPT_WIDTH) + "\n") { FontSize = 8 });

        document.Blocks.Add(headerPara);
    }

    private void AddBillInfo(FlowDocument document, TransactionDto transaction, CustomerDto? customer, RestaurantTableDto? table)
    {
        var infoPara = new Paragraph
        {
            TextAlignment = TextAlignment.Left,
            Margin = new Thickness(0, 1, 0, 1),
            LineHeight = 1.0
        };

        // Create a container for left-aligned content within centered section
        var billInfoContent = new StringBuilder();

        // Bill number and date
    var billNo = transaction.InvoiceNumber ?? transaction.Id.ToString();
    billInfoContent.AppendLine(AlignLabelValue("Bill No", billNo));
    billInfoContent.AppendLine(AlignLabelValue("Date", transaction.SellingTime.ToString("dd-MMM-yyyy HH:mm")));

        // Table number
        if (table != null && !string.IsNullOrEmpty(table.DisplayName))
        {
            billInfoContent.AppendLine(AlignLabelValue("Table", table.DisplayName));
        }

        // Customer name
        if (customer != null && !string.IsNullOrEmpty(customer.CustomerFullName))
        {
            billInfoContent.AppendLine(AlignLabelValue("Customer", customer.CustomerFullName));
        }

        // Center the entire left-aligned block
        var lines = billInfoContent.ToString().Split('\n');
        foreach (var line in lines)
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                infoPara.Inlines.Add(new Run(FillLine(line.Trim(), 8)) { FontSize = 8 });
            }
        }

        // Divider - increased width by 20%
        infoPara.Inlines.Add(new Run(new string('-', RECEIPT_WIDTH) + "\n") { FontSize = 8 });

        document.Blocks.Add(infoPara);
    }

    private void AddItemsSection(FlowDocument document, List<CartItemModel> cartItems)
    {
        // Items header - centered
        var headerPara = new Paragraph
        {
            TextAlignment = TextAlignment.Left,
            Margin = new Thickness(0, 0, 0, 1),
            LineHeight = 1.0
        };

        var headerLine = FormatItemHeader();
        headerPara.Inlines.Add(new Run(headerLine + "\n") { FontSize = 8, FontWeight = FontWeights.Bold });
        headerPara.Inlines.Add(new Run(new string('-', RECEIPT_WIDTH) + "\n") { FontSize = 8 });

        document.Blocks.Add(headerPara);

        // Items - centered
        int itemNumber = 1;
        foreach (var item in cartItems)
        {
            var itemLines = FormatItemLine(
                itemNumber,
                item.ProductName,
                item.Quantity,
                item.UnitPrice,
                item.TotalPrice
            );

            foreach (var line in itemLines)
            {
                var itemPara = new Paragraph
                {
                    TextAlignment = TextAlignment.Left,
                    Margin = new Thickness(0, 0, 0, 0),
                    LineHeight = 1.0
                };

                itemPara.Inlines.Add(new Run(FillLine(line, 8)) { FontSize = 8 });
                document.Blocks.Add(itemPara);
            }

            itemNumber++;
        }

        // Bottom divider - increased width by 20%
        var dividerPara = new Paragraph 
        { 
            TextAlignment = TextAlignment.Left,
            Margin = new Thickness(0, 1, 0, 1)
        };
        dividerPara.Inlines.Add(new Run(new string('-', RECEIPT_WIDTH) + "\n") { FontSize = 8 });
        document.Blocks.Add(dividerPara);
    }

    private void AddTotalsSection(FlowDocument document, decimal subtotal, decimal discount,
    decimal taxPercentage, decimal taxAmount, decimal serviceCharge, decimal total, CustomerDto? customer)
{
    var totalsPara = new Paragraph
    {
        TextAlignment = TextAlignment.Left, // ← align text left for consistent column layout
        Margin = new Thickness(0, 1, 0, 1),
        LineHeight = 1.0
    };

    // Helper for aligned two-column rows (same width for all)
    string AlignTwoColumns(string label, string value)
    {
        int totalWidth = RECEIPT_WIDTH;
        int labelWidth = (int)(totalWidth * 0.55); // left column width (55%)
        int valueWidth = totalWidth - labelWidth;
        return $"{label.PadRight(labelWidth)}{value.PadLeft(valueWidth)}";
    }

    // Subtotal
    totalsPara.Inlines.Add(new Run(AlignTwoColumns("Subtotal:", $"{_activeCurrencyService.CurrencySymbol} {subtotal:F2}") + "\n") { FontSize = 8 });

    // Discount
    if (discount > 0)
        totalsPara.Inlines.Add(new Run(AlignTwoColumns("Discount:", $"-{_activeCurrencyService.CurrencySymbol} {discount:F2}") + "\n") { FontSize = 8 });

    // CGST
    if (taxAmount > 0)
    {
        decimal cgst = taxAmount / 2;
        totalsPara.Inlines.Add(new Run(AlignTwoColumns($"CGST @{(taxPercentage / 2):F1}%:", $"{_activeCurrencyService.CurrencySymbol} {cgst:F2}") + "\n") { FontSize = 8 });
    }

    // SGST
    if (taxAmount > 0)
    {
        decimal sgst = taxAmount / 2;
        totalsPara.Inlines.Add(new Run(AlignTwoColumns($"SGST @{(taxPercentage / 2):F1}%:", $"{_activeCurrencyService.CurrencySymbol} {sgst:F2}") + "\n") { FontSize = 8 });
    }

    // Service Charge
    if (serviceCharge > 0)
        totalsPara.Inlines.Add(new Run(AlignTwoColumns("Service Charge:", $"{_activeCurrencyService.CurrencySymbol} {serviceCharge:F2}") + "\n") { FontSize = 8 });

    // Divider
    totalsPara.Inlines.Add(new Run(new string('=', RECEIPT_WIDTH) + "\n") { FontSize = 8 });

    // Total
    totalsPara.Inlines.Add(new Run(AlignTwoColumns("TOTAL:", $"{_activeCurrencyService.CurrencySymbol} {total:F2}") + "\n")
    {
        FontSize = 8.5,
        FontWeight = FontWeights.Bold
    });

    // Rounding
    var roundedTotal = Math.Round(total, 0);
    if (Math.Abs(roundedTotal - total) > 0.01m)
    {
        totalsPara.Inlines.Add(new Run(AlignTwoColumns("Rounding:", $"{_activeCurrencyService.CurrencySymbol} {roundedTotal - total:F2}") + "\n") { FontSize = 8 });
        totalsPara.Inlines.Add(new Run(AlignTwoColumns("NET TOTAL:", $"{_activeCurrencyService.CurrencySymbol} {roundedTotal:F0}") + "\n")
        {
            FontSize = 8.5,
            FontWeight = FontWeights.Bold
        });
    }

    // Final divider
    totalsPara.Inlines.Add(new Run(new string('=', RECEIPT_WIDTH) + "\n") { FontSize = 8 });

    // Amount in words
    var netAmount = Math.Round(total, 0);
    var amountInWords = NumberToWords(netAmount);
    var wordsLines = WrapText($"Amount: {_activeCurrencyService.CurrencySymbol} {amountInWords} Only", RECEIPT_WIDTH);

    foreach (var line in wordsLines)
        totalsPara.Inlines.Add(new Run(line + "\n") { FontSize = 7});

    document.Blocks.Add(totalsPara);
}
    private void AddFooter(FlowDocument document)
    {
        var footerPara = new Paragraph
        {
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(0, 2, 0, 0), // Reduced margin
            LineHeight = 1.0 // Reduced line height
        };

        footerPara.Inlines.Add(new Run("Thank you for your visit!\n") { FontSize = 8 });
        footerPara.Inlines.Add(new Run("We hope to see you again soon\n") { FontSize = 7 });
        footerPara.Inlines.Add(new Run("** This is a computer generated bill **\n") { FontSize = 6 });
        footerPara.Inlines.Add(new Run("E.&.O.E.\n") { FontSize = 6 });

        document.Blocks.Add(footerPara);
    }

    #region Helper Methods

    private string FormatTwoColumn(string left, string right)
    {
        int totalSpaces = RECEIPT_WIDTH - left.Length - right.Length;
        return totalSpaces > 0 ? left + new string(' ', totalSpaces) + right : left + " " + right;
    }

    private string FormatItemHeader()
    {
        // Build exact-width header line
        var header = $"{"SN",2} {"DESCRIPTION".PadRight(DESCRIPTION_WIDTH)} {"QTY".PadLeft(QTY_WIDTH)} {"RATE".PadLeft(RATE_WIDTH)} {"AMOUNT".PadLeft(AMOUNT_WIDTH)}";
        return header.PadRight(RECEIPT_WIDTH).Substring(0, RECEIPT_WIDTH);
    }

    private List<string> FormatItemLine(int sn, string description, decimal qty, decimal rate, decimal amount)
    {
        var lines = new List<string>();
        string qtyStr = FitRight(QTY_WIDTH, qty.ToString("0.##"));
        string rateStr = FitRight(RATE_WIDTH, rate.ToString("0.00"));
        string amountStr = FitRight(AMOUNT_WIDTH, amount.ToString("0.00"));
        var descriptionLines = WrapText(description, DESCRIPTION_WIDTH);

        for (int i = 0; i < descriptionLines.Count; i++)
        {
            string desc = FitLeft(DESCRIPTION_WIDTH, descriptionLines[i]);
            string line;
            if (i == 0)
            {
                line = $"{FitRight(2, sn.ToString())} {desc} {qtyStr} {rateStr} {amountStr}";
            }
            else
            {
                // continuation lines: keep column placeholders
                line = $"{new string(' ', 2)} {desc} {new string(' ', QTY_WIDTH)} {new string(' ', RATE_WIDTH)} {new string(' ', AMOUNT_WIDTH)}";
            }
            // Ensure exact width
            lines.Add(line.Length < RECEIPT_WIDTH ? line.PadRight(RECEIPT_WIDTH) : line.Substring(0, RECEIPT_WIDTH));
        }
        return lines;
    }

    // CenterText retained for potential future use but avoid in current formatting
    private string CenterText(string text) => text.Length >= RECEIPT_WIDTH ? text : text.PadRight(RECEIPT_WIDTH);

    private List<string> WrapText(string text, int maxWidth)
    {
        var lines = new List<string>();
        
        if (string.IsNullOrEmpty(text))
            return lines;

        var words = text.Split(' ');
        var currentLine = new StringBuilder();

        foreach (var word in words)
        {
            if (currentLine.Length + word.Length + 1 > maxWidth)
            {
                if (currentLine.Length > 0)
                {
                    lines.Add(currentLine.ToString().Trim());
                    currentLine.Clear();
                }
                
                // If a single word is longer than maxWidth, break it
                if (word.Length > maxWidth)
                {
                    int startIndex = 0;
                    while (startIndex < word.Length)
                    {
                        int length = Math.Min(maxWidth, word.Length - startIndex);
                        lines.Add(word.Substring(startIndex, length));
                        startIndex += length;
                    }
                    continue;
                }
            }

            if (currentLine.Length > 0)
                currentLine.Append(' ');
            
            currentLine.Append(word);
        }

        if (currentLine.Length > 0)
            lines.Add(currentLine.ToString().Trim());

        return lines;
    }

    // Helpers for strict column width enforcement
    private string FitRight(int width, string value)
    {
        if (value.Length > width) return value.Substring(0, width);
        return value.PadLeft(width);
    }

    private string FitLeft(int width, string value)
    {
        if (value.Length > width) return value.Substring(0, width);
        return value.PadRight(width);
    }

    private string FillLine(string text, double? fontSize = null, FontWeight? weight = null)
    {
        // Produce a full-width line (no centering spaces) trimmed/padded
        var raw = text.Trim();
        if (raw.Length > RECEIPT_WIDTH) raw = raw.Substring(0, RECEIPT_WIDTH);
        return raw.PadRight(RECEIPT_WIDTH) + "\n";
    }

    private string AlignLabelValue(string label, string value)
    {
        // label:value pattern with fixed label width for alignment
        const int labelWidth = 10; // e.g. "Customer:" length baseline
        var lbl = (label + ":").PadRight(labelWidth);
        var combined = lbl + " " + value;
        return combined.Length <= RECEIPT_WIDTH ? combined.PadRight(RECEIPT_WIDTH) : combined.Substring(0, RECEIPT_WIDTH);
    }

    private string NumberToWords(decimal number)
    {
        if (number == 0) return "Zero";

        long integerPart = (long)Math.Floor(number);
        
        if (integerPart == 0) return "Zero";

        string[] units = { "", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten", 
                          "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", 
                          "Eighteen", "Nineteen" };
        string[] tens = { "", "", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };

        string words = "";

        // Handle thousands
        if (integerPart >= 1000)
        {
            words += NumberToWords(integerPart / 1000) + " Thousand ";
            integerPart %= 1000;
        }

        // Handle hundreds
        if (integerPart >= 100)
        {
            words += units[integerPart / 100] + " Hundred ";
            integerPart %= 100;
        }

        // Handle tens and units
        if (integerPart > 0)
        {
            if (integerPart < 20)
            {
                words += units[integerPart];
            }
            else
            {
                words += tens[integerPart / 10];
                if (integerPart % 10 > 0)
                {
                    words += " " + units[integerPart % 10];
                }
            }
        }

        return words.Trim().Replace("  ", " ");
    }

    #endregion
}