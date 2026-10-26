using NutriPlan.Core.Calculations;
using NutriPlan.Core.Models;

namespace NutriPlan.Tests;

public class BodyCalculatorTests
{
    private static UserProfile ExampleProfile(Goal goal = Goal.Lose) => new()
    {
        Name = "Exemplo",
        Sex = Sex.Male,
        Age = 19,
        WeightKg = 112,
        HeightCm = 170,
        Activity = ActivityLevel.Sedentary,
        Goal = goal,
        WeeklyGoalKg = 0.5
    };

    [Fact]
    public void Imc_ExemploDoTrabalho_RetornaAproximadamente38Virgula8()
    {
        Assert.Equal(38.8, BodyCalculator.Imc(112, 170), 1);
    }

    [Theory]
    [InlineData(17.0, "Abaixo do peso")]
    [InlineData(22.0, "Peso normal")]
    [InlineData(27.5, "Sobrepeso")]
    [InlineData(32.0, "Obesidade grau I")]
    [InlineData(37.0, "Obesidade grau II")]
    [InlineData(41.0, "Obesidade grau III")]
    public void ClassifyImc_RetornaFaixaCorreta(double imc, string esperado)
    {
        Assert.Equal(esperado, BodyCalculator.ClassifyImc(imc));
    }

    [Fact]
    public void Tmb_Homem_UsaMifflinStJeor()
    {
        Assert.Equal(2092.5, BodyCalculator.Tmb(Sex.Male, 112, 170, 19), 2);
    }

    [Fact]
    public void Tmb_Mulher_SubtraiConstante161()
    {
        Assert.Equal(10 * 60 + 6.25 * 165 - 5 * 30 - 161, BodyCalculator.Tmb(Sex.Female, 60, 165, 30), 2);
    }

    [Theory]
    [InlineData(ActivityLevel.Sedentary, 1.2)]
    [InlineData(ActivityLevel.Light, 1.375)]
    [InlineData(ActivityLevel.Moderate, 1.55)]
    [InlineData(ActivityLevel.Heavy, 1.725)]
    [InlineData(ActivityLevel.Extreme, 1.9)]
    public void ActivityFactor_RetornaFatorDaTabela(ActivityLevel nivel, double fator)
    {
        Assert.Equal(fator, BodyCalculator.ActivityFactor(nivel));
    }

    [Fact]
    public void DailyEnergy_ExemploDoTrabalho_Retorna2511()
    {
        var tmb = BodyCalculator.Tmb(Sex.Male, 112, 170, 19);
        Assert.Equal(2511, BodyCalculator.DailyEnergy(tmb, ActivityLevel.Sedentary), 0);
    }

    [Fact]
    public void CalorieTarget_Emagrecer_SubtraiDeficitDe500()
    {
        Assert.Equal(2011, BodyCalculator.CalorieTarget(ExampleProfile(Goal.Lose)), 0);
    }

    [Fact]
    public void CalorieTarget_Ganhar_SomaSuperavitDe500()
    {
        Assert.Equal(3011, BodyCalculator.CalorieTarget(ExampleProfile(Goal.Gain)), 0);
    }

    [Fact]
    public void CalorieTarget_Manter_IgualAoGastoDiario()
    {
        Assert.Equal(2511, BodyCalculator.CalorieTarget(ExampleProfile(Goal.Maintain)), 0);
    }

    [Fact]
    public void CalorieTarget_NuncaFicaAbaixoDoPisoDeSeguranca()
    {
        var pessoaLeve = new UserProfile
        {
            Sex = Sex.Female,
            Age = 60,
            WeightKg = 45,
            HeightCm = 150,
            Activity = ActivityLevel.Sedentary,
            Goal = Goal.Lose,
            WeeklyGoalKg = 1
        };

        Assert.Equal(BodyCalculator.MinKcalFemale, BodyCalculator.CalorieTarget(pessoaLeve));
    }

    [Fact]
    public void CalorieTarget_AjusteExtraAprofundaODeficit()
    {
        var perfil = ExampleProfile(Goal.Lose);
        perfil.ExtraAdjustmentKcal = 100;

        Assert.Equal(1911, BodyCalculator.CalorieTarget(perfil), 0);
    }
}
