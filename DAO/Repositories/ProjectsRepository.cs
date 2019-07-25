using System;
using System.Collections.Generic;
using System.Linq;
using DAO.Entities;

namespace DAO.Repositories
{
    public class ProjectsRepository : IRepository<Project>
    {
        private EfContext _context;

        public ProjectsRepository()
        {
            _context = ContextSingleton.GetInstance();
        }

        public IEnumerable<Project> Get()
        {
            return _context.Projects.AsEnumerable();
        }

        public bool Add(Project item)
        {
            try
            {
                _context.Projects.Add(item);
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                ErrorHandler.Handle(e);

                return false;
            }

            return true;
        }

        public bool Remove(Project item)
        {
            try
            {
                _context.Projects.Remove(item);
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

        public void Refresh()
        {
            _context = ContextSingleton.GetInstance();
        }
    }
}