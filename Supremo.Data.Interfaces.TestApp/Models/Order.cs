using Supremo.Data.Interfaces.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Supremo.Data.Interfaces.TestApp.Models
{
    [EntityMapping]
    public class Order : TrackedEntity
    {
        public string Name { get; set; }
        public DateTime? OrderDate { get; set; }
        public virtual ICollection<OrderItem> Items { get; set; }
        public virtual Customer Customer { get; set; }
        public int? CustomerId { get; set; }

        public static void CreateEntityMapping(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>()
                .HasMany(o => o.Items);

            modelBuilder.Entity<Order>().HasOptional(o=> o.Customer).WithMany().HasForeignKey(o => o.CustomerId);
        }
    }
}
