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
            _context = ContextSingleton.GetInstance();
        }

        public IEnumerable<ItemTag> Get()
        {
            return _context.ItemsTags.AsEnumerable();
        }

        public bool Add(ItemTag itemTag)
        {
            try
            {
                _context.ItemsTags.Add(itemTag);
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                ErrorHandler.Handle(e);

                return false;
            }

            return true;
        }

        public bool Remove(ItemTag itemTag)
        {
            try
            {
                _context.ItemsTags.Remove(itemTag);
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                ErrorHandler.Handle(e);

                return false;
            }

            return true;
        }

        public bool Update(ItemTag itemTag)
        {
            try
            {
                var found = _context.ItemsTags.FirstOrDefault(i => i.TagId == itemTag.TagId &&
                                                                   i.ItemId == itemTag.ItemId) ??
                            throw new ArgumentNullException($"Not found itemTag with TagId = {itemTag.TagId} and " +
                                                            $"ItemId = {itemTag.ItemId}");

                found.ItemId   = itemTag.ItemId;
                found.TagId    = itemTag.TagId;

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