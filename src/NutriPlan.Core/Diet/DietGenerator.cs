using NutriPlan.Core.Calculations;
using NutriPlan.Core.Models;

namespace NutriPlan.Core.Diet;

public class DietGenerator
{
    private const int DaysInWeek = 7;
    private const int MinFoodsPerMeal = 2;
    private const int MaxFoodsPerMeal = 6;
    private const int MinCandidatesForVariety = 20;
    private const double GramsStep = 5;
    private static readonly double[] Tolerances = { 0.15, 0.30, 0.50 };

    private static readonly IReadOnlyDictionary<MealType, double> MealShares = new Dictionary<MealType, double>
    {
        [MealType.Breakfast] = 0.25,
        [MealType.Lunch] = 0.35,
        [MealType.Snack] = 0.15,
        [MealType.Dinner] = 0.25
    };

    private readonly IReadOnlyList<Food> _foods;
    private readonly Dictionary<(MealType, double), List<Food[]>> _candidateCache = new();

    public DietGenerator(IReadOnlyList<Food> foods)
    {
        if (foods.Count == 0)
        {
            throw new ArgumentException("A base de alimentos está vazia.", nameof(foods));
        }

        _foods = foods;
    }

    public WeeklyPlan Generate(double dailyTargetKcal, int seed)
    {
        if (dailyTargetKcal <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(dailyTargetKcal), "A meta calórica deve ser positiva.");
        }

        var random = new Random(seed);
        var used = Enum.GetValues<MealType>().ToDictionary(t => t, _ => new HashSet<string>());
        var previous = new Dictionary<MealType, string>();
        var days = new List<DayPlan>();

        for (var day = 1; day <= DaysInWeek; day++)
        {
            var meals = new List<Meal>();
            foreach (var (type, share) in MealShares)
            {
                var target = dailyTargetKcal * share;
                var combo = PickCombination(type, target, random, used[type], previous.GetValueOrDefault(type));
                var meal = BuildMeal(type, target, combo);

                used[type].Add(meal.Key);
                previous[type] = meal.Key;
                meals.Add(meal);
            }

            days.Add(new DayPlan(day, meals));
        }

        return new WeeklyPlan(dailyTargetKcal, days);
    }

    private Food[] PickCombination(
        MealType type, double target, Random random, HashSet<string> usedInWeek, string? previousKey)
    {
        var candidates = GetCandidates(type, target);
        if (candidates.Count == 0)
        {
            throw new InvalidOperationException($"Não há alimentos suficientes para montar a refeição {type}.");
        }

        var fresh = candidates.Where(c => !usedInWeek.Contains(KeyOf(c))).ToList();
        var notYesterday = candidates.Where(c => KeyOf(c) != previousKey).ToList();
        var pool = fresh.Count > 0 ? fresh : notYesterday.Count > 0 ? notYesterday : candidates;

        return pool[random.Next(pool.Count)];
    }

    private List<Food[]> GetCandidates(MealType type, double target)
    {
        var cacheKey = (type, Math.Round(target));
        if (_candidateCache.TryGetValue(cacheKey, out var cached))
        {
            return cached;
        }

        var pool = _foods.Where(f => f.Meals.Contains(type)).ToArray();
        var all = Combinatorics.EnumerateRange(pool.Length, MinFoodsPerMeal, MaxFoodsPerMeal)
            .Select(indices => indices.Select(i => pool[i]).ToArray())
            .Where(combo => MealRules.IsBalanced(type, combo))
            .ToList();

        var candidates = new List<Food[]>();
        foreach (var tolerance in Tolerances)
        {
            candidates = all.Where(c => Math.Abs(PortionKcal(c) - target) <= target * tolerance).ToList();
            if (candidates.Count >= MinCandidatesForVariety)
            {
                break;
            }
        }

        _candidateCache[cacheKey] = candidates;
        return candidates;
    }

    private static Meal BuildMeal(MealType type, double target, Food[] combo)
    {
        var factor = target / PortionKcal(combo);
        var items = combo
            .Select(f => new FoodPortion(f, Math.Max(GramsStep, Math.Round(f.PortionGrams * factor / GramsStep) * GramsStep)))
            .ToList();

        return new Meal(type, target, items);
    }

    private static double PortionKcal(IEnumerable<Food> combo) => combo.Sum(f => f.KcalPerPortion);

    private static string KeyOf(IEnumerable<Food> combo) =>
        string.Join("-", combo.Select(f => f.Id).OrderBy(id => id));
}
