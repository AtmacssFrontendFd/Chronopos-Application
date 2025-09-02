using ChronoPos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using ChronoPos.Infrastructure;
using System.Globalization;

namespace ChronoPos.Application.Services
{
    public interface IDatabaseLocalizationService
    {
        Task<string> GetTranslationAsync(string key, string? languageCode = null);
        Task<Dictionary<string, string>> GetAllTranslationsAsync(string languageCode);
        Task<List<Language>> GetAvailableLanguagesAsync();
        Task SetCurrentLanguageAsync(string languageCode);
        Task<Language?> GetCurrentLanguageAsync();
        Task<bool> SaveTranslationAsync(string key, string value, string languageCode, string? description = null);
        Task<bool> AddLanguageKeywordAsync(string key, string? description = null);
        string GetCurrentLanguageCode();
        bool IsRightToLeft();
        event EventHandler<string>? LanguageChanged;
    }

    public class DatabaseLocalizationService : IDatabaseLocalizationService
    {
        private readonly ChronoPosDbContext _context;
        private Language? _currentLanguage;
        private Dictionary<string, string> _cachedTranslations = new();
        private DateTime _lastCacheUpdate = DateTime.MinValue;
        private readonly TimeSpan _cacheExpiry = TimeSpan.FromMinutes(30);

        public event EventHandler<string>? LanguageChanged;

        public DatabaseLocalizationService(ChronoPosDbContext context)
        {
            _context = context;
            // Set default language to English
            _currentLanguage = new Language { Id = 1, LanguageName = "English", LanguageCode = "en", IsRtl = false };
        }

        public async Task<string> GetTranslationAsync(string key, string? languageCode = null)
        {
            try
            {
                languageCode ??= GetCurrentLanguageCode();
                
                // Check cache first
                var cacheKey = $"{languageCode}:{key}";
                if (_cachedTranslations.ContainsKey(cacheKey) && 
                    DateTime.UtcNow - _lastCacheUpdate < _cacheExpiry)
                {
                    return _cachedTranslations[cacheKey];
                }

                // Get from database
                var translation = await _context.LabelTranslations
                    .Include(lt => lt.Language)
                    .FirstOrDefaultAsync(lt => 
                        lt.Language.LanguageCode == languageCode && 
                        lt.TranslationKey == key &&
                        lt.Status == "Active");

                if (translation != null)
                {
                    _cachedTranslations[cacheKey] = translation.Value;
                    return translation.Value;
                }

                // Fallback to English if not found
                if (languageCode != "en")
                {
                    var englishTranslation = await _context.LabelTranslations
                        .Include(lt => lt.Language)
                        .FirstOrDefaultAsync(lt => 
                            lt.Language.LanguageCode == "en" && 
                            lt.TranslationKey == key &&
                            lt.Status == "Active");

                    if (englishTranslation != null)
                    {
                        return englishTranslation.Value;
                    }
                }

                // Return key if no translation found
                return key;
            }
            catch (Exception ex)
            {
                // Log exception if logging is available
                Console.WriteLine($"Error getting translation for key '{key}': {ex.Message}");
                return key;
            }
        }

        public async Task<Dictionary<string, string>> GetAllTranslationsAsync(string languageCode)
        {
            try
            {
                var translations = await _context.LabelTranslations
                    .Include(lt => lt.Language)
                    .Where(lt => 
                        lt.Language.LanguageCode == languageCode &&
                        lt.Status == "Active")
                    .ToDictionaryAsync(lt => lt.TranslationKey, lt => lt.Value);

                // Update cache
                foreach (var translation in translations)
                {
                    var cacheKey = $"{languageCode}:{translation.Key}";
                    _cachedTranslations[cacheKey] = translation.Value;
                }
                _lastCacheUpdate = DateTime.UtcNow;

                return translations;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting all translations for language '{languageCode}': {ex.Message}");
                return new Dictionary<string, string>();
            }
        }

        public async Task<List<Language>> GetAvailableLanguagesAsync()
        {
            try
            {
                return await _context.Languages
                    .Where(l => l.Status == "Active")
                    .OrderBy(l => l.LanguageName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting available languages: {ex.Message}");
                return new List<Language>();
            }
        }

        public async Task SetCurrentLanguageAsync(string languageCode)
        {
            try
            {
                var language = await _context.Languages
                    .FirstOrDefaultAsync(l => l.LanguageCode == languageCode && l.Status == "Active");

                if (language != null)
                {
                    _currentLanguage = language;
                    
                    // Set culture for formatting
                    var culture = new CultureInfo(languageCode == "ur" ? "ur-PK" : "en-US");
                    CultureInfo.CurrentCulture = culture;
                    CultureInfo.CurrentUICulture = culture;

                    // Clear cache to force reload
                    _cachedTranslations.Clear();
                    
                    // Notify about language change
                    LanguageChanged?.Invoke(this, languageCode);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting current language to '{languageCode}': {ex.Message}");
            }
        }

        public async Task<Language?> GetCurrentLanguageAsync()
        {
            if (_currentLanguage == null)
            {
                // Load default language from database
                _currentLanguage = await _context.Languages
                    .FirstOrDefaultAsync(l => l.LanguageCode == "en" && l.Status == "Active");
            }
            return _currentLanguage;
        }

        public string GetCurrentLanguageCode()
        {
            return _currentLanguage?.LanguageCode ?? "en";
        }

        public bool IsRightToLeft()
        {
            return _currentLanguage?.IsRtl ?? false;
        }

        public async Task<bool> SaveTranslationAsync(string key, string value, string languageCode, string? description = null)
        {
            try
            {
                var language = await _context.Languages
                    .FirstOrDefaultAsync(l => l.LanguageCode == languageCode && l.Status == "Active");

                if (language == null)
                    return false;

                // Check if keyword exists, if not create it
                var keyword = await _context.LanguageKeywords
                    .FirstOrDefaultAsync(lk => lk.Key == key);

                if (keyword == null)
                {
                    keyword = new LanguageKeyword
                    {
                        Key = key,
                        Description = description ?? $"Translation key for {key}"
                    };
                    _context.LanguageKeywords.Add(keyword);
                    await _context.SaveChangesAsync();
                }

                // Check if translation exists
                var existingTranslation = await _context.LabelTranslations
                    .FirstOrDefaultAsync(lt => 
                        lt.LanguageId == language.Id && 
                        lt.TranslationKey == key);

                if (existingTranslation != null)
                {
                    // Update existing translation
                    existingTranslation.Value = value;
                    existingTranslation.UpdatedAt = DateTime.UtcNow;
                    existingTranslation.UpdatedBy = "System";
                }
                else
                {
                    // Create new translation
                    var newTranslation = new LabelTranslation
                    {
                        LanguageId = language.Id,
                        TranslationKey = key,
                        Value = value,
                        Status = "Active",
                        CreatedBy = "System",
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.LabelTranslations.Add(newTranslation);
                }

                await _context.SaveChangesAsync();
                
                // Clear cache for this key
                var cacheKey = $"{languageCode}:{key}";
                _cachedTranslations.Remove(cacheKey);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving translation for key '{key}': {ex.Message}");
                return false;
            }
        }

        public async Task<bool> AddLanguageKeywordAsync(string key, string? description = null)
        {
            try
            {
                var existingKeyword = await _context.LanguageKeywords
                    .FirstOrDefaultAsync(lk => lk.Key == key);

                if (existingKeyword != null)
                    return true; // Already exists

                var keyword = new LanguageKeyword
                {
                    Key = key,
                    Description = description ?? $"Translation key for {key}"
                };

                _context.LanguageKeywords.Add(keyword);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding language keyword '{key}': {ex.Message}");
                return false;
            }
        }
    }
}
