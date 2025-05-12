using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BankingApi.Tests
{
    public class AccountsIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public AccountsIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _client = factory
                .WithWebHostBuilder(builder =>
                {
                    // ✅ Set environment to prevent SQL Server from being registered
                    builder.UseSetting("environment", "Test");

                    // ✅ Replace DB with isolated InMemory for each test run
                    builder.ConfigureServices(services =>
                    {
                        services.AddDbContext<BankingApi.Data.BankingContext>(opts =>
                            opts.UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString()));

                        // ✅ Seed test data
                        var sp = services.BuildServiceProvider();
                        using var scope = sp.CreateScope();
                        var db = scope.ServiceProvider.GetRequiredService<BankingApi.Data.BankingContext>();
                        db.Accounts.Add(new BankingApi.Models.Account
                        {
                            AccountId = "123",
                            DisplayName = "Richa's Account",
                            AccountType = "Savings"
                        });
                        db.SaveChanges();
                    });

                    // ✅ Inject test API key
                    builder.ConfigureAppConfiguration((ctx, cfg) =>
                    {
                        cfg.AddInMemoryCollection(new Dictionary<string, string?>
                        {
                            ["ApiKey"] = "test-key"
                        });
                    });
                })
                .CreateClient();
        }

        [Fact]
        public async Task NoApiKey_Returns401()
        {
            var resp = await _client.GetAsync("/banking/accounts");
            Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
        }

        [Fact]
        public async Task WithApiKey_Returns200AndData()
        {
            _client.DefaultRequestHeaders.Add("X-API-Key", "test-key");
            var resp = await _client.GetAsync("/banking/accounts");
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

            var json = await resp.Content.ReadAsStringAsync();
            Assert.Contains("accounts", json);
            Assert.Contains("123", json);
        }
    }
}
