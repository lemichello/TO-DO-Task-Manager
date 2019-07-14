using System;
using System.Collections.Generic;
using System.Linq;
using DAO.Entities;

namespace DAO.Repositories
{
    public class TagsRepository : IRepository<Tag>
    {
        private readonly EfContext _context;

        public TagsRepository()
        {
            _context = new EfContext();
        }
        
        public IEnumerable<Tag> Get()
        {
            return _context.Tags.AsEnumerable();
        }

        public bool Add(Tag item)
        {
            try
            {
                _context.Tags.Add(item);
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                ErrorHandler.Handle(e);

                return false;
            }

            return true;
        }

        public bool Remove(Tag item)
        {
            try
            {
                _context.Tags.Remove(item);
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