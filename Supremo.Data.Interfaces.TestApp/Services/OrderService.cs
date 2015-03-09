using Ninject;
using Supremo.Data.Interfaces.DbContextScope;
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
    internal class OrderService
    {
        private readonly IDbContextScopeFactory _dbContextScopeFactory;

        [Inject]
        public OrderRepository OrderRepository { private get; set; }

        public OrderService(IDbContextScopeFactory dbContextScopeFactory)
        {
            _dbContextScopeFactory = dbContextScopeFactory;
        }

        internal void CreateOrder(Order order)
        {
            using(var contextScope = _dbContextScopeFactory.Create())
            {
                OrderRepository.Add(order);
                contextScope.SaveChanges();
            }
        }

        internal List<Order> GetAll(List<string> include = null)
        {
            using(var contextScope = _dbContextScopeFactory.CreateReadOnly())
            {
                return OrderRepository.GetAll(include).ToList();
            }
        }
    }
}
