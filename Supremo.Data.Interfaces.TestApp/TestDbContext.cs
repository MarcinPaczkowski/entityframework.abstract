using Supremo.Data.Interfaces.Models;
using Supremo.Data.Interfaces.TestApp.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Supremo.Data.Interfaces.TestApp
{
    internal class TestDbContext : SupremoDbContext
    {
        public TestDbContext() : base()
        {
            //Database.SetInitializer<TestDbContext>(new DropCreateDatabaseIfModelChanges<TestDbContext>());
        }

        protected override string GetCurrentUserId()
        {
            return "1";
        }

        protected override List<AssemblyName> GetModelMappingAssemblyNames()
        {
            return new List<AssemblyName> { Assembly.GetCallingAssembly().GetName() };
        }

        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Customer> Customers { get; set; }
    }
}
