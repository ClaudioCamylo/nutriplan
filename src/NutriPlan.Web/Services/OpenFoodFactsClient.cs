using System.Text.Json;
using NutriPlan.Core.Foods;
using NutriPlan.Core.Models;

namespace NutriPlan.Web.Services;

public class OpenFoodFactsClient : IFoodApi
{
    private const string ErrorMessage = "Não foi possível consultar o Open Food Facts agora.";
    private readonly HttpClient _http;

    public OpenFoodFactsClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<FoodApiResult> SearchAsync(string term, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(term))
        {
            return new FoodApiResult(Array.Empty<ExternalFood>(), null);
        }

        var url = "cgi/search.pl?search_simple=1&action=process&json=1&page_size=10&fields=product_name,nutriments"
                  + $"&search_terms={Uri.EscapeDataString(term.Trim())}";

        try
        {
            using var response = await _http.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            return new FoodApiResult(Parse(json), null);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            return new FoodApiResult(Array.Empty<ExternalFood>(), ErrorMessage);
        }
    }

    public static IReadOnlyList<ExternalFood> Parse(string json)
    {
        using var document = JsonDocument.Parse(json);
        var foods = new List<ExternalFood>();

        if (!document.RootElement.TryGetProperty("products", out var products)
            || products.ValueKind != JsonValueKind.Array)
        {
            return foods;
        }

        foreach (var product in products.EnumerateArray())
        {
            if (!product.TryGetProperty("product_name", out var name)
                || name.ValueKind != JsonValueKind.String
                || string.IsNullOrWhiteSpace(name.GetString()))
            {
                continue;
            }

            if (TryReadKcal(product, out var kcal))
            {
                foods.Add(new ExternalFood(name.GetString()!.Trim(), kcal));
            }
        }

        return foods;
    }

    private static bool TryReadKcal(JsonElement product, out double kcal)
    {
        kcal = 0;
        if (!product.TryGetProperty("nutriments", out var nutriments)
            || !nutriments.TryGetProperty("energy-kcal_100g", out var value))
        {
            return false;
        }

        return value.ValueKind switch
        {
            JsonValueKind.Number => value.TryGetDouble(out kcal),
            JsonValueKind.String => double.TryParse(
                value.GetString(), System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out kcal),
            _ => false
        };
    }
}
