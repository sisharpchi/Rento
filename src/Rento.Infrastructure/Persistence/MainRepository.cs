using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Rento.Core.Persistence;
using Rento.Infrastructure.Data;

namespace Rento.Infrastructure.Persistence;

internal class MainRepository : GenericRepository<MainDbContext>, IMainRepository
{
    public DatabaseFacade Database => Context.Database;

    public MainRepository(
        MainDbContext context,
        IUnitOfWork unitOfWork)
        : base(context, unitOfWork)
    {
    }

    public DbSet<T> Set<T>() where T : class
    {
        return Context.Set<T>();
    }
}
