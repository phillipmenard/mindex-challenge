using CodeChallenge.Models;
using Microsoft.EntityFrameworkCore;

namespace CodeChallenge.Data
{
    public class EmployeeContext : DbContext
    {
        public EmployeeContext(DbContextOptions<EmployeeContext> options) : base(options)
        {

        }

        public DbSet<Employee> Employees { get; set; }

        /// <summary>
        /// Compensation data for employees.
        /// </summary>
        /// <remarks>
        /// Many compensation records may exist for any given Employee.
        /// </remarks>
        public DbSet<Compensation> Compensation { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Compensation>()
                .HasKey("EmployeeId", "EffectiveDate");
        }
    }
}
