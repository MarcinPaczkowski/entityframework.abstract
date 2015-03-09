using Supremo.Data.Interfaces.Interfaces.DbContextScope;
using Supremo.Data.Interfaces.Repositories;
using Supremo.Data.Interfaces.TestApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Supremo.Data.Interfaces.TestApp.Repositories
{
    public class CustomerRepository : GenericRepository<Customer, TestDbContext>
    {
        public CustomerRepository(IAmbientDbContextLocator locator) : base(locator) { }
    }
}
