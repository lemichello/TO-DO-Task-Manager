using System.Collections.Generic;

namespace DAO.Repositories
{
    public interface IRepository<T>
    {
        IEnumerable<T> Get();
        bool Add(T item);
        bool Remove(T item);
        void SaveChanges();
    }
}