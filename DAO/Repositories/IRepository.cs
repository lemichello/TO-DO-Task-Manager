using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DAO.Repositories
{
    public interface IRepository<T>
    {
        IEnumerable<T> Get();
        IEnumerable<T> GetByPredicate(Expression<Func<T, bool>> expression);
        bool Add(T item);
        bool Remove(T item);
        void SaveChanges();
        void Refresh();
    }
}