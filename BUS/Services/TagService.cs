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
        private readonly IRepository<Tag>     _tagRepository;
        private readonly IRepository<ItemTag> _itemTagRepository;
        private readonly int                  _userId;

        public TagService(int userId)
        {
            _tagRepository     = new TagsRepository();
            _itemTagRepository = new ItemsTagsRepository();
            _userId            = userId;
        }

        public override void RefreshRepositories()
        {
            _tagRepository.Refresh();
            _itemTagRepository.Refresh();
        }

        public void Add(TagModel tag)
        {
            var addingTag = new Tag
            {
                Text   = tag.Text,
                UserId = _userId
            };

            tag.Id = _tagRepository.Add(addingTag) ? addingTag.Id : -1;
        }

        public bool Remove(TagModel tag)
        {
            var foundItem = _tagRepository.Get().First(i => i.Id == tag.Id);

            return _tagRepository.Remove(foundItem);
        }

        public void Update(TagModel tag)
        {
            var foundItem = _tagRepository.Get().First(i => i.Id == tag.Id);

            foundItem.Text = tag.Text;

            _tagRepository.SaveChanges();
        }

        public void ReplaceItemsTags(int itemId, IEnumerable<int> tagId)
        {
            var itemsTags = _itemTagRepository.Get().Where(i => i.ItemId == itemId).ToList();

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
        }

        public IEnumerable<TagModel> Get(Func<TagModel, bool> predicate)
        {
            return _tagRepository.Get()
                .Where(i => i.UserId == _userId)
                .Select(i => new TagModel
                {
                    Id   = i.Id,
                    Text = i.Text
                })
                .Where(predicate);
        }

        public IEnumerable<TagModel> GetSelected(int itemId)
        {
            return _itemTagRepository.Get()
                .Where(i => i.ItemId == itemId)
                .Select(i => new TagModel
                {
                    Id   = i.TagOf.Id,
                    Text = i.TagOf.Text
                });
        }

        private List<Predicate<ItemTag>> GetPredicates(IEnumerable<string> projectNames)
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

        public IEnumerable<ToDoItemModel> GetItemsByTags(IEnumerable<string> tags, List<string> projectNames)
        {
            var predicates = GetPredicates(projectNames);
            var allItems = _itemTagRepository
                .Get()
                .ToList()
                .Where(i => i.TagOf.UserId == _userId && predicates.Any(p => p.Invoke(i))).ToList();
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
    }
}