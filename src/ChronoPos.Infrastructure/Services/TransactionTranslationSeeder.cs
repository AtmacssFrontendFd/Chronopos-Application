using ChronoPos.Infrastructure.Services;

namespace ChronoPos.Infrastructure.Services;

/// <summary>
/// Dedicated seeder for Transaction screen translations
/// </summary>
public static class TransactionTranslationSeeder
{
    public static async Task SeedTransactionTranslationsAsync(IDatabaseLocalizationService localizationService)
    {
        var transactionTranslations = new Dictionary<string, Dictionary<string, string>>
        {
            // Page Title and Tabs
            {
                "transaction_title",
                new Dictionary<string, string>
                {
                    { "en", "Transactions" },
                    { "ur", "لین دین" }
                }
            },
            {
                "sales_tab",
                new Dictionary<string, string>
                {
                    { "en", "Sales" },
                    { "ur", "فروخت" }
                }
            },
            {
                "refund_tab",
                new Dictionary<string, string>
                {
                    { "en", "Refund" },
                    { "ur", "واپسی" }
                }
            },
            {
                "exchange_tab",
                new Dictionary<string, string>
                {
                    { "en", "Exchange" },
                    { "ur", "تبادلہ" }
                }
            },
            
            // Search and Actions
            {
                "search_placeholder",
                new Dictionary<string, string>
                {
                    { "en", "Search transactions..." },
                    { "ur", "لین دین تلاش کریں..." }
                }
            },
            {
                "create_new_transaction",
                new Dictionary<string, string>
                {
                    { "en", "Create New Transaction" },
                    { "ur", "نیا لین دین بنائیں" }
                }
            },
            
            // Card Labels - Sales
            {
                "invoice_label",
                new Dictionary<string, string>
                {
                    { "en", "Invoice" },
                    { "ur", "انوائس" }
                }
            },
            {
                "customer_label",
                new Dictionary<string, string>
                {
                    { "en", "Customer" },
                    { "ur", "کسٹمر" }
                }
            },
            {
                "table_label",
                new Dictionary<string, string>
                {
                    { "en", "Table" },
                    { "ur", "میز" }
                }
            },
            {
                "items_label",
                new Dictionary<string, string>
                {
                    { "en", "Items" },
                    { "ur", "اشیاء" }
                }
            },
            {
                "total_label",
                new Dictionary<string, string>
                {
                    { "en", "Total" },
                    { "ur", "کل" }
                }
            },
            {
                "paid_label",
                new Dictionary<string, string>
                {
                    { "en", "Paid" },
                    { "ur", "ادا شدہ" }
                }
            },
            {
                "remaining_label",
                new Dictionary<string, string>
                {
                    { "en", "Remaining" },
                    { "ur", "باقی" }
                }
            },
            
            // Status Labels
            {
                "status_draft",
                new Dictionary<string, string>
                {
                    { "en", "DRAFT" },
                    { "ur", "مسودہ" }
                }
            },
            {
                "status_billed",
                new Dictionary<string, string>
                {
                    { "en", "BILLED" },
                    { "ur", "بل شدہ" }
                }
            },
            {
                "status_hold",
                new Dictionary<string, string>
                {
                    { "en", "HOLD" },
                    { "ur", "روکا گیا" }
                }
            },
            {
                "status_settled",
                new Dictionary<string, string>
                {
                    { "en", "SETTLED" },
                    { "ur", "مکمل" }
                }
            },
            {
                "status_pending_payment",
                new Dictionary<string, string>
                {
                    { "en", "PENDING PAYMENT" },
                    { "ur", "ادائیگی زیر التواء" }
                }
            },
            {
                "status_partial_payment",
                new Dictionary<string, string>
                {
                    { "en", "PARTIAL PAYMENT" },
                    { "ur", "جزوی ادائیگی" }
                }
            },
            {
                "status_cancelled",
                new Dictionary<string, string>
                {
                    { "en", "CANCELLED" },
                    { "ur", "منسوخ" }
                }
            },
            {
                "status_refunded",
                new Dictionary<string, string>
                {
                    { "en", "REFUNDED" },
                    { "ur", "واپس کیا گیا" }
                }
            },
            {
                "status_exchanged",
                new Dictionary<string, string>
                {
                    { "en", "EXCHANGED" },
                    { "ur", "تبدیل کیا گیا" }
                }
            },
            
            // Actions
            {
                "view_details",
                new Dictionary<string, string>
                {
                    { "en", "View Details" },
                    { "ur", "تفصیلات دیکھیں" }
                }
            },
            {
                "edit_transaction",
                new Dictionary<string, string>
                {
                    { "en", "Edit" },
                    { "ur", "ترمیم" }
                }
            },
            {
                "pay_bill",
                new Dictionary<string, string>
                {
                    { "en", "Pay Bill" },
                    { "ur", "بل ادا کریں" }
                }
            },
            {
                "print_invoice",
                new Dictionary<string, string>
                {
                    { "en", "Print Invoice" },
                    { "ur", "انوائس پرنٹ کریں" }
                }
            },
            {
                "process_refund",
                new Dictionary<string, string>
                {
                    { "en", "Process Refund" },
                    { "ur", "واپسی کریں" }
                }
            },
            {
                "process_exchange",
                new Dictionary<string, string>
                {
                    { "en", "Process Exchange" },
                    { "ur", "تبادلہ کریں" }
                }
            },
            
            // Empty State Messages
            {
                "no_sales_transactions",
                new Dictionary<string, string>
                {
                    { "en", "No sales transactions found" },
                    { "ur", "کوئی فروخت کا لین دین نہیں ملا" }
                }
            },
            {
                "no_refund_transactions",
                new Dictionary<string, string>
                {
                    { "en", "No refund transactions found" },
                    { "ur", "کوئی واپسی کا لین دین نہیں ملا" }
                }
            },
            {
                "no_exchange_transactions",
                new Dictionary<string, string>
                {
                    { "en", "No exchange transactions found" },
                    { "ur", "کوئی تبادلے کا لین دین نہیں ملا" }
                }
            },
            {
                "start_creating_sales",
                new Dictionary<string, string>
                {
                    { "en", "Click '+' to create a new sale" },
                    { "ur", "نیا لین دین بنانے کے لیے '+' پر کلک کریں" }
                }
            },
            
            // Time Labels
            {
                "just_now",
                new Dictionary<string, string>
                {
                    { "en", "Just now" },
                    { "ur", "ابھی" }
                }
            },
            {
                "minutes_ago",
                new Dictionary<string, string>
                {
                    { "en", "min ago" },
                    { "ur", "منٹ پہلے" }
                }
            },
            {
                "hours_ago",
                new Dictionary<string, string>
                {
                    { "en", "hrs ago" },
                    { "ur", "گھنٹے پہلے" }
                }
            },
            {
                "days_ago",
                new Dictionary<string, string>
                {
                    { "en", "days ago" },
                    { "ur", "دن پہلے" }
                }
            },
            
            // Refund Labels
            {
                "refund_amount_label",
                new Dictionary<string, string>
                {
                    { "en", "Refund Amount" },
                    { "ur", "واپسی کی رقم" }
                }
            },
            {
                "original_invoice_label",
                new Dictionary<string, string>
                {
                    { "en", "Original Invoice" },
                    { "ur", "اصل انوائس" }
                }
            },
            {
                "refund_reason_label",
                new Dictionary<string, string>
                {
                    { "en", "Reason" },
                    { "ur", "وجہ" }
                }
            },
            
            // Exchange Labels
            {
                "exchange_difference_label",
                new Dictionary<string, string>
                {
                    { "en", "Difference" },
                    { "ur", "فرق" }
                }
            },
            {
                "returned_items_label",
                new Dictionary<string, string>
                {
                    { "en", "Returned Items" },
                    { "ur", "واپس کی گئی اشیاء" }
                }
            },
            {
                "new_items_label",
                new Dictionary<string, string>
                {
                    { "en", "New Items" },
                    { "ur", "نئی اشیاء" }
                }
            },
            
            // Settle Popup Labels
            {
                "payment_popup_title",
                new Dictionary<string, string>
                {
                    { "en", "Payment" },
                    { "ur", "ادائیگی" }
                }
            },
            {
                "payment_method_label",
                new Dictionary<string, string>
                {
                    { "en", "Payment Method:" },
                    { "ur", ":ادائیگی کا طریقہ" }
                }
            },
            {
                "amount_paid_label",
                new Dictionary<string, string>
                {
                    { "en", "Amount Paid:" },
                    { "ur", ":ادا شدہ رقم" }
                }
            },
            {
                "credit_days_label",
                new Dictionary<string, string>
                {
                    { "en", "Credit Days (for partial payment):" },
                    { "ur", ":(جزوی ادائیگی کے لیے) کریڈٹ دن" }
                }
            },
            {
                "cancel_button",
                new Dictionary<string, string>
                {
                    { "en", "Cancel" },
                    { "ur", "منسوخ کریں" }
                }
            },
            {
                "save_settle_button",
                new Dictionary<string, string>
                {
                    { "en", "Save & Settle" },
                    { "ur", "محفوظ کریں اور مکمل کریں" }
                }
            },
            {
                "customer_pending_amount",
                new Dictionary<string, string>
                {
                    { "en", "Customer Pending Amount:" },
                    { "ur", ":کسٹمر کی باقی رقم" }
                }
            },
            {
                "remaining_amount_transaction",
                new Dictionary<string, string>
                {
                    { "en", "Remaining Amount of Transaction:" },
                    { "ur", ":لین دین کی باقی رقم" }
                }
            },
            {
                "already_paid_label",
                new Dictionary<string, string>
                {
                    { "en", "Already Paid:" },
                    { "ur", ":پہلے سے ادا شدہ" }
                }
            },
            {
                "sale_amount_label",
                new Dictionary<string, string>
                {
                    { "en", "Sale Amount:" },
                    { "ur", ":فروخت کی رقم" }
                }
            },
            {
                "bill_total_label",
                new Dictionary<string, string>
                {
                    { "en", "Bill Total:" },
                    { "ur", ":کل بل" }
                }
            },
            {
                "customer_pending_added",
                new Dictionary<string, string>
                {
                    { "en", "Customer Pending:" },
                    { "ur", ":کسٹمر کی باقی رقم" }
                }
            },
            {
                "store_credit_available",
                new Dictionary<string, string>
                {
                    { "en", "Store Credit Available:" },
                    { "ur", ":اسٹور کریڈٹ دستیاب" }
                }
            },
            {
                "added_to_bill",
                new Dictionary<string, string>
                {
                    { "en", "(Added to bill)" },
                    { "ur", "(بل میں شامل)" }
                }
            },
            {
                "deducted_from_bill",
                new Dictionary<string, string>
                {
                    { "en", "(Deducted from bill)" },
                    { "ur", "(بل سے کٹوتی)" }
                }
            }
        };

        await SeedTranslationCategory("Transaction", transactionTranslations, localizationService);
    }

    private static async Task SeedTranslationCategory(string category, Dictionary<string, Dictionary<string, string>> translations, IDatabaseLocalizationService localizationService)
    {
        Console.WriteLine($"🔧 [TransactionTranslationSeeder] Seeding {category} translations...");
        
        foreach (var keywordPair in translations)
        {
            var key = keywordPair.Key;
            var languageTranslations = keywordPair.Value;
            
            // Add keyword if it doesn't exist
            await localizationService.AddLanguageKeywordAsync(key, $"{category} - {key}");
            
            // Add translations for each language
            foreach (var translation in languageTranslations)
            {
                await localizationService.SaveTranslationAsync(key, translation.Value, translation.Key);
            }
        }
        
        Console.WriteLine($"✅ [TransactionTranslationSeeder] {category} translations seeded successfully");
    }
}
