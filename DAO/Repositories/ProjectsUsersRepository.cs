using System;
using System.Collections.Generic;
using System.Linq;
using DAO.Entities;

namespace DAO.Repositories
{
    public class ProjectsUsersRepository : IRepository<ProjectsUsers>
    {
        private EfContext _context;

        public ProjectsUsersRepository()
        {
            _context = ContextSingleton.GetInstance();
        }

        public IEnumerable<ProjectsUsers> Get()
        {
            return _context.ProjectsUsers.AsEnumerable();
        }

        public bool Add(ProjectsUsers item)
        {
            try
            {
                _context.ProjectsUsers.Add(item);
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                ErrorHandler.Handle(e);

                return false;
            }

            return true;
        }

        public bool Remove(ProjectsUsers item)
        {
            try
            {
                _context.ProjectsUsers.Remove(item);
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