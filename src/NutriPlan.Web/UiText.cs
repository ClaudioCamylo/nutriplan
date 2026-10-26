using System.Globalization;
using NutriPlan.Core.Models;

namespace NutriPlan.Web;

public static class UiText
{
    private static readonly CultureInfo PtBr = new("pt-BR");

    private static readonly string[] WeekDays =
        { "Segunda", "Terça", "Quarta", "Quinta", "Sexta", "Sábado", "Domingo" };

    public static string Number(double value, int decimals = 0) =>
        value.ToString("N" + decimals, PtBr);

    public static string Date(DateTime date) => date.ToLocalTime().ToString("dd/MM/yyyy", PtBr);

    public static string WeekDay(int day) => WeekDays[(day - 1) % WeekDays.Length];

    public static string Label(Sex sex) => sex == Sex.Male ? "Masculino" : "Feminino";

    public static string Label(Goal goal) => goal switch
    {
        Goal.Lose => "Emagrecer",
        Goal.Gain => "Ganhar peso",
        _ => "Manter o peso"
    };

    public static string Label(ActivityLevel level) => level switch
    {
        ActivityLevel.Sedentary => "Sedentário",
        ActivityLevel.Light => "Levemente ativo (1–3 dias/semana)",
        ActivityLevel.Moderate => "Moderadamente ativo (3–5 dias/semana)",
        ActivityLevel.Heavy => "Muito ativo (6–7 dias/semana)",
        _ => "Extremamente ativo (trabalho físico + treino)"
    };

    public static string Label(MealType type) => type switch
    {
        MealType.Breakfast => "Café da manhã",
        MealType.Lunch => "Almoço",
        MealType.Snack => "Lanche da tarde",
        _ => "Jantar"
    };

    public static string Label(FoodRole role) => role switch
    {
        FoodRole.Carb => "Carboidrato",
        FoodRole.Protein => "Proteína",
        FoodRole.Dairy => "Laticínio",
        FoodRole.Fruit => "Fruta",
        FoodRole.Vegetable => "Vegetal",
        _ => "Gordura boa"
    };
}
