using NutriPlan.Core.Calculations;

namespace NutriPlan.Tests;

public class CombinatoricsTests
{
    [Theory]
    [InlineData(10, 3, 120)]
    [InlineData(5, 0, 1)]
    [InlineData(5, 5, 1)]
    [InlineData(6, 2, 15)]
    [InlineData(20, 6, 38760)]
    public void Count_CalculaCombinacaoSimples(int n, int k, long esperado)
    {
        Assert.Equal(esperado, Combinatorics.Count(n, k));
    }

    [Fact]
    public void Count_KMaiorQueN_LancaExcecao()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Combinatorics.Count(3, 4));
    }

    [Fact]
    public void Count_ValorNegativo_LancaExcecao()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Combinatorics.Count(-1, 0));
    }

    [Fact]
    public void Enumerate_GeraExatamenteCNKCombinacoes()
    {
        var combinacoes = Combinatorics.Enumerate(6, 3).ToList();

        Assert.Equal(20, combinacoes.Count);
        Assert.Equal(20, combinacoes.Select(c => string.Join(",", c)).Distinct().Count());
    }

    [Fact]
    public void Enumerate_IndicesSaoCrescentesESemRepeticao()
    {
        foreach (var combinacao in Combinatorics.Enumerate(5, 3))
        {
            Assert.Equal(combinacao.OrderBy(i => i), combinacao);
            Assert.Equal(combinacao.Length, combinacao.Distinct().Count());
        }
    }

    [Fact]
    public void EnumerateRange_SomaAsCombinacoesDeCadaTamanho()
    {
        var total = Combinatorics.EnumerateRange(10, 2, 4).Count();

        Assert.Equal(45 + 120 + 210, total);
    }
}
