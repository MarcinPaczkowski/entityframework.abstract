using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Supremo.Data.Interfaces.Interfaces.DbContextScope
{
    public interface IAmbientDbContextLocator
    {
        TDbContext Get<TDbContext>() where TDbContext : DbContext;
    }
}
