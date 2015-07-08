using Supremo.Data.Interfaces.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Supremo.Data.Interfaces.TestApp.Models
{
    public class Customer : TrackedEntity
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }
}
