using NutriPlan.Core.Models;

namespace NutriPlan.Core.Validation;

public static class ProfileValidator
{
    public const int MinAge = 18;
    public const int MaxAge = 100;
    public const double MinWeight = 30;
    public const double MaxWeight = 400;
    public const double MinHeight = 100;
    public const double MaxHeight = 250;
    public const double MinWeeklyGoal = 0.1;
    public const double MaxWeeklyGoal = 1.0;

    public static IReadOnlyList<string> Validate(UserProfile profile)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(profile.Name))
        {
            errors.Add("Nome é obrigatório.");
        }

        if (profile.Age < MinAge)
        {
            errors.Add($"O NutriPlan é destinado a maiores de {MinAge} anos.");
        }
        else if (profile.Age > MaxAge)
        {
            errors.Add($"Idade deve ser no máximo {MaxAge} anos.");
        }

        if (profile.WeightKg < MinWeight || profile.WeightKg > MaxWeight)
        {
            errors.Add($"Peso deve estar entre {MinWeight} e {MaxWeight} kg.");
        }

        if (profile.HeightCm < MinHeight || profile.HeightCm > MaxHeight)
        {
            errors.Add($"Altura deve estar entre {MinHeight} e {MaxHeight} cm.");
        }

        if (profile.WeeklyGoalKg < MinWeeklyGoal || profile.WeeklyGoalKg > MaxWeeklyGoal)
        {
            errors.Add($"Meta semanal deve estar entre {MinWeeklyGoal:0.0} e {MaxWeeklyGoal:0.0} kg.");
        }

        return errors;
    }

    public static string? ValidateWeight(double weightKg) =>
        weightKg < MinWeight || weightKg > MaxWeight
            ? $"Peso deve estar entre {MinWeight} e {MaxWeight} kg."
            : null;
}
