using System;
using System.Collections.Generic;
using System.Linq;
using DAO.Entities;

namespace DAO.Repositories
{
    public class UsersRepository : IRepository<User>
    {
        private readonly EfContext _context;

        public UsersRepository()
        {
            _context = ContextSingleton.GetInstance();
        }
        
        public IEnumerable<User> Get()
        {
            return _context.Users.AsEnumerable();
        }

        public bool Add(User tag)
        {
            try
            {
                _context.Users.Add(tag);
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                ErrorHandler.Handle(e);

                return false;
            }

            return true;
        }

        public bool Remove(User tag)
        {
            try
            {
                _context.Users.Remove(tag);
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
    }
}