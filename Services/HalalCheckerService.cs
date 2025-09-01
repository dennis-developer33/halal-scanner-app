using FuzzySharp;
using HalalScanner.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HalalScanner.Services
{
    public class HalalCheckerService
    {
        private List<HaramIngredient> _haramIngredients;
        private const int FuzzyMatchThreshold = 80;

        public async Task InitializeAsync()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = "HalalScanner.Resources.haram_ingredients.json";

                using var stream = assembly.GetManifestResourceStream(resourceName);
                using var reader = new StreamReader(stream);
                var json = await reader.ReadToEndAsync();

                _haramIngredients = JsonSerializer.Deserialize<List<HaramIngredient>>(json);
            }
            catch (Exception ex)
            {
                // Fallback to basic list if JSON fails to load
                _haramIngredients = GetDefaultHaramIngredients();
            }
        }

        public HalalCheckResult CheckIngredients(string ingredientText)
        {
            var result = new HalalCheckResult { IsHalal = true };

            if (string.IsNullOrEmpty(ingredientText))
                return result;

            var ingredients = ParseIngredients(ingredientText);

            foreach (var ingredient in ingredients)
            {
                var haramMatch = FindHaramMatch(ingredient);
                if (haramMatch != null)
                {
                    result.IsHalal = false;
                    result.FoundHaramIngredients.Add(haramMatch);
                }
            }

            result.Summary = GenerateSummary(result);
            return result;
        }

        private List<string> ParseIngredients(string text)
        {
            // Simple ingredient parsing - split by common delimiters
            var separators = new char[] { ',', ';', '(', ')', '\n', '\r' };
            return text.Split(separators, StringSplitOptions.RemoveEmptyEntries)
                      .Select(i => i.Trim().ToLower())
                      .Where(i => !string.IsNullOrEmpty(i))
                      .ToList();
        }

        private HaramIngredient FindHaramMatch(string ingredient)
        {
            foreach (var haramIngredient in _haramIngredients)
            {
                // Check exact match
                if (haramIngredient.Name.Equals(ingredient, StringComparison.OrdinalIgnoreCase))
                    return haramIngredient;

                // Check alternative names
                if (haramIngredient.AlternativeNames.Any(alt =>
                    alt.Equals(ingredient, StringComparison.OrdinalIgnoreCase)))
                    return haramIngredient;

                // Fuzzy matching
                var ratio = Fuzz.Ratio(haramIngredient.Name.ToLower(), ingredient);
                if (ratio >= FuzzyMatchThreshold)
                    return haramIngredient;

                // Check fuzzy match against alternative names
                foreach (var altName in haramIngredient.AlternativeNames)
                {
                    var altRatio = Fuzz.Ratio(altName.ToLower(), ingredient);
                    if (altRatio >= FuzzyMatchThreshold)
                        return haramIngredient;
                }
            }

            return null;
        }

        private string GenerateSummary(HalalCheckResult result)
        {
            if (result.IsHalal)
                return "✅ This product appears to be HALAL based on the ingredients found.";

            return $"❌ This product is NOT HALAL. Found {result.FoundHaramIngredients.Count} haram ingredient(s).";
        }

        private List<HaramIngredient> GetDefaultHaramIngredients()
        {
            return new List<HaramIngredient>
            {
                new HaramIngredient
                {
                    Name = "pork",
                    Reason = "Pork is explicitly forbidden in Islam",
                    AlternativeNames = new List<string> { "bacon", "ham", "prosciutto", "pepperoni" }
                },
                new HaramIngredient
                {
                    Name = "alcohol",
                    Reason = "Alcohol is forbidden in Islam",
                    AlternativeNames = new List<string> { "ethanol", "wine", "beer", "rum", "vodka" }
                },
                new HaramIngredient
                {
                    Name = "gelatin",
                    Reason = "Usually derived from pork, unless specified as halal/beef gelatin",
                    AlternativeNames = new List<string> { "gelatine" }
                },
                new HaramIngredient
                {
                    Name = "lard",
                    Reason = "Pork fat",
                    AlternativeNames = new List<string> { "pig fat", "pork fat" }
                }
            };
        }
    }
}
