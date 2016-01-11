using Supremo.Data.Interfaces.Interfaces.Entity;
using Supremo.Data.Interfaces.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Supremo.Data.Interfaces.Interfaces
{
    public interface IGenericRepository<T, Tdb> where T : IEntity where Tdb : DbContext
    {
        IEnumerable<T> GetAll();
        IEnumerable<T> GetAll(List<string> include);
        IEnumerable<T> GetAll(PageInfo pageInfo);
        IEnumerable<T> GetAll(List<string> include, PageInfo pageInfo);
        T GetById(int id, List<string> include = null);
        IEnumerable<T> FindBy(Expression<Func<T, bool>> predicate);
        IEnumerable<T> FindBy(Expression<Func<T, bool>> predicate, List<string> include);
        IEnumerable<T> FindBy(Expression<Func<T, bool>> predicate, PageInfo pageInfo);
        IEnumerable<T> FindBy(Expression<Func<T, bool>> predicate, List<string> include, PageInfo pageInfo);
        T Add(T entity, HashSet<string> includedEntities = null);
        T Edit(T entity);
        void Delete(T entity);
        void Delete(int id);
    }
}
