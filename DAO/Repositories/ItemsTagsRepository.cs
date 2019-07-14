using System;
using System.Collections.Generic;
using System.Linq;
using DAO.Entities;

namespace DAO.Repositories
{
    public class ItemsTagsRepository : IRepository<ItemTag>
    {
        private readonly EfContext _context;

        public ItemsTagsRepository()
        {
            _context = new EfContext();
        }

        public IEnumerable<ItemTag> Get()
        {
            return _context.ItemsTags.AsEnumerable();
        }

        public bool Add(ItemTag item)
        {
            try
            {
                _context.ItemsTags.Add(item);
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                ErrorHandler.Handle(e);

                return false;
            }

            return true;
        }

        public bool Remove(ItemTag item)
        {
            try
            {
                _context.ItemsTags.Remove(item);
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