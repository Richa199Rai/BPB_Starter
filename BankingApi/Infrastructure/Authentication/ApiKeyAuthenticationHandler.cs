using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BankingApi.Infrastructure.Authentication
{
    public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private const string HeaderName = "X-API-Key";
        private readonly string _configuredKey;

        public ApiKeyAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IConfiguration config)
            : base(options, logger, encoder, clock)
        {
            _configuredKey = config["ApiKey"];
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue(HeaderName, out var providedKey))
                return Task.FromResult(AuthenticateResult.Fail("Missing API Key"));

            if (string.IsNullOrWhiteSpace(_configuredKey) || providedKey != _configuredKey)
                return Task.FromResult(AuthenticateResult.Fail("Invalid API Key"));

            var claims = new[] { new Claim(ClaimTypes.Name, "ApiClient") };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var ticket   = new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
