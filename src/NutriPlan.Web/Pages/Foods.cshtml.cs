using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NutriPlan.Core.Foods;
using NutriPlan.Core.Models;
using NutriPlan.Web.Data;

namespace NutriPlan.Web.Pages;

public class FoodsModel : PageModel
{
    private readonly FoodRepository _foods;
    private readonly IFoodApi _api;

    public FoodsModel(FoodRepository foods, IFoodApi api)
    {
        _foods = foods;
        _api = api;
    }

    [BindProperty(SupportsGet = true)]
    public string? Q { get; set; }

    public IReadOnlyList<Food> LocalFoods { get; private set; } = Array.Empty<Food>();
    public FoodApiResult? External { get; private set; }
    public string? Error { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        var term = Q?.Trim();
        try
        {
            LocalFoods = string.IsNullOrEmpty(term) ? _foods.GetAll() : _foods.Search(term);
        }
        catch (DataAccessException ex)
        {
            Error = ex.Message;
        }

        if (!string.IsNullOrEmpty(term))
        {
            External = await _api.SearchAsync(term, cancellationToken);
        }
    }
}
