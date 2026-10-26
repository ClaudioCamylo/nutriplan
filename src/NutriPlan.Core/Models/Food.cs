namespace NutriPlan.Core.Models;

public record Food(
    int Id,
    string Name,
    FoodRole Role,
    double KcalPer100g,
    double PortionGrams,
    MealType[] Meals)
{
    public double KcalPerPortion => KcalPer100g * PortionGrams / 100;
}

public record FoodPortion(Food Food, double Grams)
{
    public double Kcal => Food.KcalPer100g * Grams / 100;
}

public record Meal(MealType Type, double TargetKcal, IReadOnlyList<FoodPortion> Items)
{
    public double TotalKcal => Items.Sum(i => i.Kcal);

    public string Key => string.Join("-", Items.Select(i => i.Food.Id).OrderBy(id => id));
}

public record DayPlan(int Day, IReadOnlyList<Meal> Meals)
{
    public double TotalKcal => Meals.Sum(m => m.TotalKcal);
}

public record WeeklyPlan(double DailyTarget, IReadOnlyList<DayPlan> Days);

public record ExternalFood(string Name, double KcalPer100g);
