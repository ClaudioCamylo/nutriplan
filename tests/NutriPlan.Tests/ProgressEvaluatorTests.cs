using NutriPlan.Core.Achievements;
using NutriPlan.Core.Models;

namespace NutriPlan.Tests;

public class ProgressEvaluatorTests
{
    private static readonly bool[] NoHistory = Array.Empty<bool>();

    [Fact]
    public void Evaluate_EmagrecerAtingindoMeta_MarcaComoCumprida()
    {
        var r = ProgressEvaluator.Evaluate(Goal.Lose, 1.0, 100, 99, 1, NoHistory, 0);

        Assert.True(r.GoalMet);
        Assert.Equal(-1.0, r.DeltaKg, 2);
    }

    [Fact]
    public void Evaluate_EmagrecerSemAtingirMeta_NaoCumpre()
    {
        var r = ProgressEvaluator.Evaluate(Goal.Lose, 1.0, 100, 99.5, 1, NoHistory, 0);

        Assert.False(r.GoalMet);
    }

    [Fact]
    public void Evaluate_GanharAtingindoMeta_Cumpre()
    {
        var r = ProgressEvaluator.Evaluate(Goal.Gain, 0.5, 70, 70.6, 1, NoHistory, 0);

        Assert.True(r.GoalMet);
    }

    [Fact]
    public void Evaluate_ManterDentroDaTolerancia_Cumpre()
    {
        Assert.True(ProgressEvaluator.Evaluate(Goal.Maintain, 0.5, 80, 80.3, 1, NoHistory, 0).GoalMet);
        Assert.False(ProgressEvaluator.Evaluate(Goal.Maintain, 0.5, 80, 81.2, 1, NoHistory, 0).GoalMet);
    }

    [Fact]
    public void Evaluate_PrimeiraMetaCumprida_GanhaEmblemaDePrimeiraConquista()
    {
        var r = ProgressEvaluator.Evaluate(Goal.Lose, 0.5, 90, 89, 1, NoHistory, 0);

        Assert.Contains(r.NewBadges, b => b.Code == "first-goal");
        Assert.Contains(r.NewBadges, b => b.Code == "week-1");
    }

    [Fact]
    public void Evaluate_SequenciaDeDuasSemanas_GanhaEmblemaDeSequencia()
    {
        var r = ProgressEvaluator.Evaluate(Goal.Lose, 0.5, 90, 89, 2, new[] { true }, 0);

        Assert.Equal(2, r.Streak);
        Assert.Contains(r.NewBadges, b => b.Code == "streak-2");
        Assert.DoesNotContain(r.NewBadges, b => b.Code == "first-goal");
    }

    [Fact]
    public void Evaluate_MetaCumprida_ZeraAjusteExtra()
    {
        var r = ProgressEvaluator.Evaluate(Goal.Lose, 0.5, 90, 89, 3, new[] { false, false }, 200);

        Assert.Equal(0, r.ExtraAdjustmentKcal);
    }

    [Fact]
    public void Evaluate_DuasFalhasSeguidas_AumentaDeficitEm100()
    {
        var r = ProgressEvaluator.Evaluate(Goal.Lose, 0.5, 90, 90, 2, new[] { false }, 0);

        Assert.False(r.GoalMet);
        Assert.Equal(100, r.ExtraAdjustmentKcal);
    }

    [Fact]
    public void Evaluate_AjusteExtraTemLimiteDe300()
    {
        var r = ProgressEvaluator.Evaluate(Goal.Lose, 0.5, 90, 90, 6, new[] { false, false, false, false, false }, 300);

        Assert.Equal(300, r.ExtraAdjustmentKcal);
    }

    [Fact]
    public void Evaluate_Manter_NuncaTemAjusteExtra()
    {
        var r = ProgressEvaluator.Evaluate(Goal.Maintain, 0.5, 80, 85, 3, new[] { false, false }, 0);

        Assert.Equal(0, r.ExtraAdjustmentKcal);
    }

    [Fact]
    public void Evaluate_FalhaInterrompeASequencia()
    {
        var r = ProgressEvaluator.Evaluate(Goal.Lose, 0.5, 90, 90, 4, new[] { true, true, true }, 0);

        Assert.Equal(0, r.Streak);
    }
}
