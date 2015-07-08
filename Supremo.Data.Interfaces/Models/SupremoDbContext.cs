using Supremo.Data.Interfaces.Interfaces.Entity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Supremo.Data.Interfaces.Models
{
    public abstract class SupremoDbContext : DbContext
    {
        protected abstract string GetCurrentUserId();
        protected abstract List<AssemblyName> GetModelMappingAssemblyNames();

        public SupremoDbContext() : base() {}
        public SupremoDbContext(string connectionName) : base(connectionName) { }

        public override int SaveChanges()
        {
            var currentUserId = GetCurrentUserId();
            var modifiedEntries = ChangeTracker.Entries().Where(e => e.Entity is ITrackedEntity
                && (e.State == EntityState.Added || e.State == EntityState.Modified));
            foreach (var entry in modifiedEntries)
            {
                var entity = entry.Entity as ITrackedEntity;
                if (entity != null)
                {
                    var operationTime = DateTime.Now;
                    if (entry.State == EntityState.Added)
                    {
                        entity.CreatedBy = currentUserId;
                        entity.CreatedDate = operationTime;
                    }
                    else
                    {
                        base.Entry(entity).Property(e => e.CreatedBy).IsModified = false;
                        base.Entry(entity).Property(e => e.CreatedDate).IsModified = false;
                    }
                    entity.UpdatedBy = currentUserId;
                    entity.UpdatedDate = operationTime;
                }
            }

            return base.SaveChanges();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            EntityMappingConfig.CreateMappings(modelBuilder, GetModelMappingAssemblyNames());
            base.OnModelCreating(modelBuilder);
        }
    }
}
