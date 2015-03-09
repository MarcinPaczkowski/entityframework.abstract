using Supremo.Data.Interfaces.Enums;
using Supremo.Data.Interfaces.Interfaces.DbContextScope;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Supremo.Data.Interfaces.DbContextScope
{
    public class DbContextReadOnlyScope : IDbContextReadOnlyScope
    {
        DbContextScope _internalScope;

        public IDbContextCollection DbContexts
        {
            get { return _internalScope.DbContexts; }
        }

        public DbContextReadOnlyScope(IDbContextFactory dbContextFactory = null)
            : this(joiningOption: DbContextScopeOption.JoinExisting, isolationLevel: null, dbContextFactory: dbContextFactory)
        { }

        public DbContextReadOnlyScope(IsolationLevel isolationLevel, IDbContextFactory dbContextFactory = null)
            : this(joiningOption: DbContextScopeOption.ForceCreateNew, isolationLevel: isolationLevel, dbContextFactory: dbContextFactory)
        { }

        public DbContextReadOnlyScope(DbContextScopeOption joiningOption, IsolationLevel? isolationLevel, IDbContextFactory dbContextFactory = null)
        {
            _internalScope = new DbContextScope(joiningOption: joiningOption, readOnly: true, isolationLevel: isolationLevel, dbContextFactory: dbContextFactory);
        }

        public void Dispose()
        {
            _internalScope.Dispose();
        }
    }
}
