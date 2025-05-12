using Microsoft.EntityFrameworkCore;
using BankingApi.Models;

namespace BankingApi.Data
{
    public class BankingContext : DbContext
    {
        public BankingContext(DbContextOptions<BankingContext> options) : base(options) {}

        public DbSet<Account> Accounts => Set<Account>();
    }
}
