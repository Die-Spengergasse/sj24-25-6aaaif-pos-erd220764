using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Spg.Fachtheorie.Aufgabe3.API.Test;
using SPG_Fachtheorie.Aufgabe1.Model;
using SPG_Fachtheorie.Aufgabe1.Services;

namespace SPG_Fachtheorie.Aufgabe3.Test
{
    public class PaymentsControllerTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public PaymentsControllerTests(TestWebApplicationFactory factory)
        {
            // Create a factory that adds the PaymentService registration
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Register mock or real PaymentService
                    services.AddScoped<PaymentService>();
                });
            });
            _client = _factory.CreateClient();
        }

        [Theory]
        [InlineData(1, null, 3)] 
        [InlineData(null, "2024-05-13", 4)] 
        [InlineData(1, "2024-05-13", 2)] 
        [InlineData(null, null, 6)] 
        public async Task GetPayments_WithFilters_ReturnsFilteredPayments(int? cashDesk, string? dateFrom, int expectedCount)
        {
            // Arrange
            var query = new List<string>();
            if (cashDesk.HasValue)
                query.Add($"cashDesk={cashDesk}");
            if (dateFrom != null)
                query.Add($"dateFrom={dateFrom}");
            var url = $"/api/payments?{string.Join("&", query)}";

            // Act
            var response = await _client.GetAsync(url);
            var payments = await response.Content.ReadFromJsonAsync<List<Payment>>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(payments);
            Assert.Equal(expectedCount, payments.Count);
            
            // Validate filter conditions
            if (cashDesk.HasValue)
                Assert.True(payments.All(p => p.CashDesk.Number == cashDesk));
            if (dateFrom != null)
            {
                var date = DateTime.Parse(dateFrom);
                Assert.True(payments.All(p => p.PaymentDateTime.Date >= date.Date));
            }
        }

        [Theory]
        [InlineData(1, HttpStatusCode.OK)] 
        [InlineData(999, HttpStatusCode.NotFound)] 
        public async Task GetPaymentById_ReturnsCorrectStatusCode(int id, HttpStatusCode expectedStatusCode)
        {
            var response = await _client.GetAsync($"/api/payments/{id}");

            Assert.Equal(expectedStatusCode, response.StatusCode);
        }

        [Theory]
        [InlineData(1, HttpStatusCode.NoContent)] 
        [InlineData(999, HttpStatusCode.NotFound)] 
        [InlineData(2, HttpStatusCode.BadRequest)] 
        public async Task PatchPayment_ReturnsCorrectStatusCode(int id, HttpStatusCode expectedStatusCode)
        {
            // Act
            var response = await _client.PatchAsync($"/api/payments/{id}", null);

            // Assert
            Assert.Equal(expectedStatusCode, response.StatusCode);
        }

        [Theory]
        [InlineData(1, HttpStatusCode.NoContent)] 
        [InlineData(999, HttpStatusCode.NotFound)]
        public async Task DeletePayment_ReturnsCorrectStatusCode(int id, HttpStatusCode expectedStatusCode)
        {
            // Act
            var response = await _client.DeleteAsync($"/api/payments/{id}");

            // Assert
            Assert.Equal(expectedStatusCode, response.StatusCode);
        }
    }
}