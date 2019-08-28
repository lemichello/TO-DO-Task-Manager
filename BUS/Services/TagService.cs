using System;
using System.Collections.Generic;
using System.Linq;
using BUS.Models;
using DAO.Entities;
using DAO.Repositories;

namespace BUS.Services
{
    public class TagService : Service
    {
        private readonly IRepository<ProjectsUsers> _projectsUsersRepository;
        private readonly IRepository<Tag>           _tagRepository;
        private readonly IRepository<ItemTag>       _itemTagRepository;
        private readonly int                        _userId;
        private          List<ProjectsUsers>        _projectsUsers;
        private          List<Tag>                  _tags;
        private          List<ItemTag>              _itemTags;
        private static   TagService                 _self;

        private TagService(int userId)
        {
            _projectsUsersRepository = new EfRepository<ProjectsUsers>();
            _tagRepository           = new EfRepository<Tag>();
            _itemTagRepository       = new EfRepository<ItemTag>();
            _userId                  = userId;

            InitializeLists();
        }

        public void RefreshRepositories()
        {
            _projectsUsersRepository.Refresh();
            _tagRepository.Refresh();
            _itemTagRepository.Refresh();

            InitializeLists();
        }

        public static void Initialize(int userId)
        {
            _self = new TagService(userId);
        }

        public static TagService GetInstance()
        {
            return _self;
        }

        public void Add(TagModel tag)
        {
            var addingTag = new Tag
            {
                Text      = tag.Text,
                UserId    = _userId,
                ProjectId = tag.ProjectId
            };

            tag.Id = _tagRepository.Add(addingTag) ? addingTag.Id : -1;
            _tags.Add(addingTag);
        }

        public bool Remove(TagModel tag)
        {
            var foundTag = _tagRepository.GetByPredicate(i => i.Id == tag.Id).First();

            DeleteTagFromItemsTags(foundTag.Id);

            var deleteRes = _tagRepository.Remove(foundTag);

            if (deleteRes)
                _tags.Remove(foundTag);

            return deleteRes;
        }

        public bool RemoveSharedTags(int projectId)
        {
            if (_tags.Where(i => i.ProjectId == projectId).Any(i => !_tagRepository.Remove(i)))
            {
                return false;
            }

            _tags = FilterTags(_tagRepository.Get().ToList());

            return true;
        }

        public bool RemoveTagsFromTask(int taskId)
        {
            if (_itemTags.Where(i => i.ItemId == taskId).Any(i => !_itemTagRepository.Remove(i)))
            {
                return false;
            }

            _itemTags = FilterTags(_itemTagRepository.Get().ToList());

            return true;
        }

        public void Update(TagModel tag)
        {
            var foundTag = _tagRepository.Get().First(i => i.Id == tag.Id);

            _tags[_tags.IndexOf(foundTag)].Text = tag.Text;

            foundTag.Text = tag.Text;

            _tagRepository.SaveChanges();
        }

        public void ReplaceItemsTags(int itemId, IEnumerable<int> tagId)
        {
            var itemsTags = _itemTags.Where(i => i.ItemId == itemId).ToList();

            foreach (var i in itemsTags)
            {
                _itemTagRepository.Remove(i);
            }

            foreach (var i in tagId)
            {
                _itemTagRepository.Add(new ItemTag
                {
                    ItemId = itemId,
                    TagId  = i
                });
            }

            _itemTags = FilterTags(_itemTagRepository.Get().ToList());
        }

        public IEnumerable<TagModel> Get(Func<TagModel, bool> predicate)
        {
            return _tags
                .Select(i => new TagModel
                {
                    Id           = i.Id,
                    Text         = i.Text,
                    ProjectId    = i.ProjectId,
                    TagTextColor = i.ProjectId != null ? "#2295F2" : "Black"
                })
                .Where(predicate);
        }

        public IEnumerable<TagModel> GetSelected(int itemId)
        {
            return _itemTags
                .Where(i => i.ItemId == itemId)
                .Select(i => new TagModel
                {
                    Id           = i.TagOf.Id,
                    Text         = i.TagOf.Text,
                    ProjectId    = i.TagOf.ProjectId,
                    TagTextColor = i.TagOf.ProjectId != null ? "#2295F2" : "Black"
                });
        }

        public IEnumerable<ToDoItemModel> GetItemsByTags(IEnumerable<string> tags, List<string> projectNames)
        {
            var predicates = GetPredicates(projectNames);
            var allItems   = _itemTags.Where(i => predicates.Any(p => p.Invoke(i))).ToList();
            var itemsCount = new Dictionary<ToDoItem, int>();
            var tagTexts   = tags.ToList();

            foreach (var text in tagTexts)
            {
                foreach (var item in allItems)
                {
                    if (itemsCount.ContainsKey(item.ItemOf) && item.TagOf.Text == text)
                        itemsCount[item.ItemOf]++;
                    else if (item.TagOf.Text == text)
                        itemsCount[item.ItemOf] = 1;
                }
            }

            // Selecting ToDoItems, which have all searching-tags (their occurrences count are equal to
            // searching-tags length).
            var foundItems = itemsCount.Where(i => i.Value == tagTexts.Count).Select(i => i.Key).ToList();

            return foundItems.Select(i => new ToDoItemModel
            {
                Id          = i.Id,
                Header      = i.Header,
                Notes       = i.Notes,
                Date        = i.Date,
                Deadline    = i.Deadline,
                CompleteDay = i.CompleteDate,
                ProjectName = i.ProjectOf?.Name ?? "",
                ProjectId   = i.ProjectId
            });
        }

        private void InitializeLists()
        {
            _projectsUsers = FilterProjects(_projectsUsersRepository.Get().ToList());
            _tags          = FilterTags(_tagRepository.Get().ToList());
            _itemTags      = FilterTags(_itemTagRepository.Get().ToList());
        }

        private void DeleteTagFromItemsTags(int tagId)
        {
            var itemsTags = _itemTags.Where(i => i.TagId == tagId);

            foreach (var i in itemsTags)
            {
                _itemTagRepository.Remove(i);
            }

            _itemTags = FilterTags(_itemTagRepository.Get().ToList());
        }

        private static List<Predicate<ItemTag>> GetPredicates(IEnumerable<string> projectNames)
        {
            var minDate    = DateTime.MinValue.AddYears(1753);
            var predicates = new List<Predicate<ItemTag>>();

            foreach (var name in projectNames)
            {
                switch (name)
                {
                    case "Inbox":
                        predicates.Add(i => i.ItemOf.Date == minDate &&
                                            i.ItemOf.CompleteDate == minDate &&
                                            i.ItemOf.ProjectOf == null);
                        break;

                    case "Today":
                        predicates.Add(i => (i.ItemOf.Date <= DateTime.Today && i.ItemOf.Date != minDate ||
                                             i.ItemOf.Deadline <= DateTime.Today && i.ItemOf.Deadline != minDate) &&
                                            i.ItemOf.CompleteDate == minDate);
                        break;

                    case "Upcoming":
                        predicates.Add(i => i.ItemOf.Date > DateTime.Today && i.ItemOf.CompleteDate == minDate);
                        break;

                    case "Logbook":
                        predicates.Add(i => i.ItemOf.CompleteDate != minDate);
                        break;

                    default:
                        predicates.Add(i => i.ItemOf.ProjectOf != null && i.ItemOf.ProjectOf.Name == name);
                        break;
                }
            }

            return predicates;
        }

        private List<Tag> FilterTags(IEnumerable<Tag> allTags)
        {
            return allTags
                .Where(i => i.UserId == _userId ||
                            i.ProjectId != null && _projectsUsers.Any(p => p.ProjectId == i.ProjectId))
                .ToList();
        }

        private List<ItemTag> FilterTags(IEnumerable<ItemTag> itemTags)
        {
            return itemTags
                .Where(i => i.TagOf.UserId == _userId ||
                            i.TagOf.ProjectId != null &&
                            _projectsUsers.Any(p => p.ProjectId == i.TagOf.ProjectId))
                .ToList();
        }

        private List<ProjectsUsers> FilterProjects(IEnumerable<ProjectsUsers> projectsUsers)
        {
            return projectsUsers.Where(p => p.UserId == _userId &&
                                            p.IsAccepted).ToList();
        }
    }
}