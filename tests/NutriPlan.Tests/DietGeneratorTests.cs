using NutriPlan.Core.Diet;
using NutriPlan.Core.Models;

namespace NutriPlan.Tests;

public class DietGeneratorTests
{
    private readonly DietGenerator _generator = new(FoodCatalog.Seed);

    [Fact]
    public void Generate_CriaSeteDiasComQuatroRefeicoes()
    {
        var plano = _generator.Generate(2000, seed: 1);

        Assert.Equal(7, plano.Days.Count);
        Assert.All(plano.Days, d => Assert.Equal(4, d.Meals.Count));
    }

    [Theory]
    [InlineData(1200)]
    [InlineData(2011)]
    [InlineData(3011)]
    public void Generate_TotalDiarioFicaProximoDaMeta(double meta)
    {
        var plano = _generator.Generate(meta, seed: 7);

        Assert.All(plano.Days, d => Assert.InRange(d.TotalKcal, meta * 0.95, meta * 1.05));
    }

    [Fact]
    public void Generate_DivideCaloriasConformePercentuais()
    {
        var plano = _generator.Generate(2000, seed: 3);
        var dia = plano.Days[0];

        Assert.Equal(500, dia.Meals.Single(m => m.Type == MealType.Breakfast).TargetKcal, 0);
        Assert.Equal(700, dia.Meals.Single(m => m.Type == MealType.Lunch).TargetKcal, 0);
        Assert.Equal(300, dia.Meals.Single(m => m.Type == MealType.Snack).TargetKcal, 0);
        Assert.Equal(500, dia.Meals.Single(m => m.Type == MealType.Dinner).TargetKcal, 0);
    }

    [Fact]
    public void Generate_NaoRepeteORefeicaoDoDiaAnterior()
    {
        var plano = _generator.Generate(2200, seed: 11);

        foreach (var tipo in Enum.GetValues<MealType>())
        {
            for (var i = 1; i < plano.Days.Count; i++)
            {
                var anterior = plano.Days[i - 1].Meals.Single(m => m.Type == tipo).Key;
                var atual = plano.Days[i].Meals.Single(m => m.Type == tipo).Key;
                Assert.NotEqual(anterior, atual);
            }
        }
    }

    [Fact]
    public void Generate_RespeitaQuantidadeDeAlimentosPorRefeicao()
    {
        var plano = _generator.Generate(2500, seed: 5);

        Assert.All(plano.Days.SelectMany(d => d.Meals), m => Assert.InRange(m.Items.Count, 2, 6));
    }

    [Fact]
    public void Generate_AlmocoTemCarboidratoProteinaEVegetal()
    {
        var plano = _generator.Generate(2000, seed: 9);

        foreach (var almoco in plano.Days.Select(d => d.Meals.Single(m => m.Type == MealType.Lunch)))
        {
            var papeis = almoco.Items.Select(i => i.Food.Role).ToHashSet();
            Assert.Contains(FoodRole.Carb, papeis);
            Assert.Contains(FoodRole.Protein, papeis);
            Assert.Contains(FoodRole.Vegetable, papeis);
        }
    }

    [Fact]
    public void Generate_RefeicoesPrincipaisNaoTemMaisDeDoisCarboidratos()
    {
        var plano = _generator.Generate(2500, seed: 13);

        var principais = plano.Days.SelectMany(d => d.Meals)
            .Where(m => m.Type is MealType.Lunch or MealType.Dinner);
        Assert.All(principais, m => Assert.True(m.Items.Count(i => i.Food.Role == FoodRole.Carb) <= 2));
    }

    [Fact]
    public void Generate_MesmaSementeGeraMesmoPlano()
    {
        var a = _generator.Generate(2000, seed: 42);
        var b = _generator.Generate(2000, seed: 42);

        Assert.Equal(
            a.Days.SelectMany(d => d.Meals).Select(m => m.Key),
            b.Days.SelectMany(d => d.Meals).Select(m => m.Key));
    }

    [Fact]
    public void Generate_SementesDiferentesGeramCardapiosDiferentes()
    {
        var a = _generator.Generate(2000, seed: 1);
        var b = _generator.Generate(2000, seed: 2);

        Assert.NotEqual(
            a.Days.SelectMany(d => d.Meals).Select(m => m.Key),
            b.Days.SelectMany(d => d.Meals).Select(m => m.Key));
    }

    [Fact]
    public void Generate_MetaInvalida_LancaExcecao()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _generator.Generate(0, seed: 1));
    }

    [Fact]
    public void Constructor_SemAlimentos_LancaExcecao()
    {
        Assert.Throws<ArgumentException>(() => new DietGenerator(Array.Empty<Food>()));
    }
}
