using NutriPlan.Core.Models;

namespace NutriPlan.Core.Calculations;

public static class BodyCalculator
{
    public const double MinKcalMale = 1500;
    public const double MinKcalFemale = 1200;
    public const double DailyAdjustmentKcal = 500;

    public static double Imc(double weightKg, double heightCm)
    {
        var heightM = heightCm / 100;
        return weightKg / (heightM * heightM);
    }

    public static string ClassifyImc(double imc) => imc switch
    {
        < 18.5 => "Abaixo do peso",
        < 25 => "Peso normal",
        < 30 => "Sobrepeso",
        < 35 => "Obesidade grau I",
        < 40 => "Obesidade grau II",
        _ => "Obesidade grau III"
    };

    public static double Tmb(Sex sex, double weightKg, double heightCm, int age)
    {
        var baseValue = 10 * weightKg + 6.25 * heightCm - 5 * age;
        return sex == Sex.Male ? baseValue + 5 : baseValue - 161;
    }

    public static double ActivityFactor(ActivityLevel level) => level switch
    {
        ActivityLevel.Sedentary => 1.2,
        ActivityLevel.Light => 1.375,
        ActivityLevel.Moderate => 1.55,
        ActivityLevel.Heavy => 1.725,
        ActivityLevel.Extreme => 1.9,
        _ => throw new ArgumentOutOfRangeException(nameof(level))
    };

    public static double DailyEnergy(double tmb, ActivityLevel level) => tmb * ActivityFactor(level);

    public static double MinimumKcal(Sex sex) => sex == Sex.Male ? MinKcalMale : MinKcalFemale;

    public static double CalorieTarget(UserProfile profile)
    {
        var tmb = Tmb(profile.Sex, profile.WeightKg, profile.HeightCm, profile.Age);
        var energy = DailyEnergy(tmb, profile.Activity);

        var target = profile.Goal switch
        {
            Goal.Lose => energy - DailyAdjustmentKcal - profile.ExtraAdjustmentKcal,
            Goal.Gain => energy + DailyAdjustmentKcal + profile.ExtraAdjustmentKcal,
            _ => energy
        };

        return Math.Round(Math.Max(target, MinimumKcal(profile.Sex)));
    }
}
