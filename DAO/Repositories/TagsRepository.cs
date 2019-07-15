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

        public bool Update(Tag tag)
        {
            try
            {
                var found = _context.Tags.FirstOrDefault(i => i.Id == tag.Id) ??
                            throw new ArgumentNullException($"Not found tag with Id = {tag.Id}");

                found.Text = tag.Text;

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