using Microsoft.Data.Sqlite;
using NutriPlan.Core.Models;

namespace NutriPlan.Web.Data;

public class FoodRepository
{
    private readonly NutriPlanDatabase _database;

    public FoodRepository(NutriPlanDatabase database)
    {
        _database = database;
    }

    public IReadOnlyList<Food> GetAll() => Query("SELECT * FROM Foods ORDER BY Name", null);

    public IReadOnlyList<Food> Search(string term) =>
        Query("SELECT * FROM Foods WHERE Name LIKE $term ESCAPE '\\' ORDER BY Name", term);

    private IReadOnlyList<Food> Query(string sql, string? term)
    {
        try
        {
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            if (term is not null)
            {
                var escaped = term.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
                command.Parameters.AddWithValue("$term", $"%{escaped}%");
            }

            using var reader = command.ExecuteReader();
            var foods = new List<Food>();
            while (reader.Read())
            {
                foods.Add(new Food(
                    reader.GetInt32(reader.GetOrdinal("Id")),
                    reader.GetString(reader.GetOrdinal("Name")),
                    (FoodRole)reader.GetInt32(reader.GetOrdinal("Role")),
                    reader.GetDouble(reader.GetOrdinal("KcalPer100g")),
                    reader.GetDouble(reader.GetOrdinal("PortionGrams")),
                    reader.GetString(reader.GetOrdinal("Meals"))
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(m => (MealType)int.Parse(m))
                        .ToArray()));
            }

            return foods;
        }
        catch (SqliteException ex)
        {
            throw new DataAccessException("Não foi possível consultar os alimentos.", ex);
        }
    }
}
