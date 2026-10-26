using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NutriPlan.Core.Calculations;
using NutriPlan.Core.Diet;
using NutriPlan.Core.Models;
using NutriPlan.Web.Data;
using NutriPlan.Web.Services;

namespace NutriPlan.Web.Pages;

public class PlanModel : PageModel
{
    private readonly ProfileRepository _profiles;
    private readonly DietGenerator _diet;
    private readonly WeeklyProgressService _progress;

    public PlanModel(ProfileRepository profiles, DietGenerator diet, WeeklyProgressService progress)
    {
        _profiles = profiles;
        _diet = diet;
        _progress = progress;
    }

    public UserProfile Profile { get; private set; } = new();
    public double Imc { get; private set; }
    public string ImcClass { get; private set; } = "";
    public double Tmb { get; private set; }
    public double Energy { get; private set; }
    public double Target { get; private set; }
    public bool FloorApplied { get; private set; }
    public WeeklyPlan Plan { get; private set; } = new(0, Array.Empty<DayPlan>());
    public IReadOnlyList<WeightEntry> Weights { get; private set; } = Array.Empty<WeightEntry>();
    public IReadOnlyList<Achievement> Achievements { get; private set; } = Array.Empty<Achievement>();
    public int NextWeek => Weights.Count + 1;

    [TempData]
    public string? Message { get; set; }

    [TempData]
    public string? MessageKind { get; set; }

    public IActionResult OnGet(int id) => Load(id);

    public IActionResult OnPostWeight(int id, double? weight)
    {
        if (weight is null)
        {
            return Fail(id, "Informe o peso da semana.");
        }

        try
        {
            var result = _progress.RegisterWeight(id, weight.Value);
            var evaluation = result.Evaluation;
            Message = evaluation.GoalMet
                ? $"Semana {result.Week} cumprida! Variação de {UiText.Number(evaluation.DeltaKg, 1)} kg. Seu plano foi recalculado."
                : $"Semana {result.Week} registrada. Variação de {UiText.Number(evaluation.DeltaKg, 1)} kg: a meta ainda não foi atingida, continue firme.";
            MessageKind = evaluation.GoalMet ? "success" : "info";
        }
        catch (ArgumentException ex)
        {
            return Fail(id, ex.Message);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (DataAccessException ex)
        {
            return Fail(id, ex.Message);
        }

        return RedirectToPage(new { id });
    }

    public IActionResult OnPostRegenerate(int id)
    {
        try
        {
            var profile = _profiles.Get(id);
            if (profile is null)
            {
                return NotFound();
            }

            profile.PlanSeed = Random.Shared.Next();
            _profiles.Update(profile);
            Message = "Novo cardápio gerado.";
            MessageKind = "success";
            return RedirectToPage(new { id });
        }
        catch (DataAccessException ex)
        {
            return Fail(id, ex.Message);
        }
    }

    private IActionResult Fail(int id, string message)
    {
        Message = message;
        MessageKind = "error";
        return RedirectToPage(new { id });
    }

    private IActionResult Load(int id)
    {
        try
        {
            var profile = _profiles.Get(id);
            if (profile is null)
            {
                return NotFound();
            }

            Profile = profile;
            Imc = BodyCalculator.Imc(profile.WeightKg, profile.HeightCm);
            ImcClass = BodyCalculator.ClassifyImc(Imc);
            Tmb = BodyCalculator.Tmb(profile.Sex, profile.WeightKg, profile.HeightCm, profile.Age);
            Energy = BodyCalculator.DailyEnergy(Tmb, profile.Activity);
            Target = BodyCalculator.CalorieTarget(profile);
            FloorApplied = Target == BodyCalculator.MinimumKcal(profile.Sex)
                           && Energy + GoalOffset(profile) < Target;
            Plan = _diet.Generate(Target, profile.PlanSeed);
            Weights = _profiles.GetWeights(id);
            Achievements = _profiles.GetAchievements(id);
            return Page();
        }
        catch (DataAccessException ex)
        {
            TempData["Notice"] = ex.Message;
            return RedirectToPage("/Index");
        }
    }

    private static double GoalOffset(UserProfile profile) => profile.Goal switch
    {
        Goal.Lose => -(BodyCalculator.DailyAdjustmentKcal + profile.ExtraAdjustmentKcal),
        Goal.Gain => BodyCalculator.DailyAdjustmentKcal + profile.ExtraAdjustmentKcal,
        _ => 0
    };
}
