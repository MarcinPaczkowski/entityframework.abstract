using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Supremo.Data.Interfaces.Interfaces.DbContextScope
{
    public interface IDbContextScope : IDisposable
    {
        int SaveChanges();
        Task<int> SaveChangesAsync();
        Task<int> SaveChangesAsync(CancellationToken cancelToken);
        void RefreshEntitiesInParentScope(IEnumerable entities);
        Task RefreshEntitiesInParentScopeAsync(IEnumerable entities);
        IDbContextCollection DbContexts { get; }
    }
}
