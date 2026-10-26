using NutriPlan.Core.Achievements;
using NutriPlan.Core.Models;
using NutriPlan.Core.Validation;
using NutriPlan.Web.Data;

namespace NutriPlan.Web.Services;

public record WeightRegistration(WeekEvaluation Evaluation, int Week);

public class WeeklyProgressService
{
    private readonly ProfileRepository _profiles;

    public WeeklyProgressService(ProfileRepository profiles)
    {
        _profiles = profiles;
    }

    public WeightRegistration RegisterWeight(int profileId, double newWeightKg)
    {
        var weightError = ProfileValidator.ValidateWeight(newWeightKg);
        if (weightError is not null)
        {
            throw new ArgumentException(weightError, nameof(newWeightKg));
        }

        var profile = _profiles.Get(profileId)
            ?? throw new KeyNotFoundException("Perfil não encontrado.");

        var weights = _profiles.GetWeights(profileId);
        var week = weights.Count + 1;
        var history = weights.Select(w => w.GoalMet).ToList();

        var evaluation = ProgressEvaluator.Evaluate(
            profile.Goal, profile.WeeklyGoalKg, profile.WeightKg, newWeightKg,
            week, history, profile.ExtraAdjustmentKcal);

        _profiles.AddWeight(new WeightEntry
        {
            ProfileId = profileId,
            Week = week,
            WeightKg = newWeightKg,
            GoalMet = evaluation.GoalMet
        });

        profile.WeightKg = newWeightKg;
        profile.ExtraAdjustmentKcal = evaluation.ExtraAdjustmentKcal;
        if (evaluation.GoalMet)
        {
            profile.PlanSeed = Random.Shared.Next();
        }

        _profiles.Update(profile);

        foreach (var badge in evaluation.NewBadges)
        {
            _profiles.AddAchievement(new Achievement
            {
                ProfileId = profileId,
                Code = badge.Code,
                Title = badge.Title,
                Description = badge.Description
            });
        }

        return new WeightRegistration(evaluation, week);
    }
}
