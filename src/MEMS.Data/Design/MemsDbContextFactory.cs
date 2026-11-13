using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MEMS.Data.Design;

// Only used by "dotnet ef" to create migrations. The connection string is a placeholder:
// the real one comes from Aspire at run time.
public class MemsDbContextFactory : IDesignTimeDbContextFactory<MemsDbContext>
{
    public MemsDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<MemsDbContext>()
            .UseSqlServer("Server=localhost,1433;Database=memsdb;User Id=sa;Password=design-time-only;TrustServerCertificate=True")
            .Options;
        return new MemsDbContext(options, new TenantContext());
    }
}
