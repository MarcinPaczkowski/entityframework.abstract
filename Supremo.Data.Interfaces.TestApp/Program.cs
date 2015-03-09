using Ninject;
using Supremo.Data.Interfaces.DbContextScope;
using Supremo.Data.Interfaces.TestApp.Models;
using Supremo.Data.Interfaces.TestApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Supremo.Data.Interfaces.TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            IKernel kernel = new StandardKernel(new ShogunModule());
            kernel.Settings.AllowNullInjection = true;

            var testService = kernel.Get<TestCaseService>();
            testService.DoTests();

            
        }
    }
}
