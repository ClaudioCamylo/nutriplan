namespace NutriPlan.Web.Models;

public class TeamMember
{
    public string Name { get; set; } = "";
    public string Ra { get; set; } = "";
}

public class TeamInfo
{
    public string Course { get; set; } = "";
    public string Professor { get; set; } = "";
    public List<TeamMember> Members { get; set; } = new();
}
