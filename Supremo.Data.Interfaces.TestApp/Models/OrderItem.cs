using Supremo.Data.Interfaces.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Supremo.Data.Interfaces.TestApp.Models
{
    public class OrderItem : TrackedEntity
    {
        public string Code { get; set; }
        public decimal Value { get; set; }
    }
}
