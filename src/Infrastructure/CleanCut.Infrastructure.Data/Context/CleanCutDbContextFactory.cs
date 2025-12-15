using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CleanCut.Infrastructure.Data.Context;

/// <summary>
/// Design-time factory for EF Core tools so migrations can be added without booting the full host.
/// Uses environment variable CLEANCUT_CONNECTION if present; otherwise falls back to LocalDB.
/// </summary>
public class CleanCutDbContextFactory : IDesignTimeDbContextFactory<CleanCutDbContext>
{
    public CleanCutDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CleanCutDbContext>();

        var connection = Environment.GetEnvironmentVariable("CLEANCUT_CONNECTION")
                         ?? "Server=(localdb)\\MSSQLLocalDB;Database=CleanCut;Trusted_Connection=True;MultipleActiveResultSets=true";

        optionsBuilder.UseSqlServer(connection, b => b.MigrationsAssembly(typeof(CleanCutDbContext).Assembly.FullName));

        return new CleanCutDbContext(optionsBuilder.Options);
    }
}
