using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Rento.Core.Persistence
{
    public interface IMainRepository : IRepository
    {
        DbSet<T> Set<T>() where T : class;
        DatabaseFacade Database { get; }

    }
}
