using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Linq;
using DAO.Entities;

namespace DAO.Repositories
{
    public class ToDoItemsRepository : IRepository<ToDoItem>
    {
        private readonly EfContext _context;

        public ToDoItemsRepository()
        {
            _context = ContextSingleton.GetInstance();
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

        public bool Update(ToDoItem item)
        {
            try
            {
                var found = _context.ToDoItems.FirstOrDefault(i => i.Id == item.Id) ??
                            throw new ArgumentNullException($"Not found item with Id = {item.Id}");

                found.Header = item.Header;
                found.Notes = item.Notes;
                found.Date = item.Date;
                found.Deadline = item.Deadline;

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