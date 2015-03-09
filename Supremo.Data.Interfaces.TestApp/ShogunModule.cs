using Ninject.Modules;
using Supremo.Data.Interfaces.DbContextScope;
using Supremo.Data.Interfaces.Interfaces.DbContextScope;
using Supremo.Data.Interfaces.TestApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Supremo.Data.Interfaces.TestApp
{
    public class ShogunModule : NinjectModule
    {
        public override void Load()
        {            
            Bind<IDbContextScopeFactory>().To<DbContextScopeFactory>();
            Bind<IAmbientDbContextLocator>().To<AmbientDbContextLocator>();
            Bind<IDbContextFactory>().ToMethod(db => null);
        }
    }
}
