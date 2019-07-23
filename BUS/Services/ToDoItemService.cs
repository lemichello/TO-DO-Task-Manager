using System;
using System.Collections.Generic;
using System.Linq;
using BUS.Models;
using DAO.Entities;
using DAO.Repositories;

namespace BUS.Services
{
    public class ToDoItemService : Service
    {
        private readonly IRepository<ToDoItem> _repository;
        private readonly int                   _userId;

        public ToDoItemService(int userId)
        {
            _repository = new ToDoItemsRepository();
            _userId     = userId;
        }

        public void Add(ToDoItemModel item)
        {
            var addingItem = new ToDoItem
            {
                Header       = item.Header,
                Notes        = item.Notes,
                Date         = item.Date,
                Deadline     = item.Deadline,
                CompleteDate = item.CompleteDay,
                UserId       = _userId
            };

            item.Id = _repository.Add(addingItem) ? addingItem.Id : -1;
        }

        public bool Remove(ToDoItemModel item)
        {
            var foundItem = _repository.Get().First(i => i.Id == item.Id);

            return _repository.Remove(foundItem);
        }

        public void Update(ToDoItemModel item)
        {
            var foundItem = _repository.Get().First(i => i.Id == item.Id);

            foundItem.Header       = item.Header;
            foundItem.Notes        = item.Notes;
            foundItem.Date         = item.Date;
            foundItem.Deadline     = item.Deadline;
            foundItem.CompleteDate = item.CompleteDay;

            _repository.SaveChanges();
        }

        public IEnumerable<ToDoItemModel> Get(Func<ToDoItemModel, bool> predicate)
        {
            return _repository.Get()
                .Where(i => i.UserId == _userId)
                .Select(i => new ToDoItemModel
                {
                    Id          = i.Id,
                    Header      = i.Header,
                    Notes       = i.Notes,
                    Date        = i.Date,
                    Deadline    = i.Deadline,
                    CompleteDay = i.CompleteDate
                })
                .Where(predicate);
        }
    }
}