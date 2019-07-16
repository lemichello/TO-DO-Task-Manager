using System;
using System.Collections.Generic;
using System.Linq;
using BUS.Models;
using DAO.Entities;
using DAO.Repositories;

namespace BUS.Services
{
    public class TagService
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
    }
}