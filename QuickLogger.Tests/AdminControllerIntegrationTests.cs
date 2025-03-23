using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using MongoDB.Driver.Core.Configuration;
using Newtonsoft.Json;
using System.Text;

namespace QuickLogger.Tests;

public class AdminControllerIntegrationTests:IClassFixture<WebApplicationFactory<QuickLogger.Program>>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;
    private string _connectionStringMsSql;
    private string _connectionStringMySql;
    private string _connectionStringMongoDB;

    public AdminControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();

        var config = new ConfigurationBuilder()
            .AddUserSecrets<AdminControllerIntegrationTests>()
            .Build();
        _connectionStringMsSql = config["ConnectionString:MsSql"]!;
        _connectionStringMySql = config["ConnectionString:MySql"]!;
        _connectionStringMongoDB = config["ConnectionString:MongoDB"]!;
    }

    [Fact]
    public async Task Add_InValidData_ReturnsBadRequest()
    {
        // Arrange
        var data = new
        {
            ConnectionString = "ValidConnectionString",
            Name = "Item1",
            Version = "1.0"
        };
        var jsonData = JsonConvert.SerializeObject(data);
        var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/admin/add-bditem", content); // Llama al endpoint real del controlador

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest,response.StatusCode);
    }

    [Fact]
    public async Task Add_DBItem_MSSQL_ReturnsOk()
    {
        // Arrange
        var data = new
        {
            ConnectionString = _connectionStringMsSql,
            Name = "MSSQL",
            Version = "",
            IsSeed= true
        };
        var jsonData = JsonConvert.SerializeObject(data);
        var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/admin/add-bditem", content); // Llama al endpoint real del controlador

        // Assert
        response.EnsureSuccessStatusCode();  // Verifica que el código de estado sea 2xx
        var responseBody = await response.Content.ReadAsStringAsync();
        Assert.Contains("id", responseBody);  // Verifica que la respuesta contenga el ID
    }

    [Fact]
    public async Task Add_DBItem_MYSQL_ReturnsOk()
    {
        // Arrange
        var data = new
        {
            ConnectionString = _connectionStringMySql,
            Name = "MYSQL",
            Version = "",
            IsSeed = false
        };
        var jsonData = JsonConvert.SerializeObject(data);
        var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/admin/add-bditem", content); // Llama al endpoint real del controlador

        // Assert
        response.EnsureSuccessStatusCode();  // Verifica que el código de estado sea 2xx
        var responseBody = await response.Content.ReadAsStringAsync();
        Assert.Contains("id", responseBody);  // Verifica que la respuesta contenga el ID
    }

    [Fact]
    public async Task Add_DBItem_MongoDB_ReturnsOk()
    {
        // Arrange
        var data = new
        {
            ConnectionString = _connectionStringMongoDB,
            Name = "MONGODB",
            Version = "",
            IsSeed = false
        };
        var jsonData = JsonConvert.SerializeObject(data);
        var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/admin/add-bditem", content); // Llama al endpoint real del controlador

        // Assert
        response.EnsureSuccessStatusCode();  // Verifica que el código de estado sea 2xx
        var responseBody = await response.Content.ReadAsStringAsync();
        Assert.Contains("id", responseBody);  // Verifica que la respuesta contenga el ID
    }
}
