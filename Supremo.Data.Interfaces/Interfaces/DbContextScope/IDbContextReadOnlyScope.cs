using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Supremo.Data.Interfaces.Interfaces.DbContextScope
{
    public interface IDbContextReadOnlyScope : IDisposable
    {
        IDbContextCollection DbContexts { get; }
    }
}
