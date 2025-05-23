using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SPG_Fachtheorie.Aufgabe1.Infrastructure;
using SPG_Fachtheorie.Aufgabe1.Services;
using System.Net;
using System.Text;
using System.Text.Json;


namespace Spg.Fachtheorie.Aufgabe3.API.Test;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        
        builder.ConfigureServices(services =>
        {
            // Register services needed for tests
            services.AddScoped<PaymentService>();
            
            // Other service registrations...
        });
    }
    public void InitializeDatabase(Action<AppointmentContext> action)
    {
        using var scope = Services.CreateScope();
        using var db = scope.ServiceProvider.GetRequiredService<AppointmentContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();
        action(db);
    }

    public Tout QueryDatabase<Tout>(Func<AppointmentContext, Tout> query)
    {
        using var scope = Services.CreateScope();
        using var db = scope.ServiceProvider.GetRequiredService<AppointmentContext>();
        return query(db);
    }

    /// <summary>
    /// Send a GET Request and return the deserialized response.
    /// Useful for strongly typed JSON data with DTOs.
    /// </summary>
    public async Task<(HttpStatusCode, T?)> GetHttpContent<T>(string requestUrl) where T : class
    {
        using var client = CreateClient();
        var response = await client.GetAsync(requestUrl);
        if (!response.IsSuccessStatusCode) return (response.StatusCode, default);
        var dataString = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<T>(dataString, _jsonOptions);
        if (data is null) throw new Exception("Deserialization failed");
        return (response.StatusCode, data);
    }

    /// <summary>
    /// Send a GET Request and return the response as a JsonElement. Useful for dynamic JSON data without DTOs.
    /// </summary>
    /// <param name="requestUrl"></param>
    /// <returns></returns>
    public async Task<(HttpStatusCode, JsonElement)> GetHttpContent(string requestUrl)
    {
        using var client = CreateClient();
        var response = await client.GetAsync(requestUrl);
        if (!response.IsSuccessStatusCode) return (response.StatusCode, new JsonElement());
        var dataString = await response.Content.ReadAsStringAsync();
        var data = JsonDocument.Parse(dataString);
        return (response.StatusCode, data.RootElement);
    }

    /// <summary>
    /// Send a POST Request with a strongly typed payload and return the deserialized response.
    /// </summary>
    public async Task<(HttpStatusCode, JsonElement)> PostHttpContent<Tcmd>(string requestUrl, Tcmd payload) where Tcmd : class
    {
        using var client = CreateClient();
        var jsonBody = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await client.PostAsync(requestUrl, jsonBody);
        if (!response.IsSuccessStatusCode) return (response.StatusCode, new JsonElement());

        var dataString = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrEmpty(dataString)) return (response.StatusCode, new JsonElement());
        var data = JsonDocument.Parse(dataString);
        return (response.StatusCode, data.RootElement);
    }

    /// <summary>
    /// Send a PATCH Request with a strongly typed payload and return the deserialized response.
    /// </summary>
    public async Task<(HttpStatusCode, JsonElement)> PatchHttpContent<Tcmd>(string requestUrl, Tcmd payload) where Tcmd : class
    {
        using var client = CreateClient();
        var jsonBody = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await client.PatchAsync(requestUrl, jsonBody);
        if (!response.IsSuccessStatusCode) return (response.StatusCode, new JsonElement());

        var dataString = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrEmpty(dataString)) return (response.StatusCode, new JsonElement());
        var data = JsonDocument.Parse(dataString);
        return (response.StatusCode, data.RootElement);
    }

    /// <summary>
    /// Send a PUT Request with a strongly typed payload and return the deserialized response.
    /// </summary>
    public async Task<(HttpStatusCode, JsonElement)> PutHttpContent<Tcmd>(string requestUrl, Tcmd payload) where Tcmd : class
    {
        using var client = CreateClient();
        var jsonBody = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await client.PutAsync(requestUrl, jsonBody);
        if (!response.IsSuccessStatusCode) return (response.StatusCode, new JsonElement());

        var dataString = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrEmpty(dataString)) return (response.StatusCode, new JsonElement());
        var data = JsonDocument.Parse(dataString);
        return (response.StatusCode, data.RootElement);
    }

    /// <summary>
    /// Send a DELETE Request with a strongly typed payload and return the deserialized response.
    /// </summary>
    public async Task<HttpStatusCode> DeleteHttpContent(string requestUrl)
    {
        using var client = CreateClient();
        var response = await client.DeleteAsync(requestUrl);
        return response.StatusCode;
    }

    public HttpClient Client => CreateClient();
}
