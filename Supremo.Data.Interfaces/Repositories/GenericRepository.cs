using Supremo.Data.Interfaces.Interfaces;
using Supremo.Data.Interfaces.Interfaces.DbContextScope;
using Supremo.Data.Interfaces.Interfaces.Entity;
using Supremo.Data.Interfaces.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Supremo.Data.Interfaces.Repositories
{
    public class GenericRepository<T, Tdb> : IGenericRepository<T, Tdb> where T : Entity, new() where Tdb : DbContext
    {
        protected readonly IAmbientDbContextLocator _ambientDbContextLocator;
        protected IDbSet<T> _dbSet { get { return DbContext.Set<T>(); }}

        protected Tdb DbContext
        {
            get
            {
                var dbContext = _ambientDbContextLocator.Get<Tdb>();
                if (dbContext == null) throw new InvalidOperationException("No ambient DbContext exists");
                return dbContext;
            }
        }

        public GenericRepository(IAmbientDbContextLocator ambientDbContextLocator)
		{
			if (ambientDbContextLocator == null) throw new ArgumentNullException("ambientDbContextLocator");
			_ambientDbContextLocator = ambientDbContextLocator;
		}

        public IEnumerable<T> GetAll(List<string> include = null)
        {
            IQueryable<T> items = _dbSet;

            if (include != null)
            {
                foreach (var includePath in include)
                    items = items.Include(includePath);
            }

            return items.AsEnumerable<T>();
        }

        public virtual T GetById(int id, List<string> include = null)
        {
            T entity = null;

            IQueryable<T> items  = _dbSet.Where(c => c.Id == id);

            if (include != null)
            {
                foreach (var includePath in include)
                    items = items.Include(includePath);
            }

            entity = items.FirstOrDefault();

            if (entity == null)
                throw new ObjectNotFoundException(String.Format("Id: ", id));
            else
                return entity;
        }

        public IEnumerable<T> FindBy(Expression<Func<T, bool>> predicate, List<string> include = null)
        {
            IQueryable<T> items = _dbSet.Where(predicate);

            if (include != null)
            {
                foreach (var includePath in include)
                    items = items.Include(includePath);
            }

            return items.AsEnumerable<T>();
        }

        public virtual T Add(T entity, HashSet<string> includedEntities = null)
        {
            RemoveSubEntities(entity, includedEntities);
            return _dbSet.Add(entity);
        }

        public T Edit(T entity)
        {
            RemoveSubEntities(entity, null);
            AttachEntity(entity);
            DbContext.Entry(entity).State = EntityState.Modified;
            return entity;
        }

        public void Delete(T entity)
        {
            AttachEntity(entity);
            _dbSet.Remove(entity);
        }

        public void Delete(int id)
        {
            var dEntity = new T();
            dEntity.Id = id;
            Delete(dEntity);
        }

        private void AttachEntity(T entity)
        {
            var attachedEntity = _dbSet.Local.FirstOrDefault(e => e.Id == entity.Id);
            if (attachedEntity != null)
                ((IObjectContextAdapter)DbContext).ObjectContext.Detach(attachedEntity);

            _dbSet.Attach(entity);
        }

        private void RemoveSubEntities(T entity, HashSet<string> excludedEntities)
        {
            if (excludedEntities == null) excludedEntities = new HashSet<string>();
            
            foreach (var property in typeof(T).GetProperties(BindingFlags.Public|BindingFlags.Instance))
            {
                var propertyType = property.PropertyType;
                if (propertyType.GetInterfaces().Contains(typeof(IEntity)))
                {
                    if (excludedEntities.Contains(property.Name))
                        continue;

                    property.SetValue(entity, null);
                    continue;
                }

                if(propertyType.IsGenericType && 
                    propertyType.GetGenericTypeDefinition() == typeof(ICollection<>))
                {
                    var itemType = propertyType.GetGenericArguments()[0];
                    if(itemType.GetInterfaces().Contains(typeof(IEntity)))
                    {
                        if (excludedEntities.Contains(property.Name))
                            continue;

                        property.SetValue(entity, null);
                        continue;
                    }
                }
            }
        }
    }
}
