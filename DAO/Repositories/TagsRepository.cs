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
            _context = ContextSingleton.GetInstance();
        }
        
        public IEnumerable<Tag> Get()
        {
            return _context.Tags.AsEnumerable();
        }

        public bool Add(Tag tag)
        {
            try
            {
                _context.Tags.Add(tag);
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                ErrorHandler.Handle(e);

                return false;
            }

            return true;
        }

        public bool Remove(Tag tag)
        {
            try
            {
                _context.Tags.Remove(tag);
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