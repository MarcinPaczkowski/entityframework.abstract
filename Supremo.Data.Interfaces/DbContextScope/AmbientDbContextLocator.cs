using Supremo.Data.Interfaces.Interfaces.DbContextScope;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Supremo.Data.Interfaces.DbContextScope
{
    public class AmbientDbContextLocator : IAmbientDbContextLocator
    {
        public TDbContext Get<TDbContext>() where TDbContext : DbContext
        {
            var ambientDbContextScope = AmbientScope.GetAmbientScope();
            return ambientDbContextScope == null ? null : ambientDbContextScope.DbContexts.Get<TDbContext>();
        }
    }
}
