using System;
using System.Collections.Generic;
using System.Linq;
using DAO.Entities;

namespace DAO.Repositories
{
    public class ToDoItemsRepository : IRepository<ToDoItem>
    {
        private readonly EfContext _context;

        public ToDoItemsRepository()
        {
            _context = new EfContext();
        }
        
        public IEnumerable<ToDoItem> Get()
        {
            return _context.ToDoItems.AsEnumerable();
        }

        public bool Add(ToDoItem item)
        {
            try
            {
                _context.ToDoItems.Add(item);
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                ErrorHandler.Handle(e);

                return false;
            }

            return true;
        }

        public bool Remove(ToDoItem item)
        {
            try
            {
                _context.ToDoItems.Remove(item);
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