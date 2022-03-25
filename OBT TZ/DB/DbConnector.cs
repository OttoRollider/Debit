using Microsoft.EntityFrameworkCore;

namespace Debit.DB
{
    public class DbConnector : DbContext
    {
        /// <summary>
        /// Таблица БД
        /// </summary>
        public DbSet<StructDb> money_debit { get; set; }
        public DbConnector() => Database.EnsureCreated();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=92.241.230.143;Port=5432;Database=money_deb;Username=postgres;Password=019925");
        }
    }
}
