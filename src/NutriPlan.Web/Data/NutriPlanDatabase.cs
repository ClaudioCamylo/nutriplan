using Microsoft.Data.Sqlite;
using NutriPlan.Core.Models;

namespace NutriPlan.Web.Data;

public class DataAccessException : Exception
{
    public DataAccessException(string message, Exception inner) : base(message, inner)
    {
    }
}

public class NutriPlanDatabase
{
    private readonly string _connectionString;

    public NutriPlanDatabase(string connectionString)
    {
        _connectionString = connectionString;
    }

    public SqliteConnection CreateConnection()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        return connection;
    }

    public void Initialize()
    {
        try
        {
            using var connection = CreateConnection();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = Schema;
                command.ExecuteNonQuery();
            }

            SeedFoods(connection);
        }
        catch (SqliteException ex)
        {
            throw new DataAccessException("Não foi possível preparar o banco de dados.", ex);
        }
    }

    private static void SeedFoods(SqliteConnection connection)
    {
        using var count = connection.CreateCommand();
        count.CommandText = "SELECT COUNT(*) FROM Foods";
        if ((long)count.ExecuteScalar()! > 0)
        {
            return;
        }

        using var transaction = connection.BeginTransaction();
        foreach (var food in FoodCatalog.Seed)
        {
            using var insert = connection.CreateCommand();
            insert.Transaction = transaction;
            insert.CommandText = @"INSERT INTO Foods (Id, Name, Role, KcalPer100g, PortionGrams, Meals)
                                   VALUES ($id, $name, $role, $kcal, $portion, $meals)";
            insert.Parameters.AddWithValue("$id", food.Id);
            insert.Parameters.AddWithValue("$name", food.Name);
            insert.Parameters.AddWithValue("$role", (int)food.Role);
            insert.Parameters.AddWithValue("$kcal", food.KcalPer100g);
            insert.Parameters.AddWithValue("$portion", food.PortionGrams);
            insert.Parameters.AddWithValue("$meals", string.Join(",", food.Meals.Select(m => (int)m)));
            insert.ExecuteNonQuery();
        }

        transaction.Commit();
    }

    private const string Schema = @"
        CREATE TABLE IF NOT EXISTS Foods (
            Id INTEGER PRIMARY KEY,
            Name TEXT NOT NULL,
            Role INTEGER NOT NULL,
            KcalPer100g REAL NOT NULL,
            PortionGrams REAL NOT NULL,
            Meals TEXT NOT NULL
        );
        CREATE TABLE IF NOT EXISTS Profiles (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Name TEXT NOT NULL,
            Sex INTEGER NOT NULL,
            Age INTEGER NOT NULL,
            WeightKg REAL NOT NULL,
            HeightCm REAL NOT NULL,
            Activity INTEGER NOT NULL,
            Goal INTEGER NOT NULL,
            WeeklyGoalKg REAL NOT NULL,
            ExtraAdjustmentKcal INTEGER NOT NULL DEFAULT 0,
            PlanSeed INTEGER NOT NULL,
            CreatedAt TEXT NOT NULL
        );
        CREATE TABLE IF NOT EXISTS WeightEntries (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            ProfileId INTEGER NOT NULL REFERENCES Profiles(Id),
            Week INTEGER NOT NULL,
            WeightKg REAL NOT NULL,
            GoalMet INTEGER NOT NULL,
            RecordedAt TEXT NOT NULL
        );
        CREATE TABLE IF NOT EXISTS Achievements (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            ProfileId INTEGER NOT NULL REFERENCES Profiles(Id),
            Code TEXT NOT NULL,
            Title TEXT NOT NULL,
            Description TEXT NOT NULL,
            EarnedAt TEXT NOT NULL,
            UNIQUE (ProfileId, Code)
        );";
}
