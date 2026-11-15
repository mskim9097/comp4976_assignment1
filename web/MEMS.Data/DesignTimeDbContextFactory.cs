using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MEMS.Data;

// Used ONLY by the EF Core tools (dotnet ef migrations add ...). At run time Aspire supplies the real connection string.
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<MemsDbContext>
{
    public MemsDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<MemsDbContext>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=MEMS_DesignTime;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;
        return new MemsDbContext(options);
    }
}
