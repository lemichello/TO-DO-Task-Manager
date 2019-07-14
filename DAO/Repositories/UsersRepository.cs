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
            _context = new EfContext();
        }
        
        public IEnumerable<User> Get()
        {
            return _context.Users.AsEnumerable();
        }

        public bool Add(User item)
        {
            try
            {
                _context.Users.Add(item);
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                ErrorHandler.Handle(e);

                return false;
            }

            return true;
        }

        public bool Remove(User item)
        {
            try
            {
                _context.Users.Remove(item);
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                ErrorHandler.Handle(e);

                return false;
            }

            return true;
        }
    }
}