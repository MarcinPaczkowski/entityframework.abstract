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
        private readonly IDbContextScopeFactory _dbContextScopeFactory;
        [Inject]
        public CustomerRepository CustomerRepository { private get; set; }
        [Inject]
        public OrderRepository OrderRepository { private get; set; }



        internal void DoTests()
        {
            var order = OrderRepository.GetById(1);
            var fullOrder = OrderRepository.GetById(1, new List<string> {"Items", "Customer"});
        }
    }
}
