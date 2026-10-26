using System.Globalization;
using NutriPlan.Core.Diet;
using NutriPlan.Core.Foods;
using NutriPlan.Web.Data;
using NutriPlan.Web.Models;
using NutriPlan.Web.Services;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

var builder = WebApplication.CreateBuilder(args);

var dataDirectory = Path.Combine(builder.Environment.ContentRootPath, "App_Data");
Directory.CreateDirectory(dataDirectory);
var database = new NutriPlanDatabase($"Data Source={Path.Combine(dataDirectory, "nutriplan.db")}");
database.Initialize();

builder.Services.AddRazorPages();
builder.Services.AddSingleton(database);
builder.Services.AddSingleton<FoodRepository>();
builder.Services.AddSingleton<ProfileRepository>();
builder.Services.AddSingleton<WeeklyProgressService>();
builder.Services.AddSingleton(sp => new DietGenerator(sp.GetRequiredService<FoodRepository>().GetAll()));
builder.Services.AddSingleton(builder.Configuration.GetSection("Team").Get<TeamInfo>() ?? new TeamInfo());
builder.Services.AddHttpClient<IFoodApi, OpenFoodFactsClient>(client =>
{
    client.BaseAddress = new Uri("https://world.openfoodfacts.org/");
    client.Timeout = TimeSpan.FromSeconds(8);
    client.DefaultRequestHeaders.UserAgent.ParseAdd("NutriPlan/1.0 (trabalho academico GQS)");
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();

app.Run();

public partial class Program
{
}
