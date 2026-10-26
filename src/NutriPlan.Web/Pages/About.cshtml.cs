using Microsoft.AspNetCore.Mvc.RazorPages;
using NutriPlan.Web.Models;

namespace NutriPlan.Web.Pages;

public class AboutModel : PageModel
{
    public AboutModel(TeamInfo team)
    {
        Team = team;
    }

    public TeamInfo Team { get; }
}
