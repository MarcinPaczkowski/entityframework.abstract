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

        public IEnumerable<T> GetAll()
        {
            IQueryable<T> items = _dbSet;
            return items.AsEnumerable<T>();
        }

        public IEnumerable<T> GetAll(List<string> include)
        {
            IQueryable<T> query = _dbSet;
            query = GetInculded(query, include);
            return query.AsEnumerable<T>();
        }

        public IEnumerable<T> GetAll(PageInfo pageInfo)
        {
            IQueryable<T> query = _dbSet;
            query = GetPaged(query, pageInfo);
            return query.AsEnumerable<T>();
        }

        public IEnumerable<T> GetAll(List<string> include, PageInfo pageInfo)
        {
            IQueryable<T> query = _dbSet;
            query = GetInculded(query, include);
            query = GetPaged(query, pageInfo);
            return query.AsEnumerable<T>();
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

        public IEnumerable<T> FindBy(Expression<Func<T, bool>> predicate)
        {
            IQueryable<T> items = _dbSet.Where(predicate);
            return items.AsEnumerable<T>();
        }

        public IEnumerable<T> FindBy(Expression<Func<T, bool>> predicate, List<string> include)
        {
            IQueryable<T> items = _dbSet.Where(predicate);
            items = GetInculded(items, include);
            return items.AsEnumerable<T>();
        }

        public IEnumerable<T> FindBy(Expression<Func<T, bool>> predicate, PageInfo pageInfo)
        {
            IQueryable<T> items = _dbSet.Where(predicate);
            items = GetPaged(items, pageInfo);
            return items.AsEnumerable<T>();
        }

        public IEnumerable<T> FindBy(Expression<Func<T, bool>> predicate, List<string> include, PageInfo pageInfo)
        {
            IQueryable<T> items = _dbSet.Where(predicate);
            items = GetInculded(items, include);
            items = GetPaged(items, pageInfo);
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

        private IQueryable<T> GetInculded(IQueryable<T> query, List<string> include)
        {
            if (include != null)
            {
                foreach (var includePath in include)
                    query = query.Include(includePath);
            }
            return query;
        }

        private IQueryable<T> GetPaged(IQueryable<T> query, PageInfo pageInfo)
        {
            if(pageInfo != null)
                query.Skip(pageInfo.CurrentPageNumber * pageInfo.ItemsPerPage).Take(pageInfo.ItemsPerPage);
            return query;
        }
    }
}
