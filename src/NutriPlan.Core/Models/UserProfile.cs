namespace NutriPlan.Core.Models;

public class UserProfile
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public Sex Sex { get; set; }
    public int Age { get; set; }
    public double WeightKg { get; set; }
    public double HeightCm { get; set; }
    public ActivityLevel Activity { get; set; }
    public Goal Goal { get; set; }
    public double WeeklyGoalKg { get; set; }
    public int ExtraAdjustmentKcal { get; set; }
    public int PlanSeed { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class WeightEntry
{
    public int Id { get; set; }
    public int ProfileId { get; set; }
    public int Week { get; set; }
    public double WeightKg { get; set; }
    public bool GoalMet { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
}

public class Achievement
{
    public int Id { get; set; }
    public int ProfileId { get; set; }
    public string Code { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public DateTime EarnedAt { get; set; } = DateTime.UtcNow;
}
