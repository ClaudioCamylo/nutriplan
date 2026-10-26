using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NutriPlan.Core.Models;
using NutriPlan.Core.Validation;
using NutriPlan.Web.Data;

namespace NutriPlan.Web.Pages;

public class IndexModel : PageModel
{
    private readonly ProfileRepository _profiles;

    public IndexModel(ProfileRepository profiles)
    {
        _profiles = profiles;
    }

    [BindProperty]
    public ProfileInput Input { get; set; } = new();

    public IReadOnlyList<string> Errors { get; private set; } = Array.Empty<string>();

    [TempData]
    public string? Notice { get; set; }

    public void OnGet(bool example = false)
    {
        if (example)
        {
            Input = new ProfileInput
            {
                Name = "Luiz Felipe",
                Sex = Sex.Male,
                Age = 19,
                WeightKg = 112,
                HeightCm = 170,
                Activity = ActivityLevel.Sedentary,
                Goal = Goal.Lose,
                WeeklyGoalKg = 0.5
            };
        }
    }

    public IActionResult OnPost()
    {
        if (!Input.IsComplete)
        {
            Errors = new[] { "Preencha todos os campos com valores numéricos válidos." };
            return Page();
        }

        var profile = Input.ToProfile();
        var errors = ProfileValidator.Validate(profile);
        if (errors.Count > 0)
        {
            Errors = errors;
            return Page();
        }

        try
        {
            profile.PlanSeed = Random.Shared.Next();
            var id = _profiles.Add(profile);
            return RedirectToPage("/Plan", new { id });
        }
        catch (DataAccessException ex)
        {
            Errors = new[] { ex.Message };
            return Page();
        }
    }

    public class ProfileInput
    {
        public string? Name { get; set; }
        public Sex Sex { get; set; }
        public int? Age { get; set; }
        public double? WeightKg { get; set; }
        public double? HeightCm { get; set; }
        public ActivityLevel Activity { get; set; }
        public Goal Goal { get; set; }
        public double? WeeklyGoalKg { get; set; } = 0.5;

        public bool IsComplete => Age.HasValue && WeightKg.HasValue && HeightCm.HasValue && WeeklyGoalKg.HasValue;

        public UserProfile ToProfile() => new()
        {
            Name = Name?.Trim() ?? "",
            Sex = Sex,
            Age = Age!.Value,
            WeightKg = WeightKg!.Value,
            HeightCm = HeightCm!.Value,
            Activity = Activity,
            Goal = Goal,
            WeeklyGoalKg = WeeklyGoalKg!.Value
        };
    }
}
