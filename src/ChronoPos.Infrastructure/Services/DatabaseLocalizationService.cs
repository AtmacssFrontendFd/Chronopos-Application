using ChronoPos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;

namespace ChronoPos.Infrastructure.Services
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
        private readonly IServiceScopeFactory _scopeFactory;
        private Language? _currentLanguage;
        private Dictionary<string, string> _cachedTranslations = new();
        private DateTime _lastCacheUpdate = DateTime.MinValue;
        private readonly TimeSpan _cacheExpiry = TimeSpan.FromMinutes(30);
        private readonly string _languagePreferenceFile;

        public event EventHandler<string>? LanguageChanged;

        public DatabaseLocalizationService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            
            // Set path for language preference file
            var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ChronoPos");
            Directory.CreateDirectory(appDataPath);
            _languagePreferenceFile = Path.Combine(appDataPath, "language_preference.txt");
            
            // Try to load saved language preference
            var savedLanguageCode = LoadLanguagePreference();
            
            // Set default language
            _currentLanguage = new Language 
            { 
                Id = 1, 
                LanguageName = savedLanguageCode == "ur" ? "Urdu" : "English", 
                LanguageCode = savedLanguageCode ?? "en", 
                IsRtl = savedLanguageCode == "ur" 
            };
            
            Console.WriteLine($"🌐 [DatabaseLocalizationService] Initialized with language: {_currentLanguage.LanguageCode}");
        }

        private string? LoadLanguagePreference()
        {
            try
            {
                if (File.Exists(_languagePreferenceFile))
                {
                    var languageCode = File.ReadAllText(_languagePreferenceFile).Trim();
                    Console.WriteLine($"📂 [DatabaseLocalizationService] Loaded saved language preference: '{languageCode}'");
                    return languageCode;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ [DatabaseLocalizationService] Error loading language preference: {ex.Message}");
            }
            return null;
        }

        private void SaveLanguagePreference(string languageCode)
        {
            try
            {
                File.WriteAllText(_languagePreferenceFile, languageCode);
                Console.WriteLine($"💾 [DatabaseLocalizationService] Saved language preference: '{languageCode}'");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ [DatabaseLocalizationService] Error saving language preference: {ex.Message}");
            }
        }

        private async Task InitializeCurrentLanguageAsync()
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ChronoPosDbContext>();
                
                var defaultLanguage = await context.Languages
                    .FirstOrDefaultAsync(l => l.LanguageCode == "en" && l.Status == "Active");
                
                if (defaultLanguage != null)
                {
                    _currentLanguage = defaultLanguage;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing current language: {ex.Message}");
                // Keep the default language set in constructor
            }
        }

        public async Task<string> GetTranslationAsync(string key, string? languageCode = null)
        {
            try
            {
                languageCode ??= GetCurrentLanguageCode();
                Console.WriteLine($"🔍 [DatabaseLocalizationService] GetTranslationAsync - Key: '{key}', Language: '{languageCode}'");
                
                // Check cache first
                var cacheKey = $"{languageCode}:{key}";
                if (_cachedTranslations.ContainsKey(cacheKey) && 
                    DateTime.UtcNow - _lastCacheUpdate < _cacheExpiry)
                {
                    var cachedValue = _cachedTranslations[cacheKey];
                    Console.WriteLine($"📋 [DatabaseLocalizationService] Found in cache: '{cachedValue}'");
                    return cachedValue;
                }

                Console.WriteLine($"🔍 [DatabaseLocalizationService] Not in cache, querying database...");

                // Get from database
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ChronoPosDbContext>();
                
                var translation = await context.LabelTranslations
                    .Include(lt => lt.Language)
                    .FirstOrDefaultAsync(lt => 
                        lt.Language.LanguageCode == languageCode && 
                        lt.TranslationKey == key &&
                        lt.Status == "Active");

                if (translation != null)
                {
                    Console.WriteLine($"✅ [DatabaseLocalizationService] Found translation: '{translation.Value}' for key '{key}' in language '{languageCode}'");
                    _cachedTranslations[cacheKey] = translation.Value;
                    return translation.Value;
                }

                Console.WriteLine($"⚠️ [DatabaseLocalizationService] No translation found for key '{key}' in language '{languageCode}'");

                // Fallback to English if not found
                if (languageCode != "en")
                {
                    Console.WriteLine($"🔄 [DatabaseLocalizationService] Trying fallback to English for key '{key}'");
                    var englishTranslation = await context.LabelTranslations
                        .Include(lt => lt.Language)
                        .FirstOrDefaultAsync(lt => 
                            lt.Language.LanguageCode == "en" && 
                            lt.TranslationKey == key &&
                            lt.Status == "Active");

                    if (englishTranslation != null)
                    {
                        Console.WriteLine($"✅ [DatabaseLocalizationService] Found English fallback: '{englishTranslation.Value}' for key '{key}'");
                        return englishTranslation.Value;
                    }
                }

                Console.WriteLine($"❌ [DatabaseLocalizationService] No translation found anywhere for key '{key}', returning key itself");
                // Return key if no translation found
                return key;
            }
            catch (Exception ex)
            {
                // Log exception if logging is available
                Console.WriteLine($"❌ [DatabaseLocalizationService] Error getting translation for key '{key}': {ex.Message}");
                Console.WriteLine($"❌ [DatabaseLocalizationService] Stack trace: {ex.StackTrace}");
                return key;
            }
        }

        public async Task<Dictionary<string, string>> GetAllTranslationsAsync(string languageCode)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ChronoPosDbContext>();
                
                var translations = await context.LabelTranslations
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
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ChronoPosDbContext>();
                
                return await context.Languages
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
                Console.WriteLine($"🔄 [DatabaseLocalizationService] SetCurrentLanguageAsync called with: '{languageCode}'");
                
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ChronoPosDbContext>();
                
                var language = await context.Languages
                    .FirstOrDefaultAsync(l => l.LanguageCode == languageCode && l.Status == "Active");

                if (language != null)
                {
                    Console.WriteLine($"✅ [DatabaseLocalizationService] Found language: {language.LanguageName} ({language.LanguageCode})");
                    
                    var previousLanguage = _currentLanguage?.LanguageCode ?? "none";
                    _currentLanguage = language;
                    
                    // Save language preference to file
                    SaveLanguagePreference(languageCode);
                    
                    Console.WriteLine($"🔄 [DatabaseLocalizationService] Language changed from '{previousLanguage}' to '{languageCode}'");
                    
                    // Set culture for formatting
                    var culture = new CultureInfo(languageCode == "ur" ? "ur-PK" : "en-US");
                    CultureInfo.CurrentCulture = culture;
                    CultureInfo.CurrentUICulture = culture;
                    Console.WriteLine($"🌐 [DatabaseLocalizationService] Culture set to: {culture.Name}");

                    // Clear cache to force reload
                    var cacheCount = _cachedTranslations.Count;
                    _cachedTranslations.Clear();
                    Console.WriteLine($"🗑️ [DatabaseLocalizationService] Cleared {cacheCount} cached translations");
                    
                    // Notify about language change
                    Console.WriteLine($"📢 [DatabaseLocalizationService] Firing LanguageChanged event with '{languageCode}'");
                    LanguageChanged?.Invoke(this, languageCode);
                }
                else
                {
                    Console.WriteLine($"❌ [DatabaseLocalizationService] Language '{languageCode}' not found in database!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [DatabaseLocalizationService] Error setting current language to '{languageCode}': {ex.Message}");
                Console.WriteLine($"❌ [DatabaseLocalizationService] Stack trace: {ex.StackTrace}");
            }
        }

        public async Task<Language?> GetCurrentLanguageAsync()
        {
            if (_currentLanguage == null)
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ChronoPosDbContext>();
                
                // Load default language from database
                _currentLanguage = await context.Languages
                    .FirstOrDefaultAsync(l => l.LanguageCode == "en" && l.Status == "Active");
            }
            return _currentLanguage;
        }

        public string GetCurrentLanguageCode()
        {
            var code = _currentLanguage?.LanguageCode ?? "en";
            Console.WriteLine($"🎯 [DatabaseLocalizationService] GetCurrentLanguageCode returning: '{code}'");
            return code;
        }

        public bool IsRightToLeft()
        {
            return _currentLanguage?.IsRtl ?? false;
        }

        public async Task<bool> SaveTranslationAsync(string key, string value, string languageCode, string? description = null)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ChronoPosDbContext>();
                
                var language = await context.Languages
                    .FirstOrDefaultAsync(l => l.LanguageCode == languageCode && l.Status == "Active");

                if (language == null)
                    return false;

                // Check if keyword exists, if not create it
                var keyword = await context.LanguageKeywords
                    .FirstOrDefaultAsync(lk => lk.Key == key);

                if (keyword == null)
                {
                    keyword = new LanguageKeyword
                    {
                        Key = key,
                        Description = description ?? $"Translation key for {key}"
                    };
                    context.LanguageKeywords.Add(keyword);
                    await context.SaveChangesAsync();
                }

                // Check if translation exists
                var existingTranslation = await context.LabelTranslations
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
                    context.LabelTranslations.Add(newTranslation);
                }

                await context.SaveChangesAsync();
                
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
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ChronoPosDbContext>();
                
                var existingKeyword = await context.LanguageKeywords
                    .FirstOrDefaultAsync(lk => lk.Key == key);

                if (existingKeyword != null)
                    return true; // Already exists

                var keyword = new LanguageKeyword
                {
                    Key = key,
                    Description = description ?? $"Translation key for {key}"
                };

                context.LanguageKeywords.Add(keyword);
                await context.SaveChangesAsync();

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
