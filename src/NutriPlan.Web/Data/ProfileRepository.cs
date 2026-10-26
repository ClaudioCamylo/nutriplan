using System.Globalization;
using Microsoft.Data.Sqlite;
using NutriPlan.Core.Models;

namespace NutriPlan.Web.Data;

public class ProfileRepository
{
    private readonly NutriPlanDatabase _database;

    public ProfileRepository(NutriPlanDatabase database)
    {
        _database = database;
    }

    public int Add(UserProfile profile) => Execute("salvar o perfil", connection =>
    {
        using var command = connection.CreateCommand();
        command.CommandText = @"INSERT INTO Profiles
            (Name, Sex, Age, WeightKg, HeightCm, Activity, Goal, WeeklyGoalKg, ExtraAdjustmentKcal, PlanSeed, CreatedAt)
            VALUES ($name, $sex, $age, $weight, $height, $activity, $goal, $weekly, $extra, $seed, $created);
            SELECT last_insert_rowid();";
        BindProfile(command, profile);
        command.Parameters.AddWithValue("$created", FormatDate(profile.CreatedAt));
        return (int)(long)command.ExecuteScalar()!;
    });

    public UserProfile? Get(int id) => Execute("buscar o perfil", connection =>
    {
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Profiles WHERE Id = $id";
        command.Parameters.AddWithValue("$id", id);
        using var reader = command.ExecuteReader();
        return reader.Read() ? ReadProfile(reader) : null;
    });

    public void Update(UserProfile profile) => Execute("atualizar o perfil", connection =>
    {
        using var command = connection.CreateCommand();
        command.CommandText = @"UPDATE Profiles SET Name = $name, Sex = $sex, Age = $age, WeightKg = $weight,
            HeightCm = $height, Activity = $activity, Goal = $goal, WeeklyGoalKg = $weekly,
            ExtraAdjustmentKcal = $extra, PlanSeed = $seed WHERE Id = $id";
        BindProfile(command, profile);
        command.Parameters.AddWithValue("$id", profile.Id);
        command.ExecuteNonQuery();
        return 0;
    });

    public IReadOnlyList<WeightEntry> GetWeights(int profileId) => Execute("buscar os pesos", connection =>
    {
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM WeightEntries WHERE ProfileId = $id ORDER BY Week";
        command.Parameters.AddWithValue("$id", profileId);
        using var reader = command.ExecuteReader();
        var entries = new List<WeightEntry>();
        while (reader.Read())
        {
            entries.Add(new WeightEntry
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                ProfileId = reader.GetInt32(reader.GetOrdinal("ProfileId")),
                Week = reader.GetInt32(reader.GetOrdinal("Week")),
                WeightKg = reader.GetDouble(reader.GetOrdinal("WeightKg")),
                GoalMet = reader.GetInt32(reader.GetOrdinal("GoalMet")) == 1,
                RecordedAt = ParseDate(reader.GetString(reader.GetOrdinal("RecordedAt")))
            });
        }

        return entries;
    });

    public void AddWeight(WeightEntry entry) => Execute("registrar o peso", connection =>
    {
        using var command = connection.CreateCommand();
        command.CommandText = @"INSERT INTO WeightEntries (ProfileId, Week, WeightKg, GoalMet, RecordedAt)
            VALUES ($profile, $week, $weight, $met, $recorded)";
        command.Parameters.AddWithValue("$profile", entry.ProfileId);
        command.Parameters.AddWithValue("$week", entry.Week);
        command.Parameters.AddWithValue("$weight", entry.WeightKg);
        command.Parameters.AddWithValue("$met", entry.GoalMet ? 1 : 0);
        command.Parameters.AddWithValue("$recorded", FormatDate(entry.RecordedAt));
        command.ExecuteNonQuery();
        return 0;
    });

    public IReadOnlyList<Achievement> GetAchievements(int profileId) => Execute("buscar as conquistas", connection =>
    {
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Achievements WHERE ProfileId = $id ORDER BY EarnedAt, Id";
        command.Parameters.AddWithValue("$id", profileId);
        using var reader = command.ExecuteReader();
        var list = new List<Achievement>();
        while (reader.Read())
        {
            list.Add(new Achievement
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                ProfileId = reader.GetInt32(reader.GetOrdinal("ProfileId")),
                Code = reader.GetString(reader.GetOrdinal("Code")),
                Title = reader.GetString(reader.GetOrdinal("Title")),
                Description = reader.GetString(reader.GetOrdinal("Description")),
                EarnedAt = ParseDate(reader.GetString(reader.GetOrdinal("EarnedAt")))
            });
        }

        return list;
    });

    public void AddAchievement(Achievement achievement) => Execute("salvar a conquista", connection =>
    {
        using var command = connection.CreateCommand();
        command.CommandText = @"INSERT OR IGNORE INTO Achievements (ProfileId, Code, Title, Description, EarnedAt)
            VALUES ($profile, $code, $title, $description, $earned)";
        command.Parameters.AddWithValue("$profile", achievement.ProfileId);
        command.Parameters.AddWithValue("$code", achievement.Code);
        command.Parameters.AddWithValue("$title", achievement.Title);
        command.Parameters.AddWithValue("$description", achievement.Description);
        command.Parameters.AddWithValue("$earned", FormatDate(achievement.EarnedAt));
        command.ExecuteNonQuery();
        return 0;
    });

    private T Execute<T>(string action, Func<SqliteConnection, T> work)
    {
        try
        {
            using var connection = _database.CreateConnection();
            return work(connection);
        }
        catch (SqliteException ex)
        {
            throw new DataAccessException($"Não foi possível {action}.", ex);
        }
    }

    private static void BindProfile(SqliteCommand command, UserProfile profile)
    {
        command.Parameters.AddWithValue("$name", profile.Name);
        command.Parameters.AddWithValue("$sex", (int)profile.Sex);
        command.Parameters.AddWithValue("$age", profile.Age);
        command.Parameters.AddWithValue("$weight", profile.WeightKg);
        command.Parameters.AddWithValue("$height", profile.HeightCm);
        command.Parameters.AddWithValue("$activity", (int)profile.Activity);
        command.Parameters.AddWithValue("$goal", (int)profile.Goal);
        command.Parameters.AddWithValue("$weekly", profile.WeeklyGoalKg);
        command.Parameters.AddWithValue("$extra", profile.ExtraAdjustmentKcal);
        command.Parameters.AddWithValue("$seed", profile.PlanSeed);
    }

    private static UserProfile ReadProfile(SqliteDataReader reader) => new()
    {
        Id = reader.GetInt32(reader.GetOrdinal("Id")),
        Name = reader.GetString(reader.GetOrdinal("Name")),
        Sex = (Sex)reader.GetInt32(reader.GetOrdinal("Sex")),
        Age = reader.GetInt32(reader.GetOrdinal("Age")),
        WeightKg = reader.GetDouble(reader.GetOrdinal("WeightKg")),
        HeightCm = reader.GetDouble(reader.GetOrdinal("HeightCm")),
        Activity = (ActivityLevel)reader.GetInt32(reader.GetOrdinal("Activity")),
        Goal = (Goal)reader.GetInt32(reader.GetOrdinal("Goal")),
        WeeklyGoalKg = reader.GetDouble(reader.GetOrdinal("WeeklyGoalKg")),
        ExtraAdjustmentKcal = reader.GetInt32(reader.GetOrdinal("ExtraAdjustmentKcal")),
        PlanSeed = reader.GetInt32(reader.GetOrdinal("PlanSeed")),
        CreatedAt = ParseDate(reader.GetString(reader.GetOrdinal("CreatedAt")))
    };

    private static string FormatDate(DateTime date) => date.ToString("o", CultureInfo.InvariantCulture);

    private static DateTime ParseDate(string text) =>
        DateTime.Parse(text, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
}
