using System.Net;
using Microsoft.Data.Sqlite;
using NutriPlan.Core.Models;
using NutriPlan.Web.Data;
using NutriPlan.Web.Services;

namespace NutriPlan.Tests;

public class DatabaseFixture : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"nutriplan-{Guid.NewGuid():N}.db");

    public DatabaseFixture()
    {
        Database = new NutriPlanDatabase($"Data Source={_path}");
        Database.Initialize();
        Profiles = new ProfileRepository(Database);
        Foods = new FoodRepository(Database);
    }

    public NutriPlanDatabase Database { get; }
    public ProfileRepository Profiles { get; }
    public FoodRepository Foods { get; }

    public int AddProfile(Goal goal = Goal.Lose, double weight = 100, double weeklyGoal = 1.0) =>
        Profiles.Add(new UserProfile
        {
            Name = "Teste",
            Sex = Sex.Male,
            Age = 30,
            WeightKg = weight,
            HeightCm = 175,
            Activity = ActivityLevel.Light,
            Goal = goal,
            WeeklyGoalKg = weeklyGoal,
            PlanSeed = 1
        });

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        File.Delete(_path);
    }
}

public class FoodRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _db;

    public FoodRepositoryTests(DatabaseFixture db) => _db = db;

    [Fact]
    public void GetAll_RetornaOsAlimentosDaBaseInicial()
    {
        Assert.Equal(FoodCatalog.Seed.Count, _db.Foods.GetAll().Count);
    }

    [Fact]
    public void Search_BuscaPorParteDoNomeSemDiferenciarMaiusculas()
    {
        Assert.Contains(_db.Foods.Search("ARROZ"), f => f.Name == "Arroz branco cozido");
    }

    [Fact]
    public void Search_CaracteresEspeciaisNaoQuebramAConsulta()
    {
        Assert.Empty(_db.Foods.Search("100%'; DROP TABLE Foods;--"));
        Assert.NotEmpty(_db.Foods.GetAll());
    }

    [Fact]
    public void Initialize_ChamadoDuasVezes_NaoDuplicaAlimentos()
    {
        _db.Database.Initialize();

        Assert.Equal(FoodCatalog.Seed.Count, _db.Foods.GetAll().Count);
    }
}

public class WeeklyProgressServiceTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _db;
    private readonly WeeklyProgressService _service;

    public WeeklyProgressServiceTests(DatabaseFixture db)
    {
        _db = db;
        _service = new WeeklyProgressService(db.Profiles);
    }

    [Fact]
    public void RegisterWeight_MetaCumprida_SalvaPesoConquistaEAtualizaPerfil()
    {
        var id = _db.AddProfile(weight: 100, weeklyGoal: 1.0);

        var result = _service.RegisterWeight(id, 98.5);

        Assert.True(result.Evaluation.GoalMet);
        Assert.Equal(1, result.Week);
        Assert.Equal(98.5, _db.Profiles.Get(id)!.WeightKg);
        Assert.Contains(_db.Profiles.GetAchievements(id), a => a.Code == "first-goal");
        Assert.Single(_db.Profiles.GetWeights(id));
    }

    [Fact]
    public void RegisterWeight_MetaNaoCumprida_NaoGeraConquista()
    {
        var id = _db.AddProfile(weight: 100, weeklyGoal: 1.0);

        var result = _service.RegisterWeight(id, 99.8);

        Assert.False(result.Evaluation.GoalMet);
        Assert.Empty(_db.Profiles.GetAchievements(id));
    }

    [Fact]
    public void RegisterWeight_DuasFalhas_AumentaOAjusteDoPlano()
    {
        var id = _db.AddProfile(weight: 100, weeklyGoal: 1.0);

        _service.RegisterWeight(id, 100);
        _service.RegisterWeight(id, 100);

        Assert.Equal(100, _db.Profiles.Get(id)!.ExtraAdjustmentKcal);
    }

    [Fact]
    public void RegisterWeight_PesoInvalido_LancaArgumentException()
    {
        var id = _db.AddProfile();

        Assert.Throws<ArgumentException>(() => _service.RegisterWeight(id, -10));
    }

    [Fact]
    public void RegisterWeight_PerfilInexistente_LancaKeyNotFound()
    {
        Assert.Throws<KeyNotFoundException>(() => _service.RegisterWeight(9999, 80));
    }
}

public class OpenFoodFactsClientTests
{
    private sealed class FakeHandler : HttpMessageHandler
    {
        private readonly Func<HttpResponseMessage> _respond;

        public FakeHandler(Func<HttpResponseMessage> respond) => _respond = respond;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct) =>
            Task.FromResult(_respond());
    }

    private static OpenFoodFactsClient ClientReturning(Func<HttpResponseMessage> respond) =>
        new(new HttpClient(new FakeHandler(respond)) { BaseAddress = new Uri("https://example.test/") });

    [Fact]
    public void Parse_IgnoraProdutosSemNomeOuSemCalorias()
    {
        const string json = @"{""products"":[
            {""product_name"":""Biscoito"",""nutriments"":{""energy-kcal_100g"":450}},
            {""product_name"":"""",""nutriments"":{""energy-kcal_100g"":10}},
            {""product_name"":""Sem calorias"",""nutriments"":{}},
            {""product_name"":""Suco"",""nutriments"":{""energy-kcal_100g"":""42.5""}}]}";

        var foods = OpenFoodFactsClient.Parse(json);

        Assert.Equal(2, foods.Count);
        Assert.Equal(450, foods[0].KcalPer100g);
        Assert.Equal(42.5, foods[1].KcalPer100g);
    }

    [Fact]
    public async Task SearchAsync_RespostaValida_RetornaAlimentos()
    {
        var client = ClientReturning(() => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(@"{""products"":[{""product_name"":""Pão"",""nutriments"":{""energy-kcal_100g"":265}}]}")
        });

        var result = await client.SearchAsync("pão");

        Assert.True(result.Succeeded);
        Assert.Single(result.Foods);
    }

    [Fact]
    public async Task SearchAsync_FalhaDeRede_RetornaErroSemLancarExcecao()
    {
        var client = ClientReturning(() => throw new HttpRequestException("sem rede"));

        var result = await client.SearchAsync("arroz");

        Assert.False(result.Succeeded);
        Assert.Empty(result.Foods);
    }

    [Fact]
    public async Task SearchAsync_StatusDeErro_RetornaErro()
    {
        var client = ClientReturning(() => new HttpResponseMessage(HttpStatusCode.InternalServerError));

        Assert.False((await client.SearchAsync("arroz")).Succeeded);
    }

    [Fact]
    public async Task SearchAsync_JsonInvalido_RetornaErro()
    {
        var client = ClientReturning(() => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("<html>não é json</html>")
        });

        Assert.False((await client.SearchAsync("arroz")).Succeeded);
    }

    [Fact]
    public async Task SearchAsync_TermoVazio_NaoChamaAApi()
    {
        var client = ClientReturning(() => throw new InvalidOperationException("não deveria chamar"));

        var result = await client.SearchAsync("  ");

        Assert.True(result.Succeeded);
        Assert.Empty(result.Foods);
    }
}
