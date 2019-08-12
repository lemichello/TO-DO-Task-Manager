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
        private readonly IRepository<ProjectsUsers> _projectUserRepository;
        private readonly IRepository<ToDoItem>      _repository;
        private readonly int                        _userId;
        private static   ToDoItemService            _self;
        private readonly TagService                 _tagService;

        private ToDoItemService(int userId)
        {
            _projectUserRepository = new ProjectsUsersRepository();
            _repository            = new ToDoItemsRepository();
            _userId                = userId;

            _tagService = TagService.GetInstance();
        }

        public void RefreshRepositories()
        {
            _repository.Refresh();
            _projectUserRepository.Refresh();
        }

        public static void Initialize(int userId)
        {
            _self = new ToDoItemService(userId);
        }

        public static ToDoItemService GetInstance()
        {
            return _self;
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
                UserId       = _userId,
                ProjectId    = item.ProjectId
            };

            item.Id = _repository.Add(addingItem) ? addingItem.Id : -1;
        }

        public bool Remove(ToDoItemModel item)
        {
            var foundItem = _repository.Get().First(i => i.Id == item.Id);

            return _tagService.RemoveTagsFromTask(foundItem.Id) && _repository.Remove(foundItem);
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
            var projectsUsers = _projectUserRepository.Get().ToList().Where(i => i.UserId == _userId && i.IsAccepted);
            
            return _repository.Get().ToList()
                .Where(i => i.UserId == _userId && i.ProjectId == null || 
                            projectsUsers.Any(p => i.ProjectId == p.ProjectId))
                .Select(i => new ToDoItemModel
                {
                    Id          = i.Id,
                    Header      = i.Header,
                    Notes       = i.Notes,
                    Date        = i.Date,
                    Deadline    = i.Deadline,
                    CompleteDay = i.CompleteDate,
                    ProjectName = i.ProjectId != null ? $"Project : {i.ProjectOf.Name}" : "",
                    ProjectId   = i.ProjectId
                })
                .Where(predicate);
        }

        public IEnumerable<ToDoItemModel> GetSharedProjectItems(int projectId)
        {
            var minDate = DateTime.MinValue.AddYears(1753);
            
            return _repository.Get()
                .Where(i => i.ProjectId == projectId && i.CompleteDate == minDate)
                .Select(i => new ToDoItemModel
                {
                    Id          = i.Id,
                    Header      = i.Header,
                    Notes       = i.Notes,
                    Date        = i.Date,
                    Deadline    = i.Deadline,
                    CompleteDay = i.CompleteDate,
                    ProjectId   = i.ProjectId,
                    ProjectName = i.ProjectId != null ? $"Project : {i.ProjectOf.Name}" : ""
                });
        }
    }
}