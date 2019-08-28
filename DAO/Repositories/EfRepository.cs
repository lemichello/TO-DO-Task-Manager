using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace DAO.Repositories
{
    public class EfRepository<T> : IRepository<T> where T : class
    {
        public EfRepository()
        {
            _context = ContextSingleton.GetInstance();
            _set     = _context.Set<T>();
        }

        public IEnumerable<T> Get()
        {
            return _set.AsEnumerable();
        }

        public IEnumerable<T> GetByPredicate(Expression<Func<T, bool>> expression)
        {
            return _set.Where(expression);
        }

        public bool Add(T item)
        {
            try
            {
                _set.Add(item);
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                ErrorHandler.Handle(e);

                return false;
            }

            return true;
        }

        public bool Remove(T item)
        {
            try
            {
                _set.Remove(item);
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                ErrorHandler.Handle(e);

                return false;
            }

            return true;
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }

        public void Refresh()
        {
            _context = ContextSingleton.GetInstance();
            _set     = _context.Set<T>();
        }

        private EfContext _context;
        private DbSet<T>  _set;
    }
}