using Supremo.Data.Interfaces.Enums;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Supremo.Data.Interfaces.Interfaces.DbContextScope
{
    public interface IDbContextScopeFactory
    {
        IDbContextScope Create(DbContextScopeOption joiningOption = DbContextScopeOption.JoinExisting);
        IDbContextReadOnlyScope CreateReadOnly(DbContextScopeOption joiningOption = DbContextScopeOption.JoinExisting);
        IDbContextScope CreateWithTransaction(IsolationLevel isolationLevel);
        IDbContextReadOnlyScope CreateReadOnlyWithTransaction(IsolationLevel isolationLevel);
        IDisposable SuppressAmbientContext();
    }
}
