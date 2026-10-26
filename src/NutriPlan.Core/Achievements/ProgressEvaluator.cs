using NutriPlan.Core.Models;

namespace NutriPlan.Core.Achievements;

public record Badge(string Code, string Title, string Description);

public record WeekEvaluation(
    bool GoalMet,
    double DeltaKg,
    int Streak,
    int ExtraAdjustmentKcal,
    IReadOnlyList<Badge> NewBadges);

public static class ProgressEvaluator
{
    public const double MaintainToleranceKg = 0.5;
    public const int ExtraAdjustmentStepKcal = 100;
    public const int MaxExtraAdjustmentKcal = 300;
    private const int MissesBeforeAdjustment = 2;

    private static readonly (int Length, string Title)[] StreakBadges =
    {
        (2, "Dupla de ouro"),
        (4, "Mês de foco"),
        (8, "Dois meses firmes")
    };

    public static WeekEvaluation Evaluate(
        Goal goal,
        double weeklyGoalKg,
        double previousWeightKg,
        double newWeightKg,
        int weekNumber,
        IReadOnlyList<bool> history,
        int currentExtraAdjustmentKcal)
    {
        var delta = newWeightKg - previousWeightKg;
        var goalMet = IsGoalMet(goal, weeklyGoalKg, delta);

        var streak = goalMet ? TrailingCount(history, true) + 1 : 0;
        var extra = NextExtraAdjustment(goal, goalMet, TrailingCount(history, false) + 1, currentExtraAdjustmentKcal);
        var badges = goalMet ? BuildBadges(weekNumber, streak, history) : new List<Badge>();

        return new WeekEvaluation(goalMet, delta, streak, extra, badges);
    }

    private static bool IsGoalMet(Goal goal, double weeklyGoalKg, double deltaKg) => goal switch
    {
        Goal.Lose => -deltaKg >= weeklyGoalKg - 1e-9,
        Goal.Gain => deltaKg >= weeklyGoalKg - 1e-9,
        _ => Math.Abs(deltaKg) <= MaintainToleranceKg + 1e-9
    };

    private static int NextExtraAdjustment(Goal goal, bool goalMet, int consecutiveMisses, int current)
    {
        if (goal == Goal.Maintain || goalMet)
        {
            return 0;
        }

        return consecutiveMisses >= MissesBeforeAdjustment
            ? Math.Min(current + ExtraAdjustmentStepKcal, MaxExtraAdjustmentKcal)
            : current;
    }

    private static List<Badge> BuildBadges(int weekNumber, int streak, IReadOnlyList<bool> history)
    {
        var badges = new List<Badge>
        {
            new($"week-{weekNumber}", $"Semana {weekNumber} cumprida", "Você atingiu a meta de peso da semana.")
        };

        if (!history.Contains(true))
        {
            badges.Add(new Badge("first-goal", "Primeira conquista", "Sua primeira meta semanal foi alcançada."));
        }

        foreach (var (length, title) in StreakBadges.Where(b => b.Length == streak))
        {
            badges.Add(new Badge($"streak-{length}", title, $"{length} semanas seguidas cumprindo a meta."));
        }

        return badges;
    }

    private static int TrailingCount(IReadOnlyList<bool> history, bool value)
    {
        var count = 0;
        for (var i = history.Count - 1; i >= 0 && history[i] == value; i--)
        {
            count++;
        }

        return count;
    }
}
