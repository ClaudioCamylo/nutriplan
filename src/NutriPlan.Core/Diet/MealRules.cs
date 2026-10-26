using NutriPlan.Core.Models;

namespace NutriPlan.Core.Diet;

public static class MealRules
{
    private const int MaxPerRoleInMainMeal = 2;

    public static bool IsBalanced(MealType type, IReadOnlyCollection<Food> combo)
    {
        bool Has(FoodRole role) => combo.Any(f => f.Role == role);
        int Count(FoodRole role) => combo.Count(f => f.Role == role);

        return type switch
        {
            MealType.Breakfast => Has(FoodRole.Carb) && (Has(FoodRole.Protein) || Has(FoodRole.Dairy)),
            MealType.Lunch or MealType.Dinner =>
                Has(FoodRole.Carb) && Has(FoodRole.Protein) && Has(FoodRole.Vegetable)
                && Count(FoodRole.Carb) <= MaxPerRoleInMainMeal && Count(FoodRole.Protein) <= MaxPerRoleInMainMeal,
            MealType.Snack => Has(FoodRole.Fruit) || Has(FoodRole.Dairy),
            _ => true
        };
    }
}
