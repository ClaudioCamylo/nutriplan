namespace NutriPlan.Core.Calculations;

public static class Combinatorics
{
    /// <summary>C(n, k) = n! / (k! × (n − k)!), calculado sem fatoriais grandes.</summary>
    public static long Count(int n, int k)
    {
        if (n < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(n), "n não pode ser negativo.");
        }

        if (k < 0 || k > n)
        {
            throw new ArgumentOutOfRangeException(nameof(k), "k deve estar entre 0 e n.");
        }

        k = Math.Min(k, n - k);
        long result = 1;
        for (var i = 1; i <= k; i++)
        {
            result = result * (n - k + i) / i;
        }

        return result;
    }

    public static IEnumerable<int[]> Enumerate(int n, int k)
    {
        if (k < 0 || k > n)
        {
            yield break;
        }

        var indices = Enumerable.Range(0, k).ToArray();
        while (true)
        {
            yield return (int[])indices.Clone();

            var pos = k - 1;
            while (pos >= 0 && indices[pos] == n - k + pos)
            {
                pos--;
            }

            if (pos < 0)
            {
                yield break;
            }

            indices[pos]++;
            for (var j = pos + 1; j < k; j++)
            {
                indices[j] = indices[j - 1] + 1;
            }
        }
    }

    public static IEnumerable<int[]> EnumerateRange(int n, int minK, int maxK)
    {
        for (var k = minK; k <= Math.Min(maxK, n); k++)
        {
            foreach (var combination in Enumerate(n, k))
            {
                yield return combination;
            }
        }
    }
}
