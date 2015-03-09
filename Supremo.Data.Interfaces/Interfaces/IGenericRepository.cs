using Supremo.Data.Interfaces.Interfaces.Entity;
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
        IEnumerable<T> GetAll(List<string> include = null);
        T GetById(int id, List<string> include = null);
        IEnumerable<T> FindBy(Expression<Func<T, bool>> predicate, List<string> include = null);
        T Add(T entity);
        T Edit(T entity);
        void Delete(T entity);
    }
}
