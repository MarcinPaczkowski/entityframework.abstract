using Ninject;
using Supremo.Data.Interfaces.Interfaces.DbContextScope;
using Supremo.Data.Interfaces.TestApp.Models;
using Supremo.Data.Interfaces.TestApp.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Supremo.Data.Interfaces.TestApp.Services
{
    internal class TestCaseService
    {
        [Inject]
        public IDbContextScopeFactory DbContextScopeFactory { private get; set; }
        [Inject]
        public CustomerRepository CustomerRepository { private get; set; }
        [Inject]
        public OrderRepository OrderRepository { private get; set; }



        internal void DoTests()
        {
            var order = new Order { Name = "lama", Customer = new Customer { Name = "heheszka" }, 
                Items = new List<OrderItem> { new OrderItem {Code = "Test", Value = 2 } } };
            var order2 = new Order { Name = "lama2", Customer = new Customer { Name = "heheszka2" },
                                     Items = new List<OrderItem> { new OrderItem { Code = "Test", Value = 2 } }
            };

            using (var session = DbContextScopeFactory.Create())
            {
                OrderRepository.Add(order);
                OrderRepository.Add(order2, new HashSet<string> { "Customer" });
                session.SaveChanges();
                var orders = OrderRepository.GetAll(new List<string> { "Customer" });
            }

        }
    }
}
