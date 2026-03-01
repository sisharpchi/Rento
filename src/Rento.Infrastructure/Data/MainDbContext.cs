using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Rento.Core.Entities;

namespace Rento.Infrastructure.Data;

public class MainDbContext : IdentityDbContext<User, UserRole, string>
{
    public MainDbContext(DbContextOptions<MainDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(MainDbContext).Assembly);
    }
}
