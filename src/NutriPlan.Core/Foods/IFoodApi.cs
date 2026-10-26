using NutriPlan.Core.Models;

namespace NutriPlan.Core.Foods;

public record FoodApiResult(IReadOnlyList<ExternalFood> Foods, string? Error)
{
    public bool Succeeded => Error is null;
}

public interface IFoodApi
{
    Task<FoodApiResult> SearchAsync(string term, CancellationToken cancellationToken = default);
}
