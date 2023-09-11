using Microsoft.EntityFrameworkCore;

namespace Reformat.Data.EFCore.Core;

public class EFCoreDbContext : DbContext
{
    public EFCoreDbContext(DbContextOptions<EFCoreDbContext> options) : base(options)
    {
    }
}