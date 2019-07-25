using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using BUS.Models;
using DAO.Entities;
using DAO.Repositories;

namespace BUS.Services
{
    public class ProjectService : Service
    {
        private readonly IRepository<User>          _userRepository;
        private readonly IRepository<Project>       _projectRepository;
        private readonly IRepository<ProjectsUsers> _projectUserRepository;
        private readonly int                        _userId;

        public ProjectService(int userId)
        {
            _userRepository        = new UsersRepository();
            _projectRepository     = new ProjectsRepository();
            _projectUserRepository = new ProjectsUsersRepository();
            _userId                = userId;
        }

        public int AddProject(ProjectModel project, IEnumerable<string> userLogins)
        {
            var users  = _userRepository.Get().ToList();
            var logins = userLogins.ToList();

            if (!logins.All(i => users.Any(user => user.Login == i)))
            {
                MessageBox.Show("One of user logins doesn't exist");
                return -1;
            }

            if (logins.Contains(users.First(user => user.Id == _userId).Login))
            {
                MessageBox.Show("You can't invite yourself. This will be done automatically");
                return -1;
            }

            var newProject = new Project {Name = project.Name};

            if (!_projectRepository.Add(newProject))
            {
                return -1;
            }

            // Adding user, which created project.
            _projectUserRepository.Add(new ProjectsUsers
            {
                ProjectId  = newProject.Id,
                UserId     = _userId,
                IsAccepted = true
            });
            
            return AddInvitedUsers(logins, newProject, users) ? newProject.Id : -1;
        }

        private bool AddInvitedUsers(IEnumerable<string> logins, Project newProject, List<User> users)
        {
            foreach (var i in logins)
            {
                if (!_projectUserRepository.Add(new ProjectsUsers
                {
                    ProjectId  = newProject.Id,
                    UserId     = users.Find(user => user.Login == i).Id,
                    IsAccepted = false
                }))
                {
                    return false;
                }
            }

            return true;
        }

        public IEnumerable<ProjectModel> GetProjects()
        {
            return _projectUserRepository.Get().ToList().Where(i => i.UserId == _userId && i.IsAccepted).Select(
                i => new ProjectModel
                {
                    Id   = i.ProjectId,
                    Name = i.ProjectOf.Name
                });
        }
    }
}