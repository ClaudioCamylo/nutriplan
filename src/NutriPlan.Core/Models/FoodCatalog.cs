namespace NutriPlan.Core.Models;

/// <summary>Alimentos iniciais, com valores aproximados da Tabela TACO (NEPA/Unicamp).</summary>
public static class FoodCatalog
{
    private static readonly MealType[] Breakfast = { MealType.Breakfast };
    private static readonly MealType[] Main = { MealType.Lunch, MealType.Dinner };
    private static readonly MealType[] Morning = { MealType.Breakfast, MealType.Snack };
    private static readonly MealType[] Light = { MealType.Snack };
    private static readonly MealType[] Anytime =
        { MealType.Breakfast, MealType.Lunch, MealType.Snack, MealType.Dinner };

    public static IReadOnlyList<Food> Seed { get; } = new List<Food>
    {
        new(1, "Pão francês", FoodRole.Carb, 300, 50, Morning),
        new(2, "Pão integral", FoodRole.Carb, 253, 50, Morning),
        new(3, "Tapioca", FoodRole.Carb, 240, 50, Morning),
        new(4, "Aveia em flocos", FoodRole.Carb, 394, 30, Morning),
        new(5, "Arroz branco cozido", FoodRole.Carb, 128, 150, Main),
        new(6, "Arroz integral cozido", FoodRole.Carb, 124, 150, Main),
        new(7, "Macarrão cozido", FoodRole.Carb, 122, 140, Main),
        new(8, "Batata inglesa cozida", FoodRole.Carb, 52, 150, Main),
        new(9, "Mandioca cozida", FoodRole.Carb, 125, 100, Main),
        new(10, "Ovo cozido", FoodRole.Protein, 146, 50, Anytime),
        new(11, "Queijo minas frescal", FoodRole.Protein, 264, 30, Morning),
        new(12, "Presunto", FoodRole.Protein, 94, 30, Morning),
        new(13, "Peito de frango grelhado", FoodRole.Protein, 159, 100, Main),
        new(14, "Carne bovina (patinho) grelhada", FoodRole.Protein, 219, 100, Main),
        new(15, "Tilápia grelhada", FoodRole.Protein, 128, 100, Main),
        new(16, "Carne moída refogada", FoodRole.Protein, 212, 100, Main),
        new(17, "Feijão carioca cozido", FoodRole.Protein, 76, 100, Main),
        new(18, "Leite integral", FoodRole.Dairy, 61, 200, Morning),
        new(19, "Iogurte natural", FoodRole.Dairy, 51, 170, Morning),
        new(20, "Queijo muçarela", FoodRole.Dairy, 330, 30, Morning),
        new(21, "Banana prata", FoodRole.Fruit, 98, 100, Morning),
        new(22, "Maçã", FoodRole.Fruit, 56, 130, Morning),
        new(23, "Laranja", FoodRole.Fruit, 37, 180, Morning),
        new(24, "Mamão papaia", FoodRole.Fruit, 40, 150, Morning),
        new(25, "Morango", FoodRole.Fruit, 30, 100, Light),
        new(26, "Alface", FoodRole.Vegetable, 11, 40, Main),
        new(27, "Tomate", FoodRole.Vegetable, 15, 80, Main),
        new(28, "Brócolis cozido", FoodRole.Vegetable, 25, 80, Main),
        new(29, "Cenoura cozida", FoodRole.Vegetable, 30, 80, Main),
        new(30, "Couve refogada", FoodRole.Vegetable, 90, 50, Main),
        new(31, "Abobrinha cozida", FoodRole.Vegetable, 19, 100, Main),
        new(32, "Azeite de oliva", FoodRole.Fat, 884, 10, Main),
        new(33, "Abacate", FoodRole.Fat, 96, 100, Main),
        new(34, "Castanha-do-pará", FoodRole.Fat, 643, 20, Light),
        new(35, "Amendoim torrado", FoodRole.Fat, 544, 30, Light),
        new(36, "Pasta de amendoim", FoodRole.Fat, 588, 20, Morning),
        new(37, "Manteiga", FoodRole.Fat, 726, 10, Breakfast)
    };
}
