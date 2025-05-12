using BankingApi.Data;
using BankingApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace BankingApi.Controllers
{
    [Authorize(AuthenticationSchemes = "ApiKey")]
    [ApiController]
    [Route("banking/accounts")]
    public class AccountsController : ControllerBase
    {
        private readonly BankingContext _context;

        public AccountsController(BankingContext context)
        {
            _context = context;

            // Seed some dummy accounts if none exist
            if (!_context.Accounts.Any())
            {
                _context.Accounts.AddRange(new[]
                {
                    new Account { AccountId = "123", DisplayName = "Richa's Account", AccountType = "SAVINGS", AccountStatus = "Active", Currency= "AUD" },
                    new Account { AccountId = "456", DisplayName = "AK's Account", AccountType = "SAVINGS", AccountStatus = "Active",Currency= "AUD" }
                });
                _context.SaveChanges();
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAccounts()
        {
            var accounts = await _context.Accounts.ToListAsync();
            return Ok(new { data = new { accounts } });
        }
    }
}
