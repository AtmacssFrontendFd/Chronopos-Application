using System.Windows.Markup;
using System.Windows.Data;
using System.Windows;
using ChronoPos.Infrastructure.Services;

namespace ChronoPos.Desktop.MarkupExtensions
{
    /// <summary>
    /// Markup extension for database-driven translations
    /// Usage: {loc:Translate Key='nav.dashboard'}
    /// </summary>
    public class TranslateExtension : MarkupExtension
    {
        public string Key { get; set; } = string.Empty;
        public string? FallbackValue { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (string.IsNullOrEmpty(Key))
                return FallbackValue ?? Key;

            // For now, just return the key or fallback value
            // In a full implementation, this would get the translation from the service
            return FallbackValue ?? Key;
        }
    }
}
